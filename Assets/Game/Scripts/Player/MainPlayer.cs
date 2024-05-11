using System.Collections;
using System.Collections.Generic;
using BulletFury;
using BulletFury.Data;
using Unity.Mathematics;
using UnityEngine;

public class MainPlayer : MonoBehaviour
{
    #region Variables

    private int _health;
    private int _maxHealth;
    
    
    private int _heat;
    private int _maxHeat;
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
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Oncollide(BulletContainer Bcontainer, BulletCollider Bcollider)
    {
        Health -= (int)Bcontainer.Damage;
    }
}
