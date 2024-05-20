using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MMPersistentSingleton<GameManager>
{
    public CompositeCollider2D CurrentRoomCollider;

    public GameObject MAINPLAYERGAMEOBJECT;
    public MainPlayer MAINPLAYERSCRIPT;

    public MMF_Player StartCalm;
    public MMF_Player StartFighting;
    public MMF_Player FadeCalm;
    public MMF_Player FadeFighting;

    public CinemachineVirtualCamera _virtualCamera;
    private void OnEnable()
    {
        SceneManager.sceneLoaded += GetNewCollider;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= GetNewCollider;
    }

    public void SetRoomCollider(GameObject GO)
    {
        GO.GetComponentInChildren<CompositeCollider2D>();
    }
    public bool CheckIfInBounds(Vector3 point)
    {
        return CurrentRoomCollider.OverlapPoint(point);
    }
    
    public void GetNewCollider(Scene scene,LoadSceneMode load)
    {
        CurrentRoomCollider = FindObjectOfType<THISCOLLIDER>().GetComponent<CompositeCollider2D>();
    }

    public void ChangeCamera(float size)
    {
        _virtualCamera.m_Lens.OrthographicSize = size;
    }
}
