using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Linq;
using System;

[CreateAssetMenu(menuName = "Assets/Custom Tiles/Extended Rule Tile")]
[Serializable]
public class ExtendedRuleTile : RuleTile<ExtendedRuleTile.Neighbor> {
    [Header("Extended Tile")]
    [Tooltip("If enabled, the tile will connect to these tiles too when the mode is set to \"This\".")]
    public bool alwaysConnect;
    [Tooltip("If enabled, the tile will not connect to these tiles too when the mode is set to \"NotThis\", like if no tile was there.")]
    public bool overrideNotThis;
    [Tooltip("Tiles to connect to.")]
    public TileBase[] tilesToConnect;
    [Space]
    [Tooltip("Check itseft when the mode is set to \"any\".")]
    public bool checkSelf = true;
    [Tooltip("Set tile color.")]
    public Color tileColor;

    public TileDataSO tileData;

    public override void RefreshTile(Vector3Int position, ITilemap tilemap) {
        base.RefreshTile(position, tilemap);
    }

    public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData) {
        base.GetTileData(position, tilemap, ref tileData);

        tileData.color = tileColor;
        tileData.flags = TileFlags.LockColor;
    }

    public class Neighbor : RuleTile.TilingRule.Neighbor {
        public const int Any = 3;
        public const int Specified = 4;
        public const int Nothing = 5;
    }

    public override bool RuleMatch(int neighbor, TileBase tile) {
        switch (neighbor) {
            case Neighbor.This: return Check_This(tile);
            case Neighbor.NotThis: return Check_NotThis(tile);
            case Neighbor.Any: return Check_Any(tile);
            case Neighbor.Specified: return Check_Specified(tile);
            case Neighbor.Nothing: return Check_Nothing(tile);
        }
        return base.RuleMatch(neighbor, tile);
    }

    bool Check_This(TileBase tile) {
        if (!alwaysConnect) return tile == this;
        else return tilesToConnect.Contains(tile) || tile == this;
    }

    bool Check_NotThis(TileBase tile) {
        if (!overrideNotThis) return tile != this;
        else return !tilesToConnect.Contains(tile) && tile != this;
    }

    bool Check_Any(TileBase tile) {
        if (checkSelf) return tile != null;
        else return tile != null && tile != this;
    }

    bool Check_Specified(TileBase tile) {
        return tilesToConnect.Contains(tile);
    }

    bool Check_Nothing(TileBase tile) {
        return tile == null;
    }
}