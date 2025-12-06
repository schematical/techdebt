// AsepriteImporter.cs
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

// C# classes to match Aseprite's JSON structure
[System.Serializable]
public class AsepriteFrameData
{
    public int x;
    public int y;
    public int w;
    public int h;
}

[System.Serializable]
public class AsepriteFrame
{
    public string filename;
    public AsepriteFrameData frame;
    public bool rotated;
    public bool trimmed;
    public AsepriteFrameData spriteSourceSize;
    public AsepriteFrameData sourceSize;
    public int duration;
}

[System.Serializable]
public class AsepriteJson
{
    public Dictionary<string, AsepriteFrame> frames;
}

public class AsepriteImporter : AssetPostprocessor
{
    void OnPreprocessTexture()
    {
        if (!assetPath.Contains("Assets/ImportedSprites"))
        {
            return;
        }

        string jsonPath = Path.ChangeExtension(assetPath, ".json");
        if (!File.Exists(jsonPath))
        {
            return;
        }

        string jsonText = File.ReadAllText(jsonPath);
        AsepriteJson asepriteJson = JsonUtility.FromJson<AsepriteJson>(jsonText);
        
        if (asepriteJson == null || asepriteJson.frames == null) {
            Debug.LogError($"Failed to parse Aseprite JSON at {jsonPath}");
            return;
        }

        TextureImporter textureImporter = (TextureImporter)assetImporter;

        // Configure the main texture settings for pixel art.
        textureImporter.textureType = TextureImporterType.Sprite;
        textureImporter.spriteImportMode = SpriteImportMode.Multiple;
        textureImporter.spritePixelsPerUnit = 32; // Correct property for PPU
        textureImporter.filterMode = FilterMode.Point;
        textureImporter.textureCompression = TextureImporterCompression.Uncompressed;

        List<SpriteMetaData> spritesheet = new List<SpriteMetaData>();
        int frameCount = 0;
        foreach (var frame in asepriteJson.frames)
        {
            SpriteMetaData smd = new SpriteMetaData();
            smd.name = Path.GetFileNameWithoutExtension(assetPath) + "_" + frameCount;
            smd.rect = new Rect(frame.Value.frame.x, frame.Value.frame.y, frame.Value.frame.w, frame.Value.frame.h);
            smd.alignment = (int)SpriteAlignment.Center;
            smd.pivot = new Vector2(0.5f, 0.5f);
            spritesheet.Add(smd);
            frameCount++;
        }

        textureImporter.spritesheet = spritesheet.ToArray();
        
        Debug.Log($"Successfully imported and sliced {Path.GetFileName(assetPath)} with {spritesheet.Count} frames.");
    }
}
