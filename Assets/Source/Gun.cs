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

public class Gun : MonoBehaviour {

    public GunType type;
    public AmmoType ammoType; 

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
    
    public int maxAmmo;
    public int ammoCount;
    
    public int clipSize;
    public int ammoInClip;

    public int healthDamage;
    public int shieldDamage;
    public float headshotModifier;

    public float range;

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
    float overheatStartTime;
    float timeSinceRelease;
    public float timeTilOverheat;
    public float overheatDuration;


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

    public void Fire(GameObject shooter, Ray ray) {
        if (reloading || overheating) { return; }
        
        // This is so it only collides with the player's hitbox, ignoring the CharacterController,
        // the weapon shapes, etcetera
        int layerMask = (1 << 13);
        int levelLayerMask = (1 << 0);
        
        timeSinceFired = Time.time - timeFired;

        if (ammoInClip > 0 && timeSinceFired >= fireRate) {
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

            this.GetComponent<AudioSource>().Play();

            ammoInClip--;

            timeFired = Time.time;

            muzzleFlash.active = true;
        }
    }

    public void Reload() {
        if (ammoType == AmmoType.Plasma) { return; }
        
        timeReloaded = Time.time;
        reloading = true;
    }

    public void SetAmmoCount(int ammo) {
        // @TODO: do something different if its plasma
        if (ammo >= 0) {
            if (ammo > maxAmmo) { ammoCount = maxAmmo; }
            else { ammoCount = ammo; }
        }
        else {
            ammoCount = maxAmmo;
        }
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

    public void Update() {
        if (triggerHeld) {
            timeSinceRelease += Time.deltaTime;
        }
        else {
            timeSinceRelease = 0.0f;
        }
        
        if (ammoType == AmmoType.Plasma) {

            // @TODO: this really needs to be more of a cooldown because plasma pistol never overheats right now
            if (timeSinceRelease >= timeTilOverheat && !overheating) {
                overheating = true;
                overheatStartTime = Time.time;
            }

            if (overheating) {
                float timeSinceOverheat = Time.time - overheatStartTime;

                if (timeSinceOverheat >= overheatDuration) {
                    overheating = false;
                    // @TODO: what if you hold the trigger during the overheat?
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
}
