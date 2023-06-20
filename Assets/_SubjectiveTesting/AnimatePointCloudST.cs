using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatePointCloudST : AnimatePointCloudBase
{
	protected override void AdditionalFixedUpdate()
	{
        if(animate && currentIndex == CurrentMeshes.Length - 1)
        {
		    SetAnimate(true);
		    Debug.Log("ST Sequence ended.");
			STManager.Instance.OnCurrentSequenceEnded();
		}
	}

	public override void SetIsMesh(bool isMesh)
	{
		this.isMesh = isMesh;
	}

	public void ActivateObject()
	{
		// show the gameobject

		// set proper sequence

		// start from the beginning
		SetAnimate(false, true);
	}

	protected override void AdditionalSetAnimate()
	{
		if (PointCloudsLoader.Instance.isTestingScene)
		{
			if (CurrentMeshes.Length > 0)
			{
				GetComponent<MeshFilter>().mesh = CurrentMeshes[0];
			}
			gameObject.GetComponent<MeshRenderer>().enabled = animate;
		}
	}
}
