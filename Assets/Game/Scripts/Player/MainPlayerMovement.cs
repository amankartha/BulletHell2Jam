using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityHFSM;

public class MainPlayerMovement : StateBase
{

 
    
    private Vector2 movementVectorContainer = new Vector2();

    private MainPlayer _mainPlayer;
    public MainPlayerMovement(bool needsExitTime,MainPlayer player, bool isGhostState = false) : base(needsExitTime, isGhostState)
    {
        _mainPlayer = player;
    }

    public override void OnEnter()
    {
        base.OnEnter();
        _mainPlayer.canCharge = true;
    }

    public override void OnLogic()
    {
        movementVectorContainer =  new Vector2(Input.GetAxisRaw("Horizontal"),Input.GetAxisRaw("Vertical"));
        movementVectorContainer *= _mainPlayer.Speed;
        movementVectorContainer *= Time.deltaTime;
        
        _mainPlayer.gameObject.transform.Translate(movementVectorContainer);
    }

    public override void OnExit()
    {
        base.OnExit();
        _mainPlayer.canCharge = false;
    }
}
