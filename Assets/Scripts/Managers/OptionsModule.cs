using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class OptionsModule : MonoBehaviour {
    public AudioMixer mixer;
    public Slider masterVolSlider, bgmVolSlider, sfxVolSlider;
    public GameObject androidAudioText;

    void Start()
    {
        bgmVolSlider.value = PlayerPrefs.GetFloat("bgmVol", 0.8f);
        sfxVolSlider.value = PlayerPrefs.GetFloat("sfxVol", 0.8f);
        mixer.SetFloat("bgmVol", LinearToDb(bgmVolSlider.value));
        mixer.SetFloat("sfxVol", LinearToDb(sfxVolSlider.value));

        if (Application.platform != RuntimePlatform.WindowsPlayer && Application.platform != RuntimePlatform.WindowsEditor)
        {
            unityPlayerClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            currentActivity = unityPlayerClass.GetStatic<AndroidJavaObject>("currentActivity");
            context = currentActivity.Call<AndroidJavaObject>("getApplicationContext");
            audioManagerClass = new AndroidJavaClass("android.media.AudioManager");
            contextClass = new AndroidJavaClass("android.content.Context");
            AudioManager_STREAM_MUSIC = audioManagerClass.GetStatic<int>("STREAM_MUSIC");
            Context_AUDIO_SERVICE = contextClass.GetStatic<string>("AUDIO_SERVICE");
            audioService = context.Call<AndroidJavaObject>("getSystemService", Context_AUDIO_SERVICE);

            masterVolSlider.interactable = false;
            androidAudioText.SetActive(true);
            mixer.SetFloat("masterVol", 0);
        }
        else
        {
            masterVolSlider.interactable = true;
            androidAudioText.SetActive(false);
            masterVolSlider.value = PlayerPrefs.GetFloat("masterVol", 1f);
            mixer.SetFloat("masterVol", LinearToDb(masterVolSlider.value));
        }
    }
    float t = 0;
    // Update is called once per frame
    void Update()
    {
        if (Application.platform != RuntimePlatform.WindowsPlayer && Application.platform != RuntimePlatform.WindowsEditor)
        {
            t += Time.deltaTime;
            if (t > 0.5f)
            {
                t = 0;
                GetAndroidVolume();
            }
        }
    }

    public void OnMasterChange(float value)
    {
        PlayerPrefs.SetFloat("masterVol", value);
        mixer.SetFloat("masterVol", LinearToDb(value));
    }

    public void OnBGMChange(float value)
    {
        PlayerPrefs.SetFloat("bgmVol", value);
        mixer.SetFloat("bgmVol", LinearToDb(value));
    }

    public void OnSFXChange(float value)
    {
        PlayerPrefs.SetFloat("sfxVol", value);
        mixer.SetFloat("sfxVol", LinearToDb(value));
    }

    public float LinearToDb(float linear)
    {
        if (linear == 0) return -80f;
        return Mathf.Log10(linear) * 20;
    }
    AndroidJavaClass unityPlayerClass;
    AndroidJavaObject currentActivity;
    AndroidJavaObject context;
    AndroidJavaClass audioManagerClass;
    AndroidJavaClass contextClass;
    int AudioManager_STREAM_MUSIC;
    string Context_AUDIO_SERVICE;
    AndroidJavaObject audioService;

    public void GetAndroidVolume()
    {
        int vol = audioService.Call<int>("getStreamVolume", AudioManager_STREAM_MUSIC);
        int maxVol = audioService.Call<int>("getStreamMaxVolume", AudioManager_STREAM_MUSIC);

        masterVolSlider.onValueChanged = null;
        masterVolSlider.maxValue = maxVol;
        masterVolSlider.value = vol;
    }
}
