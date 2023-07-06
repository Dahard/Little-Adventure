using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy02Shoot : MonoBehaviour
{
    public Transform ShootingPoint;
    public GameObject DamageOrb;

    private Character cc;

    private void Awake()
    {
        cc = GetComponent<Character>();
    }

    public void Update()
    {
        cc.RotateToTarget();
    }
    public void ShootTheDamageOrb()
    {
        Instantiate(DamageOrb, ShootingPoint.position, Quaternion.LookRotation(ShootingPoint.forward));
    }
}
