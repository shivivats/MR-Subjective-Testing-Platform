using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Threading;
using System.IO;
using System.Xml.Linq;
using UnityEngine.XR;
using Unity.VisualScripting;

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
    public List<STTaskData> taskData;

    public bool randomiseTasks = true;

    public bool useFixedYOffsetForDistance;

    public float yOffset;

    [HideInInspector]
    public Mesh[] currentQualityMeshes;

    public List<STTaskBase> CurrentTasks { get => currentTasks; set => currentTasks = value; }
    private List<STTaskBase> currentTasks = new List<STTaskBase>();
    private int currentTaskIndex = 0;

    private int nextSequenceIndex = 0;
    private List<STSequence> currentSequencesInTask = new List<STSequence>();

    public int bufferLength = 3;

    public bool isRehearsal = true;

    private Dictionary<int, Mesh[]> rehearsalPointClouds = new Dictionary<int, Mesh[]>();
    private int currentRehearsalQuality = -1;
    AnimatePointCloudST animateComp;

    private void MakeRandomisedSequencesFromTaskInput()
    {
		List<STSequence> currentSequences = new List<STSequence>();

		currentSequences.Add(new STSequence(PCObjectType.CasualSquat, PointCloudMaterialRepresentation.Square, 2, new QualityRepresentation(EncoderType.VPCC, "r02")));
		currentSequences.Add(new STSequence(PCObjectType.FlowerDance, PointCloudMaterialRepresentation.Square, 2, new QualityRepresentation(EncoderType.GPCC_TRISOUP, "r02")));
		currentSequences.Add(new STSequence(PCObjectType.FlowerDance, PointCloudMaterialRepresentation.Square, 2, new QualityRepresentation(EncoderType.VPCC, "r04")));
		currentSequences.Add(new STSequence(PCObjectType.ReadyForWinter, PointCloudMaterialRepresentation.Square, 2, new QualityRepresentation(EncoderType.VPCC, "r04")));
		
        System.Random r = new System.Random();
		currentSequences = currentSequences.OrderBy(x => r.Next()).ToList();
		currentSequencesInTask = new List<STSequence>(currentSequences.Take(bufferLength));
		Debug.Log("Made " + currentSequences.Count + " number of sequences");

		CurrentTasks.Add(new STTaskBase(currentSequences));
		//currentSequencesInTask = new List<STSequence>(currentSequences.Take(bufferLength));
		Debug.Log(currentSequencesInTask.Count);
		nextSequenceIndex = bufferLength;
		LoadCurrentSequences();

		//foreach (var task in taskData)
		//{
		//	//task.m_qualityRepresentations = MakeRepresentationsFromQualitiesAndEncoders(task);

		//          List<STSequence> currentSequences = new List<STSequence>();

		//          List<PCObjectType> pcObjectTypes = null;
		//          List<PointCloudMaterialRepresentation> pointCloudRepresentations = null;

		//          Debug.Log("Task Types Selected " + task.m_types);
		//          Debug.Log("Task Representations selected " + task.m_representations);

		//          pcObjectTypes = Enum.GetValues(typeof(PCObjectType)).Cast<PCObjectType>().Where(e => task.m_types.HasFlag(e) && e != 0).ToList();
		//          pointCloudRepresentations = Enum.GetValues(typeof(PointCloudMaterialRepresentation)).Cast<PointCloudMaterialRepresentation>().Where(e => task.m_representations.HasFlag(e)).ToList();

		//          Debug.Log("pcObjectTypes " + pcObjectTypes.Count + " " + pcObjectTypes.ToArray().ToString());
		//          Debug.Log("pointCloudRepresentations " + pointCloudRepresentations.Count + " " + pointCloudRepresentations.ToArray().ToString());

		//          foreach (PCObjectType objectType in pcObjectTypes)
		//          {
		//              foreach (PointCloudMaterialRepresentation representation in pointCloudRepresentations)
		//              {
		//                  foreach (float distance in task.m_distances)
		//                  {
		//                      foreach (QualityRepresentation qual in task.m_qualityRepresentations)
		//                      {
		//                          {
		//                              currentSequences.Add(new STSequence(objectType, representation, distance, qual));
		//                          }
		//                      }
		//                  }
		//              }
		//          }

		//          System.Random r = new System.Random();
		//          currentSequences = currentSequences.OrderBy(x => r.Next()).ToList();
		//          currentSequencesInTask = new List<STSequence>(currentSequences.Take(bufferLength));
		//          Debug.Log("Made " + currentSequences.Count + " number of sequences");

		//          CurrentTasks.Add(new STTaskBase(currentSequences));
		//          //currentSequencesInTask = new List<STSequence>(currentSequences.Take(bufferLength));
		//          Debug.Log(currentSequencesInTask.Count);
		//          nextSequenceIndex = bufferLength;
		//          LoadCurrentSequences();
		//      }


	}

    private void OnDestroy()
    {

        foreach (PointCloudObject pco in PointCloudsLoader.Instance.pcObjects)
        {
            foreach (QualityRepresentation qr in pco.pointClouds.Keys)
            {
                pco.UnloadAssetsFromQualityRepresentation(qr);
            }
        }

        UnloadRehearsalPointClouds();
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
            if (isRehearsal)
            {
                animateComp = STSecondaryManager.Instance.currentGameObject.GetComponent<AnimatePointCloudST>();
                StartRehearsalTask();
                STSecondaryManager.Instance.startButton.SetActive(false);
            }
            else
            {
                CurrentTasks[currentTaskIndex].SetupTask();
                STSecondaryManager.Instance.startButton.SetActive(false);
            }
        }
    }

    private void StartRehearsalTask()
    {
        SetupNextRehearsalSequence();
    }

    private void SetupNextRehearsalSequence()
    {
        currentRehearsalQuality += 2;
        if (currentRehearsalQuality > 5)
        {
            OnFullTaskEnded();
            return;
        }

        animateComp.SetIsMesh(false);
        animateComp.gameObject.transform.GetChild(0).GetComponent<MeshRenderer>().materials = new Material[] { GetMaterialFromRepresentation(PointCloudMaterialRepresentation.Square) };
        Debug.Log("setting up for current rehearsal quality " + currentRehearsalQuality);
        animateComp.CurrentMeshes = rehearsalPointClouds[currentRehearsalQuality];
        SetCurrentPCDistance(2f);
        animateComp.ActivateObject();
    }

    public void RandomiseTasks()
    {
        System.Random r = new System.Random();
        CurrentTasks = CurrentTasks.OrderBy(x => r.Next()).ToList();
    }

    void Start()
    {
        isRehearsal = true;
        LoadRehearsalPointClouds();

        MakeRandomisedSequencesFromTaskInput();
        if (randomiseTasks) RandomiseTasks();

        STSecondaryManager.Instance.SetStartButtonText();
        STFeedbackManager.Instance.EnableFeedbackUI(false);
        STFeedbackManager.Instance.EnableNextVideoButton(false);
    }

    private void LoadRehearsalPointClouds()
    {
        string pcname = "ElegantWave";
        string pathPrefix = "PointClouds/Rehearsal";
        int[] qualities = new int[3] { 1, 3, 5 };

        string meshesPath;
        foreach (int q in qualities)
        {
            meshesPath = Path.Combine(pathPrefix, pcname, "r0" + q.ToString());
            rehearsalPointClouds.Add(q, Resources.LoadAll<Mesh>(meshesPath));
            Debug.Log("Loaded Elegant Wave quality" + q.ToString() + " from  " + meshesPath);
        }
    }

    private void UnloadRehearsalPointClouds()
    {
        foreach (int k in rehearsalPointClouds.Keys)
        {
            foreach (Mesh m in rehearsalPointClouds[k])
            {
                Resources.UnloadAsset(m);
                
            }
            Debug.Log("UN Loaded Elegant Wave quality" + k.ToString());
        }
    }

    private void LoadCurrentSequences()
    {
        foreach (STSequence sequence in currentSequencesInTask)
        {
            PointCloudsLoader.Instance.LoadNextPointClouds(sequence.ObjectType, sequence.QualityRepresentation);
        }
    }

    private void UnloadFirstSequence()
    {
        PointCloudsLoader.Instance.UnloadPCQualityRepresentation(currentSequencesInTask.First().ObjectType, currentSequencesInTask.First().QualityRepresentation);
        currentSequencesInTask.Remove(currentSequencesInTask.First());
    }

    public void OnFeedbackSubmitted()
    {
        if (!isRehearsal)
            UpdateSequences();
    }

    private void UpdateSequences()
    {
        // return the next 5 sequences so we may load them
        // this will always return the next 5, so chances are that 4 of these are already loaded
        // call this async after the very first call
        //isProcessing = true;
        UnloadFirstSequence();

        if (nextSequenceIndex < currentTasks[currentTaskIndex].sequences.Count)
        {
            currentSequencesInTask.Add(currentTasks[currentTaskIndex].sequences[nextSequenceIndex]);
            LoadCurrentSequences();

            nextSequenceIndex++;
            //isProcessing = false;
        }
    }



    public void OnCurrentSequenceEnded()
    {
        STFeedbackManager.Instance.EnableFeedbackUI(true);
    }

    private void OnRehearsalTaskEnded()
    {

        // unload all the rehearsal meshes
        foreach (int qual in rehearsalPointClouds.Keys)
        {
            foreach (Mesh m in rehearsalPointClouds[qual])
            {
                Resources.UnloadAsset(m);
            }
        }

        isRehearsal = false;

        UnloadRehearsalPointClouds();


        STSecondaryManager.Instance.SetStartButtonText();

        Debug.Log("rehearsal ended");

        StartNextTask();
    }

    public void OnFullTaskEnded()
    {
        if (isRehearsal)
        {
            OnRehearsalTaskEnded();
        }
        if (currentTaskIndex < CurrentTasks.Count - 1)
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
        if (isRehearsal)
        {
            // start next rehearsal sequence
            SetupNextRehearsalSequence();
        }
        else
        {
            CurrentTasks[currentTaskIndex].SetupNextSequence();
        }
    }

    public string GetCurrentSequenceString()
    {
        return CurrentTasks[currentTaskIndex].currentSequenceString;
    }

    public void SetDisplayString(string displayString)
    {
        STSecondaryManager.Instance.displayText.text = displayString;
    }

    public Material GetMaterialFromRepresentation(PointCloudMaterialRepresentation representation)
    {
        return STSecondaryManager.Instance.GetMaterialFromRepresentation(representation);
    }

}
