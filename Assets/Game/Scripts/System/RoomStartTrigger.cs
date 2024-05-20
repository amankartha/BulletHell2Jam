using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class RoomStartTrigger : MonoBehaviour,ITrigger
{
    public UnityEvent MethodstoCall;

    public GameObject[] EnemiesToSpawn;
    
    public void ActivateTrigger()
    {
        MethodstoCall?.Invoke();
        foreach (var Enemy in EnemiesToSpawn)
        {
            Enemy.SetActive(true);
        }
    }
}
