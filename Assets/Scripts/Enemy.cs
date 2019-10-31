using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class Enemy : LivingEntity {
    public enum State { Idle, Chasing, Attacking };
    State currentState;

    NavMeshAgent pathfinder;
    Transform target; // Player target to chase
    Material skinMaterial;

    Color originalColor;

    float attackDistanceThreshold = .5f;
    float timeBetweenAttacks = 1;
    float nextAttackTime;

    float myCollisionRadius;
    float targetCollisionRadius;

    protected override void Start() {
        base.Start();
        pathfinder = GetComponent<NavMeshAgent>();
        skinMaterial = GetComponent<Renderer>().material;
        originalColor = skinMaterial.color;

        currentState = State.Chasing;
        target = GameObject.FindGameObjectWithTag("Player").transform;

        myCollisionRadius = GetComponent<CapsuleCollider>().radius;
        targetCollisionRadius = target.GetComponent<CapsuleCollider>().radius;

        StartCoroutine(UpdatePath());
    }

    void Update() {
        if (Time.time > nextAttackTime) {
            float squareDistanceToTarget = (target.position - transform.position).sqrMagnitude;
            float threshold = attackDistanceThreshold + myCollisionRadius + targetCollisionRadius;
            if (squareDistanceToTarget < Mathf.Pow(threshold, 2)) {
                nextAttackTime = Time.time + timeBetweenAttacks;
                StartCoroutine(Attack());
            }
        }
    }

    IEnumerator Attack() {
        currentState = State.Attacking;
        pathfinder.enabled = false;
        skinMaterial.color = Color.red;

        Vector3 originalPosition = transform.position;
        Vector3 directionToTarget = (target.position - transform.position).normalized;
        Vector3 attackPosition = target.position - directionToTarget * (myCollisionRadius);

        float attackSpeed = 3;
        float percent = 0; // x in formula: y = (-x^2 + 2) * 4


        while (percent <= 1) {
            percent += Time.deltaTime * attackSpeed;

            float interpolation = (-Mathf.Pow(percent, 2) + percent) * 4; // formula
            transform.position = Vector3.Lerp(originalPosition, attackPosition, interpolation);

            yield return null;
        }

        skinMaterial.color = originalColor;
        currentState = State.Chasing;
        pathfinder.enabled = true;
    }

    IEnumerator UpdatePath() {
        float refreshRate = .25f;

        while (target != null) {
            if (currentState == State.Chasing) {
                Vector3 directionToTarget = (target.position - transform.position).normalized;
                Vector3 targetPosition =
                    target.position - directionToTarget * (myCollisionRadius + targetCollisionRadius + attackDistanceThreshold / 2);
                if (!dead) {
                    pathfinder.SetDestination(targetPosition); // Too expensive for every frame
                }
            }
            yield return new WaitForSeconds(refreshRate);
        }
    }
}
