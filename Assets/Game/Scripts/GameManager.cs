using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;
using UnityEngine;

public class GameManager : MMPersistentSingleton<GameManager>
{
    public CompositeCollider2D CurrentRoomCollider;

    public GameObject MAINPLAYERGAMEOBJECT;
    public MainPlayer MAINPLAYERSCRIPT;

    public void SetRoomCollider(GameObject GO)
    {
        GO.GetComponentInChildren<CompositeCollider2D>();
    }
    public bool CheckIfInBounds(Vector3 point)
    {
        return CurrentRoomCollider.bounds.Contains(point);
    }
}
