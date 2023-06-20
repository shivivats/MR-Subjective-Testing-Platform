using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class STTaskBase
{
	public List<STSequence> sequences = new List<STSequence>();

	public int currentSequenceIndex = -1;

	protected string qualityPrefix = "r";
	protected string distancePrefix = "d";

	public string currentSequenceString = "";

	private Mesh[] firstMeshes;
	private Mesh[] secondMeshes;

	AnimatePointCloudST animateComp;

	public STTaskBase(List<STSequence> sequences)
	{
		this.sequences = sequences;
	}

	protected void ActivateCurrentPointCloud()
	{
		animateComp.ActivateObject();
		Debug.Log("activating point cloud object ");
	}

	protected string MakeSequenceString(string currentObjectString, string currentQualityString, string currentDistanceString)
	{
		// the sequence string is made like so: 
		// objectname_firstquality_secondquality_distance

		// qualities are r1, r2, r3
		// distances are d100, d250, d500

		// e.g.: loot_r1_r3_d1 
		return currentObjectString + "_" + currentQualityString + "_" + currentDistanceString;
	}

	// this fn does run before every new sequence is played
	public void SetupNextSequence()
	{
		currentSequenceIndex += 1;

		if (currentSequenceIndex == sequences.Count)
		{
			OnTaskEnded();
			return;
		}

		STSequence currSequence = sequences[currentSequenceIndex];
		PointCloudObject currPcObject = PointCloudsLoader.Instance.GetPCObjectFromType(currSequence.ObjectType);

		Debug.Log(currPcObject == null);
		Debug.Log(currSequence.ObjectType);
		Debug.Log(currPcObject.pointClouds == null);
		Debug.Log(currSequence == null);
		Debug.Log(currSequence.FirstQuality);

		if (currSequence.RepresentationType == PointCloudRepresentation.Mesh)
		{
			firstMeshes = currPcObject.meshes[currSequence.FirstQuality];
			firstMeshes = firstMeshes.Take(firstMeshes.Length / 2).ToArray();

			secondMeshes = currPcObject.meshes[currSequence.SecondQuality];
			secondMeshes = secondMeshes.Skip(secondMeshes.Length / 2).ToArray();

			Material[] firstMaterials = currPcObject.meshMaterials[currSequence.FirstQuality];
			firstMaterials = firstMaterials.Take(firstMaterials.Length / 2).ToArray();

			Material[] secondMaterials = currPcObject.meshMaterials[currSequence.SecondQuality];
			secondMaterials = secondMaterials.Skip(secondMaterials.Length / 2).ToArray();

			animateComp.meshMaterials = firstMaterials.Concat(secondMaterials).ToArray();

			animateComp.SetIsMesh(true);
		}
		else
		{
			firstMeshes = currPcObject.pointClouds[currSequence.FirstQuality];
			firstMeshes = firstMeshes.Take(firstMeshes.Length / 2).ToArray();

			secondMeshes = currPcObject.pointClouds[currSequence.SecondQuality];
			secondMeshes = secondMeshes.Skip(secondMeshes.Length / 2).ToArray();

			animateComp.SetIsMesh(false);
			animateComp.gameObject.GetComponent<MeshRenderer>().materials = new Material[] { STManager.Instance.GetMaterialFromRepresentation(currSequence.RepresentationType) };
		}

		animateComp.CurrentMeshes = firstMeshes.Concat(secondMeshes).ToArray();

		STManager.Instance.SetCurrentPCDistance(currSequence.Distance);

		if (currentSequenceIndex < sequences.Count)
		{
			string firstQuality = currSequence.FirstQuality.ToString();
			string secondQuality = currSequence.SecondQuality.ToString();

			string qualityString = firstQuality + "_" + qualityPrefix + secondQuality;
			string distanceString = ((int)(currSequence.Distance * 100f)).ToString();

			string outTextFormatted = MakeSequenceString(
												PointCloudsLoader.Instance.GetPCNameFromType(currSequence.ObjectType),									
												qualityPrefix + qualityString,
												distancePrefix + distanceString);

			currentSequenceString = outTextFormatted;

			STManager.Instance.SetDisplayString(outTextFormatted);

			Debug.Log(outTextFormatted);
		}

		ActivateCurrentPointCloud();
	}

	public void SetupTask()
	{
		animateComp = STSecondaryManager.Instance.currentGameObject.GetComponent<AnimatePointCloudST>();
		currentSequenceIndex = -1;
		SetupNextSequence();
	}

	private void OnTaskEnded()
	{
		STManager.Instance.OnFullTaskEnded();
		Debug.Log("Task ended.");
	}

	void SetSequences(List<STSequence> sequences)
	{
		// sequence data will be obtained from the STManager
		// STManager will exactly tell us which sequences we have
		// it will generate and randomise the sequences

		this.sequences = sequences;	
	}
}
