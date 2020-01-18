using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour {
    public LayerMask collisionMask;
    public Color trailColor;
    float speed = 10;
    float damage = 1;

    float lifetime = 2;
    float skinWidth = .1f;

    void Start() {
        Destroy(gameObject, lifetime);

        Collider[] initialCollisions = Physics.OverlapSphere(transform.position, .1f, collisionMask);
        if (initialCollisions.Length > 0) {
            OnHitObject(initialCollisions[0], transform.position);
        }

        GetComponent<TrailRenderer>().material.SetColor("_TintColor", trailColor);
    }

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
        if (Physics.Raycast(frontRay, out hit, moveDistance + skinWidth, collisionMask, QueryTriggerInteraction.Collide)) {
            // Debug.Log("'Collision mask is: '" + collisionMask);
            OnHitObject(hit.collider, hit.point);
        }
    }

    void OnHitObject(Collider collider, Vector3 hitPoint) {
        IDamageable damageableObject = collider.GetComponent<IDamageable>();
        if (damageableObject != null) {
            damageableObject.TakeHit(damage, hitPoint, transform.forward);
        }
        GameObject.Destroy(gameObject);
    }
}
