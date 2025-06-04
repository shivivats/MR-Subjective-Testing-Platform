using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimplePCAnimation : MonoBehaviour
{
    public Mesh[] currentMeshes;

    protected int currentIndex;

    public bool animate = false;

    void Start()
    {
        currentIndex = 0;
    }

    void FixedUpdate()
    {
        // FixedUpdate updates according to a timestep set in Project Settings -> Time -> Fixed Timestep
        // set it to 0.016666 for 60 fps, or 0.0333333 for 30 fps
        if (animate)
        {
            gameObject.GetComponent<MeshFilter>().mesh = currentMeshes[++currentIndex];
            if(currentIndex==currentMeshes.Length-1)
            {
                RestartAnimation();
            }
        }
    }

    protected void RestartAnimation()
    {
        currentIndex = -1;
    }
}
