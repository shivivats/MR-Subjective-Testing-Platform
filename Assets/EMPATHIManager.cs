using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

// let us store one record per question
public struct EMPATHIRatingRecord
{
    public string userId { get; set; }
    public string sequence { get; set; }
    public string questionHeading { get; set; }
    public string questionDescription { get; set; }
    public string rating { get; set; }
    public string timestamp { get; set; }

    public EMPATHIRatingRecord(string userId, string sequence, string questionHeading, string questionDescription,
        string rating, string timestamp)
    {
        this.userId = userId;
        this.sequence = sequence;
        this.questionHeading = questionHeading;
        this.questionDescription = questionDescription;
        this.rating = rating;
        this.timestamp = timestamp;
    }
}

public class EMPATHIManager : MonoBehaviour
{
    public static EMPATHIManager Instance { get; private set; }

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

    public bool useFixedYOffsetForDistance;

    public float yOffset;

    [HideInInspector] public Mesh[] currentQualityMeshes;

    [Header("INFORMATION DIALOGUES")] public GameObject welcomeDialogueParent;
    public GameObject experienceGalleryParent;
    public GameObject introductionDialogueParent;
    public GameObject closingMessageDialogue;

    [Header("FEEDBACK")] public GameObject feedbackUIParent;
    public GameObject feedbackConfirmationDialogueParent;
    public TextMeshPro feedbackUITextHeader;
    public TextMeshPro feedbackUITextDescription;

    [Header("RATING BUTTONS")] public GameObject[] ratingButtons;
    public GameObject submitRatingButton;
    public Material defaultMaterial;
    public Material pressedMaterial;

    [Header("POINT CLOUD")] public GameObject currentPointCloudObject;

    [Header("INFORMATION")] public List<string> questionTexts;

    private int currentlySelectedRating = -1;

    private string pcToBePlayedNext;

    private int currentFeedbackQuestion = 0;


    private void Start()
    {
        welcomeDialogueParent.SetActive(true);
        experienceGalleryParent.SetActive(false);
        introductionDialogueParent.SetActive(false);
        closingMessageDialogue.SetActive(false);

        feedbackUIParent.SetActive(false);
        feedbackConfirmationDialogueParent.SetActive(false);

        currentPointCloudObject.SetActive(false);
        currentFeedbackQuestion = 0;
    }

    public void OnClick_WelcomeDialogueStart()
    {
        // hide the welcome dialogue
        welcomeDialogueParent.SetActive(false);

        // show the gallery
        experienceGalleryParent.SetActive(true);
    }

    public void OnClick_ExperienceGalleryPC(string pcName)
    {
        // hide the experience gallery
        experienceGalleryParent.SetActive(false);

        pcToBePlayedNext = pcName;

        // show the Introduction and Orientation Message
        introductionDialogueParent.SetActive(true);
    }

    public void OnClick_IntroductionDialogueButton()
    {
        // hide the introduction/orientation dialogue
        introductionDialogueParent.SetActive(false);

        // start the PC playback
        PlayNextUpPC();
    }

    private void PlayNextUpPC()
    {
        if (pcToBePlayedNext == "ReadyForWinter")
        {
            // load ready for winter
            PointCloudsLoader.Instance.LoadNextPointClouds(PCObjectType.ReadyForWinter,
                new QualityRepresentation(EncoderType.VPCC, "raw"));


            // get the meshes to set them on the pc gameobject
            currentQualityMeshes = PointCloudsLoader.Instance.GetPCObjectFromType(PCObjectType.ReadyForWinter)
                .pointClouds[new QualityRepresentation(EncoderType.VPCC, "raw")];

            // set the readyforwinter meshes
            currentPointCloudObject.GetComponent<EMPATHI_AnimatePointCloud>().CurrentMeshes = currentQualityMeshes;

            currentPointCloudObject.GetComponent<EMPATHI_AnimatePointCloud>().SetAnimate(false, true);

            // show the PC
            // play the PC is done automatically
            currentPointCloudObject.SetActive(true);
        }
        else
        {
            Debug.Log("PC not implemented!");
        }
    }

    public void OnPointCloudPlaybackFinished()
    {
        // hide the PC
        currentPointCloudObject.SetActive(false);

        // unload the PC
        PointCloudsLoader.Instance.UnloadPCQualityRepresentation(PCObjectType.ReadyForWinter,
            new QualityRepresentation(EncoderType.VPCC, "raw"));

        // show the feedback UI
        ShowFeedbackUI();
    }

    private void ShowFeedbackUI()
    {
        // show the feedback UI
        feedbackUIParent.SetActive(true);

        // show the feedback UI text based on the current feedback question number
        feedbackUITextHeader.text = questionTexts[currentFeedbackQuestion];
        feedbackUITextDescription.text = questionTexts[currentFeedbackQuestion + 1];

        Debug.Log("feedback qn counter" + currentFeedbackQuestion);
    }

    public void OnClick_CurrentFeedbackSubmitted()
    {
        Debug.Log("feedback qn counter" + currentFeedbackQuestion);
        Debug.Log("currently selected rating: " + currentlySelectedRating);


        // finalize the current rating
        if (currentlySelectedRating != -1)
        {
            // log the feedback in the CSV based on the current feedback question
            StoreQuestionFeedback(feedbackUITextHeader.text, feedbackUITextDescription.text,
                GetCurrentRating().ToString());

            // reset the visual state of all the buttons
            ResetRatingButtons();

            // move the counter up twice because we skip two elements in the list
            currentFeedbackQuestion++;
            currentFeedbackQuestion++;

            if (currentFeedbackQuestion >= questionTexts.Count)
            {
                OnFeedbackComplete();
            }
            else
            {
                // show the next question in the feedback
                ShowFeedbackUI();
            }
        }
    }

    public void OnRatingButtonPressed(int rating)
    {
        // get the button pressed based on rating
        GameObject pressedButton = ratingButtons[rating - 1];

        // set all others inactive
        ResetRatingButtons();

        // change its quad material to presed material
        GameObject pressedButtonQuad = pressedButton.transform.Find("BackPlate").Find("Quad").gameObject;
        pressedButtonQuad.GetComponent<MeshRenderer>().material = pressedMaterial;

        // update currently selected rating value
        currentlySelectedRating = rating;
    }

    private void ResetRatingButtons()
    {
        foreach (var button in ratingButtons)
        {
            GameObject quad = button.transform.Find("BackPlate").Find("Quad").gameObject;
            quad.GetComponent<MeshRenderer>().material = defaultMaterial;
        }

        currentlySelectedRating = -1;
    }

    private int GetCurrentRating()
    {
        if (currentlySelectedRating != -1)
            return currentlySelectedRating;
        else
            Debug.LogError("Rating is still -1! This should never be allowed.");
        return -1;
    }

    private void StoreQuestionFeedback(string questionHeader, string questionDescription, string rating)
    {
        Debug.Log("saved response!");
    }


    public void OnFeedbackComplete()
    {
        // hide the feedback UI
        feedbackUIParent.SetActive(false);

        // show confirmation of feedback
        feedbackConfirmationDialogueParent.SetActive(true);
    }

    public void OnClick_ExitButton()
    {
        // hide the feedback confirmation
        feedbackConfirmationDialogueParent.SetActive(false);

        // show the exit message
        closingMessageDialogue.SetActive(true);
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

        //UnloadRehearsalPointClouds();
    }
}