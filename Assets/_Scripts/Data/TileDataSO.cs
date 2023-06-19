using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/Tile Data")]
public class TileDataSO : ScriptableObject {
    [Header("Tile Info")]
    [Space(5)]

    public bool isSolid;
    public bool isSlope;
    public bool isPlatform;
    public bool isOverlap;
    public bool isCollision;

    [Space(20)]

    [Header("Tile Parameters")]
    [Space(5)]

    public bool isClimbable;
    public bool isDamaging;

    [Space(20)]

    [Header("Tile References")]
    [Space(5)]

    public bool hasDummyTile;
    public ExtendedRuleTile dummyTile;
    public bool hasOverlapTile;
    public ExtendedRuleTile overlapTile;
    public bool overlapVariations;
    public List<ExtendedRuleTile> OverlapTilesList;
    public bool hasCollisionTile;
    public ExtendedRuleTile collisionTile;
}
