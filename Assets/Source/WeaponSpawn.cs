using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SpawnType {
    Weapon,
    Grenade,
    Health,
}

public class WeaponSpawn : MonoBehaviour {

    public SpawnType spawnType;

    public GameObject weaponPrefab;
    // @TODO: do some initialization stats on that prefab like ammo count etc
    public int initialAmmo = -1;
    public int initialAmmoInClip = -1;

    public GameObject lastWeaponSpawned;

    public bool manualSpawn;

    public float spawnRate;
    float timeSinceSpawn = 0;
    bool empty;

    public void SpawnWeapon() {
        lastWeaponSpawned = Instantiate(weaponPrefab, this.transform.position, Quaternion.identity);

        Gun gun = lastWeaponSpawned.GetComponent<Gun>();
        lastWeaponSpawned.transform.rotation = Quaternion.Euler(gun.restRotation);
        gun.SetAmmoCount(initialAmmo);
        gun.SetAmmoInClip(initialAmmoInClip);
    }

    void SpawnHealth() {
        lastWeaponSpawned = Instantiate(weaponPrefab, this.transform.position + new Vector3(0, 0.25f, 0), Quaternion.Euler(new Vector3(-90, 0, 0)));
    }

    void SpawnItem() {
        switch (spawnType) {
            case SpawnType.Weapon: {
                SpawnWeapon();
            } break;

            case SpawnType.Health: {
                SpawnHealth();
            } break;
        }
    }
    
    // Use this for initialization
    void Start () {
        SpawnItem();
        
        empty = false;
        timeSinceSpawn = 0;
    }

    // This is tricky and annoying because we want them both to be triggers so that they dont collide with anything
    // (maybe we should just use masks?)
    void OnTriggerExit(Collider collider) {
        // @WARNING: we assume the collider is always parented to the gun directly
        //Debug.Log(collider.transform.parent.gameObject.name);
        
        if (collider.transform.parent.gameObject == lastWeaponSpawned) {
            empty = true;
        }
    }
	
    void Update () {
        if (manualSpawn) { return; }
        if (empty) {
            timeSinceSpawn += Time.deltaTime;

            if (timeSinceSpawn > spawnRate) {
                SpawnItem();
                
                timeSinceSpawn = 0;
                empty = false;
            }
        }

        // @MAYBE: do an OnDestroy call for a gun, but it needs to know that its owned by a spawner in that case
        if (lastWeaponSpawned == null) {
            empty = true;
        }
    }
}
