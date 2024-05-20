using System;
using System.Collections;
using System.Collections.Generic;
using MoreMountains.Feedbacks;
using UnityEngine;

public class ChargingPad : MonoBehaviour
{
    [SerializeField] private float HeatChargePerSecond = 5f;
    private ParticleSystem PS;
    public MMF_Player feel;
    private void Start()
    {
        PS = this.gameObject.GetComponentInChildren<ParticleSystem>();
    }


    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject == GameManager.Instance.MAINPLAYERGAMEOBJECT && GameManager.Instance.MAINPLAYERSCRIPT.canCharge)
        {
            PS.Play();
            feel?.PlayFeedbacks();
        }
    }

    //TODO: only add charge when in movingState
    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.gameObject == GameManager.Instance.MAINPLAYERGAMEOBJECT && GameManager.Instance.MAINPLAYERSCRIPT.canCharge)
        {
            GameManager.Instance.MAINPLAYERSCRIPT.Heat += HeatChargePerSecond * Time.fixedDeltaTime;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject == GameManager.Instance.MAINPLAYERGAMEOBJECT)
        {
            PS.Stop();
            feel?.StopFeedbacks();
        }
    }
}
