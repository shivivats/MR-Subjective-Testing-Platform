/*
* The eye tracking work in this platform is heavily based on the eye traking work by CWI which can be found here: https://github.com/zhouzhouha/PointCloud_EyeTracking/.
* Their overall concept is implemented here by Shivi Vats.
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Utilities;
using Microsoft.MixedReality.Toolkit;
using System.IO;
using Newtonsoft.Json.Linq;
using Unity.VisualScripting;

public class EyeTrackingDataRecord : MonoBehaviour
{
    // The object will rotate halfway through the sequence at a Y angle. we need to store this Y angle. The rotation is done in the AnimatePointCloudEyeTracking script.
    // We need to store in JSON: pcName, pcIndex, pcFilename, timestamp, gazeOrigin, gazeDirectionNormalised, cameraToWorldTransformationMatrix, objectRotationY

    UserGazeData gazeData;

    public GameObject pointCloudObject;

    //private StreamWriter jsonStreamWriter;

    private string pcName;
    private int pcFrameIndex;
    // i think we dont need the timestamp actually, because we can calc everything using the current frame index
    //public int timestamp; 
    private Vector3 gazeOrigin;
    private Vector3 gazeDirectionNormalised;

    private Vector3 pcWorldPosition;

    //private Matrix4x4 cameraToWorldTransformationMatrix;
    private float objectRotationY;

    private int userId;
    // quality is always raw, and thus the codec doesn't come into play either.

    
    void FixedUpdate()
    {
        // we can record the JSON data every frame by running it in FixedUpdate
        if(EyeTrackingExperimentController.Instance.playing)
        {
            // pcName and pcIndex are gotten from the ExperimentController
            // we can get the current frame of the PC from the animationcontroller and then we'd have to reconstruct the pcfilename from the frame
            pcName = EyeTrackingExperimentController.Instance.GetCurrentPcName();
            pcFrameIndex = pointCloudObject.GetComponent<EyeTrackingAnimationController>().currentIndex;

            var eyeGazeProvider = CoreServices.InputSystem?.EyeGazeProvider;
            //if (eyeGazeProvider.IsEyeCalibrationValid != true) // have to explicitly not equals true here because this is a nullable type
            //{
            //    Debug.LogError("Eye Gaze Calibration not performed for the user!");
            //}
            if (eyeGazeProvider != null && eyeGazeProvider.IsEyeTrackingEnabledAndValid)
            {
                gazeOrigin = eyeGazeProvider.GazeOrigin;
                gazeDirectionNormalised = eyeGazeProvider.GazeDirection;
                gazeDirectionNormalised.Normalize();
                //cameraToWorldTransformationMatrix = Camera.main.cameraToWorldMatrix;
                pcWorldPosition = pointCloudObject.transform.position;
				objectRotationY = pointCloudObject.GetComponent<EyeTrackingAnimationController>().currentRotation;
            }
            else
            {
                Debug.LogError("Eye Gaze Invalid!");
            }

            gameObject.GetComponent<LineRenderer>().SetPositions(new Vector3[] { gazeOrigin, gazeOrigin + (gazeDirectionNormalised * 5) });

            userId = EyeTrackingExperimentController.Instance.GetCurrentUserID();
            gazeData = new UserGazeData(pcName, pcFrameIndex, gazeOrigin, gazeDirectionNormalised, pcWorldPosition, objectRotationY, userId);

            // now we need to store this gaze data in a JSON
            string jsonString = JsonConvert.SerializeObject(gazeData, Formatting.None) + "\n";
        
            Debug.Log(jsonString); // lets test log it and see how it is
            if(!File.Exists(EyeTrackingExperimentController.Instance.GetJSONFilenameWithPath()))
            {
                File.Create(EyeTrackingExperimentController.Instance.GetJSONFilenameWithPath()).Close();
                // we should close it here to avoid any sharing violations
            }
            File.AppendAllText(EyeTrackingExperimentController.Instance.GetJSONFilenameWithPath(), jsonString);
         }
            
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green; 
        Gizmos.DrawRay(gazeOrigin, gazeDirectionNormalised);
    }

}
