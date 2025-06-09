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

	private Mesh[] meshes;

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
		Debug.Log(currSequence.QualityRepresentation.encoder);
		Debug.Log(currSequence.QualityRepresentation.quality);


		meshes = currPcObject.pointClouds[currSequence.QualityRepresentation];

		animateComp.SetIsMesh(false);
		animateComp.gameObject.transform.GetChild(0).GetComponent<MeshRenderer>().materials = new Material[] { STManager.Instance.GetMaterialFromRepresentation(currSequence.RepresentationType) };

		animateComp.CurrentMeshes = meshes;

		STManager.Instance.SetCurrentPCDistance(currSequence.Distance);

		if (currentSequenceIndex < sequences.Count)
		{
			string qualityString = EyeTrackingUtils.Instance.EncoderToString(currSequence.QualityRepresentation.encoder) + "_" + currSequence.QualityRepresentation.quality;

			string distanceString = ((int)(currSequence.Distance * 100f)).ToString();

			string outTextFormatted = MakeSequenceString(
												PointCloudsLoader.Instance.GetPCNameFromType(currSequence.ObjectType),
												qualityString,
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
