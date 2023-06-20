using UnityEngine;
[System.Flags]
public enum PCObjectType
{
[InspectorName("Deselect All")] DeselectAll = 0,
[InspectorName("Select All")] SelectAll = ~0,
LongDress = 1 <<0,
Loot = 1 <<1,
Redandblack = 1 <<2,
Soldier = 1 <<3,
};
