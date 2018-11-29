using UnityEngine;

public enum GunType {
    Magnum,
    AssaultRifle,
    PlasmaRifle,
};


public class Gun : MonoBehaviour {

    public GunType type;

    public bool owned;

    // @GACK: bring this into a two structs: positionData and GunStats or something
    public Vector3 restRotation;

    public Vector3 positionOffset;
    public Vector3 rotation;

    public Vector3 positionOffsetHolster;
    public Vector3 rotationHolster;

    public Vector3 restingRotation;



    public Sprite reticule;

    
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

    public void Fire(Ray ray) {
        timeSinceFired = Time.time - timeFired;

        if (timeSinceFired >= fireRate) {
            RaycastHit hit;
            
            if (Physics.Raycast(ray, out hit, range)) {
                GameObject objHit = hit.collider.gameObject;

                if (objHit.tag == "Head" || objHit.tag == "Body") {
                    Debug.Log(objHit.transform.root.gameObject.name);

                    // @WARNING: If players are ever parented to transforms then we are sad!!!!!!
                    GameObject playerHit = objHit.transform.root.gameObject; 
                    Health health = playerHit.GetComponent<Health>();

                    bool headshot = objHit.tag == "Head";

                    health.DamagePlayer(healthDamage, shieldDamage, headshot, headshotModifier);
                }
            }

            this.GetComponent<AudioSource>().Play();

            timeFired = Time.time;
        }
    }

    public void Reload() {
        int bulletsToReplenish = clipSize - ammoInClip;

        ammoCount -= bulletsToReplenish;

        ammoInClip = clipSize;
    }
}
