using System.Collections;
using UnityEngine;

[RequireComponent(typeof(PlayerController))]
[RequireComponent(typeof(GunController))]

public class Player : MonoBehaviour {
    public float moveSpeed = 5;
    PlayerController controller;
    GunController gunController;

    Camera viewCamera;

    void Start() {
        controller = GetComponent<PlayerController>();
        gunController = GetComponent<GunController>();
        viewCamera = Camera.main;
    }

    void Update() {
        // Player movement input
        Vector3 moveInput = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
        Vector3 moveVelocity = moveInput.normalized * moveSpeed;
        controller.Move(moveVelocity);

        // Look input (Ray from camera to mouse cursor)
        Ray ray = viewCamera.ScreenPointToRay(Input.mousePosition);
        Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
        float rayDistance;

        if (groundPlane.Raycast(ray, out rayDistance)) {
            Vector3 intersectionPoint = ray.GetPoint(rayDistance);
            // Debug.DrawLine(ray.origin, intersectionPoint, Color.red);
            controller.LookAt(intersectionPoint);
        }

        // Weapon input
        if (Input.GetMouseButton(0)) {
            // Left button is held down
            gunController.Shoot();
        }
    }
}
