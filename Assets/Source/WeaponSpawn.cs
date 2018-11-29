using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponSpawn : MonoBehaviour {

    public GameObject weaponPrefab;
    // @TODO: do some initialization stats on that prefab like ammo count etc

    GameObject lastWeaponSpawned;

    public float spawnRate;
    float timeSinceSpawn = 0;
    bool empty;
    // Use this for initialization
    void Start () {
        lastWeaponSpawned = Instantiate(weaponPrefab, this.transform.position + new Vector3(0, 1, 0), Quaternion.identity);

        Gun gun = lastWeaponSpawned.GetComponent<Gun>();
        lastWeaponSpawned.transform.rotation = Quaternion.Euler(gun.restRotation);
        
        empty = false;
        timeSinceSpawn = 0;
    }

    // This is tricky and annoying because we want them both to be triggers so that they dont collide with anything
    // (maybe we should just use masks?)
    void OnTriggerExit(Collider collider) {
        Debug.Log("left");
        
        if (collider.gameObject == lastWeaponSpawned) {
            empty = true;
        }
    }
	
    // Update is called once per frame
    void Update () {

        if (empty) {
            timeSinceSpawn += Time.deltaTime;

            if (timeSinceSpawn > spawnRate) {
                timeSinceSpawn = 0;
                empty = false;
            }
        }
    }
}
