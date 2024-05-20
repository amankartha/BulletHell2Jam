using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityHFSM;

public class MainPlayerOvercharged : StateBase
{

    private MainPlayer _player;
    private Vector2 movementVectorContainer = new Vector2();
    private Vector3 previous;
    public MainPlayerOvercharged(bool needsExitTime,MainPlayer player ,bool isGhostState = false) : base(needsExitTime, isGhostState)
    {
        _player = player;
    }

    public override void OnEnter()
    {
        base.OnEnter();
        _player.canReflect = false;
        _player.canTeleport = false;
        _player.SetOvercharging();
    }

    public override void OnLogic()
    {
        
        _player.Heat -= Time.deltaTime * _player.OverchargeCooldownPerSecond;
        
        movementVectorContainer =  new Vector2(Input.GetAxisRaw("Horizontal"),Input.GetAxisRaw("Vertical"));
        movementVectorContainer *= _player.OverheatedSpeed;
        movementVectorContainer *= Time.deltaTime;
        
        _player.gameObject.transform.Translate(movementVectorContainer);
        
        float velocity = ((_player.transform.position - previous).magnitude) / Time.deltaTime;
        previous = _player.transform.position;
        
        if(movementVectorContainer.x != 0) _player.Renderer.flipX = movementVectorContainer.x < 0;
        _player.Anim.SetBool("Walk",  math.abs(velocity) > 0);
    }

    public override void OnExit()
    {
        base.OnExit();
        _player.canReflect = true;
        _player.canTeleport = true;
        _player.SetOverChargingOff();
    }
}
