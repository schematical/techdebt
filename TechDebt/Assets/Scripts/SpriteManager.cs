using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

namespace DefaultNamespace
{
    [Serializable]
    public class SpriteManager : MonoBehaviour
    {
        public List<ColorMap> ColorMaps = new List<ColorMap>();
        public List<SpriteCollection> SpriteCollections = new List<SpriteCollection>();

        public Sprite GetRandom(string spriteCollectionId)
        {
            SpriteCollection collection = SpriteCollections.Find(x => x.Id == spriteCollectionId);
            if (collection == null)
            {
                throw new SystemException("Missing sprite collection id: " + spriteCollectionId);
            }

            int i = Random.Range(0, collection.Sprites.Count);
            Sprite sprite = collection.Sprites[i];
            return RandomizeSpriteColors(sprite);
        }

        public Sprite RandomizeSpriteColors(Sprite sprite)
        {
            Texture2D originalTexture = sprite.texture;
            Texture2D newTexture = new Texture2D(originalTexture.width, originalTexture.height);
            newTexture.filterMode = FilterMode.Point;

            Color[] pixels = originalTexture.GetPixels();

            int i = -1;
            if (ColorMaps.Count > 0)
            {
                var firstMap = ColorMaps[0];
                if (firstMap.replaceColors.Count > 0)
                {
                    i = Random.Range(0, firstMap.replaceColors.Count);
                }
            }

            if (i == -1)
            {
                throw new SystemException("Couldn't find a sutable replaceColor index");
            }

            i = 2;
            for (var p = 0; p < pixels.Length; p++)
            {
                foreach (var colorMap in ColorMaps)
                {
                    // Debug.Log($"Testing {pixels[p]} == {colorMap.findColor}");
                    if (pixels[p] == colorMap.findColor)
                    {
                      
                        // Debug.Log($"Replacing {pixels[p]} == i: {i} - {colorMap.replaceColors[i]}");
                        pixels[p] = colorMap.replaceColors[i];
                        
                    }
                }
            }


            newTexture.SetPixels(pixels);
            newTexture.Apply();
            return Sprite.Create(newTexture, sprite.rect, sprite.pivot, sprite.pixelsPerUnit);
        }
    }

    [Serializable]
    public class ColorMap
    {
        public Color findColor;
        public List<Color> replaceColors = new List<Color>();
    }

    [Serializable]
    public class SpriteCollection
    {
        public string Id;
        public List<Sprite> Sprites = new List<Sprite>();
    }
}