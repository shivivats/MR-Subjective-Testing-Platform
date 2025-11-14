using UnityEngine;
[System.Flags]
public enum PCObjectType
{
[InspectorName("Deselect All")] DeselectAll = 0,
[InspectorName("Select All")] SelectAll = ~0,
Man = 1 <<0,
Lady = 1 <<1,
ReadyForWinter = 1 <<2,
//ReadyForWinter = 1 <<3,
};
