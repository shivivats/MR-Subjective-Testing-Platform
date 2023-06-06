using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ObjectChanger : MonoBehaviour
{
	public static ObjectChanger Instance { get; private set; }

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

	/* Maintain references to the buttons
	   Implement a function to highlight the buttons - change vertex colour on the textmeshpro text
	   Change the current 
	*/

	public GameObject soldierButton;
	public GameObject longDressButton;
	public GameObject redAndBlackButton;
	public GameObject lootButton;

	private TextMeshPro soldierText;
	private TextMeshPro longDressText;
	private TextMeshPro redAndBlackText;
	private TextMeshPro lootText;

	void Start()
    {
		SetupTexts();

    }

	private void SetupTexts()
	{
        soldierText = soldierButton.GetComponentInChildren<TextMeshPro>();
        longDressText = longDressButton.GetComponentInChildren<TextMeshPro>();
        redAndBlackText = redAndBlackButton.GetComponentInChildren<TextMeshPro>();
        lootText = lootButton.GetComponentInChildren<TextMeshPro>();
    }

	public void HighlightSelectedObject(PCObjectType type)
	{
		if ((soldierText ?? longDressText ?? redAndBlackText ?? lootText) == null)
			SetupTexts();

        soldierText.color = Color.white;
		longDressText.color = Color.white;
		redAndBlackText.color = Color.white;
		lootText.color = Color.white;

		switch (type)
		{
			default:
			case PCObjectType.Soldier:
				soldierText.color = Color.green; 
				break;

			case PCObjectType.LongDress:
				longDressText.color = Color.green;
				break;

			case PCObjectType.RedAndBlack:
				redAndBlackText.color = Color.green;
				break;

			case PCObjectType.Loot:
				lootText.color = Color.green;
				break;
		}
	}

	public void OnSoldierButtonPressed()
	{
		PointCloudsManager.Instance.ChangeCurrentPCObject(PCObjectType.Soldier);
		HighlightSelectedObject(PCObjectType.Soldier);
	}

	public void OnLongDressButtonPressed()
	{
		PointCloudsManager.Instance.ChangeCurrentPCObject(PCObjectType.LongDress);
		HighlightSelectedObject(PCObjectType.LongDress);
	}

	public void OnRedAndBlackButtonPressed() 
	{
		PointCloudsManager.Instance.ChangeCurrentPCObject(PCObjectType.RedAndBlack);
		HighlightSelectedObject(PCObjectType.RedAndBlack);
	}

	public void OnLootButtonPressed()
	{
		PointCloudsManager.Instance.ChangeCurrentPCObject(PCObjectType.Loot);
		HighlightSelectedObject(PCObjectType.Loot);
	}

}
