using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExtensionMethods;
using UnityEngine.Tilemaps;
using System;

[Serializable]
public class WorldTile : ICloneable {
    public string Name { get; set; }
    public Vector3Int LocalPlace { get; set; }
    public Vector3 WorldLocation { get; set; }
    public TileBase TileBase { get; set; }
    public ExtendedRuleTile ExtendedRuleTile { get; set; }
    public TileDataSO TileDataSO { get; set; }
    public Tilemap TilemapMember { get; set; }

    public object Clone() {
        throw new NotImplementedException();
    }
}
