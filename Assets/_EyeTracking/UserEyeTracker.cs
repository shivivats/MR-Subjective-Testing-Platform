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

	// Start is called before the first frame update
	void Start()
    {
        
    }

    // Update is called once per frame
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

			

			// 1. get the current position of the eyes

			// 2. get the position of the hit on the object
			// 2.1 Check if the currently hit object is "tagged" a custom tag, for eg. <pc object>

			// 3. try to log "where" exactly the ray hit the object
			// 3.1 we can of course get the position in Unity 3D space but we also need the context of where on the person's body this hit.
			// 3.2 we can try drawing a small sphere at the hit point (but then what?)

			// THE ISSUE: point clouds are complex and their mesh representations are made of points,
			// and Unity's mesh collider system CAN NOT generate colliders for point clouds, only for triangle meshes.
			// Without a collider, the eye gaze system will not work.

			// FOR NOW: I try to do it with a capsule collider well fitted to the PC.
			// and then when the capsule is hit, I can raycast further to check for some intersection with the mesh
			// But the issue arises again that there isn't a straightforward implementation for raycasting with PC Meshes.
			
		}
    }
}
