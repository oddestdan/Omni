using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour {
    public enum FireMode { Auto, Burst, Single };
    public FireMode fireMode;

    public Transform[] projectileSpawn;            // where projectiles are shot from
    public Projectile projectile;       // which projectiles we're shooting
    public float msBetweenShots = 100;  // rate of fire
    public float muzzleVelocity = 35;   // speed of bullet leaving the gun
    public int burstCount;

    public Transform shell;
    public Transform shellEjection;
    MuzzleFlash muzzleFlash;
    float nextShotTime;

    bool triggerReleasedSinceLastShot;
    int shotsRemainingInBurst;

    void Start() {
        muzzleFlash = GetComponent<MuzzleFlash>();
        shotsRemainingInBurst = burstCount;
    }

    void Shoot() {
        if (Time.time > nextShotTime) {
            // Burst fire logic
            if (fireMode == FireMode.Burst) {
                if (shotsRemainingInBurst == 0) {
                    return;
                }
                shotsRemainingInBurst--;
            }
            // Single fire logic
            else if (fireMode == FireMode.Single) {
                if (!triggerReleasedSinceLastShot) {
                    return;
                }
            }

            foreach (Transform proj in projectileSpawn) {
                nextShotTime = Time.time + msBetweenShots / 1000;
                Projectile newProjectile = Instantiate(projectile, proj.position, proj.rotation) as Projectile;
                newProjectile.SetSpeed(muzzleVelocity);
            }

            // Shell spawning
            Instantiate(shell, shellEjection.position, shellEjection.rotation);
            muzzleFlash.Activate();
        }
    }

    public void OnTriggerHold() {
        Shoot();
        triggerReleasedSinceLastShot = false;
    }

    public void OnTriggerRelease() {
        shotsRemainingInBurst = burstCount;
        triggerReleasedSinceLastShot = true;
    }
}
