using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;
using System;
using System.Text;

public class EnumManager : MonoBehaviour
{

    // what this does is read the PCs from the disk
    // import the PCs into a list here
    // convert the list into enums by writing a new file PCEnums.cs
    // write the enums to the file 

    // also this is an editor script
    // it doesnt exectute during the play phase but rather in the unity editor

    [MenuItem("PointClouds/Update Point Clouds From Assets")]
    static void UpdatePCsFromAssets()
    {
        List<string> pcList = new List<string>();

        // read files
        string assetPath = "Assets\\Resources\\PointClouds";
        string[] dirs = Directory.GetDirectories(assetPath, "*", SearchOption.TopDirectoryOnly);
        foreach (string dir in dirs) 
        { 
            if (Directory.Exists(dir)) 
            {
                Debug.Log(dir);
                string lastDir = dir.Split("\\").Last();
                pcList.Add(lastDir);
            }
        }

        Debug.Log(pcList);

        // read the PCEnums.cs file
        string enumsFilePath = "Assets\\Scripts\\Enums\\PCEnums.cs";

        List<string> startBoilerplate = new List<string>()
        {
        "using UnityEngine;",
        "[System.Flags]",
        "public enum PCObjectType",
        "{",
        "[InspectorName(\"Deselect All\")] DeselectAll = 0,",
        "[InspectorName(\"Select All\")] SelectAll = ~0,",
        };

        List<string> endBoilerplate = new List<string>()
        {
        "};"
        };


        try
        {
            //StreamReader streamReader = new StreamReader(enumsFilePath);
            StreamWriter streamWriter = new StreamWriter(enumsFilePath, false, Encoding.UTF8);
            foreach(string bp in startBoilerplate) 
            { 
                streamWriter.WriteLine(bp);
            }

            int count = 0;
            foreach(string pc in pcList)
            {
                string line = pc + " = 1 <<" + count.ToString() + ",";
                streamWriter.WriteLine(line);
                count++;
            }

            foreach(string bp in endBoilerplate)
            {
                streamWriter.WriteLine(bp);
            }

            streamWriter.Close();
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
        
    }

}
