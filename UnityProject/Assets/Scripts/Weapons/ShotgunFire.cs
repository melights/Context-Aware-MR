using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShotgunFire : MonoBehaviour {

    [SerializeField]
    private Light m_muzzleFlashLight;

    [SerializeField]
    private Transform m_foreEndSlide;

    [SerializeField]
    private Transform m_mainBody;

    private Vector3 m_foreEndSlideStartPosition;
    private Vector3 m_mainBodyStartPosition;
    private float m_lightIntensity;

    bool m_animate = false;
    float m_timer = 0.0f;
    float m_slideOffset = 0.0f;
    float m_mainBodyOffset = 0.0f;
    float m_slideDirMod = 1.0f;
    float m_mainBodyDirMod = 1.0f;

	// Use this for initialization
	void Start () {
        m_lightIntensity = m_muzzleFlashLight.intensity;
        m_muzzleFlashLight.enabled = false;
        m_foreEndSlideStartPosition = m_foreEndSlide.localPosition;
        m_mainBodyStartPosition = m_mainBody.localPosition;
    }
	
	// Update is called once per frame
	void Update () {

        if (m_animate)
        {
            if (m_timer < 3.0f)
            {
                m_mainBodyOffset += Time.deltaTime * 4.0f * m_mainBodyDirMod;

                if (m_mainBodyOffset >= 0.6f)
                {
                    m_mainBodyDirMod = -1.0f;
                }
                m_mainBodyOffset = Mathf.Clamp(m_mainBodyOffset, 0.0f, 0.6f);
                m_mainBody.localPosition = m_mainBodyStartPosition + new Vector3(m_mainBodyOffset, 0.0f, 0.0f);


                m_timer += Time.deltaTime;

                m_muzzleFlashLight.intensity -= Time.deltaTime * 10.0f;

                // after one second then perform slide
                if (m_timer > 0.8f)
                {
                    m_slideOffset += Time.deltaTime * 4.0f * m_slideDirMod;

                    if (m_slideOffset >= 0.6f)
                    {
                        m_slideDirMod = -1.0f;
                    }

                    m_slideOffset = Mathf.Clamp(m_slideOffset, 0.0f, 0.6f);
                    m_foreEndSlide.localPosition = m_foreEndSlideStartPosition + new Vector3(m_slideOffset, 0.0f, 0.0f);
                }
            }
            else
            {
                m_muzzleFlashLight.intensity = 0.0f;
               m_animate = false;
            }
        }
	}

    public void FireWeapon()
    {
        // Reset Values to Default
        m_muzzleFlashLight.enabled = true;
        m_muzzleFlashLight.intensity = m_lightIntensity;
        m_foreEndSlide.localPosition = m_foreEndSlideStartPosition;
        m_mainBody.localPosition = m_mainBodyStartPosition;
        m_slideDirMod = 1.0f;
        m_slideOffset = 0.0f;
        m_mainBodyDirMod = 1.0f;
        m_mainBodyOffset = 0.0f;
        m_animate = true;
        m_timer = 0.0f;
    }
}
