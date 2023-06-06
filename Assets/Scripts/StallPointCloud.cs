using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using TMPro;
using Microsoft.MixedReality.Toolkit.UI;

public class StallPointCloud : MonoBehaviour
{
    //public PointCloudData[] clouds;

    //private PointCloudRenderer pcRenderComp;

    public Mesh[] currentMeshes;

    private MeshFilter meshFilterComp;

    private int meshCounter;

    public bool enableStalling = true;

    public float stallInterval = 60f;
    public float stallDuration = 30f;

    public TextMeshPro currentFPSText;
    public TextMeshPro stallIntervalText;
    public TextMeshPro stalledText;
    public TextMeshPro stallDurationText;

    string fpsTextBase = "Current Frame: ";
    string stallIntervalTextBase = "To Next Stall: ";
    string stalledTextBase = "Stalled: ";
    string stallDurationBase = "Stall Duration: ";

    private float stallTracker;
    private float stallTime;

    //private int previousIndex;

    private int currentIndex;

    bool stalled;

    public Mesh[] CurrentMeshes { get => currentMeshes; set => currentMeshes = value; }

    public GameObject progressIndicatorObject;
    private IProgressIndicator stallIndicator;
    float progress;

    // Start is called before the first frame update
    void Start()
    {
        // pcRenderComp = GetComponent<PointCloudRenderer>();
        // pcRenderComp.sourceData = clouds[0];

        meshFilterComp = GetComponent<MeshFilter>();
        //meshFilterComp.mesh = currentMeshes[0];

        //previousIndex = 0;
        currentIndex = 0;

        //stallInterval = stallInterval;

        stallTracker = stallInterval;
        stallTime = stallDuration;

        stalled = false;

        stallIndicator = progressIndicatorObject.GetComponent<IProgressIndicator>();
    }

    private async void OpenProgressIndicator()
    {
        //await stallIndicator.OpenAsync();

        progress = 0;
        while (progress < 1)
        {
            //progress += Time.deltaTime;
            //indicator.Message = "Loading...";
            stallIndicator.Progress = progress;
            await Task.Yield();
        }

        await stallIndicator.CloseAsync();
    }

    private async void ToggleIndicator(IProgressIndicator indicator)
    {
        await indicator.AwaitTransitionAsync();

        switch (indicator.State)
        {
            case ProgressIndicatorState.Closed:
                await indicator.OpenAsync();
                OpenProgressIndicator();
                break;

            case ProgressIndicatorState.Open:
                await indicator.CloseAsync();
                break;
        }
    }

    void FixedUpdate()
    {
        // FixedUpdate updates according to a timestep set in Project Settings -> Time -> Fixed Timestep
        // I have set it to 0.016666 which corresponds to 60 fps
        if (enableStalling)
        {
            currentFPSText.text = fpsTextBase + currentIndex.ToString();

            stallIntervalText.text = stallIntervalTextBase + stallTracker.ToString();

            if (stallTracker <= 0)
            {
                // pause updates for stallDuration number of frames
                ToggleIndicator(stallIndicator);
                stallTracker = stallInterval;
                stalled = true;
            }

            stalledText.text = stalledTextBase + stalled.ToString();

            if (stalled)
            {
                stallDurationText.alpha = 255f;
                stallTime--;
                stallDurationText.text = stallDurationBase + stallTime.ToString();
                progress = 1 - (stallTime / stallDuration);

                if (stallTime == 0)
                {
                    stalled = false;
                    stallTime = stallDuration;
                    stallTracker = stallInterval;
                    //ToggleIndicator(stallIndicator);
                }
            }
            else
            {
                stallTracker--;
                stallDurationText.alpha = 0f;

                meshFilterComp.mesh = CurrentMeshes[++currentIndex];

                if (currentIndex == CurrentMeshes.Length - 1)
                {
                    currentIndex = -1;
                }
            }
        }
        else
        {
            meshFilterComp.mesh = CurrentMeshes[++currentIndex];
            if (currentIndex == CurrentMeshes.Length - 1)
            {
                currentIndex = -1;
            }
        }
    }
}
