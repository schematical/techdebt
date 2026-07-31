using System.Collections.Generic;
using Infrastructure;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace DefaultNamespace.Office
{
    public class RoomBase: MonoBehaviour
    {
        public enum RoomState { Inactive, Active }
        public string Id;
        public RoomState State =  RoomState.Inactive;
        public Tilemap FloorTilemap;
        public Tilemap WallTilemap;
        public List<WorldObjectBase> worldObjects = new List<WorldObjectBase>();
        
        // Unlock Conditions?
        
        public virtual void SetState(RoomState newState)
        {
            State = newState;
            switch (newState)
            {
                case RoomState.Inactive:
                    gameObject.SetActive(false);
                    /*FloorTilemap.gameObject.SetActive(false);
                    WallTilemap.gameObject.SetActive(false);*/
                    break;
                case RoomState.Active:
                    /*FloorTilemap.gameObject.SetActive(true);
                    WallTilemap.gameObject.SetActive(true);*/
                    gameObject.SetActive(true);
                    break;
            }
        }
    }
}