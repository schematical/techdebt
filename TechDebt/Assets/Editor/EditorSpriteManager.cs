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

        ProcessSpriteSheet(MasterBodySpriteSheetPath, "NPCBody", spriteManager, spriteManager.bodySpriteLibraryAssets[0]);
        ProcessSpriteSheet(MasterHeadFrontSpriteSheetPath, "NPCHeadFront", spriteManager, null);
        ProcessSpriteSheet(MasterHeadBackSpriteSheetPath, "NPCHeadBack", spriteManager, null);
        // ProcessSpriteSheet(MasterFacialExpressionsSpriteSheetPath, "NPCFacialExpressions", spriteManager, null);
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private static void ProcessSpriteSheet(string masterSpriteSheetPath, string baseName, SpriteManager spriteManager, SpriteLibraryAsset masterLibraryAsset)
    {
        Texture2D masterTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(masterSpriteSheetPath);
        if (masterTexture == null)
        {
            Debug.LogError($"Could not find master sprite sheet at: {masterSpriteSheetPath}");
            return;
        }

        TextureImporter masterImporter = AssetImporter.GetAtPath(masterSpriteSheetPath) as TextureImporter;
        if (masterImporter == null)
        {
            Debug.LogError($"Could not get importer for: {masterSpriteSheetPath}");
            return;
        }
        SpriteReplacementContext context = spriteManager.PopulateColorReplaceCollections(masterTexture);
        
        string log = $"Color Replace Collections - Count{context.colorReplaceCollections.Count}:\n";
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
        Debug.Log(log);
        foreach (ColorReplaceCollection collection in context.colorReplaceCollections)
        {

            string newTextureDir = $"{GeneratedAssetsPath}/{baseName}";
            if (!Directory.Exists(newTextureDir))
            {
                Directory.CreateDirectory(newTextureDir);
            }

            string newTexturePath = $"{newTextureDir}/_{collection.id}.png";
            Texture2D newTexture = new Texture2D(masterTexture.width, masterTexture.height);
            newTexture.filterMode = FilterMode.Point;

            Color[] pixels = masterTexture.GetPixels();

            foreach (string key in collection.replacmentCombo.Keys)
            {
                ColorReplaceCombo combo = collection.replacmentCombo[key];
                pixels = UpdateTexture(context, pixels, combo.findColor, combo.selectedReplaceColor);
            }

            newTexture.SetPixels(pixels);
            newTexture.Apply();

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
            // CreateSpriteLibraryAsset(newTexturePath, baseName, i, j, masterLibraryAsset);
            
        }

    }

    private static Color[] UpdateTexture(SpriteReplacementContext context, Color[] pixels, Color findColor, Color replaceColor)
    {
        if (!context.colorReplacementMaps.ContainsKey(findColor.ToHexString()))
        {
            Debug.LogError($"MISSING: context.colorReplacementMaps {findColor.ToHexString()} - {string.Join(", ", context.colorReplacementMaps.Keys)}");
            return pixels;
        }
        Debug.Log($"FOUND: context.colorReplacementMaps matched {findColor.ToHexString()} - {string.Join(", ", context.colorReplacementMaps.Keys)}");
        SpriteReplacementMap spriteReplacementMap = context.colorReplacementMaps[findColor.ToHexString()];
        
        foreach (int p in spriteReplacementMap.positions)
        {
            Debug.Log($"Setting {p} From {pixels[p].ToString()} to {replaceColor.ToHexString()}");
            pixels[p] = replaceColor;
        }

        return pixels;
    }

    private static void CreateSpriteLibraryAsset(string texturePath, string baseName, int colorMapIndex, int colorIndex, SpriteLibraryAsset masterLibraryAsset)
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

        string libAssetDir = $"{GeneratedAssetsPath}/{baseName}";
        if (!Directory.Exists(libAssetDir))
        {
            Directory.CreateDirectory(libAssetDir);
        }
        string libAssetPath = $"{libAssetDir}/{colorMapIndex}_{colorIndex}_lib.asset";
        AssetDatabase.CreateAsset(asset, libAssetPath);
    }
}