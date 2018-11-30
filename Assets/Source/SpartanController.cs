using System;
using UnityEngine;
using UnityEngine.UI;

public struct ControlPrefrences {
    String moveX;
    String moveY;

    int lookSensitivity;
    bool inverted;
    String lookX;
    String lookY;

    String shoot;
    String jump;
    String reload;
    String swap;
    String melee;
    String grenade;
    String crouch;
    String zoom;
};


public class SpartanController : MonoBehaviour {

    // @GACK: these could both just use the localPlayerNum which is in scope so we dont have to pass it in
    // We can actually just this up on Start() by taking the index and assigning the strings
    bool ButtonPressed(String input, int playerIndex) {
        input = input + "_" + playerIndex;

        return Input.GetButtonDown(input);
    }

    bool ButtonHeld(String input, int playerIndex) {
        input = input + "_" + playerIndex;

        return Input.GetButton(input);
    }

    float GetAxis(String input, int playerIndex) {
        input = input + "_" + playerIndex;

        return Input.GetAxis(input);
    }

    public ControlPreferences preferences;

    float speed = 550;
    float vertAirSpeed = 450;
    float horzAirSpeed = 350;

    float vertBoostSpeed = 650;
    float horzBoostSpeed = 650;
    
    float jumpForce = 100;
    float boostForce = 100;

    public GameObject weaponHand;
    public GameObject holster;
    public GameObject sling;

    public GameObject standingHitboxes;
    public GameObject crouchingHitboxes;
    public GameObject model;
    
    
    public GameObject camera;
    Ray gunRay;
    private float xAxisClamp;

    // @NOTE: used for finding which input to use
    public int localPlayerNum;

    float gravity = 20;

    float groundDistanceThreshold = 1.5f;

    enum MovementState {
        OnGround,
        Crouching,
        Jump,
        Boost,
        Falling,
    };

    float heightStartedFall;

    float timeSinceJump;
    float jumpDuration = 0.15f;

    Vector3 velocity = new Vector3(0, 0, 0);

    public GameObject boostParticles;
    bool canBoost;
    float timeSinceBoost;
    float boostDuration = 0.15f;

    private CharacterController characterController;

    private MovementState movementState;

    GameObject[] weapons = new GameObject[2];
    int activeWeaponIndex = -1;

    public GameObject reticule;
    public GameObject pickupTextPrompt;
    public GameObject canvas;

    

    public void SpawnSpartan(Vector3 position, Quaternion rotation, GameObject weapon0, GameObject weapon1) {

        transform.position = position;
        transform.rotation = rotation;
        
        Health health = GetComponent<Health>();
        health.InitHealth();

        weapons[0]= weapon0;
        weapons[1]= weapon1;

        if (weapons[0] != null) {
            SetActiveGun(weapons[0]);
            SetInactiveGun(weapons[1]);
            activeWeaponIndex = 0;
        }
        else if (weapons[1] != null) {
            SetActiveGun(weapons[1]);
            activeWeaponIndex = 1;
        }
        else {
            activeWeaponIndex = -1;
        }

        model.active = true;
        standingHitboxes.active = true;
        crouchingHitboxes.active = true;
    }

    public void KillSpartan() {

        // @TODO: here's a tricky thing: we want to keep the body around a bit and do a death animation
        // Deactivate hitboxes and model
        model.active = false;
        standingHitboxes.active = false;
        crouchingHitboxes.active = false;
        

        for (int i = 0; i < 2; i++) {
            if (weapons[i] == null) { continue; }

            weapons[i].transform.parent = null;

            Rigidbody oldBody = weapons[i].GetComponent<Rigidbody>();
            oldBody.isKinematic = false;

            Gun oldGun = weapons[i].GetComponent<Gun>();
            oldGun.owned = false;
        }
    }
    
    void Start () {
        characterController = GetComponent<CharacterController>();
    }

    void UpdateLook() {
        // @TODO: switch on input type!

        Vector2 lookDirection = new Vector2(GetAxis("LookX", localPlayerNum),
                                            -GetAxis("LookY", localPlayerNum));

        lookDirection = lookDirection * 2;

        //Debug.Log(lookDirection);

        xAxisClamp += lookDirection.y;

        if (xAxisClamp > 90) {
            xAxisClamp = 90;
            lookDirection.y = 0;
        }
        else if (xAxisClamp < -90) {
            xAxisClamp = -90;
            lookDirection.y = 0;
        }

        Transform cameraTransform = camera.GetComponent<Transform>();
        cameraTransform.Rotate(Vector3.left * lookDirection.y);

        this.transform.Rotate(Vector3.up * lookDirection.x);

        gunRay.origin = cameraTransform.position;
        gunRay.direction = cameraTransform.rotation * Vector3.forward;
    }

    // http://gizma.com/easing/
    Vector3 CalculateJumpForce(float time) {
        float d = jumpDuration;
        float b = gravity;
        float c = jumpForce;
        float t = time / d;

        // circ ease out 
        float scale = c * Mathf.Sqrt(1 - t * t) + b;
        return Vector3.up * scale;
    }

    Vector3 CalculateBoostForce(float time) {
        float d = boostDuration;
        float b = gravity;
        float c = boostForce;
        float t = time / d;
        float scale = c * Mathf.Sqrt(1 - t * t) + gravity;
        return Vector3.up * scale;
    }

    bool canPickupWeapon = false;
    GameObject candidateWeapon;

    // @TODO: handle multiple candidates by taking the closest one
    // @TODO: if the weapon we're in the trigger of is already held, ignore
    //        if the weapon is owned by something, dont include it
    void OnTriggerEnter(Collider collider) {
        if (collider.gameObject.tag == "Gun") {
            canPickupWeapon = true;
            candidateWeapon = collider.gameObject.transform.root.gameObject;
        }

        Gun gun = candidateWeapon.GetComponent<Gun>();

        bool canPickup = true;

        // @GACK: this isnt the most elegant way to check weapons,
        if (activeWeaponIndex >= 0) {

            for (int i = 0; i < 2; i++) {
                if (weapons[i] == null) { continue; }

                Gun activeGun = weapons[i].GetComponent<Gun>();

                if (activeGun.type == gun.type) {
                    if (activeGun.ammoType == AmmoType.Plasma && gun.ammoCount > activeGun.ammoCount) {
                        canPickup = true;
                    }
                    else {
                        canPickup = false;
                        int takenAmmo = activeGun.AddAmmo(gun.ammoCount + gun.ammoInClip);
                        gun.ammoCount -= takenAmmo;
                    }
                }
                else {
                    canPickup &= true;
                }
            }
        }

        if (canPickup) {
            if (gun.owned) {
                candidateWeapon = null;
                canPickupWeapon = false;
                return;
            }

            pickupTextPrompt.active = true;
            Text text = pickupTextPrompt.GetComponent<Text>();

            String message = "Press X to Pickup ";

            // @PERF: bake these messages out!
            text.text = message + gun.type.ToString();
        }
        else {
            candidateWeapon = null;
            canPickupWeapon = false;
        }
    }

    // @BUG: what if we never actually leave the trigger?
    void OnTriggerExit(Collider collider) {
        if (collider.gameObject.tag == "Gun") {
            candidateWeapon = null;
            canPickupWeapon = false;
        }

        pickupTextPrompt.active = false;
    }

    void SetActiveGun(GameObject weapon) {
        weapon.transform.SetParent(weaponHand.transform);

        Rigidbody body = weapon.GetComponent<Rigidbody>();
        body.isKinematic = true;
                
        Gun gun = weapon.GetComponent<Gun>();                
        gun.SetHeldPosition();
        gun.owned = true;

        Image image = reticule.GetComponent<Image>();
        image.sprite = gun.reticule;
    }

    void SetInactiveGun(GameObject weapon) {
        Gun gun = weapon.GetComponent<Gun>();                

        if (gun.type == GunType.Magnum || gun.type == GunType.PlasmaPistol) {
            weapon.transform.SetParent(holster.transform);
        }
        else {
            weapon.transform.SetParent(sling.transform);
        }

        gun.SetHolsteredPosition();
    }

    public GameObject GetActiveWeapon() {
        if (activeWeaponIndex >= 0) {
            return weapons[activeWeaponIndex];
        }
        else {
            return null;
        }
    }
        
		
    void Update () {
        UpdateLook();

        MovementState prevMovementState = movementState;

        bool switchWeapon = false;
        int prevActiveWeaponIndex = activeWeaponIndex;
        if (canPickupWeapon &&
            // @GACK: really need a better way to know if you are trying to pick up a weapon when you have
            // a weapon already
            (activeWeaponIndex < 0 || candidateWeapon != weapons[activeWeaponIndex])) {
            if (ButtonPressed("Reload", localPlayerNum)) {

                // @NOTE: when we pickup a weapon we havent left the trigger
                // so we want to manually deactivate the prompt message here
                pickupTextPrompt.active = false;

                

                if (weapons[0] == null) {
                    weapons[0] = candidateWeapon;
                    activeWeaponIndex = 0;
                }
                else if (weapons[1] == null) {
                    weapons[1] = candidateWeapon;
                    activeWeaponIndex = 1;
                }
                else {
                    Rigidbody oldBody = weapons[activeWeaponIndex].GetComponent<Rigidbody>();
                    oldBody.isKinematic = false;

                    Gun oldGun = weapons[activeWeaponIndex].GetComponent<Gun>();
                    oldGun.SetDroppedPosition();
                    oldGun.owned = false;
                    
                    weapons[activeWeaponIndex] = candidateWeapon;
                }

                SetActiveGun(candidateWeapon);
            }
        }

        if (ButtonPressed("Switch", localPlayerNum)) {
            int attemptedWeaponIndex = (activeWeaponIndex + 1) % 2;

            GameObject nextWeapon = weapons[attemptedWeaponIndex];

            if (nextWeapon != null) {
                nextWeapon.transform.SetParent(weaponHand.transform);

                Gun gun = nextWeapon.GetComponent<Gun>();                
                gun.SetHeldPosition();

                Image image = reticule.GetComponent<Image>();
                image.sprite = gun.reticule;

                activeWeaponIndex = attemptedWeaponIndex;
            }
        }

        if (prevActiveWeaponIndex >= 0 && activeWeaponIndex != prevActiveWeaponIndex) {
            GameObject prevWeapon = weapons[prevActiveWeaponIndex];

            SetInactiveGun(prevWeapon);
        }

        if (activeWeaponIndex >= 0) {
                        
            GameObject activeWeapon = weapons[activeWeaponIndex];
            Gun gun = activeWeapon.GetComponent<Gun>();

            Debug.DrawRay(gunRay.origin, gunRay.direction, Color.red);
                
            if (GetAxis("Fire", localPlayerNum) < -0.5f) {

                // @GACK: its real gross to set this in the controller and not in the gun itself
                // Why do I feel like Unity is making me do this!!!!!?????
                bool wasTriggerHeld = gun.triggerHeld;
                
                if (gun.automatic) {
                    gun.Fire(this.gameObject, gunRay);
                    gun.triggerHeld = true;
                }
                else {
                    if (!wasTriggerHeld) {
                        gun.Fire(this.gameObject, gunRay);
                        gun.triggerHeld = true;
                    }
                }
            }
            else {
                gun.triggerHeld = false;
            }

            // @TODO: only if we havent picked up a weapon!
            if (ButtonPressed("Reload", localPlayerNum)) {
                gun.Reload();
            }
        }

        
        
        Vector2 direction = new Vector2(GetAxis("MoveX", localPlayerNum), -GetAxis("MoveY", localPlayerNum));

        Vector3 force = new Vector3();

        // @TODO: stop moving when there is no input

        RaycastHit hit;
        Vector3 rayDir = new Vector3(0, -1, 0);
        Vector3 rayOrig = this.transform.position;
        Ray ray = new Ray(rayOrig, rayDir);

        Debug.DrawRay(rayOrig, rayDir, Color.red);

        if (characterController.isGrounded) {
            if (movementState == MovementState.Falling) {
                Health health = GetComponent<Health>();
                
                float fallingSpeed = velocity.y;

                //Debug.Log(fallingSpeed);

                if (fallingSpeed < -50) {
                    health.DamagePlayer(100, 100, false, 0.0f);
                }
            }
            
            movementState = MovementState.OnGround;
            velocity = Vector3.zero;
        }

        if (movementState == MovementState.Crouching) {
            standingHitboxes.active = false;
            crouchingHitboxes.active = true;
        }
        else {
            standingHitboxes.active = true;
            crouchingHitboxes.active = false;
        }

        // Debug.Log(characterController.isGrounded);
        // Debug.Log(movementState);

        if (movementState == MovementState.OnGround) {
            canBoost = true;
            //Vector3 surfaceNormal = hit.normal;

            //Debug.DrawLine(hit.point, hit.point + surfaceNormal, Color.blue);
                
            // Vector3 playerForward = this.transform.rotation * Vector3.forward;

            // Vector3 groundX = Vector3.Cross(surfaceNormal, playerForward).normalized;
            // Vector3 groundZ = Vector3.Cross(groundX, surfaceNormal).normalized;

            // Debug.DrawLine(this.transform.position, this.transform.position + groundX, Color.red);
            // Debug.DrawLine(this.transform.position, this.transform.position + groundZ, Color.red);

            force = new Vector3(speed * direction.x, 0, speed * direction.y);
            force = this.transform.rotation * force;

            movementState = MovementState.OnGround;
        }

        
        if (!characterController.isGrounded &&
            movementState != MovementState.Jump && movementState != MovementState.Boost) {
            movementState = MovementState.Falling;
            heightStartedFall = transform.position.y;
        }

        if (movementState == MovementState.OnGround) {
            bool jumped = ButtonPressed("Jump", localPlayerNum);

            if (jumped) {
                force = Vector3.zero;
                timeSinceJump = 0.0f;

                movementState = MovementState.Jump;

                velocity = Vector3.zero;
            }
        }

        // @TODO: make sure we havent jumped this same frame, because our button press is still true
        if (canBoost && (movementState == MovementState.Jump ||
                         movementState == MovementState.Falling)) {
            if (ButtonPressed("Jump", localPlayerNum) && prevMovementState != MovementState.OnGround) {
                canBoost = false;
                movementState = MovementState.Boost;
                timeSinceBoost = 0;

                ParticleSystem particles = boostParticles.GetComponent<ParticleSystem>();
                particles.Play();
            }
        }

        if (movementState == MovementState.Jump) {
            timeSinceJump += Time.deltaTime;

            velocity.x = 0;
            velocity.z = 0;
            force = new Vector3(horzAirSpeed * direction.x, 0, vertAirSpeed * direction.y);
            force = this.transform.rotation * force;

            force = force + CalculateJumpForce(timeSinceJump);

            // @TODO: this is too small a hop
            if (timeSinceJump >= jumpDuration || !ButtonHeld("Jump", localPlayerNum)) {
                movementState = MovementState.Falling;
                heightStartedFall = transform.position.y;
            }
        }

        if (movementState == MovementState.Boost) {
            timeSinceBoost += Time.deltaTime;

            velocity.x = 0;
            velocity.z = 0;
            
            // @TODO: look at the direction when you first boost, apply a large force along that direction
            // for some duration, then return to normal air control
            
            force = new Vector3(horzBoostSpeed * direction.x, 0, vertBoostSpeed * direction.y);
            force = this.transform.rotation * force;

            force = force + (CalculateBoostForce(timeSinceBoost));

            if (timeSinceBoost >= boostDuration) {
                movementState = MovementState.Falling;
                heightStartedFall = transform.position.y;
            }            
        }

        if (movementState == MovementState.Falling) {
            velocity.x = 0;
            velocity.z = 0;
            
            force = new Vector3(horzAirSpeed * direction.x, 0, vertAirSpeed * direction.y);
            force = this.transform.rotation * force;
        }

        
        //if (!characterController.isGrounded) {
            Vector3 gravityForce = (-Vector3.up * gravity);
            force = force + gravityForce;
        //}

        velocity += force * Time.deltaTime;
        //Debug.Log(velocity);

        characterController.Move(velocity * Time.deltaTime);
    }
}
