using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ENDGAME : MonoBehaviour
{
    public void ENDTHEGAME()
    {
        Destroy(GameManager.Instance.MAINPLAYERGAMEOBJECT);
        Destroy(GameManager.Instance.gameObject);
    }
}
