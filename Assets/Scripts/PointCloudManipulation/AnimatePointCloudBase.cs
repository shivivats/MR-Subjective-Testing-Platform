using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class AnimatePointCloudBase : MonoBehaviour
{
    // Script Functionality: (TODO)
    //	1. Play the point cloud animation repeatedly, unless asked
    //	2. Take input from other scripts to pause/play the point cloud
    //	3. Send and display some sort of message when the array of meshes is not loaded for your point cloud (this should never be the user's fault, only code bugs)

    private Mesh[] currentMeshes;

    protected bool firstSequence = true;
    //protected MeshFilter meshFilterComp;

    protected int currentIndex;

    protected bool animate = false;

	protected bool isMesh = false;

    public Material[] meshMaterials;

    public Mesh[] CurrentMeshes { get => currentMeshes; set => currentMeshes = value; }

    private float rotationY;

	public GameObject childObject;
	private Quaternion initialRotation;


	void Start()
    {
        currentIndex = -1;
		initialRotation = gameObject.transform.rotation;
		RotatePCRandomly();
    }

    protected void RotatePCRandomly()
    {
        //rotationY = UnityEngine.Random.Range(-180.0f, 180.0f);
        List<float> rotations = new List<float> { 0f, 45f, 90f, 135f, 180f, 225f, 270f, 315f, 360f };
        rotationY = rotations.ElementAt(Random.Range(0, 8));

        //gameObject.transform.Rotate(new Vector3(0f, rotationY, 0f));
        gameObject.transform.rotation = Quaternion.Euler(gameObject.transform.rotation.eulerAngles.x, rotationY, gameObject.transform.rotation.eulerAngles.z);
        Debug.Log("Random rotation of PC set to " + rotationY);
    }

    void FixedUpdate()
    {
        // FixedUpdate updates according to a timestep set in Project Settings -> Time -> Fixed Timestep
        // set it to 0.016666 for 60 fps, or 0.0333333 for 30 fps
        if (animate)
        {
			if (firstSequence && currentIndex == 0)
			{
				// at the first frame, offset the child
				childObject.transform.localPosition = new Vector3(-1 * childObject.GetComponent<MeshFilter>().mesh.bounds.center.x, 0f, -1 * childObject.GetComponent<MeshFilter>().mesh.bounds.center.z);
			}
			//gameObject.GetComponent<MeshFilter>().mesh = CurrentMeshes[++currentIndex];
			childObject.GetComponent<MeshFilter>().mesh = currentMeshes[++currentIndex];

			//if (isMesh)
   //         {
   //             gameObject.GetComponent<MeshRenderer>().materials = new Material[] { meshMaterials[currentIndex] };
			//}
        }

        if (currentIndex == -1)
        {
            // at the first frame, offset the child
            //childObject.transform.localPosition = new Vector3(-1 * GetComponent<MeshFilter>().mesh.bounds.center.x, 0f, -1 * GetComponent<MeshFilter>().mesh.bounds.center.z);
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
        // TODO display the passed argument as a "toast" message on the screen
    }
}