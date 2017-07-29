using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThrowableObject : MonoBehaviour {

    [SerializeField]
    private GameObject m_explosionParticle;

    [SerializeField]
    private List<AudioClip> m_brokenSoundFX;

    bool m_broken = false;

    private GameController m_gameController;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void SetInformation(GameController gc)
    {
        m_gameController = gc;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.layer == 8) // collider layer
        {
            Debug.Log("Hit Raycast Layer!");

            var contacts = collision.contacts;

            Vector3 pos = contacts[0].point;
            Vector3 nrm = contacts[0].normal;

            // we need to fire a ray from contact point into mesh to allow
            // for material reaction
            Ray r = new Ray(pos + (nrm * 0.01f), -nrm);

            APARaycastHit hit;
            MaterialStruct mat;

            if (MaterialRayCastSystem.RayVsSceneMaterial(r, out hit, out mat))
            {
                // Depending on material it should maybe shatter?
                if (mat.m_breakOnImpact == 1)
                {
                    // Spawn sounds yey
                    {
                        GameObject soundSpawn = new GameObject();
                        soundSpawn.transform.position = transform.position;
                        var ac = soundSpawn.AddComponent<AudioSource>();

                        int randIndex = Random.Range(0, m_brokenSoundFX.Count);
                        ac.clip = m_brokenSoundFX[randIndex];
                        ac.Play();
                    }

                    // Trigger Game Reaction
                    Vector3 hitWsPosition = hit.point;
                    Vector3 hitWsNormal = hit.transform.TransformVector(hit.normal);

                    // Shatter
                    var particle = Instantiate(m_explosionParticle) as GameObject;
                    particle.transform.SetPositionAndRotation(hitWsPosition, Quaternion.LookRotation(hitWsNormal, Vector3.up));
                    
                    m_gameController.GameReaction(hitWsPosition, hitWsNormal, mat);

                    Destroy(gameObject);

                }




            }
        }
    }
}
