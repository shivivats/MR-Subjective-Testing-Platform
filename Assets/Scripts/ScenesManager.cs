using Microsoft.MixedReality.Toolkit.SceneSystem;
using Microsoft.MixedReality.Toolkit;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.Extensions.SceneTransitions;

public class ScenesManager : MonoBehaviour
{
    public static ScenesManager Instance { get; private set; }

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
            DontDestroyOnLoad(this);
        }
    }

    public int currentQuestionnaireUserId=0;

    // use the scene transition service by passing Tasks that are run while the camera is faded out.

    public void LoadScene(string sceneName)
    { 
        // content from only sceneName will be loaded additively (adding on to the managers, lighting, etc)
        LoadSceneAsync(sceneName);
    }

    private async void LoadSceneAsync(string sceneName)
    {
        IMixedRealitySceneSystem sceneSystem = MixedRealityToolkit.Instance.GetService<IMixedRealitySceneSystem>();
        ISceneTransitionService transition = MixedRealityToolkit.Instance.GetService<ISceneTransitionService>();

        // Fades out
        // Runs LoadContent sceneName
        // Fades back in

        await transition.DoSceneTransition(
            () => sceneSystem.LoadContent(sceneName, LoadSceneMode.Single)
        );

        // SceneOperationInProgress will be true for the duration of this operation
        // SceneOperationProgress will show 0-1 as it completes

    }
}
