using Microsoft.MixedReality.Toolkit.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using System.Threading.Tasks;

public class PresentationThings : MonoBehaviour
{
    public GameObject progressIndicatorObject;
    private IProgressIndicator stallIndicator;
    float progress;

    // Start is called before the first frame update
    void Start()
    {
        stallIndicator = progressIndicatorObject.GetComponent<IProgressIndicator>();
        ToggleIndicator(stallIndicator);
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
        // pause updates for stallDuration number of frames
        progress = 1 - Time.deltaTime;
    }
}
