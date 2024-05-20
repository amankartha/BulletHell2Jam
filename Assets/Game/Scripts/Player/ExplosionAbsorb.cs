using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosionAbsorb : MonoBehaviour
{
    // Start is called before the first frame update
    public float speed= 10;
    public Animator _animator;
    private bool move = false;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (move)
        {
            transform.position +=
                (GameManager.Instance.MAINPLAYERGAMEOBJECT.transform.position - this.transform.position) * (Time.deltaTime * speed);
        }
    }

    private void OnEnable()
    {
        _animator.SetTrigger("Absorb" );
        move = false;
    }

    public void AnimationEvent()
    {
        move = true;
    }
}
