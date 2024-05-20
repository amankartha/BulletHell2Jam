using System;
using System.Collections;
using System.Collections.Generic;
using BulletFury;
using BulletFury.Data;
using MoreMountains.Feedbacks;
using Unity.Mathematics;
using UnityEngine;

public class MainPlayerReflector : MonoBehaviour,IBulletHitHandler
{

    [SerializeField] private MainPlayer _player;

    [SerializeField] private float Cooldown = 1.0f;

    [SerializeField] private float HeatGained = 10f;

    [SerializeField] private Collider2D _reflectCollider;

    [SerializeField] private float _reflectTimerDuration = 0.4f;

    [SerializeField] private GameObject ExplosionVfx;
    
    private MMF_Player audioFeedbacks;

    public MMMiniObjectPooler _pooler;


    private void Start()
    {
        audioFeedbacks = GetComponent<MMF_Player>();
    }

    public void Hit(BulletContainer bullet)
    {
        if (_reflectCollider.enabled)
        {
            _player.Heat += bullet.Damage;
            var p = _pooler.GetPooledGameObject();
                p.transform.position = bullet.Position;
                p.SetActive(true);
        }
    }

    private void Update()
    {
        if (_player.canReflect&&_player.GetCurrentState() == "Melee" && Input.GetKeyDown(KeyCode.Mouse0))
        {
            _player.canReflect = false;
            audioFeedbacks?.PlayFeedbacks();
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
            if(_player.GetCurrentState() == "Melee") _player.canReflect = true;
    }

    private void TurnOffCollider()
    {
        _reflectCollider.enabled = false;
        
    }
}
