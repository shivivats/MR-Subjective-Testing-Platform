using Microsoft.MixedReality.Toolkit.UI.BoundsControl;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class ConfigurationSceneManager : MonoBehaviour
{
    public static ConfigurationSceneManager Instance { get; private set; }

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

        QualitySettings.vSyncCount = 0;  // VSync must be disabled to limit the FPS
        Application.targetFrameRate = 60; // Limit FPS to 60
    }

    public GameObject firstPointCloud;
    
    public GameObject pointCloudPrefab;

    private GameObject currentWorkingPointCloud;

    List<GameObject> pointCloudGameObjects;

    public float objectsDistanceOffset = 2.0f;

    public int maxNumPointClouds = 4;

    public TextMeshPro currentPcText;

    void Start()
    {
        PointCloudsLoader.Instance.LoadPointCloudsAndMeshes();
        if(!PointCloudsLoader.Instance.loadMeshes)
        {
            MaterialChanger.Instance.meshButton.SetActive(false);
        }

        /* TODO: Instantiate a new object from the prefab here. */
        pointCloudGameObjects = new List<GameObject> { firstPointCloud };
        currentWorkingPointCloud = firstPointCloud;
        SetupCurrentWorkingObject();
    }

    private void InstantiateNewObject()
    {
        GameObject refObj = pointCloudGameObjects.Last();


        GameObject tempPC = Instantiate(pointCloudPrefab, refObj.transform.parent);
        tempPC.transform.localPosition = new Vector3(refObj.transform.localPosition.x + objectsDistanceOffset,
                                                        refObj.transform.localPosition.y,
                                                        refObj.transform.localPosition.z);

        pointCloudGameObjects.Add(tempPC);

        currentWorkingPointCloud = pointCloudGameObjects.ElementAt(pointCloudGameObjects.IndexOf(tempPC));

        SetupCurrentWorkingObject();
    }

    private void SetupCurrentWorkingObject()
    {
		ObjectChanger.Instance.OnObjectButtonPressed(PointCloudsLoader.Instance.pcObjects.First().objectType);

		MaterialChanger.Instance.OnPointButtonPressed();

		QualityChanger.Instance.OnQualitySliderUpdated();

        DistanceChanger.Instance.OnDistanceSliderUpdated();
    }

    private void ChangeConfigurationFrontend()
    {
        ObjectChanger.Instance.HighlightSelectedObject(currentWorkingPointCloud.GetComponent<AnimatePointCloudPreview>().GetCurrentObject());

        MaterialChanger.Instance.HighlightSelectedMaterial(currentWorkingPointCloud.GetComponent<AnimatePointCloudPreview>().CurrentMaterial);

        PlayPauseChanger.Instance.UpdatePausedStateOfNewSelectedObject(currentWorkingPointCloud.GetComponent<AnimatePointCloudPreview>().GetAnimate());

        InteractionChanger.Instance.UpdateInteractableStateOfNewSelectedObject(currentWorkingPointCloud.GetComponent<BoxCollider>().enabled);

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

        InstantiateNewObject();        
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
            if (pointCloudGameObjects.ElementAtOrDefault(currentIndex) != null)
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

    private void ChangeCurrentPCText()
    {
        currentPcText.text = PointCloudsLoader.Instance.GetPCNameFromType(currentWorkingPointCloud.GetComponent<AnimatePointCloudPreview>().GetCurrentObject());
    }

    /* Change Representation - point, square, circle or mesh
	 * store the point, square, and circle materials as objects in this class
	 * change the meshrenderer material element 0 to the appropriate material
	 * 
	 * TODO: change to mesh
	 */

    public void ChangePCAnimationPaused(bool paused)
    {
        if (currentWorkingPointCloud != null)
        {
            currentWorkingPointCloud.GetComponent<AnimatePointCloudPreview>().SetAnimate(paused, false);
        }
    }

    public void ChangePCInteractable(bool interactable)
    {
        currentWorkingPointCloud.GetComponent<BoxCollider>().enabled = interactable;
        currentWorkingPointCloud.GetComponent<BoundsControl>().enabled = interactable;
    }

    public void ChangeCurrentPCQuality(int quality)
    {
        if (currentWorkingPointCloud != null)
        {
            currentWorkingPointCloud.GetComponent<AnimatePointCloudPreview>().SetCurrentQuality(quality);
        }
    }

    public void ChangeCurrentPCDistance(float distance)
    {
        if (currentWorkingPointCloud != null)
        {
            // here I actually want to change the position while making sure the PC is facing the user.
            // the configuration for that is:
            // same X, same Y, Z based on distance
            // rotation 0 X, -180 Y, 0 Z
            // set the rotation first!

            currentWorkingPointCloud.transform.localRotation = Quaternion.Euler(0f, -180f, 0f);
			currentWorkingPointCloud.transform.localPosition = new Vector3(currentWorkingPointCloud.transform.localPosition.x,
																            currentWorkingPointCloud.transform.localPosition.y,
																            distance);
		}
    }

    public void ChangeCurrentPCObject(PCObjectType type)
    {
        if (currentWorkingPointCloud != null)
        {
            currentWorkingPointCloud.GetComponent<AnimatePointCloudPreview>().SetCurrentObject(type);
            ChangeConfigurationFrontend();
        }
    }

    public void ChangeCurrentPCMaterial(PCMaterialType type, Material currMat=null)
    {
        /* so the way this works is:
         * if we are currently displaying pc and some point type, and we want to switch to mesh
         *      then we just set the isMesh flag true in AnimatePointCloud and it can automatically start pulling from the meshes.
         * similarly, the opposite also works for mesh to pc, where we also specify the material type so that works
         * finally, if we are changing between points for pc, we can simply set the material type
        */
        if (currentWorkingPointCloud != null)
        {
            if(type == PCMaterialType.Mesh)
            {
				// switch from PC to mesh
				if (!currentWorkingPointCloud.GetComponent<AnimatePointCloudPreview>().GetIsMesh())
                {
					currentWorkingPointCloud.GetComponent<AnimatePointCloudPreview>().SetIsMesh(true);
				}
			}
            else
            {
				// switching from mesh to PC
                if(currentWorkingPointCloud.GetComponent<AnimatePointCloudPreview>().GetIsMesh())
                {
					currentWorkingPointCloud.GetComponent<AnimatePointCloudPreview>().SetIsMesh(false);
				}

				// switching between PC point materials
				currentWorkingPointCloud.GetComponent<MeshRenderer>().materials = new Material[] { currMat };
				currentWorkingPointCloud.GetComponent<AnimatePointCloudPreview>().CurrentMaterial = type;
			}
        }
    }
    public PointCloudObject GetPCObject(PCObjectType type)
    {
        return PointCloudsLoader.Instance.GetPCObjectFromType(type);
    }
}