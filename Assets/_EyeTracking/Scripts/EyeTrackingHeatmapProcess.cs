using CsvHelper;
using CsvHelper.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.UIElements;
using static UnityEngine.GraphicsBuffer;

public class EyeTrackingHeatmapProcess : MonoBehaviour
{
    [Header("JSON File Reading")]
    public string jsonDir = @".\Assets\_EyeTracking\JSON";
    public float ignoreTime; //Ignore the initial 'ignoreTime' ms 
    public int fps;
    public bool useStartEndIndices = false;
    public int startFrame;
    public int endFrame;

    // Calculate Fixation Parameters
    [Header("Fixation Parameters")]
    public float fixationDispersionThreshold;
    public float fixationIntervalThreshold; // aka 4 frames for us

    // Point Cloud Loading
    [Header("Point Cloud Loading")]
    public EyeTrackingPointCloudData[] pointClouds;
    [Header("CasualSquat, ReadyForWinter, FlowerDance, BlueSpin")]
    public string[] pcNamesToLoad = { "BlueSpin" }; //string[] pcNamesToLoad = {"CasualSquat", "ReadyForWinter", "FlowerDance", "BlueSpin" };

    [Header("Point Clouds Display")]
    public GameObject pcGameObject;
    public GameObject pcChildObject;

    // Weight Calculation
    [Header("Point Weight Calculation Parameters")]
    private List<float> currentPointGazeImportance = new List<float>();
    public float globalAngleThreshold = 0.5f;
    public float acceptingDepthRange = 0.05f;
    public int angleSegments = 10;
    public int slices = 16;

    // Weight Storage
    [Header("Weight Storage")]
    public string pcSaveFolderRoot = @"D:/PointCloudsSaved/";

    private string pcSaveFolderName;
    public bool calcFixation = true;
    public bool aggregateStats = true;

    public bool ignoreBarycentric = false;

    public bool processPerFrame = true;

    [Tooltip("False means the average will be calculated instead")]
    public bool calcSum = true;
    //[Header("Currently DBScan bool is ignored always!!")]
    private bool dbScan = false;

    // Stats
    GazeStatistics stats;

    [Header("Debug Output Stuff")]
    public GameObject pointParent;
    public GameObject pointPrefab;

    void Start()
    {
        ProcessDataOverall();

        //dbScan = !dbScan;
        //ignoreBarycentric = !ignoreBarycentric;

        //ProcessDataOverall();
    }

    private void ProcessDataOverall()
    {

        ProcessRawGazeData();
        AggregateGazeData();

        //dbScan = !dbScan;
        //AggregateGazeData();
    }

    private void ProcessRawGazeData()
    {
        pcSaveFolderName = ignoreBarycentric ? pcSaveFolderRoot + @"NoBary/" : pcSaveFolderRoot + @"Regular/";

        if (calcFixation)
        {
            //LoadPointClouds();
            LoadJSONData();
        }

        if (aggregateStats)
        {
            AggregateStats();
        }
    }

    private void AggregateGazeData()
    {
        if (processPerFrame)
            ProcessHeatmapPerFrame();
    }

    private void ResetStats()
    {
        stats = new GazeStatistics();
    }

    private void LoadPointClouds()
    {
        for (int i = 0; i < pointClouds.Length; i++)
        {
            if (pcNamesToLoad.Contains(pointClouds[i].pcName)) // match the name of the PC we want to load
            {
                pointClouds[i].LoadAssetsFromDisk();
            }
        }
    }

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

    public Mesh[] GetPCMeshesFromName(string pcname)
    {
        foreach (var pointCloud in pointClouds)
        {
            if (pointCloud.pcName == pcname)
            {
                return pointCloud.pointCloudMeshes;
            }
        }

        return null;
    }

    private void ShowCurrentFrame(int loadedPcIndex, int frameIndex, Vector3 worldPosition, float rotationY)
    {

        pcChildObject.GetComponent<MeshFilter>().mesh = pointClouds[loadedPcIndex].LoadMeshForFrame(frameIndex);

        //Debug.LogWarning("loaded mesh with " + pcChildObject.GetComponent<MeshFilter>().sharedMesh.vertexCount + " vertices");

        Resources.UnloadUnusedAssets();

        pcChildObject.transform.localPosition = new Vector3(-1 * pcChildObject.GetComponent<MeshFilter>().sharedMesh.bounds.center.x, 0f, -1 * pcChildObject.GetComponent<MeshFilter>().sharedMesh.bounds.center.z);

        pcGameObject.transform.position = worldPosition;

        pcGameObject.transform.rotation = Quaternion.Euler(pcGameObject.transform.rotation.eulerAngles.x, rotationY, pcGameObject.transform.rotation.eulerAngles.z);

        pcGameObject.SetActive(true);
    }

    void LoadJSONData()
    {
        List<string> jsonFiles = Directory.GetFiles(jsonDir, "*.json").ToList();

        // loop over all the json files
        foreach (string path in jsonFiles)
        {
            if (path.Contains("Error") || path.Contains(".meta"))
                continue;

            string currentJsonFilepath = path;

            // extract user id and point cloud name from JSON filename
            int currentUserId = EyeTrackingUtils.Instance.GetUserIdFromJsonFilename(currentJsonFilepath);
            string currentPcName = EyeTrackingUtils.Instance.GetPcNameFromJsonFilename(currentJsonFilepath);

            Debug.Log("1. Current user id is " + currentUserId + " and current pc name is " + currentPcName);

            if (!pcNamesToLoad.Contains(currentPcName))
            {
                Debug.Log("1.1 skipping pc " + currentPcName);
                continue;
            }

            ProcessJSONFile(currentJsonFilepath, currentUserId, currentPcName);
        }
    }

    void ProcessJSONFile(string currentJsonFilepath, int currentUserId, string currentPcName)
    {
        // main working function

        // read from the JSON, deserialise into a list of the gaze class

        List<string> jsonLines = File.ReadLines(currentJsonFilepath).ToList();

        // SPLIT these lines into two parts, one for each loop of the PC
        // process the halfs individually
        if (jsonLines.Count >= 500)
        {
            List<string> firstLoop = jsonLines.GetRange(0, 250);
            List<string> secondLoop = jsonLines.GetRange(250, 250);

            ProcessLines(firstLoop, currentUserId, currentPcName, 1);
            ProcessLines(secondLoop, currentUserId, currentPcName, 2);
        }
        else
        {
            Debug.LogError("not enough JSON lines for " + currentJsonFilepath + " please check!!!");
        }

    }

    void ProcessLines(List<string> currentLines, int currentUserId, string currentPcName, int loopId)
    {
        int currentRecordIndex = 0;

        ResetStats();

        // ignore a number of initial frames (CWI ignores the first 400ms which is 400/40 = 10 frames for us)
        // or we can also ignore the first 800ms aka 20 frames

        int framesToIgnore = Mathf.FloorToInt((ignoreTime / 1000) * fps);
        //		int framesToIgnore = 0;

        List<UserGazeData> gazeWindowSet = new List<UserGazeData>();

        List<Tuple<Vector3, float>> markerAngles = GetMarkerPositionFromCSV(currentUserId);

        if (!useStartEndIndices)
        {
            startFrame = framesToIgnore;
            endFrame = currentLines.Count - 1;
        }

        // start reading the file from framesToIgnore index, it'll ignore the first 'framesToIgnore' frames

        for (currentRecordIndex = startFrame; currentRecordIndex < endFrame; currentRecordIndex++)
        {
            UserGazeData currentGazeData = JsonConvert.DeserializeObject<UserGazeData>(currentLines[currentRecordIndex]);

            string tempFilename = pcSaveFolderName + @"Raw/" +
                                    "PointCloudSaved_" + currentGazeData.pcName +
                                    "_frame" + (currentGazeData.pcFrameIndex + 1).ToString() +
                                    "_user" + currentGazeData.userId.ToString() +
                                    "_loop" + loopId.ToString() +
                                    "_rotation" + currentGazeData.objectRotationY.ToString() +
                                    ".txt";
            if (File.Exists(tempFilename))
            {
                Debug.LogWarning("File already exists " + tempFilename + ", remove it so updated data can be written!");
                continue;
            }

            stats.totalCounter += 1;

            if (!ignoreBarycentric)
            {
                // Barycentric the current gaze as well
                float currentGazeCompAngle;
                bool gazeValid = ErrorInterpolation(markerAngles, new Vector3(currentGazeData.gazeOriginX, currentGazeData.gazeOriginY, currentGazeData.gazeOriginZ), new Vector3(currentGazeData.gazeDirectionNormalisedX, currentGazeData.gazeDirectionNormalisedY, currentGazeData.gazeDirectionNormalisedZ), out currentGazeCompAngle);

                if (!gazeValid)
                {
                    stats.gazeInvalidCounter += 1;
                    continue;
                }
            }

            //Debug.Log("2. Processing pc " + currentGazeData.pcName + " for user " + currentGazeData.userId);

            //shownGazeOrigin = new Vector3(userGazeData.gazeOriginX, userGazeData.gazeOriginY, userGazeData.gazeOriginZ);
            //shownGazeDirectionNormalised = new Vector3(userGazeData.gazeDirectionNormalisedX, userGazeData.gazeDirectionNormalisedY, userGazeData.gazeDirectionNormalisedZ);

            //ShowGazeLine(shownGazeOrigin, shownGazeDirectionNormalised);

            // translate and rotate the point cloud
            ShowCurrentFrame(
                GetPcIndexFromName(currentGazeData.pcName),
                currentGazeData.pcFrameIndex + 1,
                new Vector3(currentGazeData.pcWorldPositionX, currentGazeData.pcWorldPositionY, currentGazeData.pcWorldPositionZ),
                currentGazeData.objectRotationY);

            // Reset all gaze importance - essential to do this after we've set the current frame mesh above, otherwise the amt of vertices will be wrong
            ResetGazeImportance(pcChildObject.GetComponent<MeshFilter>().sharedMesh.vertices.Length);

            // this fn returns a flag
            EFixation flag = CalculateFixation(currentGazeData, gazeWindowSet);

            // if flag == 2 then we have a fixation
            if (flag == EFixation.VALID_FIXATION)
            {
                stats.fixationCounter += 1;

                //Debug.Log("4. Valid fixation found for " + currentGazeData.pcName + " user " + currentGazeData.userId);

                // loop over all the gazes in gazeWindowSet and add their data to lists
                List<Vector3> originList = new List<Vector3>();
                List<Vector3> directionList = new List<Vector3>();
                List<int> frameList = new List<int>();
                List<float> rotationList = new List<float>();
                List<Vector3> positionList = new List<Vector3>();

                foreach (UserGazeData gaze in gazeWindowSet)
                {
                    originList.Add(EyeTrackingUtils.Instance.GetOriginFromGazeData(gaze));
                    directionList.Add(EyeTrackingUtils.Instance.GetDirectionFromGazeData(gaze));
                    positionList.Add(EyeTrackingUtils.Instance.GetWorldPosFromGazeData(gaze));
                    frameList.Add(gaze.pcFrameIndex);
                    rotationList.Add(gaze.objectRotationY);
                }

                // check if all elements of rotation list and position list are the same - they HAVE TO be, otherwise this is an invalid gaze group
                bool allRoationsAndPositionsEqual = rotationList.All(x => x == rotationList[0]) && positionList.All(x => x == positionList[0]);

                if (!allRoationsAndPositionsEqual)
                {
                    Debug.LogError("4.1.1 Not all elements in the rotation list or position list for a window set are equal! Check the data recording code!");

                    if (rotationList.Count <= 2)
                    {
                        gazeWindowSet.RemoveAt(0); // remove the first element from the gaze window set in an attempt to fix this
                        continue;
                    }
                }

                //Debug.Log("4.1 Starting Fixation calc...");

                // initialise a new fixation gaze
                // the gaze origin and gaze direction of the FixationGaze need to be the averages of the gaze origin and gaze direction from the gazeWindowSet

                FixationGaze averageFixationGaze = new FixationGaze();
                averageFixationGaze.gazeOrigin = originList.Aggregate(Vector3.zero, (a, b) => a + b) / originList.Count;
                averageFixationGaze.gazeDirectionNormalised = directionList.Aggregate(Vector3.zero, (a, b) => a + b) / directionList.Count;
                averageFixationGaze.objectRotationY = rotationList[0];
                averageFixationGaze.userId = currentGazeData.userId;
                averageFixationGaze.pcName = currentGazeData.pcName;
                averageFixationGaze.pcFrameIndex = frameList[0];
                averageFixationGaze.pcWorldPosition = positionList[0];

                // make a gaze from this current fixation here
                //ShowGaze(currentFixationFromWindowSet.gazeOrigin, currentFixationFromWindowSet.gazeDirectionNormalised);

                if (ignoreBarycentric)
                {
                    RegisterPoints(averageFixationGaze.gazeDirectionNormalised, averageFixationGaze.gazeOrigin);
                }
                else
                {
                    float compensateAngle = 0f;

                    // calc the compensateangle
                    bool gazeInSquare = ErrorInterpolation(markerAngles, averageFixationGaze.gazeOrigin, averageFixationGaze.gazeDirectionNormalised, out compensateAngle);

                    if (gazeInSquare)
                    {
                        stats.inSquareCounter += 1;

                        RegisterPoints(averageFixationGaze.gazeDirectionNormalised, averageFixationGaze.gazeOrigin, compensateAngle);
                    }

                }

                // for now just display the PC with the currentPointGazeImportance
                //ShowCurrentHeatmapPoints();

                gazeWindowSet.Clear();
            }
            else if (flag == EFixation.SHORT_FIXATION)
            {
                //Debug.Log("4.2 Short fixation found for " + currentGazeData.pcName + " user " + currentGazeData.userId);
                gazeWindowSet.Clear(); // clear the gazewindowset as the discovered points are saccades or short fixations
                stats.saccadeCounter += 1;

            }
            else if (flag == EFixation.NO_FIXATION)
            {
                //Debug.Log("4.3 No fixation found for " + currentGazeData.pcName + " user " + currentGazeData.userId);
                // here this means we have no fixation (valid or short) found so we simply add a new element to the gazewindowset and check on the next loop
                // aka do nothing and let the gaze be added in the general loop
                stats.noFixationCounter += 1;
            }

            gazeWindowSet.Add(currentGazeData);

            // we check for the last element in the list i.e. the last timestamp
            // if the flag is set to two here then we save the last group data using SaveDataWithWeightCounter()

            if (currentRecordIndex == (currentLines.Count - 1))
            {
                if (flag == EFixation.VALID_FIXATION)
                {
                    // save the last group data
                    FixationGaze tempFixationGaze = new FixationGaze();
                }
            }

            AddAndSaveWeights(currentGazeData.pcName, currentGazeData.pcFrameIndex + 1, currentGazeData.userId, (int)currentGazeData.objectRotationY, loopId);

        }

        SaveAndLogStatistics(currentUserId, currentPcName, loopId);

    }

    public List<Tuple<Vector3, float>> GetMarkerPositionFromCSV(int userId)
    {
        List<Tuple<Vector3, float>> markerAngles = new List<Tuple<Vector3, float>>();

        string currentCSVPath = @"./Assets/_EyeTracking/ErrorCSV/User_" + userId.ToString() + ".csv";

        CsvConfiguration csvConfiguration = new CsvConfiguration(CultureInfo.CurrentCulture)
        {
            HasHeaderRecord = false,
            IncludePrivateMembers = true
        };

        StreamReader streamReader = new StreamReader(currentCSVPath);
        CsvReader csvReader = new CsvReader(streamReader, csvConfiguration);

        List<ErrorDataProcessed> markerRecords = new List<ErrorDataProcessed>();
        markerRecords = csvReader.GetRecords<ErrorDataProcessed>().ToList();

        foreach (ErrorDataProcessed record in markerRecords)
        {
            Vector3 markerPos = new Vector3(record.markerPositionX, record.markerPositionY, record.markerPositionZ);
            float angle = record.averageAccuracy;

            markerAngles.Add(new Tuple<Vector3, float>(markerPos, angle));
        }

        streamReader.Close();

        return markerAngles;
    }

    // Barycentric interpolation
    bool ErrorInterpolation(List<Tuple<Vector3, float>> markerAngles, Vector3 gazeOrigin, Vector3 gazeDirection, out float rstAngle)
    {
        // z value is 2.5

        // markerAngles is a list of tuples of marker positions and averageaccuracy (average angle)

        Plane m_plane = new Plane(markerAngles[0].Item1, markerAngles[2].Item1, markerAngles[4].Item1);

        Ray gazeRay = new Ray(gazeOrigin, gazeDirection);

        float gazeDistance = 0;
        bool rst = m_plane.Raycast(gazeRay, out gazeDistance);
        rstAngle = 0;

        if (!rst && gazeDistance == 0)
        {
            Debug.LogWarning("gaze parallel to the plane of markers");
            return false;
        }
        else if (!rst && gazeDistance < 0)
        {
            Debug.LogWarning("Negative distance, how's that possible?");
            return false;
        }

        Vector3 gazePos = gazeOrigin + gazeDirection * gazeDistance;

        // get 4 markers closest to the gaze - one of them will always be the center one
        int minIdx1 = int.MaxValue;
        int minIdx2 = int.MaxValue;
        int minIdx3 = int.MaxValue;
        GetNearestMarkersIdx(markerAngles, gazePos, out minIdx1, out minIdx2, out minIdx3);

        Tuple<Vector3, float> marker0 = markerAngles[4];
        Tuple<Vector3, float> marker1 = markerAngles[minIdx1];
        Tuple<Vector3, float> marker2 = markerAngles[minIdx2];
        Tuple<Vector3, float> marker3 = markerAngles[minIdx3];

        float dist1 = Vector3.Distance(marker0.Item1, gazePos);
        float dist2 = Vector3.Distance(marker1.Item1, gazePos);
        float dist3 = Vector3.Distance(marker2.Item1, gazePos);
        float dist4 = Vector3.Distance(marker3.Item1, gazePos);

        List<float> dis_all = new List<float> { dist1, dist2, dist3, dist4 };
        List<float> error_all = new List<float> { marker0.Item2, marker1.Item2, marker2.Item2, marker3.Item2 };

        rstAngle = WeightedSum(dis_all, error_all);

        bool inSquare = (dist1 <= Vector3.Distance(marker1.Item1, marker0.Item1)) || (dist1 <= Vector3.Distance(marker2.Item1, marker0.Item1)) || (dist1 <= Vector3.Distance(marker3.Item1, marker0.Item1));

        return inSquare;
    }

    float WeightedSum(List<float> dis_all, List<float> error_all)
    {
        List<float> normalisedDistances = new List<float>();

        for (int i = 0; i < dis_all.Count; i++)
        {
            float normalDis = dis_all[i] / dis_all.Sum();
            normalDis = 1 / normalDis;
            normalisedDistances.Add(normalDis);
        }

        List<float> ratioDis = new List<float>();

        for (int i = 0; i < normalisedDistances.Count; i++)
        {
            ratioDis.Add(normalisedDistances[i] / normalisedDistances.Sum());
        }

        float weightedAverageError = 0;
        for (int i = 0; i < ratioDis.Count; i++)
        {
            weightedAverageError += ratioDis[i] * error_all[i];
        }

        return weightedAverageError;
    }

    void GetNearestMarkersIdx(List<Tuple<Vector3, float>> markerAngles, Vector3 gazepos, out int minIdx1, out int minIdx2, out int minIdx3)
    {
        List<float> distances = new List<float>();


        for (int i = 0; i < markerAngles.Count; i++)
        {
            if (i == 4) // since the center marker is index 4
            {
                distances.Add(float.MaxValue);
            }
            else
            {
                float dis = Vector3.Distance(markerAngles[i].Item1, gazepos);
                distances.Add(dis);
            }
        }

        // yes this is a horrible implementation but I'm crunched for time so I'll do this for now

        float minDis = float.MaxValue;
        minIdx1 = int.MaxValue;
        minIdx2 = int.MaxValue;
        minIdx3 = int.MaxValue;

        for (int i = 0; i < distances.Count; i++)
        {
            if (distances[i] < minDis)
            {
                minDis = distances[i];
                minIdx1 = i;
            }
        }

        minDis = float.MaxValue;

        for (int i = 0; i < distances.Count; i++)
        {
            if (distances[i] < minDis && i != minIdx1)
            {
                minDis = distances[i];
                minIdx2 = i;
            }
        }

        minDis = float.MaxValue;

        for (int i = 0; i < distances.Count; i++)
        {
            if (distances[i] < minDis && i != minIdx1 && i != minIdx2)
            {
                minDis = distances[i];
                minIdx3 = i;
            }
        }

    }

    // we decide to call this for every user for every frame
    void AddAndSaveWeights(string pcName, int frameIdx, int userId, int rotationY, int loopId)
    {
        WeightCounter counter = new WeightCounter();
        counter.Add(currentPointGazeImportance);
        List<float> weightTemp = counter.Average();
        Debug.Log("Tryna save weights for " + pcName + " frame " + frameIdx.ToString() + " user " + userId.ToString());


        //if (weightTemp != null && weightTemp.Sum() > 0) 
        if (weightTemp != null && weightTemp.Sum() > 0) // not saving the 0 frames and thus we can assume that any frame not saved is a zero frame
        {
            string saveDir = pcSaveFolderName + @"Raw/";
            string savefilename = saveDir +
                                    "PointCloudSaved_" + pcName +
                                    "_frame" + frameIdx.ToString() +
                                    "_user" + userId.ToString() +
                                    "_loop" + loopId.ToString() +
                                    "_rotation" + rotationY.ToString() +
                                    ".txt";

            if (!System.IO.Directory.Exists(saveDir))
                Directory.CreateDirectory(saveDir);

            if (!File.Exists(savefilename))
            {
                File.WriteAllText(savefilename, string.Empty);

                StreamWriter sw = new StreamWriter(savefilename, true);

                //sw.WriteLine("GazeCount");
                //sw.Flush();

                for (int i = 0; i < weightTemp.Count; i++)
                {
                    sw.WriteLine(weightTemp[i]);
                }

                sw.Close();
                sw.Dispose();

            }
            else
            {
                Debug.LogWarning("File already exists " + savefilename + ", remove it so updated data can be written!");
            }
        }
        else
        {
            stats.zeroWeightCounter += 1;
        }
    }

    void SaveAndLogStatistics(int userId, string pcName, int loopId)
    {
        string statDir = pcSaveFolderName + @"/Statistics/";
        if (!Directory.Exists(statDir))
            Directory.CreateDirectory(statDir);

        string statFilename = statDir + "Stats_" + userId.ToString() + "_" + pcName + "loop" + loopId.ToString() + ".txt";

        if (!File.Exists(statFilename))
        {
            File.WriteAllText(statFilename, string.Empty);

            StreamWriter sw = new StreamWriter(statFilename, true);

            sw.WriteLine("totalCounter: " + stats.totalCounter);
            sw.WriteLine("gazeInvalidCounter: " + stats.gazeInvalidCounter);
            sw.WriteLine("inSquareCounter: " + stats.inSquareCounter);
            sw.WriteLine("fixationCounter: " + stats.fixationCounter);
            sw.WriteLine("saccadeCounter: " + stats.saccadeCounter);
            sw.WriteLine("noFixationCounter: " + stats.noFixationCounter);
            sw.WriteLine("zeroWeightCounter: " + stats.zeroWeightCounter);

            sw.Close();
            sw.Dispose();

        }
        else
        {
            Debug.LogWarning("File already exists " + statFilename + ", remove it so updated data can be written!");
        }

    }

    void AggregateStats()
    {
        string statDir = pcSaveFolderName + @"/Statistics/";
        List<GazeStatistics> stats = new List<GazeStatistics>();

        foreach (String file in Directory.EnumerateFiles(statDir))
        {
            if (file.Contains("meta") || !file.Contains(".txt"))
                continue;

            GazeStatistics tempStats = new GazeStatistics();
            List<string> lines = File.ReadAllLines(file).ToList();
            tempStats.totalCounter = Int32.Parse(lines[0].Split(":").Last().Substring(1));
            tempStats.gazeInvalidCounter = Int32.Parse(lines[1].Split(":").Last().Substring(1));
            tempStats.inSquareCounter = Int32.Parse(lines[2].Split(":").Last().Substring(1));
            tempStats.fixationCounter = Int32.Parse(lines[3].Split(":").Last().Substring(1));
            tempStats.saccadeCounter = Int32.Parse(lines[4].Split(":").Last().Substring(1));
            tempStats.noFixationCounter = Int32.Parse(lines[5].Split(":").Last().Substring(1));
            tempStats.zeroWeightCounter = Int32.Parse(lines[6].Split(":").Last().Substring(1));

            stats.Add(tempStats);
        }

        string currentCSVPath = statDir + "overall_stats.csv";

        CsvConfiguration csvConfiguration = new CsvConfiguration(CultureInfo.CurrentCulture)
        {
            HasHeaderRecord = false,
            IncludePrivateMembers = true
        };

        StreamWriter userStreamWriter = new StreamWriter(currentCSVPath, true);
        CsvWriter userCsvWriter = new CsvWriter(userStreamWriter, csvConfiguration);

        foreach (GazeStatistics stat in stats)
        {
            userCsvWriter.WriteRecord(stat);
            userCsvWriter.NextRecord();
            userCsvWriter.Flush();
        }

        userCsvWriter.Dispose();
        userStreamWriter.Dispose();
    }

    void ResetGazeImportance(int verticeCount)
    {
        currentPointGazeImportance.Clear();

        currentPointGazeImportance = Enumerable.Repeat(0f, verticeCount).ToList();

        //for (int i = 0; i < verticeCount; i++)
        //{
        //    currentPointGazeImportance.Add(0f);
        //}
    }

    void RegisterPoints(Vector3 gazeDir, Vector3 origin, float currentAngleThreshold = 0f)
    {
        float angleThreshold;
        angleThreshold = Mathf.Max(globalAngleThreshold, currentAngleThreshold);
        angleThreshold += 0.5f;

        float minDistance = float.MaxValue;

        Vector3 normalVector = new Vector3(1f, 1f, -(gazeDir.x + gazeDir.y) / gazeDir.z);

        List<int>[] segments = new List<int>[slices * angleSegments];

        Vector3[] closestPoints = new Vector3[slices * angleSegments];

        float[] minDistances = new float[slices * angleSegments];

        List<Vector3> pointList = pcChildObject.GetComponent<MeshFilter>().sharedMesh.vertices.ToList();

        for (int i = 0; i < slices * angleSegments; i++)
        {
            segments[i] = new List<int>();
            minDistances[i] = float.MaxValue;
        }

        for (int i = 0; i < pointList.Count; i++)
        {
            Vector3 point = pcChildObject.transform.TransformPoint(pointList[i]);
            Vector3 pointDir = point - origin;

            float angleInDegree = Mathf.Abs(Vector3.Angle(gazeDir, pointDir));


            if (angleInDegree < angleThreshold)
            {
                float distance = Mathf.Abs(Vector3.Dot(pointDir, gazeDir) / gazeDir.magnitude);

                if (distance < minDistance)
                {
                    minDistance = distance;
                }

                float perAngle = angleThreshold / angleSegments;

                for (int p = 0; p < angleSegments; p++)
                {
                    if (angleInDegree <= (p + 1) * perAngle && angleInDegree > p * perAngle)
                    {
                        float lamda = gazeDir.x * point.x + gazeDir.y * point.y + gazeDir.z * point.z;
                        float k = (lamda - gazeDir.x * origin.x - gazeDir.y * origin.y - gazeDir.z * origin.z) / (gazeDir.x * gazeDir.x + gazeDir.y * gazeDir.y + gazeDir.z * gazeDir.z);
                        Vector3 intersect = origin + k * gazeDir;
                        Vector3 distanceVector = point - intersect;
                        float angle = Vector3.SignedAngle(normalVector, distanceVector, gazeDir) + 180f;
                        float perSlice = 360f / slices;
                        for (int q = 0; q < slices; q++)
                        {
                            if (angle <= (q + 1) * perSlice && angle > q * perSlice)
                            {
                                segments[p * slices + q].Add(i);
                                if (distance < minDistances[p * slices + q])
                                {
                                    minDistances[p * slices + q] = distance;
                                    closestPoints[p * slices + q] = point;
                                }
                            }
                        }
                    }
                }
            }
        }

        for (int i = 0; i < segments.Length; i++)
        {
            Vector3 dirClose = closestPoints[i] - origin;
            float mDist = Vector3.Dot(gazeDir, dirClose) / gazeDir.magnitude;
            float radius = (mDist + acceptingDepthRange) * Mathf.Tan(angleThreshold * Mathf.PI / 180);
            foreach (int j in segments[i])
            {
                Vector3 point = pcChildObject.transform.TransformPoint(pointList[j]);
                Vector3 diffvec = point - closestPoints[i];
                float depth = Vector3.Dot(gazeDir, diffvec) / gazeDir.magnitude;

                if (depth < acceptingDepthRange && depth > 0f)
                {
                    Vector3 dir = point - origin;
                    float angleInDegree = Mathf.Abs(Vector3.Angle(gazeDir, dir));
                    float pDist = Vector3.Dot(gazeDir, dir) / gazeDir.magnitude;
                    float pRadius = pDist * Mathf.Tan(angleInDegree * Mathf.PI / 180);
                    float var = radius * radius / 3f / 3f;
                    currentPointGazeImportance[j] += Mathf.Exp(-Mathf.Pow(pRadius, 2f) / (2f * var)) / Mathf.Sqrt(2f * Mathf.PI * var);
                }

            }
        }


    }

    EFixation CalculateFixation(UserGazeData currentUserGaze, List<UserGazeData> gazeWindowSet)
    {
        // calculates the "fixation" for one gaze, returns from the "Fixation" enum
        // NO_FIXATION or 0: the elements in gazeWindowSet cannot be considered as fixation - maybe not enough elements, or angles between subsequent gazes are too small(?)
        // SHORT_FIXATION or 1: the elements in gazeWindowSet are grouped i.e. angles between subsequent gazes are larger than the threshold but the timespan is too short
        // VALID_FIXATION or 2: the elements in gazeWindowSet are a fixation i.e. angles between subsequent gazes are larger than the threshold and timespan is also larger than the fixationMinInterval 

        // gazewindowset is a list of usergazedata objects
        if (gazeWindowSet.Count > 1) // more than 2 gazes in a window can be considered a fixation set
        {
            //Debug.Log("3. Found " + gazeWindowSet.Count + " (more than 1) gazes in the window set");
            Vector3 currentRay = EyeTrackingUtils.Instance.GetDirectionFromGazeData(currentUserGaze); // current direction ray of the eye gaze

            for (int i = 0; i < gazeWindowSet.Count; i++)
            {
                Vector3 rayI = EyeTrackingUtils.Instance.GetDirectionFromGazeData(gazeWindowSet[i]); // get direction of the gaze in the gazewindowset
                float angle = Mathf.Abs(Vector3.Angle(rayI, currentRay)); // the angle between the gaze in the gazeWindowSet and the current gaze
                                                                          //Debug.Log("3.1 Angle between user gaze and window set gaze is " + angle);

                if (angle >= fixationDispersionThreshold) // if angle is greater than our desired threshold
                {
                    // check the timespan between the first and the last gaze
                    float frameDifference = gazeWindowSet[gazeWindowSet.Count - 1].pcFrameIndex - gazeWindowSet[0].pcFrameIndex;
                    if (frameDifference * (1000 / fps) >= fixationIntervalThreshold) // time duration greater than (or equals) threshold time duration in ms
                        return EFixation.VALID_FIXATION;

                    // if the angle is greater than threshold but the fixation time is too short, then we classify this as a short fixation or a saccade (rapid movement of the eye)
                    return EFixation.SHORT_FIXATION;
                }
            }
        }

        //Debug.Log("3.2 no fixation found with " + gazeWindowSet.Count + " gazes in window set");
        return EFixation.NO_FIXATION; // if nothing from above then return no fixation, new items will be added to the gazeWindowSet for evaluation in the next iterations
    }

    void ProcessHeatmapPerFrame()
    {
        string saveDir = calcSum ? pcSaveFolderName + @"Processed_Sum/" : pcSaveFolderName + @"Processed/";
        saveDir = dbScan ? saveDir + @"DbScan/" : saveDir + @"NoDbScan/";

        string readDir = pcSaveFolderName + @"Raw/";

        foreach (string pcname in pcNamesToLoad)
        {
            List<string> rawFiles = Directory.GetFiles(readDir).ToList();
            //Debug.Log("raw files found " + rawFiles.Count + " in dir " + readDir);

            for (int frameIdx = 0; frameIdx < 250; frameIdx++)
            {
                string tempDir = saveDir + "PointCloudSaved_" + pcname + "_frame" + frameIdx.ToString() + ".txt";
                if (File.Exists(tempDir))
                {
                    Debug.LogWarning("File already exists " + tempDir + ", remove it so updated data can be written!");
                    continue;
                }

                WeightCounter tempCounter = new WeightCounter();

                foreach (string file in rawFiles)
                {

                    if (file.Contains(".meta") || !file.Contains("_frame" + frameIdx.ToString() + "_") || !file.Contains("PointCloudSaved_" + pcname))
                    {
                        //Debug.Log("Skipping file for heatmap " + file + " current frame index is " + frameIdx.ToString());
                        continue;
                    }

                    Debug.Log("Reading file for heatmap processing " + file + " current frame index is " + frameIdx.ToString());

                    List<string> lines = File.ReadLines(file).ToList();
                    float[] currentWeights = new float[lines.Count - 1];
                    for (int i = 0; i <= lines.Count - 2; i++) // end at count-2 since last line is blank
                    {
                        currentWeights[i] = float.Parse(lines[i]);
                    }

                    tempCounter.Add(currentWeights.ToList());
                }

                List<float> weightTemp = calcSum ? tempCounter.Sum() : tempCounter.Average();
                if (weightTemp == null)
                {
                    Debug.Log("Weight 0 overall for " + pcname + " frame " + frameIdx.ToString());
                }


                if (weightTemp != null)
                {
                    //if (dbScan)
                    //{
                    //    weightTemp = DBScan(pcname, frameIdx, weightTemp);
                    //}
                    string savefilename = saveDir +
                                "PointCloudSaved_" + pcname +
                                "_frame" + frameIdx.ToString() +
                                ".txt";

                    if (!System.IO.Directory.Exists(saveDir))
                        Directory.CreateDirectory(saveDir);

                    //Debug.LogWarning("Writing averaged pc frame to " + savefilename);

                    if (!File.Exists(savefilename))
                    {
                        File.WriteAllText(savefilename, string.Empty);

                        StreamWriter sw = new StreamWriter(savefilename, true);

                        //sw.WriteLine("GazeCount");
                        //sw.Flush();

                        for (int i = 0; i < weightTemp.Count; i++)
                        {
                            sw.WriteLine(weightTemp[i]);
                        }

                        sw.Close();
                        sw.Dispose();

                    }
                    else
                    {
                        Debug.LogWarning("File already exists " + savefilename + ", remove it so updated data can be written!");
                    }
                }
                else
                {
                    Debug.LogError("Average weight null for frame " + frameIdx);
                }
            }
        }
    }

    //List<float> DBScan(string pcname, int frameIdx, List<float> weights)
    //{
    //    // first lets load the mesh
    //    pcChildObject.GetComponent<MeshFilter>().mesh = pointClouds[GetPcIndexFromName(pcname)].LoadMeshForFrame(frameIdx);

    //    Resources.UnloadUnusedAssets();

    //    pcChildObject.transform.localPosition = new Vector3(-1 * pcChildObject.GetComponent<MeshFilter>().sharedMesh.bounds.center.x, 0f, -1 * pcChildObject.GetComponent<MeshFilter>().sharedMesh.bounds.center.z);
    //    pcGameObject.SetActive(true);

    //    // Theta is the min number of pts required inside that circle for that data pt to be classified as a core pt
    //    // Theta should increase with the pt size Alpha of a pt cloud becomes small
    //    // Theta = (2^7)/( 1 + (20*Alpha))
    //    float alpha = 0.0012f; // square shader params set to pt size 1, model scaling factor 0.002, pt scaling factor 0.6, overall get 0.0012 (verified in the scene)
    //    float theta = Mathf.Pow(2, 7) / (1 + (20*alpha));

    //    int k = 6;

    //    List<Vector3> vertices = pcChildObject.GetComponent<MeshFilter>().sharedMesh.vertices.ToList();

    //    //float epsilon = KnnGraph(vertices, k, weights);
    //    float epsilon = 0.12f; // now just going with pt size * 100 for 100 nearest neighbours
    //    Debug.Log("DBScan with min pts: " + theta + " and radius around pt: " + epsilon + " for " + pcname + " frame " + frameIdx);

    //    // Epsilon is the radius of the circle to be created around each data point to check the density
    //    // Epsilon is decided by k-distance graph.

    //    // k can be a few values:
    //    // 1. 2*dimensions = 6 (in our case). we want the 2*dim-1 (5) nearest neighbours but if we include the pt itself (distance = 0) then it's 2*dim pts we're looking for

    //    // We apply DBScan on the fixation map

    //    List<int> indicesToRemove = new List<int>();

    //    for (int i = 0; i < vertices.Count - 1; i++)
    //    {
    //        int nearPointsWithNonZeroWeight = 0;

    //        if (weights[i] == 0) // no need to do any DBScan for 0 weight points (which will be most of them)
    //        {
    //            continue;
    //        }

    //        for (int j = 0; j < vertices.Count; j++)
    //        {
    //            float dist = Vector3.Distance(pcChildObject.transform.TransformPoint(vertices[i]), pcChildObject.transform.TransformPoint(vertices[j]));
    //            if (dist <= epsilon && weights[j] != 0)
    //            { 
    //                nearPointsWithNonZeroWeight++;
    //            }
    //        }
    //        if (nearPointsWithNonZeroWeight < theta) // if there's less than minPts worth of pts nearby with non-zero weights
    //        {
    //            indicesToRemove.Add(i);
    //        }
    //    }

    //    foreach (int i in indicesToRemove)
    //    {
    //        weights[i] = 0.0f;
    //    }

    //    // we return the dbscanned weights
    //    return weights;

    //}

    //float KnnGraph(List<Vector3> vertices, int k, List<float> weights)
    //{
    //    // calc the k-dist graph
    //    // find a threshold pt
    //    // the k-dist of this threshold pt is considered the epsilon

    //    List<float> xIndices = new List<float>();
    //    List<float> distances = new List<float>();

    //    Debug.Log("DBScan Weights Count is " + weights.Count + " and vertice count is " + vertices.Count);

    //    for (int i = 0; i < vertices.Count - 1; i++)
    //    {
    //        if (weights[i] != 0) // only for the pts with non-zero weights
    //        {
    //            distances.Add(KnnDistance(vertices[i], vertices, k));
    //            xIndices.Add(i);
    //        }
    //    }

    //    List<float> slopes = new List<float>();

    //    for (int i = 0; i < xIndices.Count - 1; i++)
    //    {
    //        slopes.Add((distances[i + 1] - distances[i]) / (xIndices[i + 1] - xIndices[i]));
    //    }

    //    int maxIdx = slopes.IndexOf(slopes.Max());

    //    if (maxIdx < 50)
    //    {
    //        Debug.Log("DBScan: max slope is within the first 50 points");
    //        return distances[maxIdx];
    //    }

    //    return distances[maxIdx - 50];
    //}

    //float KnnDistance(Vector3 point, List<Vector3> vertices, int k)
    //{
    //    // Calculate the distances between the point and all other vertices
    //    var distances = vertices.Select(vertex => new { Distance = Vector3.Distance(pcChildObject.transform.TransformPoint(point), pcChildObject.transform.TransformPoint(vertex)) }).ToList();

    //    // Sort the distances by distance in ascending order
    //    var sortedDistances = distances.OrderBy(distance => distance.Distance).ToList();

    //    // get average distance of the k nearest neighbours
    //    var nearestDistances = sortedDistances.Take(k).Select(distance => distance.Distance).ToList();
    //    float kDistance = nearestDistances.Average();

    //    return kDistance;
    //}

}
