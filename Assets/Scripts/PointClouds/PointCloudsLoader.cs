using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.XR;

/** TODO: do some OnGUI or OnValidate check and display a warning that there cannot be more than one object per enum value */
/** TODO: Investigate making PCSegments into storable objects and thus doing the pre-processing in the editor before playing. */
/** TODO: Make sure the current frame index in the segment should be managed by the STSequence object */


/** <summary>
 * Class <c>PointCloudLoader</c> is responsible for loading the various <c>PCSegments</c> and also acting as an interface between the rest of the project and the <c>PointCloudObjects</c>.
 * </summary>
 */
public class PointCloudsLoader : MonoBehaviour
{
    /** <summary>
     * A static instance of this class. This instance is used whenever this class is to be referred, and the singleton code ensures that there is only one instance of the class at any time.
     * </summary>
     */
    public static PointCloudsLoader Instance { get; private set; }

    /** <summary>
     * A list of <c>PointCloudObjects</c> that will be filled in by the test director.
     * These PCs will be loaded from the disk and used for previews/tests.
     * </summary>
     */
    [Header("Point Cloud Objects")]
    public List<PointCloudObject> pcObjects;

    //public bool loadMeshes = false;

    // we need to be sure to set this boolean in the inspector for the testing scene!
    public bool isTestingScene = false;

    public string pcPathPrefix = "PointClouds/";

    private void Awake()
    {
        // Singleton code - If there is an instance, and it's not this, then delete this instance

        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    public void LoadNextPointClouds(PCObjectType pcObject, QualityRepresentation qr)
    {
        print(pcObject);
        print(qr);
        GetPCObjectFromType(pcObject).LoadQualityRepresentationFromDisk(qr);
        //StartCoroutine(GetPCObjectFromType(pcObject).LoadAssetsAsync(qr));
    }

    public void UnloadPCQualityRepresentation(PCObjectType pcObject, QualityRepresentation qr)
    {
        GetPCObjectFromType(pcObject).UnloadAssetsFromQualityRepresentation(qr);
        //StartCoroutine(GetPCObjectFromType(pcObject).UnloadAssetsAsync(qr));
    }

    //public void LoadPointCloudsAndMeshes()
    //{
    //    foreach(PointCloudObject obj in pcObjects) 
    //    {
    //        obj.LoadAllAssetsFromDisk();
    //    }
    //}

    /** <summary>
     * Helper function for getting corresponding <c>PointCloudObject</c> from a <c>PCObjectType</c>.
     * </summary>
     * <param name="type">Type of point cloud to get the object from.</param>
     */
    public PointCloudObject GetPCObjectFromType(PCObjectType type)
    {
        foreach (PointCloudObject obj in pcObjects)
        {
            if (obj.objectType == type)
            {
                return obj;
            }
        }
        return null;
    }

    /** <summary>
     * Helper function for getting the name of the corresponding <c>PointCloudObject</c> from a <c>PCObjectType</c>.
     * </summary>
     * <param name="type">Type of point cloud to get the name from.</param>
     */
    public string GetPCNameFromType(PCObjectType type)
    {
        foreach (PointCloudObject obj in pcObjects)
        {
            if (obj.objectType == type)
            {
                return obj.pcName;
            }
        }
        return null;
    }

    public string GetBasePCPathFromTypeAndEncoder(PCObjectType type, EncoderType encoder)
    {
        string pathString = pcPathPrefix; // already has slash

        switch (encoder)
        {
            case EncoderType.GPCC_OCTREE:
                pathString += "GPCC/";
                pathString += GetPCNameFromType(type) + "/";
                pathString += "octree-predlift/";
                break;

            case EncoderType.GPCC_TRISOUP:
                pathString += "GPCC/";
                pathString += GetPCNameFromType(type) + "/";
                pathString += "trisoup-raht/";
                break;

            case EncoderType.VPCC:
                pathString += "VPCC/";
                pathString += GetPCNameFromType(type) + "/";
                break;
            default:
                return null;
        }

        return pathString;
    }

}