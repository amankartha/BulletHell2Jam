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


    public override void OnLogic()
    {
        movementVectorContainer =  new Vector2(Input.GetAxis("Horizontal"),Input.GetAxis("Vertical"));
        movementVectorContainer *= _mainPlayer.Speed;
        movementVectorContainer *= Time.deltaTime;
        
        _mainPlayer.gameObject.transform.Translate(movementVectorContainer);
    }

}
