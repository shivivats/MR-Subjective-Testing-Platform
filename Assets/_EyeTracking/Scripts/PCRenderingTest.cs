using Microsoft.MixedReality.Toolkit.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class PCRenderingTest : MonoBehaviour
{

	public string pointCloudsDirectory = @".\Assets\Resources\PointClouds\VPCC\";

	Dictionary<string, List<List<Vector3>>> loadedPointCloudsDict = new Dictionary<string, List<List<Vector3>>>();

	public Vector3 testPos = new Vector3(0.623f, -1f, 2f);
	public Vector3 offset = new Vector3(0.324f, 0.933f, 0.365f);
	public float testRot = 180f;
	public Vector3 testScale = new Vector3(0.002f, 0.002f, 0.002f);

	private List<Vector3> currPc;
	List<Vector3> points;
	bool loaded = false;
	// Start is called before the first frame update
	void Start()
    {
		LoadAllPointClouds();

		currPc=LoadCurrentPCFrameAtPositionAndRotation("BlueSpin", 0, testPos, testRot);

		ShowLoadedPCFrame();

	}

	private void ShowLoadedPCFrame()
	{
		// render a collection of points
		// i could gizmo draw mesh to test here

		// when loading the PC i could load the whole mesh instead
		// translate, rotate, scale the mesh
		// get all of its points aka vertices
		// convert them from local to world space
		// use that as our "points" vector3 list

		//Bounds bd = gameObject.GetComponent<MeshFilter>().mesh.bounds;

		//Gizmos.DrawMesh(gameObject.GetComponent<MeshFilter>().sharedMesh, testPos, Quaternion.Euler(new Vector3(0f, testRot, 0f)), testScale);

		//points = new List<Vector3>();
		//Vector3[] vertices = gameObject.GetComponent<MeshFilter>().sharedMesh.vertices;
		//Matrix4x4 localToWorld = gameObject.transform.localToWorldMatrix;
		//for (int i = 0; i < vertices.Length; ++i)
		//{
		//	points.Add(localToWorld.MultiplyPoint3x4(vertices[i]));
		//}

		points = new List<Vector3>();

		for (int i = 0; i < currPc.Count; i++)
		{
			Vector3 scaledPoint = Vector3.Scale(currPc[i], testScale);
			Vector3 translatedPoint = scaledPoint + testPos;
			//Vector3 offsetPC = translatedPC - offset;
			//Vector3 rotatedPC = Quaternion.Euler(0f, testRot, 0f) * translatedPC;
			Vector3 rotationDir = translatedPoint - (translatedPoint - offset);
			rotationDir = Quaternion.Euler(0f, testRot, 0f) * offset;
			Vector3 rotatedPoint = rotationDir + offset;

			points.Add(rotatedPoint);
		}

			loaded = true;

	}

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.yellow;
		if(loaded)
		{
			for (int i = 0; i < points.Count; i += 500)
			{
				Gizmos.DrawSphere(points[i], 0.01f);
			}
		}

	
	}

	private void LoadAllPointClouds()
	{
		// load all the point clouds at the start of the script
		// we just have 4 PCs
		// we can rotate them on a case by case basis

		//string[] pcNames = { "CasualSquat", "ReadyForWinter", "FlowerDance", "BlueSpin" };
		string[] pcNames = { "BlueSpin" };

		string rawFolderName = "raw";

		List<List<Vector3>> currLoadedPc = new List<List<Vector3>>();

		foreach (string name in pcNames)
		{
			string pcDirPath = Path.Combine(pointCloudsDirectory, name, rawFolderName);
			int fileCount = 0;
			foreach (string file in Directory.GetFiles(pcDirPath))
			{
				if(Path.GetExtension(file) == ".ply")
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

			Debug.Log("Loaded " + fileCount + " files for " + name + " from " + pcDirPath);

			loadedPointCloudsDict.Add(name, currLoadedPc);
		}
		Debug.Log("Loaded all four point clouds into dictionary");
	}

	private List<Vector3> LoadCurrentPCFrameAtPositionAndRotation(string pointCloudName, int pcFrameIndex, Vector3 globalPosition, float rotationY)
	{
		if (!loadedPointCloudsDict.ContainsKey(pointCloudName))
		{
			Debug.LogError(pointCloudName + " does not exist in the loaded point clouds dictionary! Check the ");
			return null;
		}

		List<Vector3> translatedAndRotatedFrame = new List<Vector3>();

		List<Vector3> pcFrame = loadedPointCloudsDict[pointCloudName].ElementAt(pcFrameIndex);
		for (int i = 0; i < pcFrame.Count; i++)
		{
			Vector3 v = pcFrame[i];
			//Vector3 v_rotated = Quaternion.Euler(0, rotationY, 0) * v;
			translatedAndRotatedFrame.Add(v);
		}

		return translatedAndRotatedFrame;

	}

	
}
