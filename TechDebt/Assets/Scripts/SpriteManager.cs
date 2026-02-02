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
            List<ColorMap> colorMaps = GetRandomizedColorMaps();
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
  
            return RandomizeSpriteColors(sprite);
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

        protected List<ColorMap> GetRandomizedColorMaps()
        {
            foreach (ColorMap colorMap in ColorMaps)
            {
                if (colorMap.useAnyColor)
                {
                    colorMap.selectedReplaceColor = new Color(
                        Random.value, Random.value, Random.value
                    );
                 
                }
                else
                {
                    int i = Random.Range(0, colorMap.replaceColors.Count);
                    colorMap.selectedReplaceColor = colorMap.replaceColors[i];
                }
                colorMap.darkerSelectedReplaceColor = MakeDarker(colorMap.selectedReplaceColor);
                
            }

            return ColorMaps;
        }
        public Sprite RandomizeSpriteColors(Sprite sprite, List<ColorMap> populatedColorMaps = null)
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
        }
    }

    [Serializable]
    public class ColorMap
    {
        public Color findColor;
        public Color findDarkerColor;
        public bool useAnyColor;
        public Color selectedReplaceColor;
        public Color darkerSelectedReplaceColor;
        public List<Color> replaceColors = new List<Color>();
        public List<Color> replaceDarkerColors = new List<Color>();
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
}