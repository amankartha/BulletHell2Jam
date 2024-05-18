using System.Collections;
using System.Collections.Generic;
using BulletFury;
using Unity.Mathematics;
using UnityEngine;
using UnityHFSM;

public class MainPlayerMovement : StateBase
{

 
    
    private Vector2 movementVectorContainer = new Vector2();

    private MainPlayer _mainPlayer;
    
    private Vector3 previous;
    private float velocity;
    
    public MainPlayerMovement(bool needsExitTime,MainPlayer player, bool isGhostState = false) : base(needsExitTime, isGhostState)
    {
        _mainPlayer = player;
    }

    public override void OnEnter()
    {
        base.OnEnter();
        _mainPlayer.canCharge = true;
        _mainPlayer.canReflect = true;
    }

    public override void OnLogic()
    {
        movementVectorContainer =  new Vector2(Input.GetAxisRaw("Horizontal"),Input.GetAxisRaw("Vertical"));
        movementVectorContainer *= _mainPlayer.Speed;
        movementVectorContainer *= Time.deltaTime;
        
        _mainPlayer.gameObject.transform.Translate(movementVectorContainer);
        
        if(movementVectorContainer.x != 0) _mainPlayer.Renderer.flipX = movementVectorContainer.x < 0;
        
        velocity = ((_mainPlayer.transform.position - previous).magnitude) / Time.deltaTime;
        previous = _mainPlayer.transform.position;
        
       
        _mainPlayer.Anim.SetBool("Walk",  math.abs(velocity) > 0);
        
        
    }

    public override void OnExit()
    {
        base.OnExit();
        _mainPlayer.canCharge = false;
        _mainPlayer.canReflect = false;
       
    }
}
