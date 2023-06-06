using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(RenameAttribute))]
public class RenameDrawer : PropertyDrawer
{
	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		if (property.isArray)
		{
			for (int i = 0; i < property.arraySize; i++)
			{
				EditorGUI.PropertyField(position, property.GetArrayElementAtIndex(i), new GUIContent((attribute as RenameAttribute).ArrayElementName));
			}
		}
		else
		{
			EditorGUI.PropertyField(position, property, new GUIContent((attribute as RenameAttribute).ArrayElementName));
		}

	}

	bool IsArray(SerializedProperty property)
	{
		string path = property.propertyPath;
		int idot = path.IndexOf('.');
		if (idot == -1) return false;
		string propName = path.Substring(0, idot);
		SerializedProperty p = property.serializedObject.FindProperty(propName);
		return p.isArray;
	}
}