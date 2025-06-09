using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mime;
using UnityEngine;

public class DyanmicPointSize : MonoBehaviour
{
    // The idea here is to get the 10 nearest neighbours of a point in the PC and use that to determine its average distance
    // Then average that distance for all points in the PC frame
    // Then average that for all frames in the PC sequence
    // That gives us the point size
    // we only really need to do this for OCTREE point clouds but lets do this for all of them

    private List<string> pcPaths = new List<string>();

    private string pathPrefix = "./Assets/Resources/PointClouds/";

    private List<string> pcNames = new List<string>() { "CasualSquat", "ReadyForWinter", "FlowerDance", "BlueSpin" };
    private List<string> representations = new List<string>() { "octree-predlift", "trisoup-raht", "VPCC" };
    private List<string> qualities = new List<string>() { "r01", "r02", "r03", "r04", "r05", "raw" };

    Dictionary<string, List<List<Vector3>>> loadedPointCloudsDict = new Dictionary<string, List<List<Vector3>>>();

    int numNearestNeighbours = 10;

    // Start is called before the first frame update
    void Start()
    {
        InitPaths();
        LoadPCs();
        GetPointSizeForPCs();
    }

    void InitPaths()
    {
        foreach (string pcname in pcNames)
        {
            foreach (string rep in representations)
            {
                foreach (string qual in qualities)
                {
                    if (qual == "raw" && rep != "VPCC") continue;
                    if (rep == "VPCC")
                    {
                        pcPaths.Add(Path.Combine(pathPrefix, rep, pcname, qual));
                        Debug.Log("Added " + Path.Combine(pathPrefix, rep, pcname, qual) + "to pcPaths.");
                    }
                    else
                    {
                        if (rep == "trisoup-raht" && qual == "r05") continue;

                        pcPaths.Add(Path.Combine(pathPrefix, "GPCC", pcname, rep, qual));
                        Debug.Log("Added " + Path.Combine(pathPrefix, "GPCC", pcname, rep, qual) + "to pcPaths.");
                    }
                }
            }
        }

        pcPaths = pcPaths.Take(3).ToList();
    }

    void LoadPCs()
    {
        List<List<Vector3>> currLoadedPc = new List<List<Vector3>>();
        foreach (string pcpath in pcPaths)
        {
            int fileCount = 0;
            foreach (string file in Directory.GetFiles(pcpath))
            {
                if (Path.GetExtension(file) == ".ply")
                {
                    FileStream stream = File.Open(file, FileMode.Open, FileAccess.Read, FileShare.Read);
                    Debug.Log("Reading from file " + file);
                    PLYDataHeader header = PLYUtils.Instance.ReadDataHeader(new StreamReader(stream));
                    PLYDataBody body = PLYUtils.Instance.ReadDataBody(header, new BinaryReader(stream));

                    currLoadedPc.Add(body.vertices);
                    fileCount++;

                    stream.Close();

                    break;
                }
            }

            Debug.Log("Loaded " + fileCount + " files for " + pcpath);

            loadedPointCloudsDict.Add(pcpath, currLoadedPc);
        }
    }

    void GetPointSizeForPCs()
    {
        List<float> frameAverages;
        List<float> pointAverages;

        foreach (string path in loadedPointCloudsDict.Keys)
        {
            frameAverages = new List<float>();
            foreach (List<Vector3> frame in loadedPointCloudsDict[path])
            {
                pointAverages = new List<float>();
                foreach (Vector3 point in frame)
                {
                    // for each point we get the 10 nearest neighbours
                    pointAverages.Add(GetAverageOfNearestNeighbours(point, frame));
                }
                frameAverages.Add(pointAverages.Average());
            }
            float pcaverage = frameAverages.Average();
            Debug.Log("Average distance for " + path + "is " + pcaverage.ToString());
            Debug.Log("Average distance scaled is " + (pcaverage/0.002f).ToString());

        }
    }

    float GetAverageOfNearestNeighbours(Vector3 point, List<Vector3> frame)
    {
        float averageDistance = 0;
        Dictionary<Vector3, float> distancePointPairs = new Dictionary<Vector3, float>();

        for (int i = 0; i < numNearestNeighbours; i++)
        {
            if (distancePointPairs.ContainsKey(frame[i]))
                continue;
            distancePointPairs.Add(frame[i], Vector3.Distance(point, frame[i]));
        }


        foreach (Vector3 pt in frame)
        {
            float currDis = Vector3.Distance(pt, point);
            KeyValuePair<Vector3, float> largest = GetLargestTuple(distancePointPairs);
            if (currDis < largest.Value && !distancePointPairs.ContainsKey(pt))
            {
                distancePointPairs.Add(pt, currDis);
                if (distancePointPairs.Count > numNearestNeighbours)
                {
                    distancePointPairs.Remove(largest.Key);
                }
            }
        }

        foreach(KeyValuePair<Vector3, float> pair in distancePointPairs)
        {
            averageDistance+= pair.Value;
        }
        averageDistance /= distancePointPairs.Count;

        return averageDistance;
    }

    KeyValuePair<Vector3, float> GetLargestTuple(Dictionary<Vector3, float> distancePointPairs)
    {
        float maxDistance = 0;
        KeyValuePair<Vector3, float> returnPair = new KeyValuePair<Vector3, float>(Vector3.zero, 0f);
        foreach (KeyValuePair<Vector3, float> pair in distancePointPairs)
        {
            if (pair.Value > maxDistance)
            {
                maxDistance = pair.Value;
                returnPair = pair;
            }
        }

        return returnPair;
    }

}

