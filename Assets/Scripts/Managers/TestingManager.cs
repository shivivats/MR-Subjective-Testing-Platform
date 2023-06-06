using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using UnityEngine;
using Unity.VisualScripting;
using UnityEditor;
using System.IO;

public class TestingManager : MonoBehaviour
{
    private GameObject playerCamera;

    public GameObject targetPCObject;

    public TextMeshProUGUI distanceText;

    public float zAddDistance;

    private void Start()
    {
        playerCamera = GameObject.FindGameObjectWithTag("MainCamera");
    }

    void Update()
    {
        //targetPCObject.transform.SetLocalPositionAndRotation(new Vector3(playerCamera.transform.position.x, playerCamera.transform.position.y, targetPCObject.transform.position.z), targetPCObject.transform.rotation);
        //targetPCObject.transform.SetLocalPositionAndRotation(new Vector3(targetPCObject.transform.position.x, playerCamera.transform.position.y - 1.8f, playerCamera.transform.position.z + zAddDistance), targetPCObject.transform.rotation);

        distanceText.text = "x: " + (targetPCObject.transform.position.x - playerCamera.transform.position.x).ToString() +
                            ", y: " + (targetPCObject.transform.position.y - playerCamera.transform.position.y).ToString() +
                            ", z: " + (targetPCObject.transform.position.z - playerCamera.transform.position.z).ToString() +
                            ", total: " + (targetPCObject.transform.position - playerCamera.transform.position).magnitude.ToString();

    }

}