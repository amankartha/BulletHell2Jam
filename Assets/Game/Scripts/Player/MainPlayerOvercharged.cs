using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityHFSM;

public class MainPlayerOvercharged : StateBase
{

    private MainPlayer _player;
    
    public MainPlayerOvercharged(bool needsExitTime,MainPlayer player ,bool isGhostState = false) : base(needsExitTime, isGhostState)
    {
        _player = player;
    }

    public override void OnLogic()
    {
        base.OnLogic();
        
    }
}
