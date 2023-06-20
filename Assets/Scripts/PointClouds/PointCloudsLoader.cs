using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.XR;

public struct PCAnimationParams
{
    public PCObjectType type;
    public int quality;
    public bool pointCloud;

    public PCAnimationParams(PCObjectType type, int quality, bool pointCloud)
    {
        this.type = type;
        this.quality = quality;
        this.pointCloud = pointCloud;
    }
}

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

    public bool loadMeshes = true;

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

    public void LoadPointCloudsAndMeshes()
    {
        foreach(PointCloudObject obj in pcObjects) 
        {
            obj.LoadAssetsFromDisk();
        }
    }

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

    public string GetBasePCPathFromType(PCObjectType type)
    {
        return pcPathPrefix + GetPCNameFromType(type) + "/";
    }

    private void OnValidate()
    {
        // OnValidate is called whenever a value is updated in the inspector
        // Hence here we can make sure that the number of qualities in the PointCloudObjects is the same as the numQualities int here
    }

}