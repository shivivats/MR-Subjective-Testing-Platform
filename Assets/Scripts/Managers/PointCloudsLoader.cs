using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointCloudsLoader : MonoBehaviour
{
	public static PointCloudsLoader Instance { get; private set; }

	[Header("Point Cloud Objects")]
	public PointCloudObject LongDressPointCloudObject, LootPointCloudObject, RedAndBlackPointCloudObject, SoldierPointCloudObject;

	private PointCloudObject[] pointcloudsToLoad;

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

	public void StartLoader()
	{
		SetPointCloudsToLoad();
		LoadPointClouds();
	}

	void LoadPointClouds()
	{
		foreach (PointCloudObject pointCloudObject in pointcloudsToLoad)
		{
			pointCloudObject.LoadPointClouds();
			Debug.Log("Loaded all meshes.");
		}
	}

	void SetPointCloudsToLoad()
	{
		pointcloudsToLoad = new PointCloudObject[] { LongDressPointCloudObject, LootPointCloudObject, RedAndBlackPointCloudObject, SoldierPointCloudObject };
	}

}
