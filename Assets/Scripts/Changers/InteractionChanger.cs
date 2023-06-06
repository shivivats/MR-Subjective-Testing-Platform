using Microsoft.MixedReality.Toolkit.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractionChanger : MonoBehaviour
{
    public static InteractionChanger Instance { get; private set; }

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

    private bool interactable = true;

    public void OnInteractableButtonPressed()
    {
        interactable = !interactable;
        PointCloudsManager.Instance.ChangePCInteractable(interactable);
    }

    // Start is called before the first frame update
    void Start()
    {
        interactable = true;
    }
}
