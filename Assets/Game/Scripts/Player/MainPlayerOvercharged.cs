using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityHFSM;

public class MainPlayerOvercharged : StateBase
{

    private MainPlayer _player;
    private Vector2 movementVectorContainer = new Vector2();
    public MainPlayerOvercharged(bool needsExitTime,MainPlayer player ,bool isGhostState = false) : base(needsExitTime, isGhostState)
    {
        _player = player;
    }

    public override void OnEnter()
    {
        base.OnEnter();
        _player.canReflect = false;
    }

    public override void OnLogic()
    {
        _player.Heat -= Time.deltaTime * _player.OverchargeCooldownPerSecond;
        
        movementVectorContainer =  new Vector2(Input.GetAxisRaw("Horizontal"),Input.GetAxisRaw("Vertical"));
        movementVectorContainer *= _player.OverheatedSpeed;
        movementVectorContainer *= Time.deltaTime;
        
        _player.gameObject.transform.Translate(movementVectorContainer);
    }

    public override void OnExit()
    {
        base.OnExit();
        _player.canReflect = true;
    }
}
