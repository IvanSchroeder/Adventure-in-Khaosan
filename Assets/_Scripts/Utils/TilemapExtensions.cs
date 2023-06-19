using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public static class TilemapExtensions {
    // New tiles extensions

    /*private static TileBase[] GetTilesBlock(BoundsInt area, Tilemap tilemap) {
        TileBase[] array = new TileBase[area.size.x * area.size.y * area.size.z];
        int counter = 0;

        foreach (var v in area.allPositionsWithin) {
            Vector3Int pos = new Vector3Int(v.x, v.y, 0);
            array[counter] = tilemap.GetTile(pos);
            counter++;
        }

        return array;
    }

    private static void SetTilesBlock(BoundsInt area, TileState type, Tilemap tilemap) {
        int size = area.size.x * area.size.y * area.size.z;
        TileBase[] tileArray = new TileBase[size];
        FillTiles(tileArray, type);
        tilemap.SetTilesBlock(area, tileArray);
    }

    private static void FillTiles(TileBase[] arr, TileState type) {
        for (int i = 0; i < arr.Length; i++) {
            arr[i] = tileBases[type];
        }
    }*/
    
    /*public static T[] GetTiles<T>(this Tilemap tilemap) where T : TileBase {
        List<T> tiles = new List<T>();
        
        for (int y = tilemap.origin.y; y < (tilemap.origin.y + tilemap.size.y); y++)
        {
            for (int x = tilemap.origin.x; x < (tilemap.origin.x + tilemap.size.x); x++)
            {
                T tile = tilemap.GetTile<T>(new Vector3Int(x, y, 0));
                if (tile != null)
                {
                    tiles.Add(tile);
                }
            }
        }
        
        return tiles.ToArray();
    }

    public static T GetTile<T>(this Tilemap tilemap, Vector3Int cellPosition) where T : TileBase {
        T tile = tilemap.GetTile<T>(cellPosition);
        return tile;
    }

    public static void SetTile<T>(this Tilemap tilemap, Vector3Int cellPosition, TileBase tile) where T : TileBase {
        tilemap.SetTile<T>(cellPosition, tile);
    }*/

}
