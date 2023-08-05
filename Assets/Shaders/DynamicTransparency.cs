 using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DynamicTransparency : MonoBehaviour
{
    public Transform otherObjectTransform;
    public Material dynamicTransparencyMaterial;
    private float epsilon = 0.0001f; //small unit of measurement
    private Vector3 lastPosition;

    void Start()
    {
        dynamicTransparencyMaterial = GetComponent<MeshRenderer>().sharedMaterial;
        lastPosition = otherObjectTransform.position - transform.position;
        dynamicTransparencyMaterial.SetVector("_OtherObjectPosition", otherObjectTransform.position - transform.position);
    }


    void Update()
    {
        if(Vector3.Distance(otherObjectTransform.position - transform.position, lastPosition) > epsilon)
        {
            
            lastPosition = otherObjectTransform.position - transform.position;
            dynamicTransparencyMaterial.SetVector("_OtherObjectPosition", otherObjectTransform.position - transform.position);
        }
    }
}
