using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Teleporter : MonoBehaviour {

    public GameObject exitPoint;

    public GameObject linkedTeleporter;

    void Teleport(GameObject obj) {
        obj.transform.position = exitPoint.transform.position;
        obj.transform.rotation = exitPoint.transform.rotation;
        // @TODO: SCALE?!!!????!!!
    }

    void OnTriggerEnter(Collider collider) {
        if (linkedTeleporter == null) {
            return;
        }
        
        GameObject obj = collider.gameObject;

        if (obj.tag == "Player") {
            
            Teleporter link = linkedTeleporter.GetComponent<Teleporter>();
            link.Teleport(obj);
        }
    }

    // Use this for initialization
    void Start () {
		
    }
	
    // Update is called once per frame
    void Update () {
		
    }
}
