using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChargingPad : MonoBehaviour
{
    [SerializeField] private float HeatChargePerSecond = 5f;
    
   
    //TODO: only add charge when in movingState
    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.gameObject == GameManager.Instance.MAINPLAYERGAMEOBJECT)
        {
            GameManager.Instance.MAINPLAYERSCRIPT.Heat += 5 * Time.fixedDeltaTime;
        }
    }
}
