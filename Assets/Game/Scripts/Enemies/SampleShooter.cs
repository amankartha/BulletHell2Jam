using System.Collections;
using System.Collections.Generic;
using BulletFury;
using BulletFury.Data;
using UnityEngine;

public class SampleShooter : MonoBehaviour
{
    private BulletManager _bulletManager;
    void Start()
    {
        _bulletManager = this.GetComponent<BulletManager>();
    }

    // Update is called once per frame
    void Update()
    {
        _bulletManager.Spawn(this.transform.position,_bulletManager.Plane == BulletPlane.XY ? transform.up : transform.forward);
    }
}
