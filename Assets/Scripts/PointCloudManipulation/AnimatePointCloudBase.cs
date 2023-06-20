using Pcx;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class AnimatePointCloudBase : MonoBehaviour
{
    // Script Functionality: (TODO)
    //	1. Play the point cloud animation repeatedly, unless asked
    //	2. Take input from other scripts to pause/play the point cloud
    //	3. Send and display some sort of message when the array of meshes is not loaded for your point cloud (this should never be the user's fault, only code bugs)

    private Mesh[] currentMeshes;

	//protected MeshFilter meshFilterComp;

	protected int currentIndex;

    protected bool animate = false;

	protected bool isMesh = false;

    public Material[] meshMaterials;

    public Mesh[] CurrentMeshes { get => currentMeshes; set => currentMeshes = value; }

    void Start()
    {
        //gameObject.GetComponent<MeshF = gameObject.GetComponent<MeshFilter>();

        currentIndex = 0;
    }

    void FixedUpdate()
    {
        // FixedUpdate updates according to a timestep set in Project Settings -> Time -> Fixed Timestep
        // set it to 0.016666 for 60 fps, or 0.0333333 for 30 fps
        if (animate)
        {
            gameObject.GetComponent<MeshFilter>().mesh = CurrentMeshes[++currentIndex];

            if(isMesh)
            {
                gameObject.GetComponent<MeshRenderer>().materials = new Material[] { meshMaterials[currentIndex] };
			}
        }

        AdditionalFixedUpdate();
	}

    protected virtual void AdditionalFixedUpdate()
    {

    }

    public bool GetIsMesh()
    { 
        return isMesh; 
    }

    public virtual void SetIsMesh(bool isMesh)
    {

	}

    protected void RestartAnimation()
    {
        currentIndex = -1;
        // display something?
    }

    public void SetAnimate(bool pause, bool fromStart = false)
    {
        if (pause)
        {
            animate = false;
        }
        else
        {
            if (fromStart)
            {
                RestartAnimation();
            }

            animate = true;
        }
        Debug.Log("animate set to " + animate);
        AdditionalSetAnimate();


    }

    protected virtual void AdditionalSetAnimate()
    {

    }

    public bool GetAnimate()
    {
        return animate;
    }

    public void DisplayMessage(string msg)
    {
        // display the passed argument as a "toast" message on the screen
    }
}