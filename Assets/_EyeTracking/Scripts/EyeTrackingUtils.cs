/*
 * The eye tracking work in this platform is heavily based on the eye traking work by CWI which can be found here: https://github.com/zhouzhouha/PointCloud_EyeTracking/.
 * Their overall concept is implemented here by Shivi Vats.
 */

using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Security.Policy;
using UnityEditor;
using UnityEngine;

public class EyeTrackingUtils : MonoBehaviour
{
    public static EyeTrackingUtils Instance { get; private set; }

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

    public int GetUserIdFromJsonFilename(string filename)
    {
        string[] fileNameSplitReverse = filename.Split("_").Reverse().ToArray();// we reverse it after splitting because the user is in the second last element of the "split"
        string userString = fileNameSplitReverse[1]; // this will give us "UserN"
        string userNumberFromString = userString.Remove(0, 4); // remove the "User" from userstring
        return Int32.Parse(userNumberFromString); // parse the user number and get it as an int
    }

    public string GetPcNameFromJsonFilename(string filename)
    {
        string[] fileNameSplitReverse = filename.Split("_").Reverse().ToArray();// we reverse it after splitting because the pc name is in the last element of the split
        string pcString = fileNameSplitReverse[0].Remove(0, 2); // remove the "PC" from pcString
        return pcString.Remove(pcString.Length - 5); // remove the .json at the end
    }

    public Vector3 GetWorldPosFromGazeData(UserGazeData gazeData)
    {
        return new Vector3(gazeData.pcWorldPositionX, gazeData.pcWorldPositionY, gazeData.pcWorldPositionZ);
    }

    public Vector3 GetOriginFromGazeData(UserGazeData gazeData)
    {
        return new Vector3(gazeData.gazeOriginX, gazeData.gazeOriginY, gazeData.gazeOriginZ);
    }

    public Vector3 GetDirectionFromGazeData(UserGazeData gazeData)
    {
        return new Vector3(gazeData.gazeDirectionNormalisedX, gazeData.gazeDirectionNormalisedY, gazeData.gazeDirectionNormalisedZ);
    }

    public string EncoderToString(EncoderType encoderType)
    {
        switch (encoderType)
        {
            case EncoderType.GPCC_TRISOUP: return "GPCC_TRISOUP";
            case EncoderType.GPCC_OCTREE: return "GPCC_OCTREE";
            case EncoderType.VPCC: return "VPCC";
            default: return null;
        }
    }
}

public enum EFixation : int
{
    NO_FIXATION = 0,
    SHORT_FIXATION = 1,
    VALID_FIXATION = 2
}


[Serializable]
public class UserGazeData
{
    public string pcName;

    public int pcFrameIndex;

    // i think we dont need the timestamp actually, because we can calc everything using the current frame index
    //public int timestamp;

    public float gazeOriginX;
    public float gazeOriginY;
    public float gazeOriginZ;

    public float gazeDirectionNormalisedX;
    public float gazeDirectionNormalisedY;
    public float gazeDirectionNormalisedZ;

    //public Matrix4x4 cameraToWorldTransformationMatrix;

    public float pcWorldPositionX;
    public float pcWorldPositionY;
    public float pcWorldPositionZ;

    public float objectRotationY;

    public int userId;

    public UserGazeData(string name, int frameIndex, Vector3 origin, Vector3 directionNormalised, Vector3 pcPos, float rotationY, int id)
    {
        pcName = name;
        pcFrameIndex = frameIndex;

        gazeOriginX = origin.x;
        gazeOriginY = origin.y;
        gazeOriginZ = origin.z;

        gazeDirectionNormalisedX = directionNormalised.x;
        gazeDirectionNormalisedY = directionNormalised.y;
        gazeDirectionNormalisedZ = directionNormalised.z;

        pcWorldPositionX = pcPos.x;
        pcWorldPositionY = pcPos.y;
        pcWorldPositionZ = pcPos.z;

        objectRotationY = rotationY;
        userId = id;
    }
}

public class FixationGaze
{
    public Vector3 gazeOrigin;
    public Vector3 gazeDirectionNormalised;

    public Vector3 pcWorldPosition;

    public string pcName;
    public int pcFrameIndex;

    public float objectRotationY;

    public int userId;


    public FixationGaze()
    {
        this.gazeOrigin = Vector3.zero;
        this.gazeDirectionNormalised = Vector3.zero;
    }

    public FixationGaze(Vector3 gazeOrigin, Vector3 gazeDirectionNormalised)
    {
        this.gazeOrigin = gazeOrigin;
        this.gazeDirectionNormalised = gazeDirectionNormalised;
    }
}

public class ErrorProfilingData
{
    public int timestamp;

    public int markerId;

    public float markerPositionX;
    public float markerPositionY;
    public float markerPositionZ;

    public float gazeOriginX;
    public float gazeOriginY;
    public float gazeOriginZ;

    public float gazeDirectionNormalisedX;
    public float gazeDirectionNormalisedY;
    public float gazeDirectionNormalisedZ;

    public ErrorProfilingData(int timestamp, int markerId, Vector3 markerPosition, Vector3 gazeOrigin, Vector3 gazeDirectionNormalised)
    {
        this.timestamp = timestamp;
        this.markerId = markerId;

        this.markerPositionX = markerPosition.x;
        this.markerPositionY = markerPosition.y;
        this.markerPositionZ = markerPosition.z;

        this.gazeOriginX = gazeOrigin.x;
        this.gazeOriginY = gazeOrigin.y;
        this.gazeOriginZ = gazeOrigin.z;

        this.gazeDirectionNormalisedX = gazeDirectionNormalised.x;
        this.gazeDirectionNormalisedY = gazeDirectionNormalised.y;
        this.gazeDirectionNormalisedZ = gazeDirectionNormalised.z;
    }

    [JsonConstructor]
    public ErrorProfilingData(int timestamp, int markerId, float markerPositionX, float markerPositionY, float markerPositionZ, float gazeOriginX, float gazeOriginY, float gazeOriginZ, float gazeDirectionNormalisedX, float gazeDirectionNormalisedY, float gazeDirectionNormalisedZ)
    {
        this.timestamp = timestamp;
        this.markerId = markerId;

        this.markerPositionX = markerPositionX;
        this.markerPositionY = markerPositionY;
        this.markerPositionZ = markerPositionZ;

        this.gazeOriginX = gazeOriginX;
        this.gazeOriginY = gazeOriginY;
        this.gazeOriginZ = gazeOriginZ;

        this.gazeDirectionNormalisedX = gazeDirectionNormalisedX;
        this.gazeDirectionNormalisedY = gazeDirectionNormalisedY;
        this.gazeDirectionNormalisedZ = gazeDirectionNormalisedZ;
    }
}

public class ErrorDataProcessed
{
    public int userId { get; set; }

    public int markerId { get; set; }

    public float markerPositionX { get; set; }

    public float markerPositionY { get; set; }

    public float markerPositionZ { get; set; }

    //public Vector3 stdDeviation { get; set; }

    public float averageAccuracy { get; set; }

    public double rmsPrecision { get; set; }

    public float totalSamples { get; set; }

    public float excludedSamples { get; set; }


    public ErrorDataProcessed(int userId, int markerId)
    {
        this.userId = userId;
        this.markerId = markerId;
        this.markerPositionX = 0;
        this.markerPositionY = 0;
        this.markerPositionZ = 0;
        //this.stdDeviation = Vector3.zero;
        this.averageAccuracy = 0;
        this.rmsPrecision = 0;
        this.totalSamples = 0;
        this.excludedSamples = 0;
    }

    public ErrorDataProcessed(int userId, int markerId, Vector3 markerPosition, float averageAccuracy, double rmsPrecision, float totalSamples, float excludedSamples)
    {
        this.userId = userId;
        this.markerId = markerId;
        this.markerPositionX = markerPosition.x;
        this.markerPositionY = markerPosition.y;
        this.markerPositionZ = markerPosition.z;
        //this.stdDeviation = stdDeviation;
        this.averageAccuracy = averageAccuracy;
        this.rmsPrecision = rmsPrecision;
        this.totalSamples = totalSamples;
        this.excludedSamples = excludedSamples;
    }

    public ErrorDataProcessed(int userId, int markerId, float markerPositionX, float markerPositionY, float markerPositionZ, float averageAccuracy, double rmsPrecision, float totalSamples, float excludedSamples)
    {
        this.userId = userId;
        this.markerId = markerId;
        this.markerPositionX = markerPositionX;
        this.markerPositionY = markerPositionY;
        this.markerPositionZ = markerPositionZ;
        //this.stdDeviation = new Vector3(stdDeviationX, stdDeviationY, stdDeviationZ);
        this.averageAccuracy = averageAccuracy;
        this.rmsPrecision = rmsPrecision;
        this.totalSamples = totalSamples;
        this.excludedSamples = excludedSamples;

    }
}

[Serializable]
public class EyeTrackingPointCloudData
{
    public EyeTrackingPCObjectType objectType;
    public string pcName;
    public int pcIndex;
    public string pcDirectory;

    public Mesh[] pointCloudMeshes;

    private string GetPath()
    {
        return "PointClouds/" + "VPCC/" + pcName + "/" + "raw/" + "ply_xyz_rgb/";
    }

    public Mesh LoadMeshForFrame(int frameidx)
    {

        string frameFilename = Path.Join(GetPath(), pcName + "_UVG_vox10_25_0_250_" + frameidx.ToString("D4")); // resources path should not contain a file extension
        //Debug.LogWarning("Reading mesh from " + frameFilename);
        return Resources.Load<Mesh>(frameFilename);

    }

    public void LoadAssetsFromDisk()
    {
        pointCloudMeshes = Resources.LoadAll<Mesh>(GetPath());
        Debug.Log("Loaded meshes for " + pcName + "from " + GetPath());
    }
}

public enum EyeTrackingPCObjectType
{
    BlueSpin,
    CasualSquat,
    FlowerDance,
    ReadyForWinter
}

public enum EncoderType
{
    GPCC_OCTREE,
    GPCC_TRISOUP,
    VPCC
}

[System.Serializable]
public struct QualityRepresentation
{
    public EncoderType encoder;
    public string quality;

    public QualityRepresentation(EncoderType encoder, int quality)
    {
        this.encoder = encoder;
        this.quality = quality > 0 ? "r0" + quality.ToString() : "raw";
    }

    public QualityRepresentation(EncoderType encoder, string quality)
    {
        this.encoder = encoder;
        this.quality = quality;
    }
}

public class GazeStatistics
{
    public int inSquareCounter { get; set; }
    public int fixationCounter { get; set; }
    public int totalCounter { get; set; }
    public int gazeInvalidCounter { get; set; }
    public int saccadeCounter { get; set; }
    public int noFixationCounter { get; set; }

    public int zeroWeightCounter { get; set; }

    public GazeStatistics()
    {
        this.inSquareCounter = 0;
        this.fixationCounter = 0;
        this.totalCounter = 0;
        this.gazeInvalidCounter = 0;
        this.saccadeCounter = 0;
        this.noFixationCounter = 0;
        this.zeroWeightCounter = 0;
    }

    public GazeStatistics(int inSquareCounter, int fixationCounter, int totalCounter, int gazeInvalidCounter, int saccadeCounter, int noFixationCounter, int zeroWeightCounter)
    {
        this.inSquareCounter = inSquareCounter;
        this.fixationCounter = fixationCounter;
        this.totalCounter = totalCounter;
        this.gazeInvalidCounter = gazeInvalidCounter;
        this.saccadeCounter = saccadeCounter;
        this.noFixationCounter = noFixationCounter;
        this.zeroWeightCounter = zeroWeightCounter;
    }
}

public class Node
{
    public Vector3 data;
    public int splitAxis;
    public Node leftChild;
    public Node rightChild;
}

public class KdTree
{
    private Node root;

    private void BuildKdTree(List<Vector3> vertices)
    {
        if (vertices == null || vertices.Count == 0)
        {
            return;
        }

        root = BuildKdTreeRecursive(vertices, 0, vertices.Count - 1);
    }

    private Node BuildKdTreeRecursive(List<Vector3> points, int p0, int p1)
    {
        if (p0 == p1)
        {
            return new Node
            {
                data = points[p0],
                splitAxis = 0,
                leftChild = null,
                rightChild = null
            };
        }

        int axis = (p0 + p1) / 2;

        Node leftChild = BuildKdTreeRecursive(points, p0, axis);
        Node rightChild = BuildKdTreeRecursive(points, axis + 1, p1);

        return new Node
        {
            data = points[axis],
            splitAxis = axis,
            leftChild = leftChild,
            rightChild = rightChild
        };
    }

    public Vector3 FindNearestNeighbor(Vector3 queryPoint)
    {
        return FindNearestNeighborRecursive(queryPoint, root).data;
    }

    private Node FindNearestNeighborRecursive(Vector3 queryPoint, Node node)
    {
        if (node == null)
        {
            return null;
        }

        float distanceToPoint = Vector3.Distance(queryPoint, node.data);
        float distanceToNearest = float.MaxValue;
        Node nearestNode = null;

        if (distanceToPoint < distanceToNearest)
        {
            nearestNode = node;
            distanceToNearest = distanceToPoint;
        }

        int currentAxis = node.splitAxis;
        //int cmpResult = Vector3.Compare(queryPoint.x, node.data.x);
        float cmpResult = queryPoint.x - node.data.x;

        if (cmpResult < 0)
        {
            Node nearestNeighborLeft = FindNearestNeighborRecursive(queryPoint, node.leftChild);
            if (nearestNeighborLeft != null && Vector3.Distance(queryPoint, nearestNeighborLeft.data) < distanceToNearest)
            {
                nearestNode = nearestNeighborLeft;
                distanceToNearest = Vector3.Distance(queryPoint, nearestNode.data);
            }
        }
        else
        {
            Node nearestNeighborRight = FindNearestNeighborRecursive(queryPoint, node.rightChild);
            if (nearestNeighborRight != null && Vector3.Distance(queryPoint, nearestNeighborRight.data) < distanceToNearest)
            {
                nearestNode = nearestNeighborRight;
                distanceToNearest = Vector3.Distance(queryPoint, nearestNode.data);
            }
        }

        return nearestNode;
    }
}