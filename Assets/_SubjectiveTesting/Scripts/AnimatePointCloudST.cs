using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatePointCloudST : AnimatePointCloudBase
{


	protected override void AdditionalFixedUpdate()
	{
		if (currentIndex == CurrentMeshes.Length - 1)
		{
			if (firstSequence)
			{
				currentIndex = -1;
                Debug.Log("ST Sequence first loop complete.");
                firstSequence = false;
			}
			else
			{
				SetAnimate(true);
				Debug.Log("ST Sequence ended.");
				STManager.Instance.OnCurrentSequenceEnded();
				firstSequence = true;
				childObject.GetComponent<MeshFilter>().mesh = null;
                RotatePCRandomly();
			}
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
		//if (PointCloudsLoader.Instance.isTestingScene)
		//{
		//	if (CurrentMeshes.Length > 0)
		//	{
		//		GetComponent<MeshFilter>().mesh = CurrentMeshes[0];
		//	}
		//	gameObject.GetComponent<MeshRenderer>().enabled = animate;
		//}

		gameObject.SetActive(animate);

	}
}
