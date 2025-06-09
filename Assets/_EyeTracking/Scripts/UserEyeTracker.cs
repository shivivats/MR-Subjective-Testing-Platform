/*
* The eye tracking work in this platform is heavily based on the eye traking work by CWI which can be found here: https://github.com/zhouzhouha/PointCloud_EyeTracking/.
* Their overall concept is implemented here by Shivi Vats.
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Utilities;
using Microsoft.MixedReality.Toolkit;

public class UserEyeTracker : MonoBehaviour
{

	public GameObject smallSpherePrefab;

	private float defaultDistanceInMeters = 2.0f;

	public MeshRenderer targetMeshRenderer;

	
	void Start()
    {
        
    }

    
    void Update()
    {
		var eyeGazeProvider = CoreServices.InputSystem?.EyeGazeProvider;

		if (eyeGazeProvider!=null && eyeGazeProvider.IsEyeTrackingEnabled)
        {
			Vector3 spherePos = eyeGazeProvider.GazeOrigin + eyeGazeProvider.GazeDirection.normalized * defaultDistanceInMeters;

			//Debug.Log("Eye gaze not null.");
			if (EyeTrackingTarget.LookedAtEyeTarget != null) // target needs to have EyeTrackingTarget script attached for this to work.
			{
				//Debug.Log("Some target looked at.");
				Vector3 currentPosition = eyeGazeProvider.GazeOrigin;
				Debug.Log("Eye position: " + currentPosition);

				Vector3 hitPosition = eyeGazeProvider.HitPosition;
				Debug.Log("Hit position on target collider: " + hitPosition);

				spherePos = hitPosition;
				GameObject newSpawnedSphere = GameObject.Instantiate(smallSpherePrefab, spherePos, Quaternion.identity);
				Destroy(newSpawnedSphere, 5.0f);
			}
			
		}
    }
}
