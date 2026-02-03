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
        ProcessBody(spriteManager);
        ProcessHead(spriteManager);


        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    public static void ProcessHead(SpriteManager spriteManager)
    {
        Vector2 pivot =  new Vector2(0.5f, 0.375f); // 16px horizontal, 12px vertical for 32x32
        List<ProcessSpriteSheetResult> headFrontResults =  ProcessSpriteSheet(MasterHeadFrontSpriteSheetPath, "NPCHeadFront", spriteManager, pivot);
        List<ProcessSpriteSheetResult> headBackResults =  ProcessSpriteSheet(MasterHeadBackSpriteSheetPath, "NPCHeadBack", spriteManager, pivot);
        SpriteLibraryAsset asset = ScriptableObject.CreateInstance<SpriteLibraryAsset>();
        for (int i = 0; i < headFrontResults.Count; i++)
        {
            
            
            
            Sprite[] sprites = AssetDatabase.LoadAllAssetsAtPath(headFrontResults[i].newTexturePath).OfType<Sprite>().ToArray();
            for (int ii = 0; ii < sprites.Length; ii++)
            {
                Sprite sprite = sprites[ii];
                asset.AddCategoryLabel(sprite, $"{ii}/{headFrontResults[i].catId}/{headFrontResults[i].id}", "Front");
            }
            
            
            
            sprites = AssetDatabase.LoadAllAssetsAtPath(headBackResults[i].newTexturePath).OfType<Sprite>().ToArray();
            for (int ii = 0; ii < sprites.Length; ii++)
            {
                Sprite sprite = sprites[ii];
                asset.AddCategoryLabel(sprite, $"{ii}/{headBackResults[i].catId}/{headBackResults[i].id}", "Back");
            }

           
        }
        string libAssetPath = $"{GeneratedAssetsPath}/NPCHead.asset";

        AssetDatabase.CreateAsset(asset, libAssetPath);
        spriteManager.headSpriteLibraryAsset = asset;
    }
    public static void ProcessBody(SpriteManager spriteManager)
    {
        Vector2 pivot =  new Vector2(0.5f, 1); // 16px horizontal, 12px vertical for 32x32
        List<ProcessSpriteSheetResult> results = ProcessSpriteSheet(MasterBodySpriteSheetPath, "NPCBody", spriteManager, pivot);
        spriteManager.bodySpriteLibraryAssetCollections.Clear();
        foreach (ProcessSpriteSheetResult result in  results)
        {
            Debug.Log($"Processing Result: {result.catId} {result.newTexturePath}");
            SpriteLibraryAsset asset =
                CreateSpriteLibraryAsset(
                    result.newTexturePath,
                    result.subdir,
                    spriteManager.baseBodySpriteLibraryAsset
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

    private static List<ProcessSpriteSheetResult> ProcessSpriteSheet(string masterSpriteSheetPath, string baseName, SpriteManager spriteManager, Vector2 pivot)
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
            // Adjust pivots based on sprite type
            var newSpritesheet = newImporter.spritesheet;
            for (int i = 0; i < newSpritesheet.Length; i++)
            {
                newSpritesheet[i].pivot = pivot;
            }
            newImporter.spritesheet = newSpritesheet;

            EditorUtility.SetDirty(newImporter);
            newImporter.SaveAndReimport();
            results.Add(new ProcessSpriteSheetResult()
            {
                id = collection.id,
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

        if (masterLibraryAsset == null)
        {
            throw new SystemException("Missing body masterLibraryAsset");
        }

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
    public string id { get; set; }
}