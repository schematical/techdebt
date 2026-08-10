// GridManager.cs

using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using UnityEngine.Serialization;

public class GridManager : MonoBehaviour
{
    public Tile floorTilePrefab;
    public Tile shadowTilePrefab;
    public IsometricRuleTile skyTilePrefab;
    public Grid grid;
    public Tilemap floorTilemap;
    public Tilemap skyTilemap;
    public Tilemap controllerTilemap;



    public void Init()
    {
    }


    public Node NodeFromWorldPoint(Vector3 worldPosition)
    {
        Vector3Int cellPos = grid.WorldToCell(worldPosition);


        return new Node( /*worldPosition, */cellPos.x, cellPos.y);
    }

    public Vector3 AdjustWorldPointZ(Vector3 worldPosition)
    {
        return new Vector3(
            worldPosition.x,
            worldPosition.y,
            0f
        );
    }



    private Dictionary<NPCBase, List<Vector3Int>> debugTilesByNpc = new Dictionary<NPCBase, List<Vector3Int>>();

    public void DebugNPC(NPCBase npcBase, Vector3 transformPosition, List<Vector3> currentPath)
    {
        if (!debugTilesByNpc.TryGetValue(npcBase, out List<Vector3Int> previousTiles))
        {
            previousTiles = new List<Vector3Int>();
            debugTilesByNpc[npcBase] = previousTiles;
        }
        foreach (Vector3Int previousTile in previousTiles)
        {
            controllerTilemap.SetTile(previousTile, null);
        }
        previousTiles.Clear();

        Vector3Int currentCell = controllerTilemap.WorldToCell(transformPosition);
        controllerTilemap.SetTile(currentCell, shadowTilePrefab);
        previousTiles.Add(currentCell);

        if (currentPath != null)
        {
            foreach (Vector3 pathPoint in currentPath)
            {
                Vector3Int cell = controllerTilemap.WorldToCell(pathPoint);
                if (!previousTiles.Contains(cell))
                {
                    controllerTilemap.SetTile(cell, shadowTilePrefab);
                    previousTiles.Add(cell);
                }
            }
        }
    }
}