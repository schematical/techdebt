using System;
using UnityEditor;
using UnityEngine;
using System.IO;
using DefaultNamespace;
using UnityEditor.U2D.Animation;
using UnityEngine.U2D.Animation;
using System.Collections.Generic;
using UnityEditor.SceneManagement;
using System.Linq;
using Unity.VisualScripting;
using ColorUtility = UnityEngine.ColorUtility;
using Object = UnityEngine.Object;

public class EditorSpriteManager
{
    private const string MasterBodySpriteSheetPath = "Assets/ImportedSprites/NPCv2/NPCBody.png";
    private const string MasterHeadFrontSpriteSheetPath = "Assets/ImportedSprites/NPCv2/NPCHeadFront.png";
    private const string MasterHeadBackSpriteSheetPath = "Assets/ImportedSprites/NPCv2/NPCHeadBack.png";
   
    private const string GeneratedAssetsPath = "Assets/ImportedSprites/Generated";
    private const string GameScenePath = "Assets/Scenes/GameScene.unity";

    [MenuItem("Tech Debt/Generate NPC Sprite Sheets")]
    public static void GenerateSpriteSheets()
    {
        EditorSceneManager.OpenScene(GameScenePath);
        SpriteManager spriteManager = Object.FindObjectOfType<SpriteManager>();
        if (spriteManager == null)
        {
            Debug.LogError("Could not find SpriteManager in the scene. Please add it to the scene.");
            return;
        }

        if (Directory.Exists(GeneratedAssetsPath))
        {
            Directory.Delete(GeneratedAssetsPath, true);
        }
        Directory.CreateDirectory(GeneratedAssetsPath);

        List<ProcessSpriteSheetResult> results = ProcessSpriteSheet(MasterBodySpriteSheetPath, "NPCBody", spriteManager);
        spriteManager.bodySpriteLibraryAssetCollections.Clear();
        foreach (ProcessSpriteSheetResult result in  results)
        {
            Debug.Log($"Processing Result: {result.catId} {result.newTexturePath}");
            SpriteLibraryAsset asset =
                CreateSpriteLibraryAsset(
                    result.newTexturePath,
                    result.subdir,
                    spriteManager.bodySpriteLibraryAssets[0]
                );
            BodySpriteLibraryAssetCollection coll =
                spriteManager.bodySpriteLibraryAssetCollections.Find((collection => result.catId == collection.catId));
            if (coll == null)
            {
                coll = new BodySpriteLibraryAssetCollection()
                {
                    catId = result.catId,
                };
                spriteManager.bodySpriteLibraryAssetCollections.Add(coll);
            }
            coll.assets.Add(asset); ;
        }

        ProcessSpriteSheet(MasterHeadFrontSpriteSheetPath, "NPCHeadFront", spriteManager);
        ProcessSpriteSheet(MasterHeadBackSpriteSheetPath, "NPCHeadBack", spriteManager);
        // ProcessSpriteSheet(MasterFacialExpressionsSpriteSheetPath, "NPCFacialExpressions", spriteManager, null);
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
    [MenuItem("Tech Debt/Debug Sprite Sheets")]
    public static void DebugSpriteSheets()
    {
        EditorSceneManager.OpenScene(GameScenePath);
        SpriteManager spriteManager = Object.FindObjectOfType<SpriteManager>();
        if (spriteManager == null)
        {
            Debug.LogError("Could not find SpriteManager in the scene. Please add it to the scene.");
            return;
        }

        spriteManager.Init();
        foreach (ColorMap colorMap in spriteManager.ColorMaps)
        {
            Debug.Log($"Id:  {colorMap.id} - Color: {colorMap.findColor.ToHexString()} - Darker Color: {colorMap.findDarkerColor.ToHexString()}");
        }

        // Generate and save Aseprite palette
        var allColors = new HashSet<Color>();
        foreach (var colorMap in spriteManager.ColorMaps)
        {
            allColors.Add(colorMap.findColor);
            allColors.Add(colorMap.findDarkerColor);
        }

        // Build the .gpl file content
        var gplContent = new System.Text.StringBuilder();
        gplContent.AppendLine("GIMP Palette");
        gplContent.AppendLine("Name: Generated NPC Palette");
        gplContent.AppendLine("Columns: 8");
        gplContent.AppendLine("#");

        int colorIndex = 0;
        foreach (Color color in allColors)
        {
            int r = Mathf.RoundToInt(color.r * 255);
            int g = Mathf.RoundToInt(color.g * 255);
            int b = Mathf.RoundToInt(color.b * 255);
            gplContent.AppendLine($"{r}\t{g}\t{b}\tColor_{colorIndex++}");
        }

        // Save the file
        if (!Directory.Exists(GeneratedAssetsPath))
        {
            Directory.CreateDirectory(GeneratedAssetsPath);
        }
        string palettePath = Path.Combine(GeneratedAssetsPath, "GeneratedPalette.gpl");
        File.WriteAllText(palettePath, gplContent.ToString());
        Debug.Log($"Generated Aseprite palette at: {palettePath}");
    }

    private static List<ProcessSpriteSheetResult> ProcessSpriteSheet(string masterSpriteSheetPath, string baseName, SpriteManager spriteManager)
    {
        Texture2D masterTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(masterSpriteSheetPath);
        if (masterTexture == null)
        {
            throw new SystemException($"Could not find master sprite sheet at: {masterSpriteSheetPath}");
        }

        TextureImporter masterImporter = AssetImporter.GetAtPath(masterSpriteSheetPath) as TextureImporter;
        if (masterImporter == null)
        {
            throw new SystemException($"Could not get importer for: {masterSpriteSheetPath}");
        }
        SpriteReplacementContext context = spriteManager.PopulateColorReplaceCollections(masterTexture);
        
        /*string log = $"Color Replace Collections - Count{context.colorReplaceCollections.Count}:\n";
        foreach (ColorReplaceCollection collection in context.colorReplaceCollections)
        {
            log += $"  Collection ID: {collection.id}\n";
            foreach (string key in collection.replacmentCombo.Keys)
            {
                ColorReplaceCombo combo = collection.replacmentCombo[key];
                log += $"    Find Color: {combo.findColor.ToHexString()}, Replace Color: {combo.selectedReplaceColor.ToHexString()}\n";
            }
        }
        foreach(string key in context.colorReplacementMaps.Keys)
        {
            log += $"  Color Replacement Map: {key} - positions.Count: {context.colorReplacementMaps[key].positions.Count}\n";
        }
        Debug.Log(log);*/
        List<ProcessSpriteSheetResult> results = new List<ProcessSpriteSheetResult>();
        foreach (ColorReplaceCollection collection in context.colorReplaceCollections)
        {

            string newTextureDir = $"{GeneratedAssetsPath}/{baseName}";
            if (!Directory.Exists(newTextureDir))
            {
                Directory.CreateDirectory(newTextureDir);
            }

            
            Texture2D newTexture = new Texture2D(masterTexture.width, masterTexture.height);
            newTexture.filterMode = FilterMode.Point;

            Color[] pixels = masterTexture.GetPixels();
            int skin = -1;
            foreach (ColorReplaceCombo combo in collection.replacmentCombo)
            {
                if (combo.colorMapId == "skin")
                {
                    skin = combo._index;
                }
                pixels = UpdateTexture(context, pixels, combo.findColor, combo.selectedReplaceColor);
                // Debug.Log($"Trying to replace findDarkerColor {combo.findDarkerColor.ToHexString()} with {combo.darkerSelectedReplaceColor.ToHexString()} - {context.colorReplacementMaps[combo.findDarkerColor.ToHexString()].positions.Count}");
                pixels = UpdateTexture(context, pixels, combo.findDarkerColor, combo.darkerSelectedReplaceColor);
            }
            
            newTexture.SetPixels(pixels);
            newTexture.Apply();
            string subdir = $"{baseName}";
            string newTexturePath = $"{newTextureDir}/{collection.id}.png";
            if (skin != -1)
            {
                subdir = $"{baseName}/skin_{skin}";
                newTextureDir = $"{GeneratedAssetsPath}/{subdir}";
                newTexturePath = $"{newTextureDir}/{collection.id}.png";
                if (!Directory.Exists(newTextureDir))
                {
                    Directory.CreateDirectory(newTextureDir);
                }

            }
            byte[] bytes = newTexture.EncodeToPNG();
            File.WriteAllBytes(newTexturePath, bytes);
            AssetDatabase.ImportAsset(newTexturePath);
        


            TextureImporter newImporter = AssetImporter.GetAtPath(newTexturePath) as TextureImporter;
            newImporter.spriteImportMode = SpriteImportMode.Multiple;
            newImporter.spritePixelsPerUnit = 32;
            newImporter.filterMode = FilterMode.Point;
            newImporter.textureCompression = TextureImporterCompression.Uncompressed;
            newImporter.SetPlatformTextureSettings("Standalone", 2048, TextureImporterFormat.RGBA32, 100, false);
            newImporter.spritesheet = masterImporter.spritesheet;
            EditorUtility.SetDirty(newImporter);
            newImporter.SaveAndReimport();
            results.Add(new ProcessSpriteSheetResult()
            {
                catId = $"s_{skin.ToString()}",
                newTexturePath = newTexturePath,
                subdir = $"{subdir}/{collection.id}"
            });
            
           
            
        }

        return results;

    }

    private static Color[] UpdateTexture(SpriteReplacementContext context, Color[] pixels, Color findColor, Color replaceColor)
    {
        if (!context.colorReplacementMaps.ContainsKey(findColor.ToHexString()))
        {
            // Debug.LogError($"MISSING: context.colorReplacementMaps {findColor.ToHexString()} - {string.Join(", ", context.colorReplacementMaps.Keys)}");
            return pixels;
        }
        // Debug.Log($"FOUND: context.colorReplacementMaps matched {findColor.ToHexString()} - {string.Join(", ", context.colorReplacementMaps.Keys)}");
        SpriteReplacementMap spriteReplacementMap = context.colorReplacementMaps[findColor.ToHexString()];
        
        foreach (int p in spriteReplacementMap.positions)
        {
            pixels[p] = replaceColor;
        }

        return pixels;
    }

    private static SpriteLibraryAsset CreateSpriteLibraryAsset(string texturePath, string baseName, SpriteLibraryAsset masterLibraryAsset)
    {
        Sprite[] sprites = AssetDatabase.LoadAllAssetsAtPath(texturePath).OfType<Sprite>().ToArray();
        SpriteLibraryAsset asset = ScriptableObject.CreateInstance<SpriteLibraryAsset>();
        
        if(masterLibraryAsset != null)
        {
            foreach (var category in masterLibraryAsset.GetCategoryNames())
            {
                foreach (var label in masterLibraryAsset.GetCategoryLabelNames(category))
                {
                    Sprite masterSprite = masterLibraryAsset.GetSprite(category, label);
                    Sprite newSprite = sprites.FirstOrDefault(s => s.name == masterSprite.name);
                    if (newSprite != null)
                    {
                        asset.AddCategoryLabel(newSprite, category, label);
                    }
                }
            }
        } else
        {
            foreach (Sprite sprite in sprites)
            {
                asset.AddCategoryLabel(sprite, sprite.name, sprite.name);
            }
        }

        string libAssetPath = $"{GeneratedAssetsPath}/{baseName}.asset";
  
        AssetDatabase.CreateAsset(asset, libAssetPath);
        return asset;
    }
}

public class ProcessSpriteSheetResult
{
    public string newTexturePath;
    public string subdir;
    public string catId;
}