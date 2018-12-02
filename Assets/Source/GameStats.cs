using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStats : MonoBehaviour {

    public GameObject playerPrefab;


    public GameObject weaponSpawner0;
    public GameObject weaponSpawner1;

    public int startingFragGrenades;
    public int startingPlasmaGrenades;

    public float respawnRate = 2.0f;
    
    
    public int localPlayerCount;
    GameObject[] players;

    // @TODO: maybe what we should do is create a spawner which is in charge of creating weapons and we can
    // use that instead of specifying start weapons and ammo counts etc in multiple places? Or maybe that's
    // just as struct like spawndata or something

    Transform FindSpawnPoint() {
        GameObject[] spawnPoints = GameObject.FindGameObjectsWithTag("Spawn");

        float maxDistance = -Mathf.Infinity;

        GameObject bestSpawn = null;
        float dist;
        foreach(GameObject spawn in spawnPoints) {
            foreach(GameObject player in players) {
                dist = Vector3.Distance(spawn.transform.position, player.transform.position);

                if (dist > maxDistance) {
                    maxDistance = dist;
                    bestSpawn = spawn;
                }
            }
        }

        return bestSpawn.transform;
    }

    WeaponSpawn wpnSpawner0;
    WeaponSpawn wpnSpawner1;
    
    // Use this for initialization
    void Start () {
        // @TODO: really what we want to do is create the players rather than find them in the scene!!!

        wpnSpawner0 = weaponSpawner0.GetComponent<WeaponSpawn>();
        wpnSpawner1 = weaponSpawner1.GetComponent<WeaponSpawn>();

        players = new GameObject[localPlayerCount];

        for (int i = 0; i < localPlayerCount; i++) {
            GameObject player = Instantiate(playerPrefab, new Vector3(0, i * 6, i * 4), Quaternion.identity);
            SpartanController controller = player.GetComponent<SpartanController>();
            controller.localPlayerNum = i;
            players[i] = player;
        }
            
        foreach (GameObject player in players) {
            SpartanController controller = player.GetComponent<SpartanController>();

            Transform bestSpawn = FindSpawnPoint();
            
            wpnSpawner0.SpawnWeapon();
            wpnSpawner1.SpawnWeapon();
            controller.SpawnSpartan(bestSpawn.position, bestSpawn.rotation, wpnSpawner0.lastWeaponSpawned, wpnSpawner1.lastWeaponSpawned);
            
            Camera cam = controller.camera.GetComponent<Camera>();

            SpartanUI ui = controller.canvas.GetComponent<SpartanUI>();
            ui.uiScale = 1.0f / localPlayerCount;

            if (localPlayerCount == 1) {
                Rect rect = new Rect(0, 0, 1, 1);
                cam.rect = rect;
            }
            else if (localPlayerCount == 2) {
                Rect rect = new Rect();
                rect.x = 0;
                rect.y = controller.localPlayerNum * 0.5f;
                rect.width = 1;
                rect.height = 0.5f;

                cam.rect = rect;
            }
            else if (localPlayerCount == 3) {
                Rect rect = new Rect();
                
                if (controller.localPlayerNum == 0) {
                    rect.x = 1;
                    rect.y = 0;
                    rect.width = 1;
                    rect.height = 0.5f;
                }
                else {
                    rect.x = controller.localPlayerNum * 0.5f;
                    rect.y = controller.localPlayerNum * 0.5f;
                    rect.width = 0.5f;
                    rect.height = 0.5f;
                }

                cam.rect = rect;
            }
            else {
                Rect rect = new Rect();
                
                rect.x = (controller.localPlayerNum % 2) * 0.5f;
                rect.y = (controller.localPlayerNum / 2) * 0.5f;
                rect.width = 0.5f;
                rect.height = 0.5f;

                cam.rect = rect;
            }
        }
    }
	
    // Update is called once per frame
    void Update () {
        foreach (GameObject player in players) {
            SpartanController controller = player.GetComponent<SpartanController>();

            Health health = player.GetComponent<Health>();

            if (health.dead) {
                float timeSinceDied = Time.time - health.timeDied;

                Transform bestSpawn = FindSpawnPoint();
                // @TODO: find a spawn point!!!!
                if (timeSinceDied >= respawnRate) {
                    wpnSpawner0.SpawnWeapon();
                    wpnSpawner1.SpawnWeapon();                    
                    controller.SpawnSpartan(bestSpawn.position, bestSpawn.rotation, wpnSpawner0.lastWeaponSpawned, wpnSpawner1.lastWeaponSpawned);
                }
            }
        }
    }
}
