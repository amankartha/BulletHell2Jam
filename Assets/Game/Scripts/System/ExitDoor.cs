using System;
using System.Collections;
using System.Collections.Generic;
using BulletFury;
using BulletFury.Data;
using UnityEngine;
using UnityEngine.Events;

public class ExitDoor : MonoBehaviour
{
    public int NumberofEnemiesTOKill = 0;
    public int KillCount = 0;

    public UnityEvent MethodsToCall;
    

    public void FindEnemies()
    {
        Enemy[] enemies = FindObjectsOfType<Enemy>();


        foreach (Enemy enemy in enemies)
        {
            if (enemy.REQUIREDTOBEKILLED)
            {
                NumberofEnemiesTOKill++;
            }
        }
    }


    public void IncreaseKillCounter()
    {
        KillCount++;

        if (KillCount >= NumberofEnemiesTOKill)
        {
            MethodsToCall?.Invoke();
            GameManager.Instance.FadeCalm?.PlayFeedbacks();
        }
    }
}
