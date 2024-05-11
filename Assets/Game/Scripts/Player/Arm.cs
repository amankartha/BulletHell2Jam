using System;
using System.Collections;
using System.Collections.Generic;
using BulletFury;
using UnityEngine;


public class Arm : MonoBehaviour
{
    #region Variables

    public Transform pivot;
    public float Radius = 2f;

    
    private Camera mainCam;
    private BulletCollider ArmCollider;
    
    #endregion

    private void Start()
    {
        mainCam = Camera.main;
        ArmCollider = this.GetComponentInChildren<BulletCollider>();
    }

    private void Update()
    {

        ArmCollider.enabled = Input.GetKey(KeyCode.Mouse0);
        
        
        ArmRotation();
    }

    public void ArmRotation()
    {
          
        Vector2 center = pivot.position;

        Vector2 mousePos = mainCam.ScreenToWorldPoint(Input.mousePosition);

        float angle = MathF.Atan2(mousePos.y - center.y, mousePos.x - center.x);

        float x = Radius * MathF.Cos(angle);
        float y = Radius * MathF.Sin(angle);

        this.transform.position = center + new Vector2(x, y);
        this.transform.rotation = Quaternion.LookRotation(Vector3.forward,mousePos - center);
    }
}
