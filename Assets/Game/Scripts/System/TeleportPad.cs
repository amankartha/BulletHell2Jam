using System;
using System.Collections;
using System.Collections.Generic;
using MoreMountains.Feedbacks;
using UnityEngine;

public class TeleportPad : MonoBehaviour
{

    [SerializeField] private MMF_Player teleportFeedback;

    [SerializeField] private bool ChangeHub = false;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject == GameManager.Instance.MAINPLAYERGAMEOBJECT)
        {
            if (ChangeHub)
            {
                GameManager.Instance.MAINPLAYERSCRIPT.ChangeRespawn("Room_6 Final");
            }
            teleportFeedback?.PlayFeedbacks();
        }
    }
}
