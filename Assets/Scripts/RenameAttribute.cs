using UnityEngine;

public class RenameAttribute : PropertyAttribute
{
	public string MainName { get; private set; }

	public string ArrayElementName { get; private set; }
	public RenameAttribute(string mainName, string arrayElementName)
	{
		MainName = mainName;
		ArrayElementName = arrayElementName;
	}
}