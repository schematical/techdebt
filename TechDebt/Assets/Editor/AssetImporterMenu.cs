// AssetImporterMenu.cs
using UnityEngine;
using UnityEditor;
using System.IO;

public class AssetImporterMenu
{
    private const string SourceAssetFolderName = "_diagrams";
    private const string DestinationAssetPath = "Assets/ImportedSprites";

    [MenuItem("TechDebt/Import Diagram Assets")]
    public static void ImportAssets()
    {
        // Application.dataPath is "D:/.../TechDebt/Assets"
        // GetParent gives "D:/.../TechDebt" (the Unity project root)
        string unityProjectRoot = Directory.GetParent(Application.dataPath).FullName;
        // GetParent again gives "D:/.../techdebt" (the workspace root)
        string workspaceRoot = Directory.GetParent(unityProjectRoot).FullName;

        string sourcePath = Path.Combine(workspaceRoot, SourceAssetFolderName);

        if (!Directory.Exists(sourcePath))
        {
            Debug.LogError($"Source asset path not found: {sourcePath}");
            return;
        }

        if (!Directory.Exists(DestinationAssetPath))
        {
            Directory.CreateDirectory(DestinationAssetPath);
        }

        string[] pngFiles = Directory.GetFiles(sourcePath, "*.png");

        foreach (string pngFilePath in pngFiles)
        {
            string jsonFilePath = Path.ChangeExtension(pngFilePath, ".json");
            if (File.Exists(jsonFilePath))
            {
                string fileName = Path.GetFileName(pngFilePath);
                string destPngPath = Path.Combine(DestinationAssetPath, fileName);
                File.Copy(pngFilePath, destPngPath, true);

                string jsonFileName = Path.GetFileName(jsonFilePath);
                string destJsonPath = Path.Combine(DestinationAssetPath, jsonFileName);
                File.Copy(jsonFilePath, destJsonPath, true);
                
                Debug.Log($"Copied {fileName} and its JSON metadata to {DestinationAssetPath}");
            }
        }

        AssetDatabase.Refresh();
        Debug.Log("Asset import complete. Refreshing asset database.");
    }
}