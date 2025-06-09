using Microsoft.MixedReality.Toolkit;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class ErrorProfilingManager : MonoBehaviour
{
	public static ErrorProfilingManager Instance { get; private set; }
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

	private int errorCounter = 0;

	public GameObject markerParent;

	public List<GameObject> markers;

	public float markerDuration = 10f;

	ErrorProfilingData profilingData;

	bool recordData = false;

	private int timestamp;
	private int currentMarkerIndex = -1;

	private Vector3 markerPos;
	private Vector3 gazeOrigin;
	private Vector3 gazeDirectionNormalised;

	// Start is called before the first frame update
	void Start()
	{
		//StartCoroutine(WaitCoroutine());
		markerParent.SetActive(false);
		foreach (GameObject m in markers)
		{
			m.SetActive(false);
		}
	}

	public void StartErrorProfiling()
	{
		markerParent.SetActive(true);
		ShowNextMarker();
	}

	// we should show the markers one by one for some seconds
	// then record the user's gaze on that marker for that amount of time
	private void ShowNextMarker()
	{
		currentMarkerIndex++;
		if (currentMarkerIndex == markers.Count)
		{
			EndErrorProfiling();
			return;
		}

		foreach (GameObject m in markers)
		{
			m.SetActive(false);
		}
		markers[currentMarkerIndex].SetActive(true);
		recordData = true;
		StartCoroutine(WaitCoroutine());
	}

	private void EndErrorProfiling()
	{
		recordData = false;
		markerParent.SetActive(false);
		currentMarkerIndex = -1;
		foreach (GameObject m in markers)
		{
			m.SetActive(false);
		}
		EyeTrackingExperimentController.Instance.OnErrorProfilingEnded();
		errorCounter++;
	}

	private void FixedUpdate()
	{
		if (recordData && currentMarkerIndex > -1 && currentMarkerIndex < markers.Count)
		{
			timestamp = Time.frameCount;

			var eyeGazeProvider = CoreServices.InputSystem?.EyeGazeProvider;

			if (eyeGazeProvider != null && eyeGazeProvider.IsEyeTrackingEnabledAndValid)
			{

				markerPos = markers[currentMarkerIndex].transform.position;
				gazeOrigin = eyeGazeProvider.GazeOrigin;
				gazeDirectionNormalised = eyeGazeProvider.GazeDirection;
				gazeDirectionNormalised.Normalize();

			}
			else
			{
				Debug.LogError("Eye Gaze Invalid!");
			}

			gameObject.GetComponent<LineRenderer>().SetPositions(new Vector3[] { gazeOrigin, gazeOrigin + (gazeDirectionNormalised * 5) });

			profilingData = new ErrorProfilingData(timestamp, currentMarkerIndex, markerPos, gazeOrigin, gazeDirectionNormalised);

			// now we need to store this gaze data in a JSON
			string jsonString = JsonConvert.SerializeObject(profilingData, Formatting.None) + "\n";

			Debug.Log(jsonString); // lets test log it and see how it is
			if (!File.Exists(EyeTrackingExperimentController.Instance.GetErrorProfilingJSONWithPath(errorCounter)))
			{
				File.Create(EyeTrackingExperimentController.Instance.GetErrorProfilingJSONWithPath(errorCounter)).Close();
				// we should close it here to avoid any sharing violations
			}
			File.AppendAllText(EyeTrackingExperimentController.Instance.GetErrorProfilingJSONWithPath(errorCounter), jsonString);
		}
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.green;
		Gizmos.DrawRay(gazeOrigin, gazeDirectionNormalised);
	}

	private IEnumerator WaitCoroutine()
	{
		yield return new WaitForSeconds(markerDuration);
		ShowNextMarker();
	}
}

