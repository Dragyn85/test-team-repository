using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : ServiceBehaviour
{
    [SerializeField] AudioMixer audioMixer;
    [SerializeField] float volumeChangeSpeed;

    public event Action<float, MixerParameter> onAnyVolumeChanged;

    string musicVolumeParameter = "MusicVolume";
    string sfxVolumeParameter = "SFXVolume";

    Dictionary<string, MixerParameter> audioMixerDict = new Dictionary<string, MixerParameter>();

    private void Start()
    {
        InitializeServiece();
    }

    private void InitializeSoundParameter(string parameter)
    {
        audioMixerDict.Add(parameter, new MixerParameter(audioMixer, parameter));
        float volume = PlayerPrefs.GetFloat(parameter, 80);
        audioMixerDict[parameter].SetVolume(volume);
    }

    private void HandleSFXVolumeChanged(float obj, MixerParameter parameter)
    {
        onAnyVolumeChanged?.Invoke(obj, parameter);
        PlayerPrefs.SetFloat(parameter.Name, obj);
    }

    public void incrMusicVolume()
    {
        if (!audioMixerDict.ContainsKey(musicVolumeParameter)) return;
        ChangeVolume(musicVolumeParameter, volumeChangeSpeed);
    }

    public void decrMusicVolume()
    {
        if (!audioMixerDict.ContainsKey(musicVolumeParameter)) return;
        ChangeVolume(musicVolumeParameter, -volumeChangeSpeed);
    }
    public void incrSFXVolume()
    {
        if (!audioMixerDict.ContainsKey(sfxVolumeParameter)) return;

        ChangeVolume(sfxVolumeParameter, volumeChangeSpeed);
    }

    public void decrSFXVolume()
    {
        if (!audioMixerDict.ContainsKey(sfxVolumeParameter)) return;

        ChangeVolume(sfxVolumeParameter, -volumeChangeSpeed);
    }

    internal float GetVolume(string soundParameterName)
    {
        return audioMixerDict[soundParameterName].GetVolume();
    }

    internal float GetSoundNormalizedParameterValue(string soundParameterName)
    {
        float volume = GetVolume(soundParameterName);
        return volume / 100;
    }

    /*private float GetMixerValue(float value)
    {
        return Mathf.Log10(value);// * _multipier;
    }*/

    public void ChangeVolume(string soundParameterName, float pctAmount)
    {
        if (!audioMixerDict.ContainsKey(soundParameterName)) return;

        float currentVolume = GetVolume(soundParameterName);
        SetVolume(soundParameterName, currentVolume + pctAmount);
    }

    public void SetVolume(string soundParameterName, float pctValue)
    {
        if (!audioMixerDict.ContainsKey(soundParameterName)) return;

        audioMixerDict[soundParameterName].SetVolume(pctValue);
    }

    internal MixerParameter GetMixerParameter(string parameterName)
    {
        return audioMixerDict[parameterName];
    }

    protected override void Initialize()
    {
        audioMixer.SetFloat(musicVolumeParameter, PlayerPrefs.GetFloat(musicVolumeParameter, 80) - 80);
        audioMixer.SetFloat(sfxVolumeParameter, PlayerPrefs.GetFloat(sfxVolumeParameter, 80) - 80);

        InitializeSoundParameter(musicVolumeParameter);
        InitializeSoundParameter(sfxVolumeParameter);

        MixerParameter.ParameterChanged += HandleSFXVolumeChanged;
    }

    internal float GetVolumeRaw(string parameter)
    {
        audioMixer.GetFloat(parameter, out float volume);
        return volume;
    }
}
public class MixerParameter
{
    AudioMixer audioMixer;
    string parameter;
    float currentValue;

    public string Name => parameter;

    public static event Action<float, MixerParameter> ParameterChanged;
    public MixerParameter(AudioMixer mixer, string parameter)
    {
        audioMixer = mixer;
        this.parameter = parameter;
    }
    public float GetVolume()
    {
        return currentValue;
    }
    public void SetVolume(float perc)
    {
        //audioMixer.GetFloat(parameter, out float currentMixerValue);
        perc = Mathf.Clamp(perc, 0.001f, 100);
        float currentMixerValue = perc - 80;
        currentValue = perc;
        currentMixerValue = MathF.Log10(perc / 100) * 30f;

        audioMixer.SetFloat(parameter, currentMixerValue);
        ParameterChanged?.Invoke(perc, this);
    }
}
