using UnityEngine;
using System.Collections;

public class ParticleSystemAutoDestroy : MonoBehaviour
{
    // Use this for initialization
    void Start()
    {
        var ps = gameObject.GetComponentsInChildren<ParticleSystem>();

        if (ps != null)
        {
            float maxTime = 0.0f;
            for (int i = 0; i < ps.Length; i++)
            {
                float dur = ps[i].main.duration;
                if (dur > maxTime)
                {
                    maxTime = dur;
                }

                GameObject.Destroy(ps[i].gameObject, dur);
            }
            GameObject.Destroy(gameObject, maxTime);           
        }
    }
}