using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Arm : MonoBehaviour
{
    #region Variables

    public Transform pivot;
    public float Radius = 2f;

    
    private Camera mainCam;

    
    #endregion

    private void Start()
    {
        mainCam = Camera.main;
       
    }

    private void Update()
    {

       
        
        
        ArmRotation();
    }

    public void ArmRotation()
    {
          
        Vector2 center = pivot.position;

        Vector2 mousePos = mainCam.ScreenToWorldPoint(Input.mousePosition);

        float angle = MathF.Atan2(mousePos.y - center.y, mousePos.x - center.x);

        float x = Radius * MathF.Cos(angle);
        float y = Radius * MathF.Sin(angle);
        
        GameManager.Instance.MAINPLAYERSCRIPT.Anim.SetFloat("Blend",y);
        
        this.transform.position = center + new Vector2(x, y);
        this.transform.rotation = Quaternion.LookRotation(Vector3.forward,mousePos - center);
    }
}
