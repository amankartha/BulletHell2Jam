using System.Collections.Generic;
using UnityEngine;

namespace BulletFury
{
    [CreateAssetMenu(menuName = "Bulletfury/Spawner Preset")]
    public class BulletSpawnerPreset : ScriptableObject
    { 
        public bool UseMain;
        public BulletMainData Main;
        public bool UseShape;
        public SpawnShapeData ShapeData;
        public bool UseBurstData;
        public BurstData BurstData;
        public bool UseSubSpawners;
        public SubSpawnerData[] SubSpawners;

        public bool UseModules;
        
#if SERIALIZEREFERENCE_EXTENSIONS
        [SerializeReference, SubclassSelector]
#endif
        public List<IBaseBulletModule> BulletModules = new ();
    }
}