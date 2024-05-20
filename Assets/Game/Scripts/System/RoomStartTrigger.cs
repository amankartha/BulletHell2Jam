using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class RoomStartTrigger : MonoBehaviour,ITrigger
{
    public UnityEvent MethodstoCall;

    public GameObject[] EnemiesToSpawn;

    public ExitDoor _ExitDoor;
    
    public void ActivateTrigger()
    {
        GameManager.Instance.FadeFighting?.PlayFeedbacks();
        MethodstoCall?.Invoke();
        foreach (var Enemy in EnemiesToSpawn)
        {
            Enemy.SetActive(true);
            _ExitDoor.FindEnemies();
        }
        gameObject.SetActive(false);
    }
}
