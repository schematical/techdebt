using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace DefaultNamespace
{
    [Serializable]
    public class SpriteManager: MonoBehaviour
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
            return collection.Sprites[i];
        }

        public Sprite RandomizeSpriteColors(Sprite sprite)
        {
            int i = -1;
            foreach (ColorMap colorMap in ColorMaps)
            {
                if (i == -1)
                {
                    i = Random.Range(0, colorMap.replaceColors.Count);
                }
                //TODO Find the colors and replace them.
            }
            return sprite; // TODO: Write this code
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
        public List<Sprite> Sprites= new List<Sprite>();
    }
    
}