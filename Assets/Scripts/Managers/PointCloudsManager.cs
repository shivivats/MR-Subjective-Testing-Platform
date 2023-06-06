using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.UI.BoundsControl;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;



public class PointCloudsManager : MonoBehaviour
{
	public static PointCloudsManager Instance { get; private set; }

	/*
     * The management tasks for this script are twofold:
     *  1. The script needs to use PointCloudsLoader to load all the 
     *      objects and refers to PointCloudsLoader for them whenever needed.
     *  2. The script keeps track of all the point cloud game objects displayed
     *      in the scene at any moment.
     *  3. Correctly position PC game objects, set appropriate animate (and other) 
     *      components, and add/destroy them as needed.
     */

	public GameObject firstPointCloud;

	public GameObject pointCloudPrefab;

	private GameObject currentWorkingPointCloud;

	List<GameObject> pointCloudGameObjects;

	public float objectsDistanceOffset = 0.0f;

	public int maxNumPointClouds = 4;

    public TextMeshPro currentPcText;


    private void Awake()
	{
		// Singleton code - If there is an instance, and it's not this, then delete this instance

		if (Instance != null && Instance != this)
		{
			Destroy(this);
		}
		else
		{
			Instance = this;
			DontDestroyOnLoad(gameObject);
		}

        QualitySettings.vSyncCount = 0;  // VSync must be disabled
        Application.targetFrameRate = 60; // Limit fps to 60
    }

	void Start()
	{
		PointCloudsLoader.Instance.StartLoader();

		pointCloudGameObjects = new List<GameObject> { firstPointCloud };
		currentWorkingPointCloud = firstPointCloud;
		SetupCurrentWorkingObject();
	}


    private void SetupCurrentWorkingObject()
    {
        QualityChanger.Instance.OnQualitySliderUpdated();

        DistanceChanger.Instance.OnDistanceSliderUpdated();

        ObjectChanger.Instance.OnSoldierButtonPressed();

        MaterialChanger.Instance.OnPointButtonPressed();


    }

	private void ChangeConfigurationFrontend()
	{
        ObjectChanger.Instance.HighlightSelectedObject(currentWorkingPointCloud.GetComponent<AnimatePointCloud>().GetCurrentObject());

        MaterialChanger.Instance.HighlightSelectedMaterial(currentWorkingPointCloud.GetComponent<AnimatePointCloud>().CurrentMaterial);

        ChangeCurrentPCText();
    }

    /* Add Button Press
	/* Check if max number is reached, 
       instantiate a new object from prefab, 
       position the object properly, 
       set up the new PC, 
       add the object to the array, 
       set currentWorkingPointCloud to new PC
    */
    public void OnAddPointCloudButtonPressed()
	{
		if (pointCloudGameObjects.Count >= maxNumPointClouds)
			return;

		GameObject refObj = pointCloudGameObjects.Last();


        GameObject tempPC = Instantiate(pointCloudPrefab, refObj.transform.parent);
		tempPC.transform.localPosition = new Vector3(refObj.transform.localPosition.x + objectsDistanceOffset,
                                                        refObj.transform.localPosition.y,
                                                        refObj.transform.localPosition.z);

		pointCloudGameObjects.Add(tempPC);

		currentWorkingPointCloud = pointCloudGameObjects.ElementAt(pointCloudGameObjects.IndexOf(tempPC));
		
		SetupCurrentWorkingObject();
	}

	/* Remove Button Press
	/* Check if current working point cloud is not null, 
       remove from array, 
       delete the point cloud
    */
	public void OnRemovePointCloudButtonPressed()
	{
		if (pointCloudGameObjects.Count <= 1)
			return;

		pointCloudGameObjects.Remove(currentWorkingPointCloud);
		GameObject.Destroy(currentWorkingPointCloud);
		
		currentWorkingPointCloud = pointCloudGameObjects.Last();

        ChangeConfigurationFrontend();
    }

	/* Next Button Press and Previous Button Press
	/* Increment or decrement the index in the array, 
       set currentWorkingPointCloud to the point cloud at new index
	   Invoked via the buttons in the scene
    */
	public void OnChangeActivePointCloudButtonsPressed(bool increment) 
	{
		int currentIndex = pointCloudGameObjects.IndexOf(currentWorkingPointCloud);

		if (increment)
		{
			currentIndex += 1;
			Debug.Log("currentIndex" + currentIndex);
			if(pointCloudGameObjects.ElementAtOrDefault(currentIndex) != null)
                currentWorkingPointCloud = pointCloudGameObjects[currentIndex];
			else
                currentWorkingPointCloud = pointCloudGameObjects.First();
		}
		else
		{
			currentIndex -= 1;
            Debug.Log("currentIndex" + currentIndex);
            if (pointCloudGameObjects.ElementAtOrDefault(currentIndex) != null)
                currentWorkingPointCloud = pointCloudGameObjects[currentIndex];
            else
                currentWorkingPointCloud = pointCloudGameObjects.Last();
        }

        ChangeConfigurationFrontend();

    }

    public string GetPCObjectTypeAsString(PCObjectType type)
    {
        switch (type)
        {
            case PCObjectType.LongDress: return "LongDress";
            case PCObjectType.Loot: return "Loot";
            case PCObjectType.RedAndBlack: return "RedAndBlack";
            case PCObjectType.Soldier: return "Soldier";
        }
        return null;
    }

    private void ChangeCurrentPCText()
	{
		currentPcText.text = GetPCObjectTypeAsString(currentWorkingPointCloud.GetComponent<AnimatePointCloud>().GetCurrentObject());
	}

	/* Change Representation - point, square, circle or mesh
	 * store the point, square, and circle materials as objects in this class
	 * change the meshrenderer material element 0 to the appropriate material
	 * 
	 * TODO: change to mesh
	 */

	 public void ChangePCAnimationPaused(bool paused)
	 {
		if(currentWorkingPointCloud != null)
		{
            currentWorkingPointCloud.GetComponent<AnimatePointCloud>().SetAnimate(paused, false);
        }
	 }

	public void ChangePCInteractable(bool interactable)
	{
		currentWorkingPointCloud.GetComponent<BoxCollider>().enabled = interactable;
		currentWorkingPointCloud.GetComponent<BoundsControl>().enabled = interactable;
	}

	 public void ChangeCurrentPCQuality(int quality)
	 {
		if(currentWorkingPointCloud != null) 
		{
			currentWorkingPointCloud.GetComponent<AnimatePointCloud>().SetCurrentQuality(quality);
        }
    }

	 public void ChangeCurrentPCDistance(float distance)
	 {
		if(currentWorkingPointCloud != null)
		{
			currentWorkingPointCloud.transform.localPosition = new Vector3(currentWorkingPointCloud.transform.localPosition.x,
																			currentWorkingPointCloud.transform.localPosition.y,
																			distance);
        }
	}

	 public void ChangeCurrentPCObject(PCObjectType type)
	 {
		if(currentWorkingPointCloud != null)
		{
            currentWorkingPointCloud.GetComponent<AnimatePointCloud>().SetCurrentObject(type);
            ChangeConfigurationFrontend();
        }
     }

	 public void ChangeCurrentPCMaterial(Material currMat, PCMaterialType type)
	 {
		if(currentWorkingPointCloud != null)
		{
			if (currentWorkingPointCloud.GetComponent<AnimatePointCloud>().isMesh)
				ChangeMeshAndPC(false);

			currentWorkingPointCloud.GetComponent<MeshRenderer>().materials = new Material[] {currMat};
			currentWorkingPointCloud.GetComponent<AnimatePointCloud>().CurrentMaterial = type;
        }
    }

	public void ChangeMeshAndPC(bool toMesh)
	{
		// if toMesh is true then we go from PC to mesh
		// -> the material will change
		// its again a matter of changing meshes (or prefabs) at runtime

		
	}



	public PointCloudObject GetPCObject(PCObjectType type)
	{
		switch (type)
		{
			case PCObjectType.LongDress:
				return PointCloudsLoader.Instance.LongDressPointCloudObject;
			case PCObjectType.Loot:
				return PointCloudsLoader.Instance.LootPointCloudObject;
			case PCObjectType.RedAndBlack:
				return PointCloudsLoader.Instance.RedAndBlackPointCloudObject;
			case PCObjectType.Soldier:
				return PointCloudsLoader.Instance.SoldierPointCloudObject;
		}

		return null;
	}
}
