using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExtensionMethods;

public class Platform : MonoBehaviour {
    public BoxCollider2D PlatformCollider { get; private set; }
    public PlatformEffector2D PlatformEffector { get; private set; }
    public bool isSolid = true;
    [SerializeField] private FloatSO platformSeconds;

    public Vector3 localPosition;
    private Player player;
    private WaitForSeconds platformDisabledSeconds;
    private Coroutine platformDisableCoroutine;

    private void Start() {
        if (PlatformCollider == null) PlatformCollider = GetComponent<BoxCollider2D>();
        if (PlatformEffector == null) PlatformEffector = GetComponent<PlatformEffector2D>();

        isSolid = true;
        PlatformCollider.enabled = true;
        localPosition = transform.position;
        platformDisabledSeconds = new WaitForSeconds(platformSeconds.Value);
    }

    public void DisableCollider() {
        if (platformDisableCoroutine != null) {
            StopCoroutine(platformDisableCoroutine);
            platformDisableCoroutine = null;
        }

        platformDisableCoroutine = StartCoroutine(DisablePlatformRoutine());
    }

    private IEnumerator DisablePlatformRoutine() {
        SetCollider(false);
        yield return platformDisabledSeconds;
        SetCollider(true);
    }

    public void SetCollider(bool enable) {
        var tile = WorldMapManager.instance.GetTileAt(localPosition);
        isSolid = enable;
        PlatformCollider.enabled = enable;
        Debug.Log($"Tile {tile.Name} collision is {isSolid}");

        tile.TilemapMember.SetColor(tile.LocalPlace, Color.green);
    }

    private void OnCollisionEnter2D(Collision2D collision) {
        player = collision.gameObject.GetComponent<Player>();
    }

    private void OnCollisionStay2D(Collision2D collision) {
        if (player == null) return;

        if (player.playerData.isIgnoringPlatforms) {
            player = null;
            DisableCollider();
        }
    }

    private void OnCollisionExit2D(Collision2D collision) {
        player = null;
    }
}
