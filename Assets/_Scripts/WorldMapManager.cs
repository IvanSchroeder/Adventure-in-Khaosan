using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExtensionMethods;
using UnityEngine.Tilemaps;
using System;

public class WorldMapManager : MonoBehaviour {
    public static WorldMapManager instance;
    public LevelManager LevelManager { get; private set; }
    public List<Tilemap> TilemapsList;
    public Tilemap groundFillTilemap;
    public Tilemap groundOverlapTilemap;
    public Tilemap groundCollisionTilemap;
    public Tilemap platformTilemap;
    public Tilemap platformCollisionTilemap;
    public Tilemap dummyTilemap;

    public Dictionary<Vector3, WorldTile> groundFillTiles = new Dictionary<Vector3, WorldTile>();
    public Dictionary<Vector3, WorldTile> platformTiles = new Dictionary<Vector3, WorldTile>();

    public bool worldIsLoaded = false;

    public float waitSecondsLoad;
    private WaitForSeconds loadSeconds;

    public static Action OnWorldLoaded; 

    private void Awake() {
        if (instance == null) {
            instance = this;
        }
        else if (instance != this) {
            Destroy(gameObject);
        }

        loadSeconds = new WaitForSeconds(waitSecondsLoad);

        // GetGroundWorldTiles();
        // GetPlatformWorldTiles();
        GetWorldTiles(groundFillTilemap, groundFillTiles);
        GetWorldTiles(platformTilemap, platformTiles);
    }

    private void Start() {
        if (LevelManager == null) LevelManager = LevelManager.instance;
        StartCoroutine(LoadWorld());
    }

    public IEnumerator LoadWorld() {
        worldIsLoaded = false;
        yield return loadSeconds;
        Automapping(groundFillTilemap, groundOverlapTilemap, groundCollisionTilemap, groundFillTiles);
        yield return loadSeconds;
        Automapping(platformTilemap, null, platformCollisionTilemap, platformTiles);
        yield return loadSeconds;
        SetDummyTiles(platformTilemap, groundFillTilemap, platformTiles);
        worldIsLoaded = true;
        yield return loadSeconds;
        OnWorldLoaded?.Invoke();
        yield return null;
    }

    private void GetWorldTiles(Tilemap fillTilemap, Dictionary<Vector3, WorldTile> fillTiles) {
        foreach (Vector3Int pos in fillTilemap.cellBounds.allPositionsWithin) {
            var localPlace = new Vector3Int(pos.x, pos.y, pos.z);

            if (!fillTilemap.HasTile(localPlace)) continue;

            var worldTile = new WorldTile {
                Name = $"{localPlace.x} , {localPlace.y}",
                LocalPlace = localPlace,
                WorldLocation = fillTilemap.CellToWorld(localPlace),
                TilemapMember = fillTilemap,
                TileBase = fillTilemap.GetTile(localPlace),
                ExtendedRuleTile = fillTilemap.GetTile<ExtendedRuleTile>(localPlace),
                TileDataSO = fillTilemap.GetTile<ExtendedRuleTile>(localPlace).tileData,
            };

            fillTiles.Add(worldTile.WorldLocation, worldTile);
        }
    }

    // private void GetGroundWorldTiles() {
    //     groundFillTiles = new Dictionary<Vector3, WorldTile>();

    //     foreach (Vector3Int pos in groundFillTilemap.cellBounds.allPositionsWithin) {
    //         var localPlace = new Vector3Int(pos.x, pos.y, pos.z);

    //         if (!groundFillTilemap.HasTile(localPlace)) continue;

    //         var worldTile = new WorldTile {
    //             Name = $"{localPlace.x} , {localPlace.y}",
    //             LocalPlace = localPlace,
    //             WorldLocation = groundFillTilemap.CellToWorld(localPlace),
    //             TilemapMember = groundFillTilemap,
    //             TileBase = groundFillTilemap.GetTile(localPlace),
    //             ExtendedRuleTile = groundFillTilemap.GetTile<ExtendedRuleTile>(localPlace),
    //             TileDataSO = groundFillTilemap.GetTile<ExtendedRuleTile>(localPlace).tileData,
    //         };

    //         groundFillTiles.Add(worldTile.WorldLocation, worldTile);
    //     }
    // }

    // private void GetPlatformWorldTiles() {
    //     platformTiles = new Dictionary<Vector3, WorldTile>();

    //     foreach (Vector3Int pos in platformTilemap.cellBounds.allPositionsWithin) {
    //         var localPlace = new Vector3Int(pos.x, pos.y, pos.z);

    //         if (!platformTilemap.HasTile(localPlace)) continue;

    //         var worldTile = new WorldTile {
    //             Name = $"{localPlace.x} , {localPlace.y}",
    //             LocalPlace = localPlace,
    //             WorldLocation = platformTilemap.CellToWorld(localPlace),
    //             TilemapMember = platformTilemap,
    //             TileBase = platformTilemap.GetTile(localPlace),
    //             ExtendedRuleTile = platformTilemap.GetTile<ExtendedRuleTile>(localPlace),
    //             TileDataSO = platformTilemap.GetTile<ExtendedRuleTile>(localPlace).tileData,
    //         };

    //         platformTiles.Add(worldTile.WorldLocation, worldTile);
    //     }
    // }

    private void Automapping(Tilemap fillTilemap, Tilemap overlapTilemap, Tilemap collisionTilemap, Dictionary<Vector3, WorldTile> fillTiles) {
        foreach (Vector3Int pos in fillTilemap.cellBounds.allPositionsWithin) {
            var localPlace = new Vector3Int(pos.x, pos.y, pos.z);

            if (!fillTilemap.HasTile(localPlace)) continue;

            WorldTile tile;
            if (fillTiles.TryGetValue(localPlace, out tile)) {
                if (overlapTilemap != null) {
                    if (!overlapTilemap.HasTile(localPlace) && tile.TileDataSO.hasOverlapTile) {
                        overlapTilemap.SetTile(localPlace, tile.TileDataSO.overlapTile);
                    }
                }

                if (collisionTilemap != null) {
                    if (!collisionTilemap.HasTile(localPlace) && tile.TileDataSO.hasCollisionTile) {
                        collisionTilemap.SetTile(localPlace, tile.TileDataSO.collisionTile);
                    }
                }
            }
        }
    }

    private void SetDummyTiles(Tilemap fillTilemap, Tilemap solidTilemap, Dictionary<Vector3, WorldTile> fillTiles) {
        foreach (Vector3Int pos in fillTilemap.cellBounds.allPositionsWithin) {
            var localPlace = new Vector3Int(pos.x, pos.y, pos.z);

            if (!fillTilemap.HasTile(localPlace)) continue;

            WorldTile tile;
            if (fillTiles.TryGetValue(localPlace, out tile)) {
                if (tile.TileDataSO.hasDummyTile) {
                    Vector3Int left = tile.LocalPlace + Vector3Int.left;
                    Vector3Int right = tile.LocalPlace + Vector3Int.right;

                    if (!fillTilemap.HasTile(left) && !fillTilemap.HasTile(left) && solidTilemap.HasTile(left)) {
                        fillTilemap.SetTile(left, tile.TileDataSO.dummyTile);
                    }

                    if (!fillTilemap.HasTile(right) && !fillTilemap.HasTile(right) && solidTilemap.HasTile(right)) {
                        fillTilemap.SetTile(right, tile.TileDataSO.dummyTile);
                    }
                }
            }
        }
    }

    public WorldTile GetTileAt(Vector3 position) {
        WorldTile tile;
        var worldPoint = new Vector3Int(Mathf.FloorToInt(position.x), Mathf.FloorToInt(position.y), 0);

        if (groundFillTiles.TryGetValue(worldPoint, out tile)) {
            return tile;
        }

        return null;
    }
}
