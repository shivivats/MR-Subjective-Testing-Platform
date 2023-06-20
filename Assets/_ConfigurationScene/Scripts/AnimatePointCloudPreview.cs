using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatePointCloudPreview : AnimatePointCloudBase
{
	private int currentQuality = 2;

	private PCObjectType currentObjectType = 0; // 0 is gonna be the first entry in the PCObjectType and thus the default value

	private PCMaterialType currentMaterial = PCMaterialType.Point; // point is the default material type

	public PCMaterialType CurrentMaterial { get => currentMaterial; set => currentMaterial = value; }

	// Start is called before the first frame update
	void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

	protected override void AdditionalFixedUpdate()
	{
		if(animate)
		{
			if (currentIndex == CurrentMeshes.Length - 1) // upon sequence complete
			{
				RestartAnimation(); // loops the animation
			}
		}
	}

	public override void SetIsMesh(bool isMesh)
	{
		if (isMesh != this.isMesh)
		{
			this.isMesh = isMesh;
			RestartAnimation();
			UpdateCurrrentMeshes();
		}
		this.isMesh = isMesh;
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

	public int GetCurrentQuality() { return currentQuality; }

	protected void UpdateCurrrentMeshes()
	{
		// change the current meshes array to appropriate type using current object type and current quality
		PointCloudObject currentObject = ConfigurationSceneManager.Instance.GetPCObject(currentObjectType);
		CurrentMeshes = isMesh ? currentObject.meshes[currentQuality] : currentObject.pointClouds[currentQuality];
		meshMaterials = isMesh ? currentObject.meshMaterials[currentQuality] : null;
		// if we are switching from mesh to pc or vice versa then reset currentindex to avoid any out of bounds errors

	}
}
