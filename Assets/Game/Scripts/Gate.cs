using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gate : MonoBehaviour
{
    public GameObject GateVisual;
    private Collider gateCollider;
    public float OpenDuration = 2f;
    public float OpenTargetY = -1.5f;

    private void Awake()
    {
        gateCollider = GetComponent<Collider>();
    }

    IEnumerator OpenGateAnimation()
    {
        float currentOpenDuration = 0;
        Vector3 startPos = GateVisual.transform.position;
        Vector3 targetPos = startPos + Vector3.up * OpenTargetY;

        while (currentOpenDuration < OpenDuration)
        {
            currentOpenDuration += Time.deltaTime;
            GateVisual.transform.position = Vector3.Lerp(startPos, targetPos, currentOpenDuration / OpenDuration);

            yield return null;
        }

        gateCollider.enabled = false;
    }

    public void Open()
    {
        Debug.Log("Gate has been opened");
        StartCoroutine(OpenGateAnimation());
    }

}
