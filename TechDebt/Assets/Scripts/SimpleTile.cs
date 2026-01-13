using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu]
public class SimpleTile : Tile
{
    public Color color = Color.white;

    public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData)
    {
        base.GetTileData(position, tilemap, ref tileData);
        tileData.sprite = null;
        tileData.color = color;
        tileData.colliderType = ColliderType.None;
    }
}
