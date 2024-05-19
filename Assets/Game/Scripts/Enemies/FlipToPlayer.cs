using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlipToPlayer : MonoBehaviour
{

    private SpriteRenderer _spriteRenderer;
    // Start is called before the first frame update
    void Start()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        if ( this.transform.position.x - GameManager.Instance.MAINPLAYERGAMEOBJECT.transform.position.x > 0)
        {
            _spriteRenderer.flipX = false;
        }
        else
        {
            _spriteRenderer.flipX = true;
        }
    }
}
