using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class EyeTrackingDataRead : MonoBehaviour
{
    public string jsonDir;

    // just manually get the filepath for now
    public string testPath;

    private Vector3 gazeOrigin;
    private Vector3 gazeDirectionNormalised;
    private Vector3 pcPosition;
    private float pcRotation;

    public GameObject pcObject;

    List<string> jsonLines;
    int currentIndex = 0;

    public float shownGazeLength = 5f;

    /* This is just a test script to read the JSON file data.
     */

    bool read = false;

    // Start is called before the first frame update
    void Start()
    {
        
        jsonLines = File.ReadLines(testPath).ToList();
		//StartCoroutine(WaitCoroutine());
		read = true;

		pcObject.GetComponent<EyeTrackingAnimationController>().currentMeshes = EyeTrackingExperimentController.Instance.GetPCMeshesFromName(GetPcFromFilename());
		pcObject.SetActive(true);
		pcObject.GetComponent<EyeTrackingAnimationController>().SetAnimate(true, true);

		LineRenderer lineRenderer = gameObject.GetComponent<LineRenderer>();
		lineRenderer.material = new Material(Shader.Find("Sprites/Default"));

		//float alpha = 1.0f;
		// start colour is red
		// end colour is green
		//Gradient gradient = new Gradient();
		//gradient.SetKeys(
		//	new GradientColorKey[] { new GradientColorKey(UnityEngine.Color.green, 0.0f), new GradientColorKey(UnityEngine.Color.red, 1.0f) },
		//	new GradientAlphaKey[] { new GradientAlphaKey(alpha, 0.0f), new GradientAlphaKey(alpha, 0.0f) });
        // lineRenderer.colorGradient = gradient;

	}

    private IEnumerator WaitCoroutine()
    {
        yield return new WaitForSeconds(10);
        read = true;
    }

    string GetPcFromFilename()
    {
        // ./Assets/_EyeTracking/JSON/GazeData_User51_PCBlueSpin.json
        return Path.GetFileNameWithoutExtension(testPath).Split("_").Last().Remove(0, 2);

	}

	void ReadDataPerPointcloud()
    {
        // get a point cloud name, from the array
        // get all GazeData_UserN_PCXYZ for that point cloud
        // render the PC
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if(read)
        {
            if(currentIndex < jsonLines.Count)
            {
                UserGazeData gazeData = JsonConvert.DeserializeObject<UserGazeData>(jsonLines[currentIndex]);
                currentIndex++;

                pcPosition = new Vector3(gazeData.pcWorldPositionX, gazeData.pcWorldPositionY, gazeData.pcWorldPositionZ);
                pcRotation = gazeData.objectRotationY;
                gazeOrigin = new Vector3(gazeData.gazeOriginX, gazeData.gazeOriginY, gazeData.gazeOriginZ);
                gazeDirectionNormalised = new Vector3(gazeData.gazeDirectionNormalisedX, gazeData.gazeDirectionNormalisedY, gazeData.gazeDirectionNormalisedZ);
                
                gameObject.GetComponent<LineRenderer>().SetPositions(new Vector3[] { gazeOrigin, gazeOrigin + (gazeDirectionNormalised * shownGazeLength) });

				pcObject.transform.position = pcPosition;

				pcObject.transform.rotation = Quaternion.Euler(pcObject.transform.rotation.eulerAngles.x, pcRotation, pcObject.transform.rotation.eulerAngles.z);


				Debug.Log("Read gaze data " + gazeOrigin + ", " + gazeDirectionNormalised + " from user data");
            }
            else
            {
                currentIndex = 0;
            }
        }
    }

    

}
