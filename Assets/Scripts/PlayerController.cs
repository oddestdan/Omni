using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour {
    Rigidbody myRigidbody;
    Vector3 velocity;

    void Start() {
        myRigidbody = GetComponent<Rigidbody>();
    }

    public void FixedUpdate() {
        myRigidbody.MovePosition(myRigidbody.position + velocity * Time.fixedDeltaTime);
    }

    public void Move(Vector3 _velocity) {
        velocity = _velocity;
    }

    public void LookAt(Vector3 point) {
        Vector3 sameHeightPoint = new Vector3(point.x, transform.position.y, point.z);
        transform.LookAt(sameHeightPoint);
    }
}
