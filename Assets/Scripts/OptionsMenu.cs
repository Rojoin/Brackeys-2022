using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class OptionsMenu : MonoBehaviour
{

    [SerializeField] private AudioMixer audioMixer;

    private void Awake()
    {
       // audioMixer = GameObject.FindGameObjectWithTag("AudioManager").GetComponent<AudioMixer>();
    }
    public void ToggleFullScreen(bool fullScreen) 
    {
        Screen.fullScreen = fullScreen;
    }

    public void changeVolum(float volum) 
    {
        audioMixer.SetFloat("Volumen",volum);
    }
}