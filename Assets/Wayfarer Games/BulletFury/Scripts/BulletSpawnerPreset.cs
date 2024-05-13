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
        public bool UseInitModules;
        public bool UseSpawnModules;
        
        [SerializeReference]
#if SERIALIZEREFERENCE_EXTENSIONS
        [SubclassSelector]
#endif
        public List<IBulletModule> BulletModules = new ();
        [SerializeReference]
#if SERIALIZEREFERENCE_EXTENSIONS
        [SubclassSelector]
#endif
        public List<IBulletInitModule> BulletInitModules = new ();
        [SerializeReference]
#if SERIALIZEREFERENCE_EXTENSIONS
        [SubclassSelector]
#endif
        public List<IBulletSpawnModule> SpawnModules = new ();
    }
}