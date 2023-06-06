using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UIElements;

[System.Flags] // Flags allows to select multiple enum values
public enum PointCloudType
{ 
    [InspectorName("Deselect All")]  DeselectAll = 0,
	[InspectorName("Select All")]  SelectAll = LongDress | Loot | RedAndBlack | Soldier,
    LongDress = 1<<0, 
    Loot = 1<<1, 
    RedAndBlack = 1<<2, 
    Soldier = 1<<3
}; // Need to assign ints in the power of 2 for Flags to work

[System.Flags]
public enum PointCloudRepresentation { Point = 1, Disk = 2, Square = 4, Mesh = 8};

[System.Serializable]
public class TestTask
{
    /*
     * A task contains the following: checklist of objects, checklist of representations, list of float distances, list of vector2 qualities
     * If possible then also display the "total number of sequences" at the bottom after every update, OnValidate function is a good option
     */

    public PointCloudType m_types;
    public PointCloudRepresentation m_representations;

	[Rename("Distances (m)", "Distance (m)")]
    [FormerlySerializedAs("Stance")]
	public float[] m_distances;

	[Rename("Qualities", "Quality (from, to)")]
	public Vector2[] m_qualities;

}