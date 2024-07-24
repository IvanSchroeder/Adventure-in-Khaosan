using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExtensionMethods;
using UnityEngine.Tilemaps;
using System;

public class WorldMapManager : MonoBehaviour {
    public static WorldMapManager instance;
    public LevelManager LevelManagerInstance { get; private set; }
    private LevelStructure levelStructure;

    public Dictionary<Vector3, WorldTile> groundFillTiles = new Dictionary<Vector3, WorldTile>();
    public Dictionary<Vector3, WorldTile> platformTiles = new Dictionary<Vector3, WorldTile>();
    public Dictionary<Vector3, WorldTile> spikesTiles = new Dictionary<Vector3, WorldTile>();

    public bool worldIsLoaded = false;

    public float waitSecondsLoad;
    private WaitForSeconds loadSeconds;

    public static Action OnWorldMapLoaded;
    private Coroutine loadWorldMapCoroutine;

    private void OnEnable() {
        LevelManager.OnLevelLoaded += LoadWorldMap;
        LevelManager.OnMainMenuLoadStart += ResetWorldMap;
        LevelManager.OnLevelRestart += ResetWorldMap;
    }

    private void OnDisable() {
        LevelManager.OnLevelLoaded -= LoadWorldMap;
        LevelManager.OnMainMenuLoadStart -= ResetWorldMap;
        LevelManager.OnLevelRestart -= ResetWorldMap;
    }

    private void Awake() {
        if (instance.IsNull()) {
            instance = this;
        }
        else if (instance != this) {
            Destroy(gameObject);
        }

        loadSeconds = new WaitForSeconds(waitSecondsLoad);
    }

    private void Start() {
        if (LevelManagerInstance.IsNull()) LevelManagerInstance = LevelManager.instance;
    }

    public void ResetWorldMap() {
        groundFillTiles = new Dictionary<Vector3, WorldTile>();
        platformTiles = new Dictionary<Vector3, WorldTile>();
        spikesTiles = new Dictionary<Vector3, WorldTile>();
    }

    public void LoadWorldMap() {
        levelStructure = LevelManagerInstance.LevelStructure;

        GetWorldTiles(levelStructure.GroundFillTilemap, groundFillTiles);
        GetWorldTiles(levelStructure.PlatformTilemap, platformTiles);
        GetWorldTiles(levelStructure.SpikesTilemap, spikesTiles);

        loadWorldMapCoroutine = StartCoroutine(LoadWorldMapRoutine());
    }

    public IEnumerator LoadWorldMapRoutine() {
        worldIsLoaded = false;

        bool[] directionsArray = {false, false, false, false};
        Automapping(levelStructure.GroundFillTilemap, levelStructure.GroundOverlapTilemap, levelStructure.GroundCollisionTilemap, groundFillTiles);
        yield return loadSeconds;

        Automapping(levelStructure.PlatformTilemap, null, levelStructure.PlatformCollisionTilemap, platformTiles);
        yield return loadSeconds;

        directionsArray = new bool[] {true, false, true, false};
        SetDummyTiles(levelStructure.PlatformTilemap, levelStructure.GroundFillTilemap, platformTiles, directionsArray);
        yield return loadSeconds;

        Automapping(levelStructure.SpikesTilemap, null, levelStructure.SpikesCollisionTilemap, spikesTiles);
        yield return loadSeconds;

        directionsArray = new bool[] {true, true, true, true};
        SetDummyTiles(levelStructure.SpikesTilemap, levelStructure.GroundFillTilemap, spikesTiles, directionsArray);
        SetDummyTiles(levelStructure.SpikesCollisionTilemap, levelStructure.GroundFillTilemap, spikesTiles, directionsArray);
        worldIsLoaded = true;
        yield return loadSeconds;

        OnWorldMapLoaded?.Invoke();

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

    private void SetDummyTiles(Tilemap fillTilemap, Tilemap solidTilemap, Dictionary<Vector3, WorldTile> fillTiles, bool[] directions) {
        bool leftCheck = directions[0];
        bool upCheck = directions[1];
        bool rightCheck = directions[2];
        bool downCheck = directions[3];

        foreach (Vector3Int pos in fillTilemap.cellBounds.allPositionsWithin) {
            var localPlace = new Vector3Int(pos.x, pos.y, pos.z);

            if (!fillTilemap.HasTile(localPlace)) continue;

            WorldTile tile;
            if (fillTiles.TryGetValue(localPlace, out tile)) {
                if (tile.TileDataSO.hasDummyTile) {
                    Vector3Int left = tile.LocalPlace + Vector3Int.left;
                    Vector3Int up = tile.LocalPlace + Vector3Int.up;
                    Vector3Int right = tile.LocalPlace + Vector3Int.right;
                    Vector3Int down = tile.LocalPlace + Vector3Int.down;

                    if (leftCheck && !fillTilemap.HasTile(left) && solidTilemap.HasTile(left)) {
                        fillTilemap.SetTile(left, tile.TileDataSO.dummyTile);
                    }

                    if (upCheck && !fillTilemap.HasTile(up) && solidTilemap.HasTile(up)) {
                        fillTilemap.SetTile(up, tile.TileDataSO.dummyTile);
                    }

                    if (rightCheck && !fillTilemap.HasTile(right) && solidTilemap.HasTile(right)) {
                        fillTilemap.SetTile(right, tile.TileDataSO.dummyTile);
                    }

                    if (downCheck && !fillTilemap.HasTile(down) && solidTilemap.HasTile(down)) {
                        fillTilemap.SetTile(down, tile.TileDataSO.dummyTile);
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
