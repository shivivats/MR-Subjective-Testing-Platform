using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PCObjectType { LongDress, Loot, RedAndBlack, Soldier };

[System.Serializable]
public class PointCloudObject
{
    public PCObjectType currentObjectType;

    public string pathPrefix;

    public string quality1MeshesPath;
    public string quality3MeshesPath;
    public string quality5MeshesPath;

    [HideInInspector]
    public Mesh[] quality1Meshes;

    [HideInInspector]
    public Mesh[] quality3Meshes;

    [HideInInspector]
    public Mesh[] quality5Meshes;

    public string GetQualityPath(int quality)
    {
        switch (quality)
        {
            case 1: return "PointClouds/" + pathPrefix + "/" + quality1MeshesPath;
            case 3: return "PointClouds/" + pathPrefix + "/" + quality3MeshesPath;
            case 5: return "PointClouds/" + pathPrefix + "/" + quality5MeshesPath;
        }
        return null;
    }

    public void LoadPointClouds()
    {
        quality1Meshes = Resources.LoadAll<Mesh>(GetQualityPath(1));
        Debug.Log("Loaded quality 1 meshes");

        quality3Meshes = Resources.LoadAll<Mesh>(GetQualityPath(3));
        Debug.Log("Loaded quality 3 meshes");

        quality5Meshes = Resources.LoadAll<Mesh>(GetQualityPath(5));
        Debug.Log("Loaded quality 5 meshes");
    }

    public Mesh[] GetQualityMeshes(int quality)
    {
        switch (quality)
        {
            case 1:
                return quality1Meshes;
            case 3:
                return quality3Meshes;
            case 5:
                return quality5Meshes;
        }
        return null;
    }

    public string GetObjectTypeAsString()
    {
        switch(currentObjectType)
        {
            case PCObjectType.LongDress: return "LongDress";
            case PCObjectType.Loot: return "Loot";
            case PCObjectType.RedAndBlack: return "RedAndBlack";
            case PCObjectType.Soldier: return "Soldier";
        }
        return null;
    }
}