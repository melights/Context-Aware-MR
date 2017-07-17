using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum ECameraSystem
{
    MeshPreProcessSystem,
    ZEDRealtimeSystem
}

public class GameController : MonoBehaviour {

    [SerializeField]
    private ECameraSystem m_cameraSystemEnum;

    [SerializeField]
    private MaterialRayCastSystem m_materialRayCastSystem;

    [SerializeField]
    private DataManager m_dataManager;

    [SerializeField]
    private Color m_errorColour;

    [SerializeField]
    private Camera m_meshRenderCamera;

    [SerializeField]
    private Transform m_debugHitPoint;

    [SerializeField]
    private Text m_uiMaterialTextOutput;

    [SerializeField]
    private Image m_uiMaterialColourOutput;

    [SerializeField]
    private GameObject m_uiTriggerMl;


    // MESH PRE PROCESS SYSTEMS

    [SerializeField]
    private GameObject m_meshSystemGO;

    [SerializeField]
    private GameObject m_meshRenderer;

    [SerializeField]
    private GameObject m_meshParticleParent;

    [SerializeField]
    private GameObject m_meshDecalsParent;

    [SerializeField]
    private AudioSource m_meshGunAudioSource;

    [SerializeField]
    private ShotgunFire m_meshFireWeapon;

    // ZED REALTIME SYSTEMS

    [SerializeField]
    private GameObject m_zedSystemGO;

    [SerializeField]
    private ZEDMaterialRaycast m_zedRayCastSystem;

    [SerializeField]
    private GameObject m_zedParticleParent;

    [SerializeField]
    private GameObject m_zedDecalsParent;

    [SerializeField]
    private AudioSource m_zedGunAudioSource;

    [SerializeField]
    private ShotgunFire m_zedFireWeapon;


    private Vector3 lastHitPos = Vector3.zero;
    private WeaponStruct[] m_weaponDataCopy;

    private Transform particleParent;
    private Transform decalParent;
    private AudioSource gunAudioSource;
    private ShotgunFire shotgunFire;

    // Use this for initialization
    void Start () {
        m_weaponDataCopy = m_dataManager.GetWeaponDataArray();

        m_meshSystemGO.SetActive(false);
        m_zedSystemGO.SetActive(false);
        particleParent = null;
        decalParent = null;
        gunAudioSource = null;
        shotgunFire = null;

        if (m_cameraSystemEnum == ECameraSystem.MeshPreProcessSystem)
        {
            m_meshSystemGO.SetActive(true);
            m_uiTriggerMl.SetActive(false);
            particleParent = m_meshParticleParent.transform;
            decalParent = m_meshDecalsParent.transform;
            gunAudioSource = m_meshGunAudioSource;
            shotgunFire = m_meshFireWeapon;
        }
        else if (m_cameraSystemEnum == ECameraSystem.ZEDRealtimeSystem)
        {
            m_zedSystemGO.SetActive(true);
            particleParent = m_zedParticleParent.transform;
            decalParent = m_zedDecalsParent.transform;
            gunAudioSource = m_zedGunAudioSource;
            shotgunFire = m_zedFireWeapon;
        }
    }
	
	// Update is called once per frame
	void Update () {

        // Fire Ray but check if on UI too
        if (Input.GetMouseButtonDown(0) && !UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
        {
            shotgunFire.FireWeapon();

            if (m_cameraSystemEnum == ECameraSystem.MeshPreProcessSystem)
            {
                MeshPreProcessInteractionRoutine();
            }
            else if (m_cameraSystemEnum == ECameraSystem.ZEDRealtimeSystem)
            {
                ZEDRealTimeInteractionRoutine();
            }
        }
        m_debugHitPoint.position = lastHitPos;
    }

    // brief: Switches between real colour rendering and material colour rendering
    public void ToggleMeshRenderer()
    {
        m_meshRenderer.SetActive(!m_meshRenderer.activeSelf);
        m_zedRayCastSystem.ToggleCameraTexture();
    }

    private void MeshPreProcessInteractionRoutine()
    {
        Ray r = m_meshRenderCamera.ScreenPointToRay(Input.mousePosition);
        APARaycastHit hit;
        MaterialStruct mat;

        if (m_materialRayCastSystem.RayVsSceneMaterial(r, out hit, out mat))
        {
            Vector3 hitWsPosition = hit.point;
            Vector3 hitWsNormal = hit.transform.TransformVector(hit.normal);

            GameReaction(hitWsPosition, hitWsNormal, mat);
        }
        else
        {
            m_uiMaterialColourOutput.color = m_errorColour;
            m_uiMaterialTextOutput.text = "No Data / Ray Missed ";
        }
    }

    private void ZEDRealTimeInteractionRoutine()
    {
        m_zedRayCastSystem.TriggerRayCast();
    }

    public void GameReaction(Vector3 hitWsPosition, Vector3 hitWsNormal, MaterialStruct mat)
    {
        Color finalMaterialColour;
        string finalMaterialName = "";

        hitWsPosition = hitWsPosition + (hitWsNormal * 0.01f);

        if (mat != null)
        {
            // We know the material...so...
            finalMaterialName = mat.m_name;
            finalMaterialColour = mat.m_colour;
        }
        else
        {
            finalMaterialName = "Error";
            finalMaterialColour = m_errorColour;
        }

        m_uiMaterialColourOutput.color = finalMaterialColour;
        m_uiMaterialTextOutput.text = finalMaterialName;

        int matIndex = mat == null ? 0 : mat.m_index;

        lastHitPos = hitWsPosition;

        // Grab Weapon Struct
        int weaponIndex = 0;
        var weaponStruct = m_weaponDataCopy[weaponIndex];

        // Spawn Particle Effects
        {
            // note: uses material index to go into array
            var newParticle = Instantiate(weaponStruct.m_hitParticlePrefabs[matIndex]) as GameObject;
            newParticle.transform.SetParent(particleParent, false);

            // Orientate to normal 
            newParticle.transform.SetPositionAndRotation(hitWsPosition, Quaternion.LookRotation(hitWsNormal, Vector3.up));
        }

        // Spawn Decal Effects
        {
            // note: uses material index to go into array
            var newDecal = Instantiate(weaponStruct.m_hitDecalPrefabs[matIndex]) as GameObject;
            newDecal.transform.SetParent(decalParent, false);

            // Orientate to normal
            newDecal.transform.SetPositionAndRotation(hitWsPosition, Quaternion.LookRotation(hitWsNormal, Vector3.up));
        }

        // Play Sounds
        {
            gunAudioSource.clip = weaponStruct.m_weaponFireSFX;
            gunAudioSource.Play();
        }
    }
}
