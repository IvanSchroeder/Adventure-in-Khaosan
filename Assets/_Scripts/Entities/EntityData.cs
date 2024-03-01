using UnityEngine;
using ExtensionMethods;

public enum Direction {
    Left,
    Right
}

public abstract class EntityData : ScriptableObject {
    [Header("--- References ---")]
    [Space(5)]
    public SpriteFlashConfiguration damageFlash;
    public SpriteFlashConfiguration invulnerabilityFlash;
    public SpriteFlashConfiguration interactedFlash;

    [Space(20)]

    [Header("--- Entity Info ---")]
    [Space(5)]
    [Header("Values")]
    [Space(5)]
    public Vector2 currentVelocity;
    public ColliderConfiguration currentColliderConfiguration;
    public Direction facingDirection = Direction.Right;
    public int currentLives;
    public float currentHealth;
    public int currentHearts;
    public bool isMaxHealth;
    public string currentLayer;
    public float currentGravityScale;
    public float currentFallSpeed;
    public int amountOfJumpsLeft;
    public float slopeDownAngle;
    public float slopeSideAngle;

    [Space(20)]

    [Header("--- Health Parameters ---")]
    [Space(5)]
    public LayerMask damagedBy;
    public HealthType healthType = HealthType.Hearts;
    [Range(1, 99)] public int maxLives;
    [Range(1, 10)] public int maxHearts;
    [Range(1, 10)] public int startingHearts;
    [Range(1f, 100f)] public float maxHealth;
    public float invulnerabilitySeconds;
    public float minKnockbackTime;
    public float maxKnockbackTime;
    public float deadOnGroundTime;

    public abstract void Init();
    public abstract void OnEnable();
}
