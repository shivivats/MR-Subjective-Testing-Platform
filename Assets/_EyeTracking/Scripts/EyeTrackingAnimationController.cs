using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class EyeTrackingAnimationController : MonoBehaviour
{
    /* I am not inheriting from the AnimatePointCloudBase class here since we don't have a need for the mesh functionalities so I'll avoid a bit of code here
     * and its not too much repetition anyways.
     */

    public Mesh[] currentMeshes;

    public int currentIndex;

    public bool animate = false;

    public float minRotation = 0.0f;
    public float maxRotation = 180.0f;

    public float currentRotation;

    private Quaternion initialRotation;

    private bool firstSequence = true;

    public GameObject childObject;

    void Start()
    {
        currentIndex = -1;
        initialRotation = gameObject.transform.rotation;
        currentRotation = initialRotation.eulerAngles.y;
		//childObject.transform.localPosition = new Vector3(-1 * childObject.GetComponent<MeshFilter>().mesh.bounds.center.x, 0f, -1 * childObject.GetComponent<MeshFilter>().mesh.bounds.center.z);
		currentRotation = RotatePC();
    }

    // FixedUpdate updates according to a timestep set in Project Settings -> Time -> Fixed Timestep
    // set it to 0.016666 for 60 fps, or 0.0333333 for 30 fps
    void FixedUpdate()
    {
        if (animate)
        {
            if (firstSequence && currentIndex == -1)
            {
                // at the first frame, offset the child
                childObject.transform.localPosition = new Vector3(-1*childObject.GetComponent<MeshFilter>().mesh.bounds.center.x, 0f, -1*childObject.GetComponent<MeshFilter>().mesh.bounds.center.z);
            }

            childObject.GetComponent<MeshFilter>().mesh = currentMeshes[++currentIndex];

            //if(currentIndex == currentMeshes.Length / 2)
            //{
                
            //}

            if(currentIndex == currentMeshes.Length - 1) 
            {
				RestartAnimation();
				if (firstSequence)
                {
                    RestartAnimation();
                    firstSequence = false;
                }
                else
                {
                    // go back to the controller
                    EyeTrackingExperimentController.Instance.OnSequenceEnded();
                    childObject.GetComponent<MeshFilter>().mesh = null;
                    firstSequence = true;
                }
            }
        }
    }

    void RestartAnimation()
    {
        currentIndex = -1;
    }

    public void SetAnimate(bool animate, bool fromStart = false)
    {
        if(fromStart) 
        {
            RestartAnimation();
            RotatePC();
        }

        this.animate = animate;
        Debug.Log("animate set to" + animate);
    }

    public float RotatePC()
    {
        List<float> rotations = new List<float> { 0f, 45f, 90f, 135f, 180f, 225f, 270f, 315f, 360f };
        float randomRotation = rotations.ElementAt(Random.Range(0, 8));

        gameObject.transform.rotation = Quaternion.Euler(gameObject.transform.rotation.eulerAngles.x, randomRotation, gameObject.transform.rotation.eulerAngles.z);

        return randomRotation;
    }

    void ResetRotation()
    {
        gameObject.transform.rotation = initialRotation;
        currentRotation = initialRotation.eulerAngles.y;
    }

}
