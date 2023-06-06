using Microsoft.MixedReality.Toolkit.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QualityChanger : MonoBehaviour
{
	public static QualityChanger Instance { get; private set; }
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

	/* Maintain a reference to the quality slider
		 Implement the value updated fn of it
	 */

	public PinchSlider qualitySlider;

	// Start is called before the first frame update
	void Start()
	{
		qualitySlider.SliderValue = 0.5f;
	}

	public void OnQualitySliderUpdated()
	{
		// change the quality of the selected game object
		PointCloudsManager.Instance.ChangeCurrentPCQuality(GetCurrentQuality());
	}

	public int GetCurrentQuality()
	{
		// return 1, 3 or 5 for quality
		switch (qualitySlider.SliderValue)
		{
			case 0: return 1;

			default:
			case 0.5f: return 3;

			case 1: return 5;
		}
	}
}
