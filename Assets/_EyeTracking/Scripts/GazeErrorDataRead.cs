using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.MixedReality.Toolkit.Utilities.Solvers;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using static UnityEditor.PlayerSettings;
using static UnityEditor.Recorder.OutputPath;
using static UnityEngine.GraphicsBuffer;

/*
 *  class ErrorDataProcessed
 *  public int userId;
	public int markerId;
	public Vector3 markerPosition;
	public Vector3 stdDeviation;
	public float averageAccuracy;
	public float rmsPrecision;
	public float totalSamples;
	public float excludedSamples;
 */

public class GazeErrorDataRead : MonoBehaviour
{
    public static GazeErrorDataRead Instance { get; private set; }

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

    public int ignoreTime = 1500;
    float ignoreFrames;

    public string jsonDir = @".\Assets\_EyeTracking\JSON";
    public float gazeAngleThreshold = 7.5f;

    private void Start()
    {
        // we run the game at 60 fps and since we use frames since start as the timestamp, this is how we get how much data to ignore for every marker

        ignoreFrames = (ignoreTime / 1000) * 60;

        ReadAllErrorFiles();

        CloseUserCSVWriter();

        EditorApplication.isPlaying = false;
    }

    int GetUserIDFromErrorFilename(string str)
    {
        string userString = str.Split("_").Reverse().ToArray()[1]; // this will give us "UserN" since its the second last element after splitting
        string userNumberFromString = userString.Remove(0, 4); // remove the "User" from userstring
        Debug.Log("userNumberFromString " + userNumberFromString);
        return Int32.Parse(userNumberFromString);
    }

    int GetErrorIndexFromErrorFilename(string str)
    {
        string[] filenameSplitReverse = str.Split("_").Reverse().ToArray();
        string errorString = filenameSplitReverse[0].Remove(0, 5); // remove the "Error" from string
        errorString = errorString.Remove(errorString.Length - 5); // remove the ".json" from the string
        return Int32.Parse(errorString);
    }


    CsvConfiguration csvConfiguration = new CsvConfiguration(CultureInfo.CurrentCulture)
    {
        HasHeaderRecord = false,
        IncludePrivateMembers = true
    };

    StreamWriter userStreamWriter;
    CsvWriter userCsvWriter;

    private void InitialiseNewUserCSVWriter(int userId)
    {
        string currentCSVPath = @"./Assets/_EyeTracking/ErrorCSV/User_" + userId.ToString() + ".csv";

        userStreamWriter = new StreamWriter(currentCSVPath, true);
        userCsvWriter = new CsvWriter(userStreamWriter, csvConfiguration);

    }

    private void WriteToUserCSV(ErrorDataProcessed processedErrorData)
    {
        userCsvWriter.WriteRecord(processedErrorData);
        userCsvWriter.NextRecord();
        userCsvWriter.Flush();
    }

    private void CloseUserCSVWriter()
    {
        userCsvWriter.Dispose();
        userStreamWriter.Dispose();
    }

    void ReadAllErrorFiles()
    {
        List<string> jsonFiles = Directory.GetFiles(jsonDir, "*.json").ToList();

        HashSet<int> userIDs = new HashSet<int>();

        // lets extract the user IDs from the JSON files
        foreach (string path in jsonFiles)
        {
            if (path.Contains(".meta") || !path.Contains("Error"))
                continue;

            userIDs.Add(GetUserIDFromErrorFilename(path));
        }

        // loop over all the user IDs and then only use the JSON files for that user ID
        foreach (int userId in userIDs)
        {
            if(userCsvWriter!=null) CloseUserCSVWriter();
            InitialiseNewUserCSVWriter(userId);

            Dictionary<int, List<string>> linesPerMarker = new Dictionary<int, List<string>>();

            foreach (string path in jsonFiles) // for every file in the directory
            {

                // if its a meta file OR if it doesnt contain error in the name OR if it doesnt belong to the currently considered user
                if (path.Contains(".meta") || !path.Contains("Error") || !path.Contains(userId.ToString()))
                    continue;

                string currentJsonFilepath = path;
                int currentUserId = GetUserIDFromErrorFilename(path);
                int currentErrorIndex = GetErrorIndexFromErrorFilename(path);

                Debug.Log("Error Data Read: Current user id is " + currentUserId + " and current error index is: " + currentErrorIndex + " for file " + currentJsonFilepath);

                List<string> jsonLines = File.ReadLines(currentJsonFilepath).ToList(); // getting all the lines from the error files for one user

                float initialTimestamp = float.MaxValue;
                int prevMarkerId = -1;

                int writeCounter = 0;
                int totalCounter = 0;

                for (int i = 0; i < jsonLines.Count; i++)
                {
                    ErrorProfilingData gazeData = JsonConvert.DeserializeObject<ErrorProfilingData>(jsonLines[i]);

                    if (gazeData.markerId != prevMarkerId)
                    {
                        initialTimestamp = gazeData.timestamp;
                        prevMarkerId = gazeData.markerId;
                    }

                    if (gazeData.timestamp - initialTimestamp > ignoreFrames)
                    {
                        if (!linesPerMarker.ContainsKey(gazeData.markerId))
                            linesPerMarker.Add(gazeData.markerId, new List<string>());

                        linesPerMarker[gazeData.markerId].Add(jsonLines[i]);
                        writeCounter++;
                    }
                    totalCounter++;
                }
                Debug.Log("Wrote " + writeCounter + " lines out of " + totalCounter + " lines");

            }

            // in linesPerMarker we now have all the json lines associated with every marker

            /*
             * class ErrorDataProcessed
             * 	public int userId;
                public int markerId;
                public Vector3 markerPosition;
                public Vector3 stdDeviation;
                public float averageAccuracy;
                public float rmsPrecision;
                public float totalSamples;
                public float excludedSamples;
             */

            // for every marker, we want to skip the ignoreLines amount of lines.
            foreach (int k in linesPerMarker.Keys)
            {
                ErrorDataProcessed processedData = new ErrorDataProcessed(userId, k);

                Dictionary<int, List<float>> markerAngles = new Dictionary<int, List<float>>();

                Dictionary<int, List<float>> interSampleDifferences = new Dictionary<int, List<float>>();

                Vector3 prevGaze = Vector3.zero;

                ErrorProfilingData tempData = JsonConvert.DeserializeObject<ErrorProfilingData>(linesPerMarker[k][0]);

                processedData.markerPositionX = tempData.markerPositionX;
                processedData.markerPositionY = tempData.markerPositionY;
                processedData.markerPositionZ = tempData.markerPositionZ;

                processedData.totalSamples = linesPerMarker[k].Count;

                int validSamples = 0;

                for (int i = 0; i < linesPerMarker[k].Count; i++)
                {
                    ErrorProfilingData gazeData = JsonConvert.DeserializeObject<ErrorProfilingData>(linesPerMarker[k][i]);

                    Vector3 gazeOrigin = new Vector3(gazeData.gazeOriginX, gazeData.gazeOriginY, gazeData.gazeOriginZ);

                    Vector3 gazeDirectionNormalised = new Vector3(gazeData.gazeDirectionNormalisedX, gazeData.gazeDirectionNormalisedY, gazeData.gazeDirectionNormalisedZ);

                    Vector3 markerPosition = new Vector3(gazeData.markerPositionX, gazeData.markerPositionY, gazeData.markerPositionZ);

                    Vector3 dirToMarker = markerPosition - gazeOrigin;

                    float angleDiff = Mathf.Abs(Vector3.Angle(dirToMarker, gazeDirectionNormalised));

                    if (angleDiff > gazeAngleThreshold) // invalid gaze, we ignore
                        continue;

                    // from this point on, this is a valid gaze that we want to use

                    validSamples++;

                    if (prevGaze != Vector3.zero)
                    {
                        if (!interSampleDifferences.ContainsKey(gazeData.markerId))
                        {
                            interSampleDifferences.Add(gazeData.markerId, new List<float>());
                        }
                        interSampleDifferences[gazeData.markerId].Add(Mathf.Abs(Vector3.Angle(prevGaze, gazeDirectionNormalised)));
                    }

                    if (!markerAngles.ContainsKey(gazeData.markerId))
                    {
                        markerAngles.Add(gazeData.markerId, new List<float>());
                    }
                    markerAngles[gazeData.markerId].Add(angleDiff);

                    Debug.Log("Read gaze data " + gazeOrigin + ", " + gazeDirectionNormalised + ", " + markerPosition + " from user data");
                    Debug.Log("Angle difference for marker " + gazeData.markerId + " and timestamp " + gazeData.timestamp + " is: " + angleDiff);

                    prevGaze = gazeDirectionNormalised;
                }

                // now we've processed the line for one marker and one user for one error file
                processedData.excludedSamples = processedData.totalSamples - validSamples;

                if (markerAngles.ContainsKey(k))
                {
                    processedData.averageAccuracy = markerAngles[k].Average();

                }
                else
                {
                    processedData.averageAccuracy = -1; // marker is invalid
                }

                if (interSampleDifferences.ContainsKey(k))
                {
                    double square = 0.0f;
                    for (int i = 0; i < interSampleDifferences[processedData.markerId].Count; i++)
                    {
                        square += Mathf.Pow(interSampleDifferences[processedData.markerId][i], 2);
                    }

                    double mean = square / interSampleDifferences[processedData.markerId].Count;
                    double root = Math.Sqrt(mean);
                    processedData.rmsPrecision = root;
                }
                else
                {
                    processedData.rmsPrecision = -1; // marker is invalid
                }

                WriteToUserCSV(processedData);

            }

        }
    }
}

