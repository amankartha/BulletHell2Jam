using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnableDisableScript : MonoBehaviour
{
    [SerializeField] 
    private GameObject Object2Disable, Object2Enable;

    public void EnableDisbale()
    {
        Object2Disable.SetActive(false);
        Object2Enable.SetActive(true);
    }
}
