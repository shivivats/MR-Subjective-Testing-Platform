using Microsoft.MixedReality.Toolkit.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DistanceChanger : ChangerBase
{

	public static DistanceChanger Instance { get; private set; }
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


	/* Maintain a reference to the distance slider
	 Implement the value updated fn of it
	*/

	public PinchSlider distanceSlider;

	public TextMeshPro sliderValueText;

	public float maxDistance = 5f;
	public float minDistance = 1f;

	// Start is called before the first frame update
	void Start()
    {
		distanceSlider.SliderValue = 0.5f;
		UpdateSliderValueText();

		//PointCloudsManager.Instance.ChangeCurrentPCDistance(GetCurrentDistance());
	}

	public void OnDistanceSliderUpdated()
	{
		UpdateSliderValueText();
		
		// change the distance of the currently selected game object
		PointCloudsManager.Instance.ChangeCurrentPCDistance(GetCurrentDistance());
	}

	private void UpdateSliderValueText()
	{
		sliderValueText.text = GetCurrentDistance().ToString("0.00");
	}

	public float GetCurrentDistance()
	{
		// slider value is between 0 and 1 - 0 is minDistance (1m), 1 is maxDistance (5m)

		return ((maxDistance - minDistance) * distanceSlider.SliderValue) + minDistance;
	}

}
