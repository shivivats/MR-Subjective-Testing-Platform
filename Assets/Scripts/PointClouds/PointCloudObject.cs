using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;

[System.Serializable]
public class PointCloudObject
{
    public PCObjectType objectType;

    public string pcName; // aka the pathPrefix

    //public int[] qualities = new int[] { 1, 2, 3, 4, 5 };

    private QualityRepresentation[] representations = new QualityRepresentation[]
    {
        //new QualityRepresentation(EncoderType.GPCC_OCTREE, "r01"),
        //new QualityRepresentation(EncoderType.GPCC_OCTREE, "r02"),
        new QualityRepresentation(EncoderType.GPCC_OCTREE, "r03"),
        new QualityRepresentation(EncoderType.GPCC_OCTREE, "r04"),
        new QualityRepresentation(EncoderType.GPCC_OCTREE, "r05"),

        new QualityRepresentation(EncoderType.GPCC_TRISOUP, "r01"),
        new QualityRepresentation(EncoderType.GPCC_TRISOUP, "r02"),
        new QualityRepresentation(EncoderType.GPCC_TRISOUP, "r03"),
        new QualityRepresentation(EncoderType.GPCC_TRISOUP, "r04"),

        new QualityRepresentation(EncoderType.VPCC, "r01"),
        new QualityRepresentation(EncoderType.VPCC, "r02"),
        new QualityRepresentation(EncoderType.VPCC, "r03"),
        new QualityRepresentation(EncoderType.VPCC, "r04"),
        new QualityRepresentation(EncoderType.VPCC, "r05"),
        new QualityRepresentation(EncoderType.VPCC, "raw")
    };

    public Dictionary<QualityRepresentation, Mesh[]> pointClouds = new Dictionary<QualityRepresentation, Mesh[]>();

    //public string qualityPrefix="r0";

    //public Dictionary<int,Mesh[]> meshes = new Dictionary<int, Mesh[]>();
    //public Dictionary<int, Material[]> meshMaterials = new Dictionary<int, Material[]>();

    //private string GetQualityPath(int quality)
    //{
    //    return "PointClouds/" + pcName + "/" + quality + "/";
    //}

    //public string GetPointCloudFolder(int quality)
    //{
    //    return GetQualityPath(quality) + "PointClouds" + "/";
    //}

    private string GetQualityPath(QualityRepresentation qualityRepresentation)
    {
        return PointCloudsLoader.Instance.GetBasePCPathFromTypeAndEncoder(this.objectType, qualityRepresentation.encoder) + qualityRepresentation.quality + "/";
    }

    //public string GetMeshFolder(int quality)
    //{
    //    return GetQualityPath(quality) + "Meshes" + "/";
    //}

    //   public void LoadAllAssetsFromDisk()
    //   {
    //       foreach(QualityRepresentation rep in representations)
    //       {
    //           LoadQualityRepresentationFromDisk(rep);
    //	}
    //}

    public void UnloadAssetsFromQualityRepresentation(QualityRepresentation qr)
    {
        int currentUnloadedCount = 0;
        if (pointClouds.ContainsKey(qr))
        {
            // unload this
            foreach (Mesh mesh in pointClouds.GetValueOrDefault(qr))
            {
                Resources.UnloadAsset(mesh);
                currentUnloadedCount++;
            }
            pointClouds.Remove(qr);

            Debug.Log("UN loaded " + currentUnloadedCount + " Point Clouds for " + PointCloudsLoader.Instance.GetPCNameFromType(this.objectType) + " with quality " + qr.quality + "and encoder" + qr.encoder);
        }
    }

    //public IEnumerator UnloadAssetsAsync(QualityRepresentation qr)
    //{
    //    int currentUnloadedCount = 0;
    //    if (pointClouds.ContainsKey(qr))
    //    {
    //        // unload this
    //        foreach (Mesh mesh in pointClouds.GetValueOrDefault(qr))
    //        {
    //            Resources.UnloadAsset(mesh);
    //            currentUnloadedCount++;
    //        }
    //        pointClouds.Remove(qr);

    //        Debug.Log("UN loaded " + currentUnloadedCount + " Point Clouds for " + PointCloudsLoader.Instance.GetPCNameFromType(this.objectType) + " with quality " + qr.quality + "and encoder" + qr.encoder);
    //    }

    //    yield return null;
    //}

    //public IEnumerator LoadAssetsAsync(QualityRepresentation qr)
    //{
    //    //List<Mesh> meshes = new List<Mesh>();
    //    //foreach(string path in Directory.GetFiles(GetQualityPath(qr)))
    //    //{
    //    //	if (Path.GetExtension(path) == ".ply")
    //    //	{
    //    //		ResourceRequest req = Resources.LoadAsync<Mesh>(path);
    //    //		yield return req;
    //    //		meshes.Add(req.asset as Mesh);
    //    //	}
    //    //	else
    //    //	{
    //    //		yield return null;
    //    //	}
    //    //}

    //    //      pointClouds.Add(qr, meshes.ToArray());

    //    LoadQualityRepresentationFromDisk(qr);
    //    yield return null;

    //}

    public void LoadQualityRepresentationFromDisk(QualityRepresentation qr)
    {
        if (pointClouds.ContainsKey(qr))
            return;

        string pathToLoadFrom = GetQualityPath(qr);
        Mesh[] currentLoadedMeshes;
        //if(asyncLoad)
        //{
        //	ResourceRequest req = Resources.LoadAsync<Mesh>(pathToLoadFrom);
        //	req.completed += MeshesLoaded;
        //}
        //else
        {
            currentLoadedMeshes = Resources.LoadAll<Mesh>(pathToLoadFrom);
        }
        pointClouds.Add(qr, currentLoadedMeshes);
        Debug.Log("Loaded " + currentLoadedMeshes.Length + " Point Clouds for " + PointCloudsLoader.Instance.GetPCNameFromType(this.objectType) + " with quality " + qr.quality + "and encoder" + qr.encoder + " from " + pathToLoadFrom);
    }

    public void LoadQualityRepresentationFromDisk(List<QualityRepresentation> qrList)
    {
        foreach (QualityRepresentation qr in qrList)
        {
            LoadQualityRepresentationFromDisk(qr);
        }
    }

}

