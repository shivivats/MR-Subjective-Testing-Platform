using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[System.Serializable]
public class PointCloudObject
{
    public PCObjectType objectType;

    public string pcName; // aka the pathPrefix

    public int[] qualities;

    public Dictionary<int, Mesh[]> pointClouds = new Dictionary<int, Mesh[]>();

    public Dictionary<int,Mesh[]> meshes = new Dictionary<int, Mesh[]>();
    public Dictionary<int, Material[]> meshMaterials = new Dictionary<int, Material[]>();

    private string GetQualityPath(int quality)
    {
        return "PointClouds/" + pcName + "/" + "q" + quality + "/";
    }

    public string GetPointCloudFolder(int quality)
    {
        return GetQualityPath(quality) + "PointClouds" + "/";
    }

    public string GetMeshFolder(int quality)
    {
        return GetQualityPath(quality) + "Meshes" + "/";
    }

    public void LoadAssetsFromDisk()
    {
        foreach(int quality in qualities)
        {
            Mesh[] currentLoadedMeshes = Resources.LoadAll<Mesh>(PointCloudsLoader.Instance.GetBasePCPathFromType(this.objectType)
                                                                    + "q" + quality.ToString() + "/"
                                                                    + "PointClouds/");

			pointClouds.Add(quality, currentLoadedMeshes);

            Debug.Log("Loaded Point Clouds for " + PointCloudsLoader.Instance.GetPCNameFromType(this.objectType) + " with quality " + quality.ToString());
        }
        if(PointCloudsLoader.Instance.loadMeshes)
        {
			foreach (int quality in qualities)
			{
				GameObject[] currentLoadedMeshObjects = Resources.LoadAll<GameObject>(PointCloudsLoader.Instance.GetBasePCPathFromType(this.objectType)
																		+ "q" + quality.ToString() + "\\"
																		+ "Meshes\\");

				List<Mesh> currentLoadedMeshes = new List<Mesh>();
				List<Material> currentLoadedMeshMaterials = new List<Material>();

				foreach (GameObject meshObject in currentLoadedMeshObjects)
				{
					currentLoadedMeshes.Add(meshObject.GetComponentInChildren<MeshFilter>().sharedMesh);
					currentLoadedMeshMaterials.Add(meshObject.GetComponentInChildren<MeshRenderer>().sharedMaterials[0]);

				}

				meshes.Add(quality, currentLoadedMeshes.ToArray());
				meshMaterials.Add(quality, currentLoadedMeshMaterials.ToArray());

				Debug.Log("Loaded Meshes for " + PointCloudsLoader.Instance.GetPCNameFromType(this.objectType) + " with quality " + quality.ToString());
			}
		}
	}
}

