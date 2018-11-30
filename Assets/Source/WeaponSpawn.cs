using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponSpawn : MonoBehaviour {

    public GameObject weaponPrefab;
    // @TODO: do some initialization stats on that prefab like ammo count etc
    public int initialAmmo = -1;
    public int initialAmmoInClip = -1;

    GameObject lastWeaponSpawned;

    public float spawnRate;
    float timeSinceSpawn = 0;
    bool empty;

    void SpawnWeapon() {
        lastWeaponSpawned = Instantiate(weaponPrefab, this.transform.position + new Vector3(0, 1, 0), Quaternion.identity);

        Gun gun = lastWeaponSpawned.GetComponent<Gun>();
        lastWeaponSpawned.transform.rotation = Quaternion.Euler(gun.restRotation);
        gun.SetAmmoCount(initialAmmo);
        gun.SetAmmoInClip(initialAmmoInClip);
    }
    
    // Use this for initialization
    void Start () {
        SpawnWeapon();
        
        empty = false;
        timeSinceSpawn = 0;
    }

    // This is tricky and annoying because we want them both to be triggers so that they dont collide with anything
    // (maybe we should just use masks?)
    void OnTriggerExit(Collider collider) {
        Debug.Log("left");

        // @WARNING: we assume the collider is always parented to the gun directly
        Debug.Log(collider.transform.parent.gameObject.name);
        Debug.Log("last " + lastWeaponSpawned.name);
        
        if (collider.transform.parent.gameObject == lastWeaponSpawned) {
            empty = true;
        }
    }
	
    // Update is called once per frame
    void Update () {

        if (empty) {
            timeSinceSpawn += Time.deltaTime;

            if (timeSinceSpawn > spawnRate) {
                SpawnWeapon();
                
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
