using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[System.Flags]
public enum PointCloudRepresentation { Point = 1, Disk = 2, Square = 4, Mesh = 8 };

[System.Serializable]
public class STTaskData
{
	/*
     * A task contains the following: checklist of objects, checklist of representations, list of float distances, list of vector2 qualities
     */

	// TODO: If possible then also display the "total number of sequences" at the bottom after every update, OnValidate function is a good option

	public PCObjectType m_types;
    public PointCloudRepresentation m_representations;

    public float[] m_distances;

    //[Rename("Qualities", "Quality (from, to)")]
    public Vector2[] m_qualities;


}
