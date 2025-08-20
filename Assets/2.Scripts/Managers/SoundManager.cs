using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class SoundManager: MonoBehaviour
{
    public static SoundManager Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject go = new GameObject("@SoundManager");
                SoundManager soundManager = go.AddComponent<SoundManager>();
                instance = soundManager;
            }

            return instance;
        }
    }
    private static SoundManager instance;

    private AudioMixer audioMixer;
    [SerializeField] private List<AudioMixerGroup> audioMixerGroups = new List<AudioMixerGroup>();

    private AudioSource masterSource;
    private AudioSource[] bgmSources = new AudioSource[2];
    private AudioSource[] sfxSources = new AudioSource[10];

    private Dictionary<SFXName, List<AudioClip>> sfxDict = new Dictionary<SFXName, List<AudioClip>>();

    [Range(0.0001f, 1f)] private List<float> groupVolumes = new List<float>();
    private Dictionary<int, float> clipLastPlayedTime = new Dictionary<int, float>();
    private float duplicateFilterTime = 0.05f; // 같은 클립은 0.05초 내 중복 재생 제한

    private GameObject obj = null;
    private Coroutine changeCor = null;
    private float lerpTime = 1f;
    private int queueCount;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        // InitializeSound(5);
        // LoadSFXClipsFromResources();
    }


    public void InitializeSound(int queueCount)
    {
        MixerGroup[] mixerGroups = (MixerGroup[])Enum.GetValues(typeof(MixerGroup));

        audioMixer = Resources.Load<AudioMixer>("Audio/AudioMixer");

        for (int i = 0; i < mixerGroups.Length; i++)
        {
            audioMixerGroups.Add(audioMixer.FindMatchingGroups(mixerGroups[i].ToString())[0]);
        }

        this.queueCount = queueCount;

        SettingSources();

        groupVolumes.Clear();

        for (int i = 0; i < audioMixerGroups.Count; ++i)
        {
            string key = ((MixerGroup)i).ToString();
            float value = PlayerPrefs.GetFloat(key, 1f);
            groupVolumes.Add(value);
        }

        CoroutineManager.Instance.StartCoroutine(DelayedSettingVolumes());
    }

    private void LoadSFXClipsFromResources()
    {
        sfxDict.Clear();

        string rootPath = "Music/Dev_SFX";

        AudioClip[] audioClips = Resources.LoadAll<AudioClip>(rootPath);

        foreach (var audioClip in audioClips)
        {
            string clipName = audioClip.name;

            string[] strArr = clipName.Split('_');

            string lastStr = strArr[strArr.Length - 1];

            SFXName sfxName = SFXName.None;

            if (int.TryParse(lastStr, out int result))
            {
                if (result < 10)
                {
                    int lastIndex = clipName.LastIndexOf('_');
                    string newName = clipName.Substring(0, lastIndex);

                    sfxName = (SFXName)Enum.Parse(typeof(SFXName), newName);
                }
            }
            else
            {
                sfxName = (SFXName)Enum.Parse(typeof(SFXName), clipName);
            }

            if (!sfxDict.ContainsKey(sfxName))
            {
                sfxDict.Add(sfxName, new List<AudioClip>());
            }

            sfxDict[sfxName].Add(audioClip);
        }
    }

    private IEnumerator DelayedSettingVolumes()
    {
        yield return null;
        SettingVolumes();
    }


    private void SettingSources()
    {
        if (obj == null)
            obj = gameObject;
        if (masterSource == null)
        {
            masterSource = obj.AddComponent<AudioSource>();
            masterSource.loop = false;
            masterSource.playOnAwake = false;
            masterSource.outputAudioMixerGroup = audioMixerGroups[(int)MixerGroup.Master];
        }

        for (int i = 0; i < bgmSources.Length; ++i)
        {
            if (bgmSources[i] == null)
            {
                bgmSources[i] = obj.AddComponent<AudioSource>();
                bgmSources[i].loop = true;
                bgmSources[i].playOnAwake = false;
                bgmSources[i].outputAudioMixerGroup = audioMixerGroups[(int)MixerGroup.BGM];
            }
        }

        for (int i = 0; i < sfxSources.Length; ++i)
        {
            if (sfxSources[i] == null)
            {
                sfxSources[i] = obj.AddComponent<AudioSource>();
                sfxSources[i].loop = false;
                sfxSources[i].playOnAwake = false;
                // sfxSources[i].outputAudioMixerGroup = audioMixerGroups[(int)MixerGroup.SFX_Common];
            }
        }
    }

    public void PlayBGM(AudioClip bgmClip)
    {
        if (null != changeCor)
        {
            CoroutineManager.Instance.StopCoroutine(changeCor);
            changeCor = null;
        }

        if (!bgmSources[0].isPlaying)
        {
            bgmSources[0].clip = bgmClip;
            changeCor = CoroutineManager.Instance.StartCoroutine(ChangeBGMClip(bgmSources[0], bgmSources[1]));
        }
        else
        {
            bgmSources[1].clip = bgmClip;
            changeCor = CoroutineManager.Instance.StartCoroutine(ChangeBGMClip(bgmSources[1], bgmSources[0]));
        }
    }

    public void PlayBGMOnce(AudioClip bgmClip)
    {
        if (bgmClip == null) return;

        foreach (var source in bgmSources)
        {
            if (source.isPlaying && source.clip == bgmClip)
                return; // 이미 재생 중이면 무시
        }

        PlayBGM(bgmClip);
    }


    IEnumerator ChangeBGMClip(AudioSource target, AudioSource turnOff)
    {
        float current = 0f;

        target.Play();

        while (current < lerpTime)
        {
            current += Time.deltaTime;

            target.volume = Mathf.Lerp(0, 1, (current / lerpTime));
            turnOff.volume = Mathf.Lerp(1, 0, (current / lerpTime));

            yield return null;
        }

        target.volume = 1f;
        turnOff.Stop();
        turnOff.clip = null;

        changeCor = null;
    }

    public void StopBGM()
    {
        foreach (var source in bgmSources)
            source.Stop();
    }

    public void PlaySFX(SFXName sfxName)
    {
        if (sfxName == SFXName.None) return;

        if (sfxDict.TryGetValue(sfxName, out var clip))
        {
            AudioMixerGroup audioMixerGroup = audioMixerGroups[(int)ResourceManager.Instance.GetMixerGroupBySFXName(sfxName)];
            PlaySFX(clip, audioMixerGroup); // 기존 AudioClip용 함수 재사용
        }
        else
        {
            Debug.LogWarning($"[SoundManager] {sfxName} 사운드를 찾을 수 없습니다.");
        }
    }


    public void PlaySFX(List<AudioClip> clips, AudioMixerGroup audioMixerGroup)
    {
        if (clips == null || clips.Count == 0) return;

        AudioClip audioClip = clips.Count == 1 ? clips[0] : clips[UnityEngine.Random.Range(0, clips.Count)];

        // Debug.Log(audioClip);

        if (!CanPlayClip(audioClip)) return;

        for (int i = 0; i < sfxSources.Length; i++)
        {
            if (!sfxSources[i].isPlaying)
            {
                sfxSources[i].clip = audioClip;
                sfxSources[i].outputAudioMixerGroup = audioMixerGroup;
                sfxSources[i].Play();
                return;
            }
        }

        int index = Time.frameCount % sfxSources.Length;
        sfxSources[index].Stop();
        sfxSources[index].outputAudioMixerGroup = audioMixerGroup;
        sfxSources[index].clip = audioClip;
        sfxSources[index].Play();
    }

    public void SetMasterVolume(float volume)
    {
        groupVolumes[(int)MixerGroup.Master] = volume;
        SettingVolumes();
        PlayerPrefs.SetFloat((MixerGroup.Master).ToString(), groupVolumes[(int)MixerGroup.Master]);
        PlayerPrefs.Save();
    }


    public void SetBGMVolume(float volume)
    {
        groupVolumes[(int)MixerGroup.BGM] = volume;
        SettingVolumes();
        PlayerPrefs.SetFloat((MixerGroup.BGM).ToString(), groupVolumes[(int)MixerGroup.BGM]);
        PlayerPrefs.Save();
    }

    public void SetSFXVolume(float volume)
    {
        // groupVolumes[(int)MixerGroup.SFX_Parent] = volume;
        SettingVolumes();
        // PlayerPrefs.SetFloat((MixerGroup.SFX_Parent).ToString(), groupVolumes[(int)MixerGroup.SFX_Parent]);
        PlayerPrefs.Save();
    }

    public float GetMasterVolume()
    {
        return groupVolumes[(int)MixerGroup.Master];
    }

    public float GetBGMVolume()
    {
        return groupVolumes[(int)MixerGroup.BGM];
    }

    // public float GetSFXVolume()
    // {
    //     return groupVolumes[(int)MixerGroup.SFX_Parent];
    // }

    public void SettingVolumes()
    {
        for (int i = 0; i < 3; ++i)
        {
            string param = ((MixerGroup)i).ToString();

            float linearVolume = groupVolumes[i];
            float dbVolume = Mathf.Log10(Mathf.Max(linearVolume, 0.0001f)) * 20f;

            bool success = audioMixer.SetFloat(param, dbVolume);
        }
    }

    // 중복 재생 방지
    private bool CanPlayClip(AudioClip clip)
    {
        if (clip == null) return false;

        int id = clip.GetInstanceID();
        float now = Time.unscaledTime;

        if (clipLastPlayedTime.TryGetValue(id, out float lastTime))
        {
            if (now - lastTime < duplicateFilterTime)
                return false;
        }

        clipLastPlayedTime[id] = now;
        return true;
    }
}