using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExtensionMethods;

public class EnemyPatrol : MonoBehaviour {
    public GameObject waypointA;
    public GameObject waypointB;
    private Rigidbody2D rb;
    private Animator anim;
    private Transform currentPoint;

    void Start() {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        currentPoint = waypointB.transform;
        anim.SetBool("move", true);
    }

    void Update() {
        Vector2 point = currentPoint.position - transform.position;
        
        if (currentPoint == waypointB.transform) {
            
        }
    }
}
