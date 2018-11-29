using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpartanUI : MonoBehaviour {

    public GameObject health;
    public GameObject camera;
    public GameObject shieldBar;
    public GameObject shieldMask;

    public GameObject healthBar;
    public GameObject healthMask;
    
    public GameObject reticule;

    public float uiScale;

    Rect maskRect = new Rect();

    float maxShieldMaskWidth;

    float maxHealthMaskWidth;

    public bool dead = false;
    public float timeDied;

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

            healthRect.anchoredPosition3D = new Vector3(screenDim.x * 0.05f, -screenDim.y * 0.1f, 0.0f);
            healthRect.localScale = shieldRect.localScale;
            maxHealthMaskWidth = healthRect.rect.width;
        }

        // {
        //     RectTransform reticuleRect = reticule.GetComponent<RectTransform>();

        //     reticuleRect.localScale = new Vector3(cam.rect.width, cam.rect.height, 1);
        // }
    }

    // Update is called once per frame
    void Update () {
        RectTransform shieldMaskRect = shieldMask.GetComponent<RectTransform>();
        RectTransform healthMaskRect = healthMask.GetComponent<RectTransform>();

        Health h = health.GetComponent<Health>();

        // @NOTE: we cant modify the width directly, so we do this
        shieldMaskRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, h.NormalizedShield() * maxShieldMaskWidth);

        healthMaskRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, h.NormalizedHealth() * maxHealthMaskWidth);
    }
}
