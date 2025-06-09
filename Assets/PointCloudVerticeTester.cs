using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class PointCloudVerticeTester : MonoBehaviour
{

    public string verticeFoldername;
    public GameObject cubeToScalePrefab;

    public GameObject cubeToScaleParent;

    private int currentFrameIndex = 14;

    // Start is called before the first frame update
    void Start()
    {
        ShowNextFrame();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            currentFrameIndex++;
            ShowNextFrame();
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            currentFrameIndex--;
            ShowNextFrame();
        }
    }

    private void ShowNextFrame()
    {
        foreach(MeshRenderer childrender in cubeToScaleParent.transform.GetComponentsInChildren<MeshRenderer>())
        {
            Destroy(childrender.gameObject);
        }
        string currentFilename = verticeFoldername + "PointCloudSaved_BlueSpin_frame" + currentFrameIndex.ToString() + "_vertices.txt";
        string[] lines = File.ReadAllLines(currentFilename);
        for (int i = 0; i < lines.Length; i += 10)
        {
            string[] splitted = lines[i].Split(',');
            float x = float.Parse(splitted[0]);
            float y = float.Parse(splitted[1]);
            float z = float.Parse(splitted[2]);
            Instantiate(cubeToScalePrefab, new Vector3(x, y, z), Quaternion.identity, cubeToScaleParent.transform);

        }
    }
}
