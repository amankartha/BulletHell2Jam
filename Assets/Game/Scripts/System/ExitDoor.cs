using System.Collections;
using System.Collections.Generic;
using BulletFury;
using BulletFury.Data;
using UnityEngine;

public class ExitDoor : MonoBehaviour, IBulletHitHandler
{
    private float health = 100f;

    public GameObject nextRoom;

    public GameObject[] EnemiesToDespawn;
    
    public void Hit(BulletContainer bullet)
    {
        health -= bullet.Damage;

        if (health <= 0)
        {
            ChargeDoor();
        }
    }

    private void ChargeDoor()
    {
        DespawnEnemies();
    }

    private void DespawnEnemies()
    {
        foreach (var enemies in EnemiesToDespawn)
        {
            enemies.SetActive(false);
        }   
    }
}
