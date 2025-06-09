using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class PLYUtils : MonoBehaviour
{
	public static PLYUtils Instance { get; private set; }

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

	public PLYDataBody ReadDataBody(PLYDataHeader header, BinaryReader reader)
	{
		PLYDataBody dataBody = new PLYDataBody(header.vertexCount);

		float x = 0, y = 0, z = 0;

		for (int i = 0; i < header.vertexCount; i++)
		{
			foreach (PLYDataProperty property in header.properties)
			{
				switch (property)
				{
					case PLYDataProperty.X: x = reader.ReadSingle(); break;
					case PLYDataProperty.Y: y = reader.ReadSingle(); break;
					case PLYDataProperty.Z: z = reader.ReadSingle(); break;

					case PLYDataProperty.Xd: x = (float)reader.ReadDouble(); break;
					case PLYDataProperty.Yd: y = (float)reader.ReadDouble(); break;
					case PLYDataProperty.Zd: z = (float)reader.ReadDouble(); break;

					case PLYDataProperty.Data8: reader.ReadByte(); break;
					case PLYDataProperty.Data16: reader.BaseStream.Position += 2; break;
					case PLYDataProperty.Data32: reader.BaseStream.Position += 4; break;
					case PLYDataProperty.Data64: reader.BaseStream.Position += 8; break;
				}
			}
			dataBody.AddPoint(x, y, z);
		}

		for (int i = 0; i < 1; i++)
		{
			Debug.Log("Vertice " + dataBody.vertices[i].x + ", " + dataBody.vertices[i].y + ", " + dataBody.vertices[i].z + " added to databody");
		}


		return dataBody;
	}
	public PLYDataHeader ReadDataHeader(StreamReader reader)
	{
		PLYDataHeader dataHeader = new PLYDataHeader();
		int readCount = 0;

		Debug.Log("Reading header...");

		string line;
		line = reader.ReadLine();
		readCount += line.Length + 1;
		if (line != "ply")
			Debug.LogError("Invalid PLY file! First line not \"ply\"");

		line = reader.ReadLine();
		readCount += line.Length + 1;
		if (line != "format binary_little_endian 1.0")
			Debug.LogError("Invalid format! Should be binary little endian for this usage.");

		while (true)
		{
			line = reader.ReadLine();
			//Debug.Log(line);
			readCount += line.Length + 1;

			if (line.Contains("comment"))
				continue;

			if (line == "end_header")
				break;

			string[] cols = line.Split(" ");
			//Debug.Log(cols);
			if (cols[0] == "element" && cols[1] == "vertex")
			{
				dataHeader.vertexCount = int.Parse(cols[2]);
			}
			else if (cols[0] == "property")
			{

				PLYDataProperty property = PLYDataProperty.Invalid;

				switch (cols[2])
				{
					case "x": property = (cols[1] == "double") ? PLYDataProperty.Xd : PLYDataProperty.X; break;
					case "y": property = (cols[1] == "double") ? PLYDataProperty.Yd : PLYDataProperty.Y; break;
					case "z": property = (cols[1] == "double") ? PLYDataProperty.Zd : PLYDataProperty.Z; break;
				}

				if (cols[1] == "char" || cols[1] == "uchar")
				{
					if (property == PLYDataProperty.Invalid)
						property = PLYDataProperty.Data8;
					else if (GetPropertySize(property) != 1)
						throw new ArgumentException("Invalid property type ('" + line + "').");
				}
				else if (cols[1] == "short" || cols[1] == "ushort")
				{
					if (property == PLYDataProperty.Invalid)
						property = PLYDataProperty.Data16;
					else if (GetPropertySize(property) != 2)
						throw new ArgumentException("Invalid property type ('" + line + "').");
				}
				else if (cols[1] == "int" || cols[1] == "uint" || cols[1] == "float")
				{
					if (property == PLYDataProperty.Invalid)
						property = PLYDataProperty.Data32;
					else if (GetPropertySize(property) != 4)
						throw new ArgumentException("Invalid property type ('" + line + "').");
				}
				else if (cols[1] == "double" || cols[1] == "float64")
				{
					if (property == PLYDataProperty.Invalid)
						property = PLYDataProperty.Data64;
					else if (GetPropertySize(property) != 8)
						throw new ArgumentException("Invalid property type ('" + line + "').");
				}
				else
				{
					throw new ArgumentException("Unsupported property type ('" + line + "').");
				}

				dataHeader.properties.Add(property);
			}
		}

		// Rewind the stream back to the exact position of the reader.
		reader.BaseStream.Position = readCount;

		return dataHeader;
	}

	public int GetPropertySize(PLYDataProperty plyDataProperty)
	{
		switch (plyDataProperty)
		{
			case PLYDataProperty.X: return 4;
			case PLYDataProperty.Y: return 4;
			case PLYDataProperty.Z: return 4;

			case PLYDataProperty.Xd: return 8;
			case PLYDataProperty.Yd: return 8;
			case PLYDataProperty.Zd: return 8;

			default: return 0;
		}
	}
}

public class PLYDataHeader
{
	public List<PLYDataProperty> properties = new List<PLYDataProperty>();
	public int vertexCount = -1;
}

public enum PLYDataProperty
{
	Invalid,
	X, Y, Z,
	Xd, Yd, Zd,
	R, G, B, A,
	Data8, Data16, Data32, Data64
}

public class PLYDataBody
{
	public List<Vector3> vertices;

	public PLYDataBody(int vertexCount)
	{
		vertices = new List<Vector3>(vertexCount);
	}

	public void AddPoint(float x, float y, float z)
	{
		vertices.Add(new Vector3(x, y, z));
	}
}