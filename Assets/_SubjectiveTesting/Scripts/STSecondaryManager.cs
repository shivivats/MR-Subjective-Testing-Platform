using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class STSecondaryManager : MonoBehaviour
{
	public static STSecondaryManager Instance { get; private set; }

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

	public GameObject startButton;
	public TextMeshPro startButtonText;

	public TextMeshProUGUI displayText;
	[HideInInspector]
	public GameObject playerCamera;

	public GameObject currentGameObject;

	public Material pointMaterial;
	public Material diskMaterial;
	public Material squareMaterial;

	// Start is called before the first frame update
	void Start()
	{
		playerCamera = GameObject.FindGameObjectWithTag("MainCamera");

		startButton.SetActive(true);
		SetStartButtonText();
	}

	public void SetStartButtonText()
	{
		if (STManager.Instance.isRehearsal)
		{
			startButtonText.text = "Start Rehearsal";
			startButtonText.fontSize = 0.05f;
		}
		else
		{
			startButtonText.text = "Start Test";
			startButtonText.fontSize = 0.08f;
		}
	}

	public void OnFullTaskEnded()
	{
		displayText.gameObject.SetActive(false);
	}

	public void SetCurrentPCDistance(float currentDistance, bool useFixedYOffsetForDistance, float yOffset)
	{
		Vector3 newPos = new Vector3(currentGameObject.transform.localPosition.x, 0f, playerCamera.transform.position.z + currentDistance + 0.2f);

		if (useFixedYOffsetForDistance)
			newPos.y = playerCamera.transform.position.y + yOffset;
		else
			newPos.y = currentGameObject.transform.localPosition.y;

		currentGameObject.transform.SetLocalPositionAndRotation(newPos, currentGameObject.transform.localRotation);

		Debug.Log("Distance set to " + currentDistance);
	}

	public Material GetMaterialFromRepresentation(PointCloudMaterialRepresentation rep)
	{
		switch (rep)
		{
			case PointCloudMaterialRepresentation.Point: return pointMaterial;

			case PointCloudMaterialRepresentation.Disk: return diskMaterial;

			case PointCloudMaterialRepresentation.Square: return squareMaterial;

			default: return pointMaterial;
		}
	}
}
