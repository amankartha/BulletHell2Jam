using System.Collections;
using System.Collections.Generic;
using BulletFury;
using BulletFury.Modules;
using UnityEngine;

public class AssignTargetMainPlayer : MonoBehaviour
{
    
    
    // Start is called before the first frame update
    void Start()
    {
        foreach (var module in this.GetComponent<BulletSpawner>().GetModulesOfType<AimedShotModule>())
        {
            module.SetTarget(GameManager.Instance.MAINPLAYERGAMEOBJECT.transform);
            module.SetSelf(this.transform);
        }

        foreach (var module in this.GetComponent<BulletSpawner>().GetModulesOfType<TrackObjectModule>())
        {
            module.SetTarget(GameManager.Instance.MAINPLAYERGAMEOBJECT.transform);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
