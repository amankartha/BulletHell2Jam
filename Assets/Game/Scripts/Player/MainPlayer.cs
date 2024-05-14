using System;
using System.Collections;
using System.Collections.Generic;
using BulletFury;
using BulletFury.Data;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Events;
using UnityHFSM;

public class MainPlayer : MonoBehaviour, IBulletHitHandler
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
    private float _heat;
    [SerializeField]
    private int _maxHeat;

  
    
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

    public float Heat
    {
        get
        {
            return _heat;
        }
        set
        {
            _heat = Mathf.Clamp(value, 0, _maxHeat);
        }
    }

    #endregion

    #region Events


    public UnityEvent OnPlayerHit;

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

        GameManager.Instance.MAINPLAYERGAMEOBJECT = this.gameObject;
        GameManager.Instance.MAINPLAYERSCRIPT = this;

    }

    // Update is called once per frame
    void Update()
    {
        fsm.OnLogic();
        
        
        //teleport logic
        Teleport();
        
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

    public float TeleportCooldown = 2.0f;
    private bool canTeleport = true;

    private void Teleport()
    {
        if (canTeleport && Input.GetKeyDown(KeyCode.Space) && TryConsumeHeat(TeleportCost) && GameManager.Instance.CheckIfInBounds((Vector2)mainCamera.ScreenToWorldPoint(Input.mousePosition)))
        {
            TeleportSequence();    
        }
    }

    private void TeleportSequence()
    {
        transform.position = (Vector2)mainCamera.ScreenToWorldPoint(Input.mousePosition);
        StartCoroutine(TeleportCoolDownCounter());
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

    #region COLLISION

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.layer == 3)
        {
            if (col.TryGetComponent(out ITrigger trigger))
            {
                trigger.ActivateTrigger();
            }
        }
    }

    #endregion

    #region Coroutines

    IEnumerator TeleportCoolDownCounter()
    {
        canTeleport = false;
        float counter = TeleportCooldown;

        while (counter > 0)
        {
            counter -= Time.deltaTime;
            yield return null;
        }

        canTeleport = true;
    }

    #endregion

    public void Hit(BulletContainer bullet)
    {
        Health -= (int)bullet.Damage;
        OnPlayerHit?.Invoke();
    }
}
