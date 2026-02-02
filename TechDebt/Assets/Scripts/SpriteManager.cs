using System;
using System.Collections.Generic;
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

      
        public static Color MakeDarker(Color color, float amount = .2f)
        {
            return new Color(
                color.r - amount,
                color.g - amount,
                color.b - amount,
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
            foreach(ColorMap colorMap in ColorMaps)
            {
              
                colorMap.findDarkerColor = MakeDarker(colorMap.findColor);
                colorMap.replaceDarkerColors = new List<Color>();
                foreach(Color replaceColor in colorMap.replaceColors) {
                    colorMap.replaceDarkerColors.Add(MakeDarker(replaceColor));
                }
            }
        }

        protected List<ColorReplaceCollection> PopulateColorReplaceCollections(Sprite sprite)
        {
            List<ColorReplaceCollection> colorReplaceCollections = new List<ColorReplaceCollection>();
            Dictionary<Color, SpriteReplacementMap> colorReplacementMaps = new Dictionary<Color, SpriteReplacementMap>();
            foreach (ColorMap colorMap in ColorMaps)
            {
                colorMap.findDarkerColor = MakeDarker(colorMap.findColor);
                if (colorReplacementMaps.ContainsKey(colorMap.findColor))
                {
                    throw new SystemException("Duplicate color map found: " + colorMap.findColor);
                }
                SpriteReplacementMap spriteReplacementMap = GetSpriteReplacementMap(sprite, colorMap.findColor);
                if (spriteReplacementMap.positions.Count > 0)
                {
                    colorReplacementMaps.Add(colorMap.findColor, spriteReplacementMap);
                }
               
                
                SpriteReplacementMap darkerSpriteReplacementMap = GetSpriteReplacementMap(sprite, colorMap.findColor);
                if (spriteReplacementMap.positions.Count > 0)
                {
                    colorReplacementMaps.Add(colorMap.findDarkerColor, darkerSpriteReplacementMap);
                }
            
            }
            // Now we have all the pixels of each color that we need to replace.
            SpriteReplacementContext context = new SpriteReplacementContext()
            {
                colorReplacementMaps = colorReplacementMaps,
                colorReplaceCollections = colorReplaceCollections
            };
            BuildColorReplaceCollectionsRecursive(context,  0, new ColorReplaceCollection());
            
            return colorReplaceCollections;
        }

        protected void BuildColorReplaceCollectionsRecursive(SpriteReplacementContext contex, int depth, ColorReplaceCollection currColorReplaceCollection)
        {

            for (int i = depth; i < ColorMaps.Count; i++)
            {
                ColorMap colorMap = ColorMaps[i];
                bool isDarker = false;
                SpriteReplacementMap spriteReplacementMap = contex.colorReplacementMaps[colorMap.findColor];
                SpriteReplacementMap darkerSpriteReplacementMap = contex.colorReplacementMaps[colorMap.findColor];
                if (spriteReplacementMap == null &&  darkerSpriteReplacementMap == null)
                {
                    return; //No update needed
                }
                
                for (int ii = 0; ii < colorMap.replaceColors.Count; ii++)
                {
                    ColorReplaceCollection newColorReplaceCollection = currColorReplaceCollection.Clone();

                    newColorReplaceCollection.id += $"_{ii}";
                    newColorReplaceCollection.replacmentCombo.Add(colorMap.replaceColors[ii], new ColorReplaceCombo()
                    {
                        findColor = colorMap.replaceColors[ii],
                        findDarkerColor = colorMap.replaceDarkerColors[ii],
                        selectedReplaceColor =  colorMap.replaceColors[ii],
                        darkerSelectedReplaceColor = colorMap.replaceDarkerColors[ii]
                    });
                    if (depth < contex.colorReplacementMaps.Count)
                    {
                        BuildColorReplaceCollectionsRecursive(contex, depth, newColorReplaceCollection);
                    } else {
                        // This is the end of the line
                        contex.colorReplaceCollections.Add(newColorReplaceCollection);
                    }
                }
            }
        }

        public SpriteReplacementMap GetSpriteReplacementMap(Sprite sprite, Color color)
        {
            SpriteReplacementMap spriteReplacementMap = new SpriteReplacementMap();
            Color[] pixels = sprite.texture.GetPixels();

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
        public Dictionary<Color, ColorReplaceCombo> replacmentCombo = new Dictionary<Color, ColorReplaceCombo>();

        public ColorReplaceCollection Clone()
        {
            ColorReplaceCollection clone = new ColorReplaceCollection
            {
                id = this.id,
                replacmentCombo = new Dictionary<Color, ColorReplaceCombo>(this.replacmentCombo)
            };
            return clone;
        }
    }
    public class ColorReplaceCombo
    {
        public Color findColor;
        public Color findDarkerColor;
        public Color selectedReplaceColor;
        public Color darkerSelectedReplaceColor;
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
        public Dictionary<Color, SpriteReplacementMap> colorReplacementMaps;
        public List<ColorReplaceCollection> colorReplaceCollections;
        
    }
}