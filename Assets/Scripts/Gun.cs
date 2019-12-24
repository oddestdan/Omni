﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour {
    public Transform muzzle;            // where projectiles are shot from
    public Projectile projectile;       // which projectiles we're shooting
    public float msBetweenShots = 100;  // rate of fire
    public float muzzleVelocity = 35;   // speed of bullet leaving the gun

    public Transform shell;
    public Transform shellEjection;

    MuzzleFlash muzzleFlash;

    float nextShotTime;

    void Start() {
        muzzleFlash = GetComponent<MuzzleFlash>();
    }

    public void Shoot() {
        if (Time.time > nextShotTime) {
            nextShotTime = Time.time + msBetweenShots / 1000;
            Projectile newProjectile = Instantiate(projectile, muzzle.position, muzzle.rotation) as Projectile;
            newProjectile.SetSpeed(muzzleVelocity);

            // Shell spawning
            Instantiate(shell, shellEjection.position, shellEjection.rotation);
            muzzleFlash.Activate();
        }
    }
}
