using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    [SerializeField] AudioMixer mixer;
    [SerializeField] Slider musicSlider;

    const string MIXER_MASTER = "MasterVolume";

    private void Awake()
    {
        musicSlider.onValueChanged.AddListener(SetMusicVolume);
    }

    void SetMusicVolume(float volume) 
    {
        mixer.SetFloat(MIXER_MASTER, Mathf.Log10(volume) * 20);
    }
}
