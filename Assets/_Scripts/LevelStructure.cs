using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExtensionMethods;
using UnityEngine.Tilemaps;

public class LevelStructure : MonoBehaviour {
    [field: SerializeField] public Tilemap GroundFillTilemap { get; private set; }
    [field: SerializeField] public Tilemap GroundOverlapTilemap { get; private set; }
    [field: SerializeField] public Tilemap GroundCollisionTilemap { get; private set; }
    [field: SerializeField] public Tilemap PlatformTilemap { get; private set; }
    [field: SerializeField] public Tilemap PlatformCollisionTilemap { get; private set; }
    [field: SerializeField] public Tilemap SpikesTilemap { get; private set; }
    [field: SerializeField] public Tilemap SpikesCollisionTilemap { get; private set; }
    [field: SerializeField] public Tilemap DummyTilemap { get; private set; }
}
