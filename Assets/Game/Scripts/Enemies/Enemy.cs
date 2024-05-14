using System.Collections;
using System.Collections.Generic;
using BulletFury;
using BulletFury.Data;
using DG.Tweening;
using UnityEngine;

public class Enemy : MonoBehaviour, IBulletHitHandler
{
    #region Variables

    private BulletSpawner _bulletSpawner;
    private DOTweenPath _doTweenPath;

    private float _health = 10f;
    public float MaxHealth = 10f;
    
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
        this.gameObject.SetActive(false);
    }

    protected void Respawn()
    {
        
    }
    

    public void Hit(BulletContainer bullet)
    {
        Health -= bullet.Damage;
    }
}
