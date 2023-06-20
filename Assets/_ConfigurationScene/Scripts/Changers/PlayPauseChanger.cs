using Microsoft.MixedReality.Toolkit.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayPauseChanger : ChangerBase
{
	public static PlayPauseChanger Instance { get; private set; }

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

    public Texture playIcon;
    public Texture pauseIcon;

    public GameObject playPauseButton;

    private bool paused = false;

    public void OnPlayPauseButtonPressed()
    {
        paused = !paused;
		ConfigurationSceneManager.Instance.ChangePCAnimationPaused(paused);
        UpdateButtonIcon();
    }

    public void UpdatePausedStateOfNewSelectedObject(bool animate)
    {
        paused = animate;
        UpdateButtonIcon();
    }

    private void UpdateButtonIcon()
    {
        Texture currIcon = paused ? playIcon : pauseIcon;
        playPauseButton.GetComponent<ButtonConfigHelper>().SetQuadIcon(currIcon);
    }

    // Start is called before the first frame update
    void Start()
    {
        paused = false;
        UpdateButtonIcon();
    }
}
