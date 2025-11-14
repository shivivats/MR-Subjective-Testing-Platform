using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EMPATHI_AnimatePointCloud : AnimatePointCloudBase
{
    private int numPlaybacks = 3;

    private int currentPlaybackCounter = 0;
    
    protected override void AdditionalFixedUpdate()
    {
        if (currentIndex == CurrentMeshes.Length - 1)
        {
            if (currentPlaybackCounter > numPlaybacks)
            {
                SetAnimate(true);
                Debug.Log("EMPATHI PC Playback ended.");
                EMPATHIManager.Instance.OnPointCloudPlaybackFinished();
                childObject.GetComponent<MeshFilter>().mesh = null;
            }
            else
            {
                SetAnimate(false,true);
                currentPlaybackCounter++;
            }
                
        }
    }
}
