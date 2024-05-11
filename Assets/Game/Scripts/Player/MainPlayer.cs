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
    public int ShootThreshold = 60;
    public int ShootingDrainPerSecond = 10;
    
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
    public int Health {
        get => _health;
        set
        {
            _health = math.clamp(value, 0, _maxHealth);
        }
    }
    public int MaxHealth { get;private set; }

    public int Heat
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
        
        
        
        fsm.AddTransition(new Transition("Melee","Shooting",transition => TransitionToShooting()));
        fsm.AddTransition(new Transition("Shooting","Melee",transition => TransitionFromShooting()));


        fsm.SetStartState("Melee");
        fsm.Init();
      
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

    #region TransitionFunctions

    private bool TransitionToShooting()
    {
        if (Heat > ShootThreshold)
        {
            return true;
        }

        return false;
    }

    private bool TransitionFromShooting()
    {
        if (Heat == 0)
        {
            return true;
        }

        return false;
    }

    #endregion
}
