using System;
using System.Collections;
using System.Collections.Generic;
using BulletFury;
using BulletFury.Data;
using UnityEngine;

public class MainPlayerReflector : MonoBehaviour,IBulletHitHandler
{

    [SerializeField] private MainPlayer _player;

    [SerializeField] private float Cooldown = 1.0f;

    [SerializeField] private float HeatGained = 10f;

    [SerializeField] private BoxCollider2D _reflectCollider;

    [SerializeField] private float _reflectTimerDuration = 0.4f;
  

    public void Hit(BulletContainer bullet)
    {
        if(_reflectCollider.enabled) _player.Heat += HeatGained;
    }

    private void Update()
    {
        if (_player.canReflect && Input.GetKeyDown(KeyCode.Mouse0))
        {
            _player.canReflect = false;
            _reflectCollider.enabled = true;
            _player.Anim.SetTrigger("Attack");
            StartCoroutine(CoolDown());
            Invoke(nameof(TurnOffCollider),_reflectTimerDuration);
        }
    }


    private IEnumerator CoolDown()
    {
        float Timer = Cooldown;

        while (Timer>= 0)
        {
            Timer -= Time.deltaTime;
            yield return null;
        }

        _player.canReflect = true;
    }

    private void TurnOffCollider()
    {
        _reflectCollider.enabled = false;
        
    }
}
