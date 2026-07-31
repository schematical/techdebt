// GridManager.cs
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using UnityEngine.Serialization;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance { get; private set; }

  
    
   public Tile floorTilePrefab;
   public Tile shadowTilePrefab;
   public IsometricRuleTile skyTilePrefab;
   public Grid grid;
    public Tilemap floorTilemap;
    public Tilemap skyTilemap;

    void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    public void Init()
    {
        
    }

   

    public Node NodeFromWorldPoint(Vector3 worldPosition)
    {
        
        
        Vector3Int cellPos = grid.WorldToCell(worldPosition);


       
        return new Node(/*worldPosition, */cellPos.x, cellPos.y);
    }

    public Vector3 AdjustWorldPointZ(Vector3 worldPosition)
    {
        // Vector3 screenPoint = Camera.main.WorldToScreenPoint(worldPosition);
        Vector3 newWorldPoint = new Vector3(
            worldPosition.x,
            worldPosition.y,
            worldPosition.y / 1000
        );
        return newWorldPoint;
    }

   
}