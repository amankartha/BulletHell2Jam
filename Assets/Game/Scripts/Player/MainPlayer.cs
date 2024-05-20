using System;
using System.Collections;
using System.Collections.Generic;
using BulletFury;
using BulletFury.Data;
using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityHFSM;

public class MainPlayer : MonoBehaviour, IBulletHitHandler
{
    #region Variables

    [Header("Costs")] 
    public int TeleportCost = 20;
    public int ReflectHeatChargeUp = 5;
    public int ShootThreshold = 60;
    public float ShootingDrainPerSecond = 10f;
    public float OverchargeCooldownPerSecond = 10f;
    [SerializeField]    
    private int _health;
    [SerializeField]
    private int _maxHealth;
    
    [SerializeField]
    private float _heat;
    [SerializeField]
    private int _maxHeat;

    public float Speed = 5f;
    public float OverheatedSpeed = 2f;
    
    private StateMachine fsm;
    private Camera mainCamera;

    public Animator Anim;


    #region MMF stuff

    [SerializeField] private MMProgressBar _heatBar;
    [SerializeField] private MMProgressBar _healthBar;
    public MMF_Player OverChargedFeedbacks;
    public MMF_Player OverChargedFeedbacksEnd;
    public MMF_Player DeathFeedbacks;
    public MMF_Player TeleportFeedbacks;
    
    
   

    #endregion
    
    public int Health {
        get => _health;
        set
        {
            _health = math.clamp(value, 0, _maxHealth);
            _healthBar.UpdateBar01((float)_health/_maxHealth);
            if (_health == 0)
            {
                Respawn();
            }
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
            _heatBar.UpdateBar(_heat,0,_maxHeat);
        }
    }

    public bool canCharge = true;
    public bool canReflect = true;
    public bool canTakeDamage = true;
    public bool teleported = false;
    #endregion

    #region Events


    public UnityEvent OnPlayerHit;
    
    #endregion

    #region Links

    [Header("TO BE LINKED")] 
    public BulletSpawner armBulletSpawner;

    public SpriteRenderer Renderer;

    public GameObject TeleportText;

    #endregion
    private void Awake()
    {
          GameManager.Instance.MAINPLAYERGAMEOBJECT = this.gameObject;
          GameManager.Instance.MAINPLAYERSCRIPT = this;
          
          DontDestroyOnLoad(gameObject);
          SceneManager.sceneLoaded += SetPlayerPositionToSpawnPoint;
    }

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
        fsm.AddTransition(new Transition("Shooting","Melee",transition => teleported));

        fsm.AddTransition(new Transition("Melee","OverCharged",transition => TransitionToOverheated()));
        fsm.AddTransition(new Transition("OverCharged","Melee",transition => TransitionFromOverheated()));
        
        

        fsm.SetStartState("Melee");
        fsm.Init();

        armBulletSpawner.Stop();

    }

    // Update is called once per frame
    void Update()
    {
        fsm.OnLogic();
        
        TeleportText.SetActive(canTeleport && Heat>TeleportCost);
        
        //teleport logic
        Teleport();
        
    }

    private void FixedUpdate()
    {
        
    }

    private void Respawn()
    {
        DeathFeedbacks?.PlayFeedbacks();
        GameManager.Instance.FadeCalm?.PlayFeedbacks();
        Health = _maxHealth;
        Heat = 0f;
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
    public bool canTeleport = true;

    private void Teleport()
    {
        if (canTeleport && Input.GetKeyDown(KeyCode.Space) &&  GameManager.Instance.CheckIfInBounds((Vector2)mainCamera.ScreenToWorldPoint(Input.mousePosition)) && TryConsumeHeat(TeleportCost))
        {
            TeleportSequence();    
        }
    }

    private void TeleportSequence()
    {
        TeleportFeedbacks?.PlayFeedbacks();
        teleported = true;
        transform.position = (Vector2)mainCamera.ScreenToWorldPoint(Input.mousePosition);
        StartCoroutine(TeleportCoolDownCounter());
    }

    public string GetCurrentState()
    {
        return fsm.ActiveStateName;
    }

    public void SetOvercharging()
    {
        OverChargedFeedbacks?.PlayFeedbacks();
    }

    public void SetOverChargingOff()
    {
        
        OverChargedFeedbacks?.StopFeedbacks();
        OverChargedFeedbacksEnd?.PlayFeedbacks();
       
    }
    
    #region TransitionFunctions

    private bool TransitionToShooting()
    {
        if (Heat > ShootThreshold && Input.GetKeyDown(KeyCode.Mouse1))
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

    private bool TransitionToOverheated()
    {
        if (Math.Abs(Heat - _maxHeat) < 0.1)
        {
            return true;
        }

        return false;
    }

    private bool TransitionFromOverheated()
    {
        if (Heat == 0)
        {
            return true;
        }

        return false;
    }
    
    private void SetPlayerPositionToSpawnPoint(Scene scene, LoadSceneMode mode)
    {
        SpawnPosition spawnPos = FindObjectOfType<SpawnPosition>();
       

        if (spawnPos != null)
        {
            transform.position = spawnPos.transform.position;
        }
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

    public void SetCanHit()
    {
        canTakeDamage = true;
    }

    public void Hit(BulletContainer bullet)
    {
        if (canTakeDamage)
        {
            Health -= (int)bullet.Damage;
            OnPlayerHit?.Invoke();
            canTakeDamage = false;
        }
    }
}
