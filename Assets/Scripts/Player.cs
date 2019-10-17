using System.Collections;
using UnityEngine;

[RequireComponent(typeof(PlayerController))]
public class Player : MonoBehaviour {
    public float moveSpeed = 5;
    PlayerController controller;

    Camera viewCamera;

    void Start() {
        controller = GetComponent<PlayerController>();
        viewCamera = Camera.main;
    }

    void Update() {
        // Player movement
        Vector3 moveInput = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
        Vector3 moveVelocity = moveInput.normalized * moveSpeed;
        controller.Move(moveVelocity);

        // Ray from camera to mouse cursor
        Ray ray = viewCamera.ScreenPointToRay(Input.mousePosition);
        Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
        float rayDistance;

        if (groundPlane.Raycast(ray, out rayDistance)) {
            Vector3 intersectionPoint = ray.GetPoint(rayDistance);
            // Debug.DrawLine(ray.origin, intersectionPoint, Color.red);
            controller.LookAt(intersectionPoint);
        }
    }
}
