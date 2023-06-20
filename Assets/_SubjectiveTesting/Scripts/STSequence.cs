using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * A sequence is the most granular unit of the subjective test.
 * It contains information regarding one playback of the object.
 * Distance, quality (first and second), object type, and object representation are stored
 */
public class STSequence
{
	public STSequence(PCObjectType currentObjectType, PointCloudRepresentation currentMaterialType, float currentDistance, int currentFirstQuality, int currentSecondQuality)
	{
		this.ObjectType = currentObjectType;
		this.RepresentationType = currentMaterialType;
		this.Distance = currentDistance;
		this.FirstQuality = currentFirstQuality;
		this.SecondQuality = currentSecondQuality;
	}

	private PCObjectType objectType;
	private PointCloudRepresentation representationType;

	private float distance;
	private int firstQuality;
	private int secondQuality;

	public PCObjectType ObjectType { get => objectType; set => objectType = value; }
	public PointCloudRepresentation RepresentationType { get => representationType; set => representationType = value; }
	public float Distance { get => distance; set => distance = value; }
	public int FirstQuality { get => firstQuality; set => firstQuality = value; }
	public int SecondQuality { get => secondQuality; set => secondQuality = value; }
}
