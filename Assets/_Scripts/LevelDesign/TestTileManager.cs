using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExtensionMethods;
using UnityEngine.Tilemaps;

public class TestTileManager : MonoBehaviour {
    private WorldTile tile;
    private Dictionary<Vector3, WorldTile> tiles;
    private Camera mainCamera;

    private void Awake() {
        if (mainCamera == null) mainCamera = this.GetMainCamera();
        if (tiles == null) tiles = WorldMapManager.instance.groundFillTiles;
    }
    
    private void Update() {
        if (Input.GetMouseButtonDown(0)) {
            Vector3 point = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            var worldPoint = new Vector3Int(Mathf.FloorToInt(point.x), Mathf.FloorToInt(point.y), 0);

            if (tiles.TryGetValue(worldPoint, out tile)) {
                Debug.Log($"Tile {tile.Name} is solid? -> {tile.TileDataSO.isSolid}");
                tile.TilemapMember.SetTileFlags(tile.LocalPlace, TileFlags.None);
                tile.TilemapMember.SetColor(tile.LocalPlace, Color.green);
            }
        }
    }
}
