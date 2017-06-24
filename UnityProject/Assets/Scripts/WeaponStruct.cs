using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct WeaponStruct {

    public string m_name;
    public string m_soundPath;
    public List<string> m_hitParticlePaths;

    public List<GameObject> m_prefabHitParticles;

    public WeaponStruct(
        string name,
        string soundPath,
        List<string> particleHitPaths

        )
    {
        m_name = name;
        m_soundPath = soundPath;
        m_hitParticlePaths = particleHitPaths;

        m_prefabHitParticles = new List<GameObject>();

        // Load all the prefabs up!
        // todo: this isn't the most efficient way of doing this
        // as we could be calling resource load on same elements for different weapons
        // but whatever

        for (int i = 0; i < particleHitPaths.Count; i++)
        {
            var prefabGo = Resources.Load(particleHitPaths[i]) as GameObject;
            m_prefabHitParticles.Add(prefabGo);
        }
    }

    //public string SOUND_PATH_ACTIVATE_WEAPON;

    //public string PARTICLE_PATH_HIT_BRICK;
    //public string PARTICLE_PATH_HIT_CARPET;
    //public string PARTICLE_PATH_HIT_CERAMIC;
    //public string PARTICLE_PATH_HIT_FABRIC;
    //public string PARTICLE_PATH_HIT_FOLIAGE;
    //public string PARTICLE_PATH_HIT_FOOD;
    //public string PARTICLE_PATH_HIT_GLASS;
    //public string PARTICLE_PATH_HIT_HAIR;
    //public string PARTICLE_PATH_HIT_LEATHER;
    //public string PARTICLE_PATH_HIT_METAL;
    //public string PARTICLE_PATH_HIT_MIRROR;
    //public string PARTICLE_PATH_HIT_OTHER;
    //public string PARTICLE_PATH_HIT_PAINTED;
    //public string PARTICLE_PATH_HIT_PAPER;
    //public string PARTICLE_PATH_HIT_PLASTIC;
    //public string PARTICLE_PATH_HIT_POLISHEDSTONE;
    //public string PARTICLE_PATH_HIT_SKIN;
    //public string PARTICLE_PATH_HIT_SKY;
    //public string PARTICLE_PATH_HIT_STONE;
    //public string PARTICLE_PATH_HIT_TILE;
    //public string PARTICLE_PATH_HIT_WALLPAPER;
    //public string PARTICLE_PATH_HIT_WATER;
    //public string PARTICLE_PATH_HIT_WOOD;
}
