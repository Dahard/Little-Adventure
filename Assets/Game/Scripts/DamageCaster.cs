using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageCaster : MonoBehaviour
{
    private Collider damageCasterCollider;
    public int Damage = 30;
    public string TargetTag;
    private List<Collider> damagedTargetList;

    private void Awake()
    {
        damageCasterCollider = GetComponent<Collider>();
        damageCasterCollider.enabled = false;
        damagedTargetList = new List<Collider>();
    }

    private void OnTriggerEnter(Collider other)
    {

        if ( other.tag.Equals(TargetTag) && !damagedTargetList.Contains(other))
        {

            Character targetCC = other.GetComponent<Character>();

            if (targetCC != null)
            {
                targetCC.ApplyDamage(Damage, transform.parent.position);

                PlayerVFXManger playerVFXManager = transform.parent.GetComponent<PlayerVFXManger>();

                if(playerVFXManager != null )
                {
                    RaycastHit hit;
                    Vector3 originalPos = transform.position + (-damageCasterCollider.bounds.extents.z) * transform.forward;
                    bool isHit = Physics.BoxCast(originalPos, damageCasterCollider.bounds.extents / 2, transform.forward, out hit, transform.rotation, damageCasterCollider.bounds.extents.z, 1 << 6);
                    
                    if(isHit )
                    {
                        playerVFXManager.PlaySlash(hit.point + new Vector3(0, 0.5f, 0));
                    }
                }
            }

            damagedTargetList.Add(other);

        }
    }

    public void EnableDamageCaster()
    {
        damagedTargetList.Clear();
        damageCasterCollider.enabled = true;
    }

    public void DisableDamageCaster()
    {
        damagedTargetList.Clear();
        damageCasterCollider.enabled = false;
    }

    //Gizmo for debugging on hit Enemy
    //private void OnDrawGizmos()
    //{
    //    if (damageCasterCollider == null)
    //    {
    //        damageCasterCollider = GetComponent<Collider>();
    //    }

    //    RaycastHit hit;
    //    Vector3 originalPos = transform.position + (- damageCasterCollider.bounds.extents.z) * transform.forward;
    //    bool isHit = Physics.BoxCast(originalPos, damageCasterCollider.bounds.extents / 2, transform.forward, out hit, transform.rotation, damageCasterCollider.bounds.extents.z, 1 << 6);

    //    if (isHit)
    //    {
    //        Gizmos.color = Color.red;
    //        Gizmos.DrawWireSphere(hit.point, 0.3f);
    //    }
    //}
}
