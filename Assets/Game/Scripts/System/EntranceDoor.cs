using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class EntranceDoor : MonoBehaviour
{
    private DOTweenAnimation[] _doTweenAnimationList;
    private void Start()
    {
        _doTweenAnimationList = this.GetComponentsInChildren<DOTweenAnimation>();
    }

    public void PlayAnimation()
    {
        foreach (var doTween in _doTweenAnimationList)
        {
            doTween.DOPlay();
        }
        
    }

    
}
