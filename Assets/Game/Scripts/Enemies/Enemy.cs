using System;
using System.Collections;
using System.Collections.Generic;
using BulletFury;
using BulletFury.Data;
using DG.Tweening;
using MoreMountains.Feedbacks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class Enemy : MonoBehaviour, IBulletHitHandler
{
    #region Variables

    private BulletSpawner _bulletSpawner;
    private DOTweenPath _doTweenPath;

    public float _health = 10f;
    public float MaxHealth = 10f;

    public bool canRespawnAfterDelay = false;
    public bool isRobot = false;    
    
    public float delay = 10f;
    public GameObject Child;
    public Collider2D colliderToDisable;

    public float PunchDamage = 1f;


    public MMF_Player RobotDown;
    public MMF_Player RobotUp;

    public bool REQUIREDTOBEKILLED = false;
    #endregion"

    #region Properties

    public float Health
    {
        get
        {
            return _health;
        }
        set
        {
            _health = value;
            if (_health <= 0)
            {
                _health = 0;
                Death();
            }
        }
    }

    #endregion

    #region Events

    public UnityEvent OnTakeDamage;

    #endregion
    
    // Start is called before the first frame update
    void Start()
    {
        _bulletSpawner = this.GetComponentInChildren<BulletSpawner>();
        _doTweenPath = this.GetComponent<DOTweenPath>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

 

    protected void Death()
    {
        if (canRespawnAfterDelay)
        {
            if (isRobot)
            {
                DisableRobot();
                Invoke(nameof(EnableRobot),delay);
            }
            else
            {
                _bulletSpawner.CancelAll();
                Child.SetActive(false);
                colliderToDisable.enabled = false;
                Invoke(nameof(Respawn),delay);
            }
            
        }
        else
        {
            if (REQUIREDTOBEKILLED)
            {
                FindObjectOfType<ExitDoor>().IncreaseKillCounter();
            }
            this.gameObject.SetActive(false);
        }
    }

    protected void Respawn()
    {
        Child.SetActive(true);
        colliderToDisable.enabled = true;
    }
    

    public void Hit(BulletContainer bullet)
    {
        Health -= bullet.Damage;
        OnTakeDamage?.Invoke();
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject == GameManager.Instance.MAINPLAYERGAMEOBJECT)
        {
            Health -= PunchDamage;
        }
    }

    private void DisableRobot()
    {
        RobotDown?.PlayFeedbacks();
    }

    private void EnableRobot()
    {
        RobotUp?.PlayFeedbacks();
    }
    
}
