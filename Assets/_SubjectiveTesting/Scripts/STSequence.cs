using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * A sequence is the most granular unit of the subjective test.
 * It contains information regarding one playback of the object.
 * Distance, quality, object type, and object representation are stored
 */
public class STSequence
{
	public STSequence(PCObjectType currentObjectType, PointCloudMaterialRepresentation currentMaterialType, float currentDistance, QualityRepresentation currentQR)
	{
		this.ObjectType = currentObjectType;
		this.RepresentationType = currentMaterialType;
		this.Distance = currentDistance;
		this.QualityRepresentation = currentQR;
	}

	private PCObjectType objectType;
	private PointCloudMaterialRepresentation materialRepresentationType;

	private float distance;
	private QualityRepresentation qualityRepresentation;

	public PCObjectType ObjectType { get => objectType; set => objectType = value; }
	public PointCloudMaterialRepresentation RepresentationType { get => materialRepresentationType; set => materialRepresentationType = value; }
	public float Distance { get => distance; set => distance = value; }
	public QualityRepresentation QualityRepresentation { get => qualityRepresentation; set => qualityRepresentation = value; }
}
