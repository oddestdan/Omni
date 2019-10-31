using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour {
    public LayerMask collisionMask;
    float speed = 10;
    float damage = 1;

    void Update() {
        float moveDistance = speed * Time.deltaTime;
        CheckCollisions(moveDistance);
        transform.Translate(Vector3.forward * moveDistance);
    }

    public void SetSpeed(float newSpeed) {
        speed = newSpeed;
    }

    void CheckCollisions(float moveDistance) {
        Ray frontRay = new Ray(transform.position, transform.forward);
        RaycastHit hit;
        if (Physics.Raycast(frontRay, out hit, moveDistance, collisionMask, QueryTriggerInteraction.Collide)) {
            OnHitObject(hit);
        }
    }

    void OnHitObject(RaycastHit hit) {
        IDamageable damageableObject = hit.collider.GetComponent<IDamageable>();
        if (damageableObject != null) {
            damageableObject.TakeHit(damage, hit);
        }
        GameObject.Destroy(gameObject);
    }
}
