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
    }

    public override void OnLogic()
    {
        base.OnLogic();
        TimerCounter += Time.deltaTime;

        if (TimerCounter >= 1f)
        {
            TimerCounter = 0f;
            _player.Heat -= _player.ShootingDrainPerSecond;
        }
    }

    public override void OnExit()
    {
        base.OnExit();
    }

   
}
