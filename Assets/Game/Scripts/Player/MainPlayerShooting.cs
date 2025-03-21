using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityHFSM;

public class MainPlayerShooting : StateBase
{
    private MainPlayer _player;
    
   
    private float TimerCounter = 0f;
    
    public MainPlayerShooting(bool needsExitTime,MainPlayer player, bool isGhostState = false) : base(needsExitTime, isGhostState)
    {
        _player = player;
    }
    
    
    

    public override void OnEnter()
    {
        base.OnEnter();
        TimerCounter = 0f;
        _player.armBulletSpawner.gameObject.GetComponent<SpriteRenderer>().enabled = true;
        _player.Anim.SetBool("Dump",true);
        _player.armBulletSpawner.Play();
        _player.canReflect = false;
        _player.teleported = false;
    }

    public override void OnLogic()
    {



        _player.Heat -= _player.ShootingDrainPerSecond * Time.deltaTime;

    }

    public override void OnExit()
    {
        base.OnExit();
        _player.canReflect = true;
        _player.armBulletSpawner.gameObject.GetComponent<SpriteRenderer>().enabled = false;
        _player.Anim.SetBool("Dump",false);
        _player.armBulletSpawner.Stop();
    }

   
}
