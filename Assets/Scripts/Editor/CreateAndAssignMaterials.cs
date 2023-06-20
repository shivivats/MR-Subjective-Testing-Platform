using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Security.Policy;

public class CreateAndAssignMaterials : MonoBehaviour
{
    [MenuItem("Temp/CreateMaterialsForTextures")]
    static void CreateMaterials()
    {
        try
        {
            AssetDatabase.StartAssetEditing();
            var textures = Selection.GetFiltered(typeof(Texture), SelectionMode.Assets).Cast<Texture>();
            foreach (var tex in textures)
            {
                string path = AssetDatabase.GetAssetPath(tex);
                path = path.Substring(0, path.LastIndexOf(".")) + ".mat";
                if (AssetDatabase.LoadAssetAtPath(path, typeof(Material)) != null)
                {
                    Debug.LogWarning("Can't create material, it already exists: " + path);
                    continue;
                }
                var mat = new Material(Shader.Find("Standard"));
                //mat.mainTexture = tex;
                mat.SetTexture("_MainTex", tex);
                AssetDatabase.CreateAsset(mat, path);
            }
        }
        finally
        {
            AssetDatabase.StopAssetEditing();
            AssetDatabase.SaveAssets();
        }
    }

    [MenuItem("Temp/RenameAndPlaceMaterials")]
    static void AssignMaterials()
    {
        //change the "on demand remap" material of each mesh prefab
        try
        {
            AssetDatabase.StartAssetEditing();
            var materials = Selection.GetFiltered(typeof(Material), SelectionMode.Assets).Cast<Material>();
            foreach (var currMat in materials)
            {
                string path = AssetDatabase.GetAssetPath(currMat);
                if (path.Contains("default"))
                {
                    Debug.Log("skipping default mat");
                    continue;
                }
                // Assets/Resources/PointClouds/Loot/q1/Meshes/Textures/loot_0001_tex.mat
                // Goal: loot_0001-defaultMat.mat
                path = path.Substring(0, path.LastIndexOf("/")); // -> Assets/Resources/PointClouds/Loot/q1/Meshes/Textures
                path = path.Substring(0, path.LastIndexOf("/")); // -> Assets/Resources/PointClouds/Loot/q1/Meshes
                path += "/Materials/";
                path += AssetDatabase.GetAssetPath(currMat).Substring(AssetDatabase.GetAssetPath(currMat).LastIndexOf("/"), AssetDatabase.GetAssetPath(currMat).LastIndexOf("_") - AssetDatabase.GetAssetPath(currMat).LastIndexOf("/")); // -> loot_0001
                path += "-defaultMat.mat";
                var newMat = new Material(currMat);
                AssetDatabase.CreateAsset(newMat, path);
                Debug.Log("From " + AssetDatabase.GetAssetPath(currMat) + " to " + path);
            }

        }
        finally
        {
            AssetDatabase.StopAssetEditing();
            AssetDatabase.SaveAssets();
        }
    }
}
