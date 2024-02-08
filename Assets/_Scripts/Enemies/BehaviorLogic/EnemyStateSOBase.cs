using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExtensionMethods;

public class EnemyStateSOBase : ScriptableObject {
    protected Enemy enemy;
    protected EnemyData enemyData;
    protected Transform transform;
    protected GameObject gameObject;
    protected EnemyState enemyState;

    protected Transform playerTransform;

    public virtual void Initialize(GameObject gameObject, Enemy enemy, EnemyData enemyData, EnemyState enemyState) {
        this.gameObject = gameObject;
        transform = gameObject.transform;
        this.enemy = enemy;

        this.enemyData = enemyData;
        this.enemyState = enemyState;

        // playerTransform = LevelManager.instance.PlayerInstance.transform;
    }

    public virtual void DoEnterLogic() {}

    public virtual void DoExitLogic() { ResetValues(); }

    public virtual void DoUpdateLogic() {}

    public virtual void DoFixedUpdateLogic() {}

    public virtual void DoAnimationTriggerLogic() {}

    public virtual void DoAnimationFinishLogic() {}

    public virtual void ResetValues() {}
}
