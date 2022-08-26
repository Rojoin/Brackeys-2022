using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class OptionsMenu : MonoBehaviour
{

    [SerializeField] private AudioMixer audioMixer;
    public void ToggleFullScreen(bool fullScreen) 
    {
        Screen.fullScreen = fullScreen;
    }

    public void changeVolum(float volum) 
    {
        audioMixer.SetFloat("volum",volum);
    }
}