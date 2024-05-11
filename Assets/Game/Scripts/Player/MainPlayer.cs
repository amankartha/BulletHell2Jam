using System.Collections;
using System.Collections.Generic;
using BulletFury;
using BulletFury.Data;
using Unity.Mathematics;
using UnityEngine;
using UnityHFSM;

public class MainPlayer : MonoBehaviour
{
    #region Variables

    [Header("Costs")] 
    public int TeleportCost = 20;
    public int RelectHeatChargeUp = 5;
    
    [SerializeField]
    private int _health;
    [SerializeField]
    private int _maxHealth;
    
    [SerializeField]
    private int _heat;
    [SerializeField]
    private int _maxHeat;

    [Header("CONNECTIONS")] 
    public BulletCollider ArmCollider;
    
    private StateMachine fsm;
    private Camera mainCamera;
    private int Health {
        get => _health;
        set
        {
            _health = math.clamp(value, 0, _maxHealth);
        }
    }
    private int MaxHealth { get; set; }

    private int Heat
    {
        get
        {
            return _heat;
        }
        set
        {
            _heat = math.clamp(value, 0, _maxHeat);
        }
    }

    #endregion


    // Start is called before the first frame update
    void Start()
    {
        mainCamera = Camera.main;
        fsm = new StateMachine();
        fsm.AddState("Melee",new MainPlayerMovement(false,this));
        fsm.AddState("Shooting", new MainPlayerShooting(false,this));
        fsm.AddState("OverCharged", new MainPlayerOvercharged(false,this));
        
        
        
        fsm.SetStartState("Melee");
        fsm.Init();
        
        //TODO: REMOVE THIS
        Heat = 50;
    }

    // Update is called once per frame
    void Update()
    {
        fsm.OnLogic();
        
        
        //teleport logic
        Teleport();
        
    }

    public void Oncollide(BulletContainer Bcontainer, BulletCollider Bcollider)
    {
        Health -= (int)Bcontainer.Damage;
    }

    public void OnCollideArm(BulletContainer bulletContainer, BulletCollider bulletCollider)
    {
        Heat += RelectHeatChargeUp ;
    }

    public bool TryConsumeHeat(int value)
    {
        if (Heat >= value)
        {
            Heat -= value;
            return true;
        }
        else
        {
            return false;
        }
    }

    private void Teleport()
    {
        if (Input.GetKeyDown(KeyCode.Space) && TryConsumeHeat(TeleportCost) )
        {
            TeleportSequence();    
        }
    }

    private void TeleportSequence()
    {
        transform.position = (Vector2)mainCamera.ScreenToWorldPoint(Input.mousePosition);
    }
}
