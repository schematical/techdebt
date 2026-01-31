using System;
using System.Collections.Generic;
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
            
            // Create a temporary RenderTexture
            RenderTexture tmp = RenderTexture.GetTemporary(
                originalTexture.width,
                originalTexture.height,
                0,
                RenderTextureFormat.Default,
                RenderTextureReadWrite.Linear);

            // Blit the pixels on texture to the RenderTexture
            Graphics.Blit(originalTexture, tmp);

            // Backup the currently active RenderTexture
            RenderTexture previous = RenderTexture.active;

            // Set the current RenderTexture to the temporary one we created
            RenderTexture.active = tmp;

            // Create a new readable Texture2D to copy the pixels to it
            Texture2D newTexture = new Texture2D(originalTexture.width, originalTexture.height);

            // Copy the pixels from the RenderTexture to the new Texture
            newTexture.ReadPixels(new Rect(0, 0, tmp.width, tmp.height), 0, 0);
            newTexture.Apply();

            // Reset the active RenderTexture
            RenderTexture.active = previous;

            // Release the temporary RenderTexture
            RenderTexture.ReleaseTemporary(tmp);
            
            newTexture.filterMode = FilterMode.Point;

            var pixels = newTexture.GetPixels();

            var i = -1;
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


            for (var p = 0; p < pixels.Length; p++)
            {
                foreach (var colorMap in ColorMaps)
                {
                    if (pixels[p] == colorMap.findColor)
                    {
                        
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