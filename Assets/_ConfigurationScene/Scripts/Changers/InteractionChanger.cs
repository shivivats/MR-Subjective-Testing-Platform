using Microsoft.MixedReality.Toolkit.UI;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime;
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

	public void UpdateInteractableStateOfNewSelectedObject(bool enabled)
	{
		interactable = enabled;
	}

	public void OnInteractableButtonPressed()
    {
        interactable = !interactable;
		ConfigurationSceneManager.Instance.ChangePCInteractable(interactable);
    }

    // Start is called before the first frame update
    void Start()
    {
        interactable = true;
    }
}
