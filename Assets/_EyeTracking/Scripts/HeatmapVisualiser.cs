using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Xml.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class HeatmapVisualiser : MonoBehaviour
{
    // 1. Read the heatmap for one frame for every user (and both loops)
    // 2. Display the points weights as a heatmap for every frame
    // 3. Upon space press move to the next frame
    // 4. Have some text stuff to display what point cloud and frame we're on

    [Header("Display Stuff")]
    public TextMeshProUGUI pcText;
    public TextMeshProUGUI frameText;
    public TextMeshProUGUI infoText;

    // Point Cloud Loading
    [Header("Point Cloud Loading")]
    public EyeTrackingPointCloudData[] pointClouds;
    [Header("CasualSquat, ReadyForWinter, FlowerDance, BlueSpin")]
    public string[] pcNamesToLoad = { "BlueSpin" }; //string[] pcNamesToLoad = {"CasualSquat", "ReadyForWinter", "FlowerDance", "BlueSpin" };
    private int currPcIndex = 0;

    [Header("Point Clouds Display")]
    public GameObject pcGameObject;
    public GameObject pcChildObject;

    //public GameObject pcChildObject2;

    [Header("Params")]
    public string weightsDirRoot;
    //private string weightsDirNoDbScan;
    //private string weightsDirDbScan;
    private string weightsDir;
    private int currentFrameIndex = -1;
    [Tooltip("Default should be -1.")]
    public int startIndex = -1;
    public bool dbScan = true;

    [Header("Debug Output Stuff")]
    public GameObject pointParent;
    public GameObject pointPrefab;

    [Header("Playback")]
    public bool autoPlayback = true;
    public bool autoPlayOnlyToNextDiff = true;

    public Camera screenshotCameraFront;
    public Camera screenshotCameraBack;
    public Camera screenshotCameraLeft;
    public Camera screenshotCameraRight;
    private string screenshotDir;
    public UnityEngine.Color[] heatmapColors;

    private void Start()
    {
        currentFrameIndex = startIndex;

        if (!gameObject.GetComponent<EyeTrackingHeatmapProcess>().isActiveAndEnabled)
        {
            string processedDir = "Processed_Sum/";
            //weightsDirNoDbScan = weightsDirRoot + @"/Regular/" + processedDir + "NoDbScan/";
            //weightsDirDbScan = weightsDirRoot + @"/Regular/" + processedDir + "DbScan/";
            weightsDir = dbScan ? weightsDirRoot + @"/Regular/" + processedDir + "DbScan/" : weightsDirRoot + @"/Regular/" + processedDir + "NoDbScan/";

            screenshotDir = weightsDirRoot + "/Regular/";
            screenshotDir = screenshotDir + "Screenshots_Sum/";
            screenshotDir = dbScan ? screenshotDir + "DbScan/" : screenshotDir + "NoDbScan/";

            if (!autoPlayback)
                ShowNextFrame();

        }
        else
        {
            Debug.LogWarning("HeatmapVisualiser being disabled because EyeTrackingHeatmapProcess is active.");
            enabled = false;
        }
    }

    private void FixedUpdate()
    {
        if (autoPlayback || autoPlayOnlyToNextDiff)
            ShowNextFrame();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.RightArrow) && !autoPlayback)
        {
            if (autoPlayOnlyToNextDiff)
            {
                autoPlayback = true;
            }
            else
            {
                ShowNextFrame();
            }
        }

        if (Input.GetKeyDown(KeyCode.LeftArrow) && !autoPlayback)
        {
            if (autoPlayOnlyToNextDiff)
            {
                autoPlayback = true;
            }
            else
            {
                currentFrameIndex-=2;
                ShowNextFrame();
            }
        }
    }

    //private void LoadPointClouds()
    //{
    //    for (int i = 0; i < pointClouds.Length; i++)
    //    {
    //        if (pcNamesToLoad.Contains(pointClouds[i].pcName)) // match the name of the PC we want to load
    //        {
    //            pointClouds[i].LoadAssetsFromDisk();
    //        }
    //    }
    //}


    void ShowNextFrame()
    {
        if (currPcIndex >= pcNamesToLoad.Length)
            return;

        foreach (MeshRenderer childpt in pointParent.transform.GetComponentsInChildren<MeshRenderer>())
        {
            if (childpt != null)
            {
                Destroy(childpt.gameObject);
            }
        }

        currentFrameIndex++;
        if (currentFrameIndex > 249)
        {
            currentFrameIndex = startIndex;
            currPcIndex++;
            return;
        }

        pcChildObject.GetComponent<MeshFilter>().mesh = pointClouds[GetPcIndexFromName(pcNamesToLoad[currPcIndex])].LoadMeshForFrame(currentFrameIndex);
        //pcChildObject2.GetComponent<MeshFilter>().mesh = pointClouds[GetPcIndexFromName(pcNamesToLoad[currPcIndex])].LoadMeshForFrame(currentFrameIndex);

        Resources.UnloadUnusedAssets();

        pcChildObject.transform.localPosition = new Vector3(-1 * pcChildObject.GetComponent<MeshFilter>().sharedMesh.bounds.center.x, 0f, -1 * pcChildObject.GetComponent<MeshFilter>().sharedMesh.bounds.center.z);
        //pcChildObject2.transform.localPosition = new Vector3(-1 * pcChildObject.GetComponent<MeshFilter>().sharedMesh.bounds.center.x, 0f, -1 * pcChildObject.GetComponent<MeshFilter>().sharedMesh.bounds.center.z);

        //pcGameObject.transform.position = worldPosition;
        //pcGameObject.transform.rotation = Quaternion.Euler(pcGameObject.transform.rotation.eulerAngles.x, 0f, pcGameObject.transform.rotation.eulerAngles.z);

        pcGameObject.SetActive(true);

        pcText.text = "Point Cloud: " + pcNamesToLoad[currPcIndex];
        frameText.text = "Frame: " + currentFrameIndex.ToString();
        infoText.text = "";

        bool stopPlayback = ShowHeatmapPoints(pcNamesToLoad[currPcIndex], currentFrameIndex);

        if (stopPlayback && autoPlayOnlyToNextDiff)
        {
            autoPlayback = false;
        }

        SaveCameraViewScreenshot(pcNamesToLoad[currPcIndex], currentFrameIndex, "front");
        SaveCameraViewScreenshot(pcNamesToLoad[currPcIndex], currentFrameIndex, "back");
        SaveCameraViewScreenshot(pcNamesToLoad[currPcIndex], currentFrameIndex, "left");
        SaveCameraViewScreenshot(pcNamesToLoad[currPcIndex], currentFrameIndex, "right");
    }

    bool ShowHeatmapPoints(string pcname, int frameIdx)
    {
        // Read the weight file
        // PointCloudSaved_BlueSpin_frame85

        //List<string> txtFiles = Directory.GetFiles(weightsDir).ToList();

      //string noDbScanFilename = weightsDirNoDbScan + @"PointCloudSaved_" + pcname + "_frame" + frameIdx.ToString() + ".txt";
        //string DbScanFilename = weightsDirDbScan + @"PointCloudSaved_" + pcname + "_frame" + frameIdx.ToString() + "_dbscan.txt";
        string pcfilename = dbScan ? weightsDir + @"PointCloudSaved_" + pcname + "_frame" + frameIdx.ToString() + "_dbscan.txt" : 
                                    weightsDir + @"PointCloudSaved_" + pcname + "_frame" + frameIdx.ToString() + ".txt";


        infoText.text = "";


        //int pts1 = ShowPoints(noDbScanFilename, pcname, frameIdx, pcChildObject);
        //int pts2 = ShowPoints(DbScanFilename, pcname, frameIdx, pcChildObject2);
        
        int ptsScreenshotMode = ShowPoints(pcfilename, pcname, frameIdx, pcChildObject);

        //if (pts1 != -1 && pts2 != -1 && pts1 != pts2)
        //    return true;
        
        if (ptsScreenshotMode != -1)
            return true;

        return false;
    }

    int ShowPoints(string filename, string pcname, int frameIdx, GameObject childObject)
    {

        infoText.text += "\n" + "Filename: " + filename;

        if (!File.Exists(filename))
        {
            Debug.LogWarning("File doesn't exist for reading weights " + filename + " !!");
            return -1;
        }

        List<string> lines = File.ReadLines(filename).ToList();
        List<float> weights = new List<float>();
        for (int i = 0; i < lines.Count; i++)
        {
            weights.Add(float.Parse(lines[i]));
        }

        float maxWeight = weights.Max();
        float minWeight = weights.Min(); // will always be zero

        List<Vector3> vertices = childObject.GetComponent<MeshFilter>().sharedMesh.vertices.ToList();

        //SavePcVertices(pcname, frameIdx);

        //if (lines.Count - 2 != vertices.Count)
        //{
        //    Debug.LogError("Vertice and weight data not matching up! " + (lines.Count-2).ToString() + ", " + vertices.Count);
        //}
        int pointCounter = 0;
        for (int i = 0; i <= weights.Count - 2; i++) // end at count-2 since last line is blank
        {
            if (weights[i] > 0f)
            {
                pointCounter++;
                GameObject sphere = Instantiate(pointPrefab, childObject.transform.TransformPoint(vertices[i]), Quaternion.identity, pointParent.transform);
                if (sphere != null)
                {
                    sphere.GetComponent<MeshRenderer>().material.color = GetColorFromWeight(weights[i], minWeight, maxWeight);
                }

            }
        }
        infoText.text += "\n" + "Points: " + pointCounter.ToString();
        return pointCounter;
    }

    //private void SavePcVertices(string pcname, int frameIdx)
    //{
    //	string savefilename = weightsDir + @"PointCloudSaved_" + pcname + "_frame" + frameIdx.ToString() + "_vertices.txt";


    //	if (!File.Exists(savefilename))
    //	{
    //		File.WriteAllText(savefilename, string.Empty);

    //		StreamWriter sw = new StreamWriter(savefilename, true);

    //		//sw.WriteLine("GazeCount");
    //		
    //sw.Flush();

    //		List<Vector3> vertices = pcChildObject.GetComponent<MeshFilter>().sharedMesh.vertices.ToList();

    //		for (int i = 0; i < vertices.Count; i++)
    //		{
    //			Vector3 converted = pcChildObject.transform.TransformPoint(vertices[i]);
    //			string vectorLine = converted.x + ", " + converted.y + ", " + converted.z;
    //			sw.WriteLine(vectorLine);
    //		}

    //		sw.Close();
    //		sw.Dispose();
    //	}
    //}


    private int GetPcIndexFromName(string pcName)
    {
        for (int i = 0; i < pointClouds.Length; i++)
        {
            if (pointClouds[i].pcName == pcName)
            {
                return i;
            }
        }

        Debug.LogError("PC Name not found in the point clouds array!");
        return -1;
    }

    // generate heatmap colours
    public UnityEngine.Color GetColorFromWeight(float weight, float minWeight, float maxWeight)
    {
        int Alpha = 1;
        List<UnityEngine.Color> ColorGradient = heatmapColors.ToList();

        float weightPercentage = (weight - minWeight) / (maxWeight - minWeight);

        float percentagePerColor = 1f / (ColorGradient.Count - 1);

        float colorBlock = weightPercentage / percentagePerColor;

        int blockIdx = (int)Math.Truncate(colorBlock);

        UnityEngine.Color cTarget = ColorGradient[blockIdx];
        UnityEngine.Color cNext = weight == maxWeight ? ColorGradient[blockIdx] : ColorGradient[blockIdx + 1];

        float weightPercentageResidual = weightPercentage - (blockIdx * percentagePerColor);
        float colorFillPercentage = weightPercentageResidual / percentagePerColor;

        float deltaR = cNext.r - cTarget.r;
        float deltaG = cNext.g - cTarget.g;
        float deltaB = cNext.b - cTarget.b;

        float R = cTarget.r + (deltaR * colorFillPercentage);
        float G = cTarget.g + (deltaG * colorFillPercentage);
        float B = cTarget.b + (deltaB * colorFillPercentage);

        UnityEngine.Color c = new UnityEngine.Color(R, G, B, Alpha);

        return c;
    }

    void SaveCameraViewScreenshot(string pcname, int frameidx, string cameraPosition)
    {
        RenderTexture screenTexture = new RenderTexture(Screen.width, Screen.height, 16);
        switch (cameraPosition)
        {
            case "front":
                screenshotCameraFront.targetTexture = screenTexture;
                break;
            case "back":
                screenshotCameraBack.targetTexture = screenTexture;
                break;
            case "right":
                screenshotCameraRight.targetTexture = screenTexture;
                break;
            case "left":
                screenshotCameraLeft.targetTexture = screenTexture;
                break;
            default:
                Debug.LogError("Incorrect camera orientation given.");
                break;
        }

        RenderTexture.active = screenTexture;
        switch (cameraPosition)
        {
            case "front":
                screenshotCameraFront.Render();
                break;
            case "back":
                screenshotCameraBack.Render();
                break;
            case "right":
                screenshotCameraRight.Render();
                break;
            case "left":
                screenshotCameraLeft.Render();
                break;
            default:
                Debug.LogError("Incorrect camera orientation given.");
                break;
        }

        Texture2D renderedTexture = new Texture2D(Screen.width, Screen.height);
        renderedTexture.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
        RenderTexture.active = null;
        byte[] byteArray = renderedTexture.EncodeToPNG();

        if (!Directory.Exists(screenshotDir))
            Directory.CreateDirectory(screenshotDir);

        System.IO.File.WriteAllBytes(screenshotDir + pcname + "_" + cameraPosition + "_" + frameidx + ".png", byteArray);

        switch (cameraPosition)
        {
            case "front":
                screenshotCameraFront.targetTexture = null;
                break;
            case "back":
                screenshotCameraBack.targetTexture = null;
                break;
            case "right":
                screenshotCameraRight.targetTexture = null;
                break;
            case "left":
                screenshotCameraLeft.targetTexture = null;
                break;
            default:
                screenshotCameraFront.targetTexture = null;
                Debug.LogError("Incorrect camera orientation given.");
                break;
        }
        Destroy(renderedTexture);
        Destroy(screenTexture);
    }



}
