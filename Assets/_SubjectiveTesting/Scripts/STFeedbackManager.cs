using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using CsvHelper.Configuration;
using CsvHelper;
using System.Globalization;
using System.IO;
using System.Linq;
using System;
using Microsoft.MixedReality.Toolkit.UI;

public struct RatingRecord
{
	public string userId { get; set; }
	public string sequence { get; set; }
	public string rating { get; set; }
	public string questionnaireId { get; set; }
	public string timestamp { get; set; }

	public RatingRecord(string userId, string sequence, string rating, string questionnaireId, string timestamp)
	{
		this.userId = userId;
		this.sequence = sequence;
		this.rating = rating;
		this.questionnaireId = questionnaireId;
		this.timestamp = timestamp;
	}
}


public class STFeedbackManager : MonoBehaviour
{
	public static STFeedbackManager Instance { get; private set; }

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

	public GameObject[] ratingButtons;
	public GameObject submitRatingButton;

	private int currentlySelectedRating = -1;

	public GameObject nextVideoButton;

	public Material defaultMaterial;
	public Material pressedMaterial;

	int sequenceCounter = 0;

	// Start is called before the first frame update
	void Start()
	{
		currentUserId = GetNewUserId();
		Debug.Log("User ID for current user is " + currentUserId.ToString());

        EnableFeedbackUI(false);
        ResetRatingButtons();

        InitialiseCSV();
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

	private int GetCurrentRating()
	{
		if (currentlySelectedRating != -1)
			return currentlySelectedRating;
		else
			Debug.LogError("Rating is still -1! This should never be allowed.");
		return -1;
	}

	public void OnSubmitButtonPress()
	{
		if (currentlySelectedRating != -1)
		{
			if (!STManager.Instance.isRehearsal)
			{
				StoreSequenceFeedback(STManager.Instance.GetCurrentSequenceString(), GetCurrentRating().ToString());
			}
			//STManager.Instance.AdvanceCurrentTask(); -> we dont advance the task after submitting now, but rather we do it when the specific "next video" button is pressed
			EnableFeedbackUI(false);
			ResetRatingButtons();
			STManager.Instance.OnFeedbackSubmitted();
			EnableNextVideoButton(true);
		}
	}

	public void OnNextVideoButtonPress()
	{
		STManager.Instance.AdvanceCurrentTask();
		EnableNextVideoButton(false);
	}

	public void EnableFeedbackUI(bool enable)
	{
		foreach (var button in ratingButtons)
			button.transform.parent.gameObject.SetActive(enable);

		submitRatingButton.gameObject.SetActive(enable);

		if (enable)
		{
			// set the position to be near the user
			// i.e. set it relative to the camera
			// world position: +1 Z
			submitRatingButton.transform.parent.gameObject.transform.SetPositionAndRotation(Camera.main.transform.position + Camera.main.transform.forward * 0.4f, transform.rotation);
			submitRatingButton.transform.parent.gameObject.transform.LookAt(Camera.main.transform);
			submitRatingButton.transform.parent.gameObject.transform.Rotate(new Vector3(0f, 180f, 0f));
		}

	}

	public void EnableNextVideoButton(bool enable)
	{
		nextVideoButton.gameObject.SetActive(enable);
	}

	public bool IsFeedbackUIEnabled()
	{
		return submitRatingButton.gameObject.activeSelf;
	}

	public string ratingCsvPath;
	StreamWriter ratingStreamWriter;
	CsvWriter ratingCsvWriter;

	CsvConfiguration csvConfiguration = new CsvConfiguration(CultureInfo.CurrentCulture)
	{
		HasHeaderRecord = false,
		IncludePrivateMembers = true
	};

	private int currentUserId = 0;

	private void InitialiseCSV()
	{
		ratingStreamWriter = new StreamWriter(Application.dataPath + ratingCsvPath, true);
		ratingCsvWriter = new CsvWriter(ratingStreamWriter, csvConfiguration);
	}

	// make function to store user feedback
	public void StoreSequenceFeedback(string currentSequence, string currentRating)
	{
		Debug.Log("currentUserId " + currentUserId);
		Debug.Log("currentSequence " + currentSequence);
		Debug.Log("currentRating " + currentRating);
		Debug.Log(" SceneManager.Instance.currentQuestionnaireUserId.ToString() " + ScenesManager.Instance.currentQuestionnaireUserId.ToString());
		Debug.Log(" DateTime.Now.ToString(\"yyyyMMddHHmmss\") " + DateTime.Now.ToString("yyyy-MM-dd-HH:mm:ss"));
		// "yyyy-MM-dd-HH:mm:ss"

		RatingRecord ratingRecord = new RatingRecord(currentUserId.ToString(), currentSequence, currentRating, ScenesManager.Instance.currentQuestionnaireUserId.ToString(), DateTime.Now.ToString("yyyy-MM-dd-HH:mm:ssK"));

		Debug.Log("ratingRecord " + ratingRecord);
		Debug.Log("ratingCsvWriter " + ratingCsvWriter);

		ratingCsvWriter.WriteRecord(ratingRecord);
		ratingCsvWriter.NextRecord();
		ratingCsvWriter.Flush();
		Debug.Log("Wrote Rating Record with values " + currentUserId.ToString() + ", " + currentSequence + ", " + currentRating + ", " + ScenesManager.Instance.currentQuestionnaireUserId.ToString());
		sequenceCounter++;
		Debug.Log("Sequences done: " + sequenceCounter);
	}

	private int GetNewUserId()
	{
		StreamReader streamReader = new StreamReader(Application.dataPath + ratingCsvPath);
		CsvReader csvReader = new CsvReader(streamReader, csvConfiguration);

		List<RatingRecord> records = new List<RatingRecord>();
		records = csvReader.GetRecords<RatingRecord>().ToList();

		int lastUserId = 0;
		if (records.Count > 0)
		{
			try
			{
				lastUserId = int.Parse(records.Last().userId);
			}
			catch (FormatException e)
			{
				Debug.Log(e.Message);
				return 0;
			}
		}

		streamReader.Close();

		return lastUserId + 1;
	}
}
