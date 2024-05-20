using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlashingText : MonoBehaviour
{

    public GameObject text;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnEnable()
    {
        StartCoroutine(Blink());
    }

    private void OnDisable()
    {
        StopCoroutine(Blink());
    }

    private IEnumerator Blink()
    {
        while (true)
        {
            text.SetActive(!text.activeSelf);
            yield return new WaitForSeconds(0.5f);
        }
    }
}
