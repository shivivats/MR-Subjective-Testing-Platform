using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System;
using System.Linq;
using System.Xml.Linq;

public class EyeTrackingExperimentController : MonoBehaviour
{
	public static EyeTrackingExperimentController Instance { get; private set; }

	public EyeTrackingPointCloudData[] pointClouds;
	private int currentPCIndex = -1;

	public string currentJSONDirectory;
	private int currentUserId;

	public GameObject pcGameObject;

	public bool playing = false;

	public GameObject startTaskButton;
	public GameObject nextVideoButton;
	public GameObject errorMeasureButton;

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
	}

	void Start()
	{
		System.Random r = new System.Random();
		pointClouds = pointClouds.ToList().OrderBy(x => r.Next()).ToArray();
		LoadAllPointClouds();
		UpdateNewUserID();

		pcGameObject.SetActive(false);
		playing = false;
		SetCurrentPCDistance(2.0f, false, 1.6f);

		ShowButtons(nextButton: false, startButton: false, errorButton: true);

		//StartCoroutine(WaitCoroutine());\
		SetFeedbackUIPosition();
	}

	public void SetFeedbackUIPosition()
	{


		// set the position to be near the user
		// i.e. set it relative to the camera
		// world position: +1 Z
		nextVideoButton.transform.parent.gameObject.transform.SetPositionAndRotation(Camera.main.transform.position + Camera.main.transform.forward * 0.4f, transform.rotation);
		nextVideoButton.transform.parent.gameObject.transform.LookAt(Camera.main.transform);
		nextVideoButton.transform.parent.gameObject.transform.Rotate(new Vector3(0f, 180f, 0f));

		startTaskButton.transform.parent.gameObject.transform.SetPositionAndRotation(Camera.main.transform.position + Camera.main.transform.forward * 0.4f, transform.rotation);
		startTaskButton.transform.parent.gameObject.transform.LookAt(Camera.main.transform);
		startTaskButton.transform.parent.gameObject.transform.Rotate(new Vector3(0f, 180f, 0f));

		errorMeasureButton.transform.parent.gameObject.transform.SetPositionAndRotation(Camera.main.transform.position + Camera.main.transform.forward * 0.4f, transform.rotation);
		errorMeasureButton.transform.parent.gameObject.transform.LookAt(Camera.main.transform);
		errorMeasureButton.transform.parent.gameObject.transform.Rotate(new Vector3(0f, 180f, 0f));


	}
	private void ShowButtons(bool nextButton, bool startButton, bool errorButton)
	{
		nextVideoButton.SetActive(nextButton);
		startTaskButton.SetActive(startButton);
		errorMeasureButton.SetActive(errorButton);
	}

	public void OnNextVideoButtonPressed()
	{
		ShowButtons(nextButton: false, startButton: false, errorButton: false);
		StartNextSequence();
	}

	public void OnErrorProfilingButtonPressed()
	{
		ShowButtons(nextButton: false, startButton: false, errorButton: false);
		ShowErrorProfilingScene();
	}

	private void ShowNextVideoButton()
	{
		ShowButtons(nextButton: true, startButton: false, errorButton: false);
	}

	private void ShowErrorProfilingScene()
	{
		ErrorProfilingManager.Instance.StartErrorProfiling();
	}

	public void OnErrorProfilingEnded()
	{
		ShowNextVideoButton();
		//StartNextSequence();

	}

	public void OnStartButtonPressed()
	{
		ShowButtons(nextButton: false, startButton: false, errorButton: true);
	}

	//private IEnumerator WaitCoroutine()
	//{
	//    yield return new WaitForSeconds(10);
	//    StartNextSequence();
	//}

	public void OnSequenceEnded()
	{
		pcGameObject.SetActive(false);
		playing = false;

		// show the next button button
		if (currentPCIndex + 1 >= pointClouds.Count())
		{
			// ended the playback for all sequences here
			// just dont do anything for now
			// we move on to Task 2 of the overall test - do it manually
			// give the user a break in between as well
		}
		else
		{
			ShowButtons(false, false, true);
		}

	}

	private void StartNextSequence()
	{
		currentPCIndex++;
		pcGameObject.GetComponent<EyeTrackingAnimationController>().currentMeshes = pointClouds[currentPCIndex].pointCloudMeshes;
		pcGameObject.SetActive(true);
		playing = true;
		pcGameObject.GetComponent<EyeTrackingAnimationController>().SetAnimate(true, true);
	}

	private void LoadAllPointClouds()
	{
		foreach (var pointCloud in pointClouds)
		{
			pointCloud.LoadAssetsFromDisk();
		}
	}

	private void UpdateNewUserID()
	{
		int maxUserID = -1;
		foreach (string file in Directory.EnumerateFiles(currentJSONDirectory, "*.json"))
		{
			int userId = EyeTrackingUtils.Instance.GetUserIdFromJsonFilename(file);
			if (userId > maxUserID)
			{
				maxUserID = userId;
			}
		}
		currentUserId = maxUserID + 1; // max ID plus one is the next user's ID
	}

	public string GetCurrentPcName()
	{
		return pointClouds[currentPCIndex].pcName;
	}

	public Mesh[] GetPCMeshesFromName(string pcname)
	{
		foreach (var pointCloud in pointClouds)
		{
			if (pointCloud.pcName == pcname)
			{
				return pointCloud.pointCloudMeshes;
			}
		}

		return null;
	}

	public int GetCurrentPCIndex()
	{
		return pointClouds[currentPCIndex].pcIndex;
	}

	public int GetCurrentUserID()
	{
		return currentUserId;
	}

	public string GetErrorProfilingJSONWithPath(int whichError)
	{
		return currentJSONDirectory + "ErrorData_" + "User" + GetCurrentUserID().ToString() + "_Error" + whichError.ToString() + ".json";
	}

	public string GetJSONFilenameWithPath()
	{
		// for each user there will be one JSON file per point cloud
		// hence the JSON file should have information about the user and the PC in the name

		return currentJSONDirectory + "GazeData_" + "User" + GetCurrentUserID().ToString() + "_PC" + GetCurrentPcName() + ".json";
	}

	public void SetCurrentPCDistance(float currentDistance, bool useFixedYOffsetForDistance, float yOffset)
	{
		Vector3 newPos = new Vector3(pcGameObject.transform.localPosition.x, 0f, Camera.main.transform.position.z + currentDistance + 0.2f);

		if (useFixedYOffsetForDistance)
			newPos.y = Camera.main.transform.position.y + yOffset;
		else
			newPos.y = pcGameObject.transform.localPosition.y;

		pcGameObject.transform.SetLocalPositionAndRotation(newPos, pcGameObject.transform.localRotation);

		Debug.Log("Distance set to " + currentDistance);
	}

}

