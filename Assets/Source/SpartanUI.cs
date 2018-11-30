﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpartanUI : MonoBehaviour {

    public GameObject player;
    public GameObject camera;
    public GameObject shieldBar;
    public GameObject shieldMask;

    public GameObject healthBar;
    public GameObject healthMask;
    
    public GameObject reticule;

    public GameObject ammoCount;

    public float uiScale;

    Rect maskRect = new Rect();

    float maxShieldMaskWidth;

    float maxHealthMaskWidth;


    // Use this for initialization
    void Start () {
        Vector2 screenDim = new Vector2(Screen.width, Screen.height);
        
        Camera cam = camera.GetComponent<Camera>();

        float aspect = (screenDim.x * cam.rect.width) / (screenDim.y * cam.rect.height);

        {
            RectTransform shieldRect = shieldBar.GetComponent<RectTransform>();

            Vector3 anchoredPosition3D = shieldRect.anchoredPosition3D;
            
            anchoredPosition3D.x = screenDim.x * 0.05f;
            anchoredPosition3D.y = -screenDim.y * 0.05f;
            
            shieldRect.anchoredPosition3D = anchoredPosition3D;

            // @NOTE: we dont need to do anything more complicated because PreserveAspect
            // is set on the Image component
            shieldRect.localScale = new Vector3(cam.rect.width, cam.rect.height, 1);

            maxShieldMaskWidth = shieldRect.rect.width;

            RectTransform healthRect = healthBar.GetComponent<RectTransform>();

            healthRect.anchoredPosition3D = new Vector3(screenDim.x * 0.05f, -screenDim.y * 0.11f, 0.0f);
            healthRect.localScale = shieldRect.localScale;
            maxHealthMaskWidth = healthRect.rect.width;
        }

        {
            RectTransform ammoRect = ammoCount.GetComponent<RectTransform>();

            Vector3 anchoredPosition3D = ammoRect.anchoredPosition3D;
            
            anchoredPosition3D.x = screenDim.x * 0.85f;
            anchoredPosition3D.y = -screenDim.y * 0.05f;

            ammoRect.anchoredPosition3D = anchoredPosition3D;

            float width = screenDim.x * 0.1f; float height = screenDim.y * 0.1f;
            ammoRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
            ammoRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, height);
        }

        // {
        //     RectTransform reticuleRect = reticule.GetComponent<RectTransform>();

        //     reticuleRect.localScale = new Vector3(cam.rect.width, cam.rect.height, 1);
        // }
    }

    // @TODO: do health nubs instead of a bar because its a bit clearer
    // @PERF: setting this every frame is probably wasteful, we could instead just call UI functions when needed
    void Update () {
        RectTransform shieldMaskRect = shieldMask.GetComponent<RectTransform>();
        RectTransform healthMaskRect = healthMask.GetComponent<RectTransform>();

        
        Health health = player.GetComponent<Health>();

        // @NOTE: we cant modify the width directly, so we do this
        shieldMaskRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, health.NormalizedShield() * maxShieldMaskWidth);

        healthMaskRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, health.NormalizedHealth() * maxHealthMaskWidth);

        Text text = ammoCount.GetComponent<Text>();
        SpartanController controller = player.GetComponent<SpartanController>();
        GameObject activeGun = controller.GetActiveWeapon();

        if (activeGun != null) {
            Gun gun = activeGun.GetComponent<Gun>();

            text.text = string.Format("{0} \\\\\\ {1}", gun.ammoInClip, gun.ammoCount);

            if (gun.ammoType == AmmoType.Plasma) {
                // do a thing with the overheat bar
            }
        }
        else {
            text.text = string.Format("0 \\\\\\ 0");
        }
    }
}
