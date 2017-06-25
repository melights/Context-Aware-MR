using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct WeaponStruct {

    // todo: weapon struct could contain info for how long to wait between each shot etc?

    public string m_name;
    public List<GameObject> m_hitParticlePrefabs;
    public AudioClip m_weaponFireSFX;

    public WeaponStruct(
        string name,
        string soundPath,
        List<string> particleHitPaths
        )
    {
        m_name = name;
        m_hitParticlePrefabs = new List<GameObject>();

        // Load all the prefabs up!
        // todo: this isn't the most efficient way of doing this
        // as we could be calling resource load on same elements for different weapons
        // but whatever
        for (int i = 0; i < particleHitPaths.Count; i++)
        {
            var prefabGo = Resources.Load(particleHitPaths[i]) as GameObject;
            m_hitParticlePrefabs.Add(prefabGo);
        }

        // Load SFX
        m_weaponFireSFX = Resources.Load(soundPath) as AudioClip;
    }
}
