using UnityEngine;

public enum GunType {
    Magnum,
    AssaultRifle,
    PlasmaRifle,
    PlasmaPistol,
};

public enum AmmoType{
    Ballistic,
    Plasma,
};

public enum ProjectileType{
    Ray,
    Projectile,
};

public struct DamageData {
    public int healthDamage;
    public int shieldDamage;
    public float headshotModifier;
};

public class Gun : MonoBehaviour {

    public GunType type;
    public AmmoType ammoType;
    public ProjectileType projectileType;

    public GameObject projectilePrefab;
    public GameObject projectilePoint;

    public bool owned;

    // @GACK: bring this into a two structs: positionData and GunStats or something
    public Vector3 restRotation;

    public Vector3 positionOffset;
    public Vector3 rotation;

    public Vector3 positionOffsetHolster;
    public Vector3 rotationHolster;

    public Vector3 restingRotation;

    
    public Sprite reticule;
    public GameObject muzzleFlash;
    public float muzzleFlashDuration;


    public float recoil = 0.0f;
    
    // @NOTE: maxAmmo is really the max ammo you can have in reserve
    public int maxAmmo;
    public int ammoCount;
    
    public int clipSize;
    public int ammoInClip;

    public int healthDamage;
    public int shieldDamage;
    public float headshotModifier;

    public float range;
    public float radius;

    public float fireRate;
    public bool automatic;

    public bool triggerHeld;
    
    float timeFired;
    float timeSinceFired;
    // This is in seconds

    bool reloading;
    float timeReloaded;
    public float reloadDuration;

    bool overheating;

    // normalized
    float heat;
    public float heatPerShot;
    // this is points per second
    public float heatDecayRate;
    
    float timeSinceRelease;

    public void SetHeldPosition() {
        transform.localPosition = positionOffset;
        transform.localRotation = Quaternion.Euler(rotation);
    }

    public void SetHolsteredPosition() {
        transform.localPosition = positionOffsetHolster;
        transform.localRotation = Quaternion.Euler(rotationHolster);
    }

    public void SetDroppedPosition() {
        RaycastHit hit;
        Vector3 rayDir = new Vector3(0, -1, 0);
        Vector3 rayOrig = this.transform.position;
        Ray ray = new Ray(rayOrig, rayDir);

        if (Physics.Raycast(ray, out hit)) {
            transform.parent = null;
            transform.position = hit.point;
            transform.rotation = Quaternion.Euler(restingRotation);
        }
        else {
            transform.parent = null;
            transform.localPosition = new Vector3();

            transform.rotation = Quaternion.Euler(restingRotation);
        }
    }

    public float GetHeat() {
        if (ammoType == AmmoType.Plasma) {
            return heat;
        }

        return 0.0f;
    }

    public bool Fire(GameObject shooter, Ray ray, Quaternion rotation) {
        if (reloading || overheating) { return false; }
        
        // This is so it only collides with the player's hitbox, ignoring the CharacterController,
        // the weapon shapes, etcetera
        int layerMask = (1 << 13);
        int levelLayerMask = (1 << 0);
        
        timeSinceFired = Time.time - timeFired;

        bool fired = false;

        if (ammoInClip > 0 && timeSinceFired >= fireRate) {

            if (projectileType == ProjectileType.Ray) {
                // Vary the ray
                Vector3 pt = rotation * new Vector3(Random.Range(-radius, radius), Random.Range(-radius, radius), 0);
                Vector3 diff = pt - ray.origin;
                //ray.direction = diff.normalized;

                Debug.DrawRay(ray.origin, ray.direction, Color.red);
            
                float minDistance = Mathf.Infinity;

                // @NOTE: this is so we dont have to RaycastAll against the level, we can just take the closest
                // and compare its distance to the hitbox intersections
                RaycastHit levelHit;
                if (Physics.Raycast(ray, out levelHit, range, levelLayerMask)) {
                    minDistance = levelHit.distance;
                }

                // @NOTE: RaycastAll so the ray originates from the camera.
                // We cant use layers for this problem because all players need to be on the same layer
                // and there isnt a way to pass in more specific culling feature to Raycast :(
            
                GameObject hitObj = null;
                RaycastHit[] allHits = Physics.RaycastAll(ray, range, layerMask);
                foreach (RaycastHit hit in allHits) {
                    GameObject obj = hit.collider.gameObject;

                    if (obj.transform.root.gameObject == shooter) { continue; }

                    if (hit.distance < minDistance && (obj.tag == "Head" || obj.tag == "Body")) {
                        hitObj = obj;
                        minDistance = hit.distance;
                    }
                }

                if (hitObj != null) {
                    GameObject playerHit = hitObj.transform.root.gameObject; 
                    Health health = playerHit.GetComponent<Health>();

                    bool headshot = hitObj.tag == "Head";

                    health.DamagePlayer(healthDamage, shieldDamage, headshot, headshotModifier);
                }
            }
            else {
                GameObject obj = Instantiate(projectilePrefab, projectilePoint.transform.position, rotation);

                Projectile projectile = obj.GetComponent<Projectile>();
                projectile.damageData.healthDamage = healthDamage;
                projectile.damageData.shieldDamage = shieldDamage;
                projectile.damageData.headshotModifier = headshotModifier;
            }

            this.GetComponent<AudioSource>().Play();

            ammoInClip--;

            timeFired = Time.time;

            muzzleFlash.active = true;

            fired = true;
        }

        if (fired && ammoType == AmmoType.Plasma) {
            heat += heatPerShot;
            if (heat > 1) { heat = 1; }
        }

        return fired;
    }

    public void Reload() {
        if (ammoType == AmmoType.Plasma) { return; }
        
        timeReloaded = Time.time;
        reloading = true;
    }

    public void SetAmmoCount(int ammo) {
        if (ammo >= 0) {
            // @GAME: maybe this is weird and we should just clip maxAmmo?
            int spaceInClip = clipSize - ammoInClip;
            
            if (ammo > maxAmmo + spaceInClip) { ammoCount = maxAmmo + spaceInClip; }
            else { ammoCount = ammo; }
        }
        else {
            ammoCount = maxAmmo;
        }
    }

    public int AddAmmo(int ammo) {
        int initialAmmoCount = ammoCount;
        SetAmmoCount(ammoCount + ammo);

        return ammoCount - initialAmmoCount;
    }

    public void SetAmmoInClip(int ammo) {
        if (ammo >= 0) {
            if (ammo > clipSize) { ammoInClip = ammo; }
            else { ammoInClip = ammo; }
        }
        else {
            ammoInClip = clipSize;
        }
    }

    void DestroyGun() {
        foreach (Transform child in this.transform) {
            Destroy(child.gameObject);
        }

        Destroy(this.gameObject);
    }

    public void Update() {
        if (owned) {
            if (triggerHeld) {
                timeSinceRelease += Time.deltaTime;
            }
            else {
                timeSinceRelease = 0.0f;
            }
        
            if (ammoType == AmmoType.Plasma) {

                if (heat >= 1) {
                    overheating = true;
                }

                heat -= heatDecayRate * Time.deltaTime;

                if (heat < 0) {
                    heat = 0;
                }

                if (overheating) {
                    if (heat <= 0) {
                        overheating = false;
                    }
                }
            }
        
            if (reloading) {
                float timeSinceReload = Time.time - timeReloaded;

                if (timeSinceReload > reloadDuration) {
                    int bulletsToReplenish = clipSize - ammoInClip;
                
                    if (bulletsToReplenish > ammoCount) {
                        ammoInClip = ammoCount;
                        ammoCount = 0;
                    }
                    else {
                        ammoCount -= bulletsToReplenish;
                        ammoInClip = clipSize;
                    }

                    reloading = false;
                }
            }
            else {
                timeSinceFired = Time.time - timeFired;
                if (timeSinceFired >= muzzleFlashDuration) {
                    muzzleFlash.active = false;
                }
            
            }
        }
        else {
            muzzleFlash.active = false;
            
            if (ammoCount + ammoInClip == 0) {
                DestroyGun();
            }
        }
    }
}
