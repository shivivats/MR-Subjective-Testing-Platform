using Pcx;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class AnimatePointCloud : MonoBehaviour
{
	// Script Functionality: (TODO)
	//	1. Play the point cloud animation repeatedly, unless asked
	//	2. Take input from other scripts to pause/play the point cloud
	//	3. Send and display some sort of message when the array of meshes is not loaded for your point cloud (this should never be the user's fault, only code bugs)

	public Mesh[] currentMeshes;

	private MeshFilter meshFilterComp;

	private int currentIndex;

	private bool animate = false;

	private PCObjectType currentObjectType = PCObjectType.Soldier;

	private PCMaterialType currentMaterial = PCMaterialType.Point;

	private int currentQuality = 3;

	public bool isMesh = false;

	public Mesh[] CurrentMeshes { get => currentMeshes; set => currentMeshes = value; }
    public PCMaterialType CurrentMaterial { get => currentMaterial; set => currentMaterial = value; }

    void Start()
	{
		meshFilterComp = GetComponent<MeshFilter>();

		currentIndex = 0;
	}

	void FixedUpdate()
	{
		// FixedUpdate updates according to a timestep set in Project Settings -> Time -> Fixed Timestep
		// set it to 0.016666 for 60 fps, or 0.0333333 for 30 fps
		if (animate)
		{
			meshFilterComp.mesh = CurrentMeshes[++currentIndex];
			if (currentIndex == CurrentMeshes.Length - 1)
			{
				ResetAnimation(); // looping animation
			}
		}
	}

	private void ResetAnimation()
	{
		currentIndex = -1;
		// display something?
	}

	public void SetAnimate(bool pause, bool fromStart=false)
	{
		if(pause)
		{
				animate = false;
		}
		else 
		{
			if (fromStart)
			{
				ResetAnimation();
			}

			animate = true;
		}
	}

	public void SetCurrentObject(PCObjectType type)
	{
		currentObjectType = type;

		UpdateCurrrentMeshes();

		// start animation from beginning
		SetAnimate(false, true);

	}

	public PCObjectType GetCurrentObject() { return currentObjectType; }

	public void SetCurrentQuality(int quality) 
	{
		currentQuality = quality;

		UpdateCurrrentMeshes();

		// if mesh then the quality change will look different
	}

	private void UpdateCurrrentMeshes()
	{
		// change the current meshes array to appropriate type using current object type and current quality
		PointCloudObject currentObject = PointCloudsManager.Instance.GetPCObject(currentObjectType);
		switch(currentQuality) 
		{
			case 1: currentMeshes = currentObject.quality1Meshes; break;

			default:
			case 3: currentMeshes = currentObject.quality3Meshes; break;

			case 5: currentMeshes = currentObject.quality5Meshes; break;
		}
	}

	public int GetCurrentQuality() { return currentQuality; }

	public void DisplayMessage(string msg)
	{
		// display the passed argument as a "toast" message on the screen
	}
}