using UnityEngine;
[System.Flags]
public enum PCObjectType
{
[InspectorName("Deselect All")] DeselectAll = 0,
[InspectorName("Select All")] SelectAll = ~0,
BlueSpin = 1 <<0,
CasualSquat = 1 <<1,
FlowerDance = 1 <<2,
ReadyForWinter = 1 <<3,
};
