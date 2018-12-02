using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthPack : MonoBehaviour {

    void OnTriggerEnter(Collider collider) {
        GameObject obj = collider.gameObject;

        if (obj.tag == "Player") {
            Debug.Log("Collided with player!!!!!");
            Health health = obj.GetComponent<Health>();

            if (health.RestoreHealth()) {
                Destroy(this.gameObject.transform.parent.gameObject);
                Destroy(this.gameObject);
            }
        }
    }

    // Use this for initialization
    void Start () {
		
    }
	
    // Update is called once per frame
    void Update () {
		
    }
}
