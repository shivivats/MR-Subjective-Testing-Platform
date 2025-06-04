using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimplePCDistanceSetter : MonoBehaviour
{
    public GameObject referenceObject;

    public float fixedDistance = 2.5f;

    // Update is called once per frame
    void Update()
    {
        if(referenceObject!=null)
        {
            // keep this gameObject with the same X value of the 'referenceObject'
            // and at 'fixedDistance' from the 'referenceObject'
            
            gameObject.transform.position = new Vector3(referenceObject.transform.position.x, gameObject.transform.position.y, referenceObject.transform.position.z + fixedDistance);
            
        }
    }
}
