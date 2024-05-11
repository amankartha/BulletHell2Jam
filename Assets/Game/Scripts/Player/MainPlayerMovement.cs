using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainPlayerMovement : MonoBehaviour
{

    public float Speed = 5f;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    private Vector2 movementVectorContainer = new Vector2();
    void Update()
    {
        movementVectorContainer =  new Vector2(Input.GetAxis("Horizontal"),Input.GetAxis("Vertical"));
        movementVectorContainer *= Speed;
        movementVectorContainer *= Time.deltaTime;
        
        transform.Translate(movementVectorContainer);
    }
}
