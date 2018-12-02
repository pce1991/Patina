using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour {

    public float speed;
    public Vector3 rotation;

    Vector3 direction;

    // @PERF: we can accelerate this by just having a damage table so we dont have to store the data
    public DamageData damageData;

    Rigidbody body;

    float timeCreated;

    void OnCollisionEnter(Collision collision) {
        //Debug.Log("collided with " + collision.gameObject.name);

        GameObject hitObj = collision.gameObject;

        if (hitObj.gameObject.tag == "Head" || hitObj.gameObject.tag == "Body") {

            bool headshot = hitObj.tag == "Head";

            GameObject playerHit = hitObj.transform.root.gameObject; 
            Health health = playerHit.GetComponent<Health>();

            health.DamagePlayer(damageData.healthDamage, damageData.shieldDamage, headshot, damageData.headshotModifier);
        }

        Destroy(this.gameObject);
    }

    void Start () {
        direction = transform.rotation * Vector3.forward;

        transform.rotation = transform.rotation * Quaternion.Euler(rotation);

        body = GetComponent<Rigidbody>();

        timeCreated = Time.time;
    }
	
    void FixedUpdate () {
        body.AddForce(direction * speed);

        if (Time.time > timeCreated + 10) {
            Destroy(this.gameObject);
        }
    }
}
