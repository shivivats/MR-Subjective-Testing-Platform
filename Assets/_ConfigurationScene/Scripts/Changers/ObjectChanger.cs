using Microsoft.MixedReality.Toolkit.UI;
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

	// TODO: dynamically spawn buttons based on the number of objects in the PointCloudsLoader.Instance.pcObjects array
	// for each pcObject, there needs to be a button
	// for each button, there is a text

	public GameObject objectButtonParent;
	public GameObject objectButtonPrefab;
	// Spawn them 0.0225 apart in X, starting from -0.0275 X
	// Y and Z are 0
	// Scale is 0.2, 0.33, 0.2

	public Dictionary<PCObjectType, GameObject> objectButtons = new Dictionary<PCObjectType, GameObject>();

	private Dictionary<PCObjectType, TextMeshPro> objectText = new Dictionary<PCObjectType, TextMeshPro>();

	void Start()
    {
		SpawnAndSetupButtons();
    }

	private void SpawnAndSetupButtons()
	{
		int counter = 0;
		float spacing = 0.0225f;
		foreach(var pcObject in PointCloudsLoader.Instance.pcObjects)
		{
			GameObject currentButton = Instantiate(objectButtonPrefab, objectButtonParent.transform);
			currentButton.transform.localPosition = new Vector3(-0.0275f + counter* spacing, 0f, 0f);
			currentButton.transform.localScale = new Vector3(0.2f, 0.33f, 0.2f);
			currentButton.GetComponent<Interactable>().OnClick.AddListener(() => OnObjectButtonPressed(pcObject.objectType));

			objectButtons.Add(pcObject.objectType, currentButton);

			counter++;
		}

		SetupTexts();

	}

	private void SetupTexts()
	{
		foreach (PCObjectType type in objectButtons.Keys)
		{
			objectText.Add(type, objectButtons[type].GetComponentInChildren<TextMeshPro>());
			objectText[type].text = PointCloudsLoader.Instance.GetPCNameFromType(type);
		}
    }

	public void HighlightSelectedObject(PCObjectType type)
	{
		foreach(PCObjectType keyType in objectText.Keys)
		{
			if(keyType == type)
			{
				objectText[keyType].color = Color.green;
			}
			else
			{
				objectText[keyType].color = Color.white;
			}
		}
	}

	public void OnObjectButtonPressed(PCObjectType type)
	{
		ConfigurationSceneManager.Instance.ChangeCurrentPCObject(type);
		HighlightSelectedObject(type);
	}

}
