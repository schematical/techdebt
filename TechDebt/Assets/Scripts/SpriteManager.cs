using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.VisualScripting;
using UnityEditor.U2D.Animation;
using UnityEngine;
using UnityEngine.U2D.Animation;
using Random = UnityEngine.Random;

namespace DefaultNamespace
{
    [Serializable]
    public class SpriteManager : MonoBehaviour
    {
        private bool _initialized = false;
        public List<ColorMap> ColorMaps = new List<ColorMap>();

        public List<SpriteCollection> SpriteCollections = new List<SpriteCollection>();
        public List<HeadSpriteCollection> headSpriteCollections = new List<HeadSpriteCollection>();
        public List<SpriteLibraryAsset> bodySpriteLibraryAssets = new List<SpriteLibraryAsset>();

        public List<BodySpriteLibraryAssetCollection> bodySpriteLibraryAssetCollections =
            new List<BodySpriteLibraryAssetCollection>();

        public static Color MakeDarker(Color color, float amount = .2f)
        {
            float r = color.r - amount;
            if (r < 0)
            {
                r = 0;
            }
            float g = color.g - amount;
            if (g < 0)
            {
                g = 0;
            }
            float b = color.b - amount;
            if (b < 0)
            {
                b = 0;
            }
            return new Color(
                r,
                g,
                b,
                color.a
            );
        }

        public HeadSpriteCollection GetHeadSpriteCollection()
        {
            HeadSpriteCollection found = headSpriteCollections[Random.Range(0, headSpriteCollections.Count)];
            // List<ColorMap> colorMaps = GetRandomizedColorMaps();
            HeadSpriteCollection newColl = new HeadSpriteCollection()
            {
                headFront = found.headFront, // RandomizeSpriteColors(found.headFront, colorMaps),
                headBack = found.headBack, // RandomizeSpriteColors(found.headBack, colorMaps),
            };
            return newColl;
        }

        public Sprite GetRandom(string spriteCollectionId)
        {
            SpriteCollection collection = SpriteCollections.Find(x => x.Id == spriteCollectionId);
            if (collection == null)
            {
                throw new SystemException("Missing sprite collection id: " + spriteCollectionId);
            }

            if (collection.Sprites.Count == 0)
            {
                throw new SystemException("Missing sprite collection with 0 elements: " + spriteCollectionId);
            }

            int i = Random.Range(0, collection.Sprites.Count);
            Sprite sprite = collection.Sprites[i];

            return sprite; // RandomizeSpriteColors(sprite);
        }

        public void Init()
        {
            if (_initialized)
            {
                return;
            }

            foreach (ColorMap colorMap in ColorMaps)
            {
                colorMap.findDarkerColor = MakeDarker(colorMap.findColor);
                colorMap.replaceDarkerColors = new List<Color>();
                foreach (Color replaceColor in colorMap.replaceColors)
                {
                    colorMap.replaceDarkerColors.Add(MakeDarker(replaceColor));
                }
            }
        }

        public SpriteReplacementContext PopulateColorReplaceCollections(Texture2D texture)
        {
            Init();
            List<ColorReplaceCollection> colorReplaceCollections = new List<ColorReplaceCollection>();
            Dictionary<string, SpriteReplacementMap> colorReplacementMaps =
                new Dictionary<string, SpriteReplacementMap>();
            foreach (ColorMap colorMap in ColorMaps)
            {
                colorMap.findDarkerColor = MakeDarker(colorMap.findColor);
                if (colorReplacementMaps.ContainsKey(colorMap.findColor.ToHexString()))
                {
                    throw new SystemException("Duplicate color map found: " + colorMap.findColor.ToHexString());
                }

                SpriteReplacementMap spriteReplacementMap = GetSpriteReplacementMap(texture, colorMap.findColor);
                if (spriteReplacementMap.positions.Count > 0)
                {
                    colorReplacementMaps.Add(colorMap.findColor.ToHexString(), spriteReplacementMap);
                }


                SpriteReplacementMap darkerSpriteReplacementMap = GetSpriteReplacementMap(texture, colorMap.findColor);
                if (spriteReplacementMap.positions.Count > 0)
                {
                    colorReplacementMaps.Add(colorMap.findDarkerColor.ToHexString(), darkerSpriteReplacementMap);
                }
            }

            // Now we have all the pixels of each color that we need to replace.
            SpriteReplacementContext context = new SpriteReplacementContext()
            {
                colorReplacementMaps = colorReplacementMaps,
                colorReplaceCollections = colorReplaceCollections
            };
            BuildColorReplaceCollectionsRecursive(context, 0, new ColorReplaceCollection());

            return context;
        }

        protected void BuildColorReplaceCollectionsRecursive(SpriteReplacementContext context, int depth,
            ColorReplaceCollection currColorReplaceCollection)
        {
            // Debug.Log($"BuildColorReplaceCollectionsRecursive: {depth} - currColorReplaceCollection.id: {currColorReplaceCollection.id}");
            /*for (int i = depth; i < ColorMaps.Count; i++)
            {*/
            if (depth == context.colorReplacementMaps.Count - 1)
            {
                // This is the end of the line
                context.colorReplaceCollections.Add(currColorReplaceCollection);
                return;
            }

            ColorMap colorMap = ColorMaps[depth];
            bool isDarker = false;

            if (
                !context.colorReplacementMaps.ContainsKey(colorMap.findColor.ToHexString()) &&
                !context.colorReplacementMaps.ContainsKey(colorMap.findDarkerColor.ToHexString()))
            {
                BuildColorReplaceCollectionsRecursive(context, depth + 1, currColorReplaceCollection);
                return; //No update needed
            }

            for (int ii = 0; ii < colorMap.replaceColors.Count; ii++)
            {
                ColorReplaceCollection newColorReplaceCollection = currColorReplaceCollection.Clone();
                string id = ii.ToString();
                if (colorMap.id != null && colorMap.id.Length > 0)
                {
                    id = $"{colorMap.id}-{ii}";
                }

                newColorReplaceCollection.id += $"_{id}";
                // Debug.Log($"depth: {depth}  - ii: {ii} - colorMap.replaceColors.Count {colorMap.replaceColors.Count} - {colorMap.replaceColors[ii].ToHexString()}");
                if (newColorReplaceCollection.replacmentCombo.ContainsKey(colorMap.replaceColors[ii].ToHexString()))
                {
                    throw new SystemException("Duplicate color map found: " +id + " - " + colorMap.replaceColors[ii].ToHexString() +
                                                                            " --> Keys: " + string.Join(", ",
                                                                                newColorReplaceCollection.replacmentCombo.Keys));
                }

                newColorReplaceCollection.replacmentCombo.Add(colorMap.replaceColors[ii].ToHexString(),
                    new ColorReplaceCombo()
                    {
                        colorMapId = colorMap.id,
                        _index = ii,
                        findColor = colorMap.findColor,
                        findDarkerColor = colorMap.findDarkerColor,
                        selectedReplaceColor = colorMap.replaceColors[ii],
                        darkerSelectedReplaceColor = colorMap.replaceDarkerColors[ii]
                    });

                BuildColorReplaceCollectionsRecursive(context, depth + 1, newColorReplaceCollection);
            }
            //}
        }

        public SpriteReplacementMap GetSpriteReplacementMap(Texture2D texture, Color color)
        {
            SpriteReplacementMap spriteReplacementMap = new SpriteReplacementMap();
            Color[] pixels = texture.GetPixels();

            for (int p = 0; p < pixels.Length; p++)
            {
                if (pixels[p] == color)
                {
                    spriteReplacementMap.positions.Add(p);
                }
            }

            return spriteReplacementMap;
        }

        /*public Sprite RandomizeSpriteColors(Sprite sprite, List<ColorMap> populatedColorMaps = null)
        {
            Init();
            if (populatedColorMaps == null)
            {
                populatedColorMaps = GetRandomizedColorMaps();
            }

            Texture2D originalTexture = sprite.texture;
            Texture2D newTexture = new Texture2D(originalTexture.width, originalTexture.height);
            newTexture.filterMode = FilterMode.Point;

            Color[] pixels = originalTexture.GetPixels();



            for (int p = 0; p < pixels.Length; p++)
            {
                foreach (ColorMap colorMap in populatedColorMaps)
                {
                    // Debug.Log($"Testing {pixels[p]} == {colorMap.findColor}");
                    if (pixels[p] == colorMap.findColor)
                    {

                        // Debug.Log($"Replacing {pixels[p]} == i: {i} - {colorMap.replaceColors[i]}");
                        pixels[p] = colorMap.selectedReplaceColor;

                    }
                    if (pixels[p] == colorMap.findDarkerColor)
                    {

                        // Debug.Log($"Replacing {pixels[p]} == i: {i} - {colorMap.replaceColors[i]}");
                        pixels[p] = colorMap.darkerSelectedReplaceColor;

                    }
                }
            }


            newTexture.SetPixels(pixels);
            newTexture.Apply();

            Vector2 pivot = new Vector2(sprite.pivot.x / sprite.rect.width, sprite.pivot.y / sprite.rect.height);
            return Sprite.Create(newTexture, sprite.rect, pivot, sprite.pixelsPerUnit);
        }*/
    }

    [Serializable]
    public class ColorMap
    {
        public string id;
        public Color findColor;
        public Color findDarkerColor;
        public List<Color> replaceColors = new List<Color>();
        public List<Color> replaceDarkerColors = new List<Color>();
    }

    public class ColorReplaceCollection
    {
        public string id;
        public Dictionary<string, ColorReplaceCombo> replacmentCombo = new Dictionary<string, ColorReplaceCombo>();

        public ColorReplaceCollection Clone()
        {
            ColorReplaceCollection clone = new ColorReplaceCollection
            {
                id = this.id,
                replacmentCombo = new Dictionary<string, ColorReplaceCombo>(this.replacmentCombo)
            };
            return clone;
        }
    }

    public class ColorReplaceCombo
    {
        public string colorMapId;
        public Color findColor;
        public Color findDarkerColor;
        public Color selectedReplaceColor;
        public Color darkerSelectedReplaceColor;
        public int _index;
    }

    [Serializable]
    public class SpriteCollection
    {
        public string Id;
        public List<Sprite> Sprites = new List<Sprite>();
    }

    [Serializable]
    public class HeadSpriteCollection
    {
        public Sprite headFront;
        public Sprite headBack;
    }

    public class SpriteReplacementMap
    {
        public List<int> positions = new List<int>();
    }

    public class SpriteReplacementContext
    {
        public Dictionary<string, SpriteReplacementMap> colorReplacementMaps;
        public List<ColorReplaceCollection> colorReplaceCollections;
    }

    public class BodySpriteLibraryAssetCollection
    {
        public string id;
        public List<SpriteLibraryAsset> assets = new List<SpriteLibraryAsset>();
    }
}