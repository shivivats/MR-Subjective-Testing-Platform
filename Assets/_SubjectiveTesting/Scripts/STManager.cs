using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using System.Linq;

public class STManager : MonoBehaviour
{
	public static STManager Instance { get; private set; }

	private void Awake()
	{
		// Singleton code - If there is an instance, and it's not this, then delete this instance

		if (Instance != null && Instance != this)
		{
			Destroy(this);
		}
		else
		{
			Instance = this;
			DontDestroyOnLoad(gameObject);
		}
	}

	[SerializeField]
    public List<STTaskData> tasks;

	public bool randomiseTasks = true;

	public bool useFixedYOffsetForDistance;

	public float yOffset;

	[HideInInspector]
	public Mesh[] currentQualityMeshes;

	private List<STTaskBase> currentTasks = new List<STTaskBase>();
	private int currentTaskIndex = 0;

	private void MakeSequencesFromTaskInput()
	{
		foreach (var task in tasks) 
		{
			List<STSequence> currentSequences = new List<STSequence>();

			List<PCObjectType> pcObjectTypes = null;
			List<PointCloudRepresentation> pointCloudRepresentations = null;

			Debug.Log("Task Types Selected " + task.m_types);
			Debug.Log("Task Representations selected " + task.m_representations);

			pcObjectTypes = Enum.GetValues(typeof(PCObjectType)).Cast<PCObjectType>().Where(e => task.m_types.HasFlag(e) && e!=0).ToList();
			pointCloudRepresentations = Enum.GetValues(typeof(PointCloudRepresentation)).Cast<PointCloudRepresentation>().Where(e => task.m_representations.HasFlag(e)).ToList();

			Debug.Log("pcObjectTypes " + pcObjectTypes.Count + " " +pcObjectTypes.ToArray().ToString());
			Debug.Log("pointCloudRepresentations " + pointCloudRepresentations.Count + " " + pointCloudRepresentations.ToArray().ToString());

			foreach (PCObjectType objectType in pcObjectTypes)
			{
				foreach (PointCloudRepresentation representation in pointCloudRepresentations)
				{
					foreach(float distance in task.m_distances) 
					{
						foreach(Vector2 quality in task.m_qualities)
						{
							currentSequences.Add(new STSequence(objectType, representation, distance, (int)quality.x, (int)quality.y));
						}
					}
				}
			}

			System.Random r = new System.Random();
			currentSequences = currentSequences.OrderBy(x => r.Next()).ToList();

			currentTasks.Add(new STTaskBase(currentSequences));
		}
	}

	public void StartNextTask()
	{
		// basically if the button isn't active then we just try to activate the button first and wait for the button press to start the next task
		if (!STSecondaryManager.Instance.startButton.activeSelf)
		{
			STSecondaryManager.Instance.startButton.SetActive(true);
		}
		else
		{
			currentTasks[currentTaskIndex].SetupTask();
			STSecondaryManager.Instance.startButton.SetActive(false);
		}
	}

	public void RandomiseTasks()
	{
		System.Random r = new System.Random();
		currentTasks = currentTasks.OrderBy(x => r.Next()).ToList();
	}

	void Start()
    {
		PointCloudsLoader.Instance.LoadPointCloudsAndMeshes();
		if (!PointCloudsLoader.Instance.loadMeshes)
		{
			MaterialChanger.Instance.meshButton.SetActive(false);
		}

		MakeSequencesFromTaskInput();
		RandomiseTasks();

		STFeedbackManager.Instance.EnableFeedbackUI(false);

	}

	public void OnCurrentSequenceEnded()
	{
		STFeedbackManager.Instance.EnableFeedbackUI(true);
	}

	public void OnFullTaskEnded()
	{
		if (currentTaskIndex < currentTasks.Count - 1)
		{
			StartNextTask();
			currentTaskIndex++;
			STSecondaryManager.Instance.SetStartButtonText();
		}
		STSecondaryManager.Instance.OnFullTaskEnded();
	}


	public void SetCurrentPCDistance(float currentDistance)
	{
		STSecondaryManager.Instance.SetCurrentPCDistance(currentDistance, useFixedYOffsetForDistance, yOffset);
	}

	public void AdvanceCurrentTask()
	{
		currentTasks[currentTaskIndex].SetupNextSequence();
	}

	public string GetCurrentSequenceString()
	{
		return currentTasks[currentTaskIndex].currentSequenceString;
	}

	public void SetDisplayString(string displayString)
	{
		STSecondaryManager.Instance.displayText.text = displayString;
	}

	public Material GetMaterialFromRepresentation(PointCloudRepresentation representation)
	{
		return STSecondaryManager.Instance.GetMaterialFromRepresentation(representation); 
	}

}
