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

	public GameObject feedbackSliderGameObject;
	private PinchSlider feedbackSlider;

	public TextMeshPro ratingText;

	public float ratingTextScale;

	// Start is called before the first frame update
	void Start()
    {
		feedbackSlider = feedbackSliderGameObject.GetComponent<PinchSlider>();

		currentUserId = GetNewUserId();
		Debug.Log("User ID for current user is " + currentUserId.ToString());

		InitialiseCSV();
	}

    // Update is called once per frame
    void Update()
    {
        
    }

	public void OnSliderValueUpdate()
	{
		UpdateCurrentRatingAsInt();
		ratingText.text = "(" + UpdateCurrentRatingAsInt().ToString() + "/" + "10" + ")";
	}

	public void ChangeSliderRatingFromButton(bool decrement)
    {
		float incrementStep = 0.1111111f;

		feedbackSlider.SliderValue = decrement ? Math.Clamp(feedbackSlider.SliderValue - incrementStep, 0, 1) : Math.Clamp(feedbackSlider.SliderValue + incrementStep, 0, 1);
		Debug.Log(feedbackSlider.SliderValue);
	}

	private int UpdateCurrentRatingAsInt()
	{
		return Mathf.RoundToInt(feedbackSlider.SliderValue / ratingTextScale) + 1;
	}

	public void OnSubmitButtonPress()
	{
		StoreSequenceFeedback(STManager.Instance.GetCurrentSequenceString(), UpdateCurrentRatingAsInt().ToString());
		STManager.Instance.AdvanceCurrentTask();
		EnableFeedbackUI(false);
	}

	public void EnableFeedbackUI(bool enable)
	{
		feedbackSliderGameObject.transform.parent.gameObject.SetActive(enable);
	}

	public bool IsFeedbackUIEnabled()
	{
		return feedbackSliderGameObject.transform.parent.gameObject.activeSelf;
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
		Debug.Log(" SceneManager.Instance.currentQuestionnaireUserId.ToString() " + SceneManager.Instance.currentQuestionnaireUserId.ToString());
		Debug.Log(" DateTime.Now.ToString(\"yyyyMMddHHmmss\") " + DateTime.Now.ToString("yyyy-MM-dd-HH:mm:ss"));
		// "yyyy-MM-dd-HH:mm:ss"

		RatingRecord ratingRecord = new RatingRecord(currentUserId.ToString(), currentSequence, currentRating, SceneManager.Instance.currentQuestionnaireUserId.ToString(), DateTime.Now.ToString("yyyy-MM-dd-HH:mm:ssK"));

		Debug.Log("ratingRecord " + ratingRecord);
		Debug.Log("ratingCsvWriter " + ratingCsvWriter);

		ratingCsvWriter.WriteRecord(ratingRecord);
		ratingCsvWriter.NextRecord();
		ratingCsvWriter.Flush();
		Debug.Log("Wrote Rating Record with values " + currentUserId.ToString() + ", " + currentSequence + ", " + currentRating + ", " + SceneManager.Instance.currentQuestionnaireUserId.ToString());
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
