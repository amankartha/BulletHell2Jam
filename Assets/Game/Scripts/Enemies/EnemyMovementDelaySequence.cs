using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class EnemyMovementDelaySequence : MonoBehaviour
{
    private DOTweenPath _doTweenPath;


    // Start is called before the first frame update
    void Start()
    {
        _doTweenPath = this.gameObject.GetComponent<DOTweenPath>();
        transform.DOPath(_doTweenPath.path.wps, _doTweenPath.duration).SetLoops(-1).OnWaypointChange(Pause);
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void Pause(int i)
    {
        transform.DOPause();
   
        Invoke(nameof(Resume), _doTweenPath.delay);
    }

    private void Resume()
    {
        transform.DOPlay();
    }
}
    
    
