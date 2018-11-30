using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour {

    public float health;
    public float shield;

    float maxHealth = 30;
    float maxShield = 70;

    float timeSinceDamaged = 0.0f;
    float shieldRechargeDelay = 2.0f;

    // @NOTE: this is in points perSecond
    float shieldRechargeRate = 35;

    public bool dead = false;
    public float timeDied;

    public void InitHealth() {
        dead = false;
        health = maxHealth;
        shield = maxShield;
    }

    // Use this for initialization
    void Start () {
        if (health > maxHealth) { health = maxHealth; }
        if (shield > maxShield) { shield = maxShield; }
    }

    public float NormalizedShield() {
        return shield / maxShield;
    }

    public float NormalizedHealth() {
        return health / maxHealth;
    }

    // @GACK: maybe put these in a struct
    public void DamagePlayer(int healthDamage, int shieldDamage, bool headshot, float headshotMultiplier) {

        if (shield > 0) {
            if (headshot) {
                shield -= shieldDamage * headshotMultiplier;
            }
            else {
                shield -= shieldDamage;
            }
        }

        if (shield <= 0) {
            int overflowShieldDamage = (int)Mathf.Abs(shield);

            // @GAME @TODO: this may need some balancing!!!!!
            // if (overflowShieldDamage > 0) {
            //     Debug.Log(overflowShieldDamage);
            //     healthDamage = overflowShieldDamage;
            // }
            
            if (headshot) {
                health -= healthDamage * headshotMultiplier;
            }
            else {
                health -= healthDamage;
            }
        }
        
        timeSinceDamaged = 0.0f;

        if (shield < 0) {
            shield = 0;
        }
    }

    void Update() {
        if (timeSinceDamaged > shieldRechargeDelay && shield < maxShield) {
            shield += shieldRechargeRate * Time.deltaTime;

            if (shield > maxShield) { shield = maxShield; }
        }

        timeSinceDamaged += Time.deltaTime;
    }
	
    // Update is called once per frame
    // @NOTE: we do late update because we dont want to kill any players before they got a chance to run
    // there normal update!
    void LateUpdate () {
        SpartanController player = this.GetComponent<SpartanController>();
        
        if (health <= 0 && !dead) {
            dead = true;
            timeDied = Time.time;

            player.KillSpartan();
            
            Debug.Log("player " + player.localPlayerNum);
            
            // @TODO: kill player. Its weird tho, do we destroy the instance of that player, or just
            // change some stuff about it? Drop equipment, reset health, change position?
        }
    }
}
