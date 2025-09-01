using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Audio;
using Cysharp.Threading.Tasks;

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
    // UniTask crossfade control
    private CancellationTokenSource bgmFadeCts;
    private float lerpTime = 1f; // crossfade duration
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

    DelayedSettingVolumesAsync().Forget();
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

    private async UniTaskVoid DelayedSettingVolumesAsync()
    {
        await UniTask.Yield();
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
        if (bgmClip == null) return;
        PlayBGMAsync(bgmClip).Forget();
    }

    private async UniTask PlayBGMAsync(AudioClip bgmClip)
    {
        bgmFadeCts?.Cancel();
        bgmFadeCts?.Dispose();
        bgmFadeCts = new CancellationTokenSource();
        var ct = bgmFadeCts.Token;

        AudioSource target;
        AudioSource turnOff;
        if (!bgmSources[0].isPlaying)
        {
            target = bgmSources[0];
            turnOff = bgmSources[1];
        }
        else
        {
            target = bgmSources[1];
            turnOff = bgmSources[0];
        }

        target.clip = bgmClip;
        await CrossfadeAsync(target, turnOff, lerpTime, ct);
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


    private async UniTask CrossfadeAsync(AudioSource target, AudioSource turnOff, float duration, CancellationToken ct)
    {
        float time = 0f;
        target.volume = 0f;
        if (!target.isPlaying)
            target.Play();

        float startTurnOffVol = turnOff.isPlaying ? turnOff.volume : 0f;

        while (time < duration)
        {
            if (ct.IsCancellationRequested) return;
            time += Time.unscaledDeltaTime;
            float t = duration <= 0f ? 1f : Mathf.Clamp01(time / duration);
            target.volume = Mathf.Lerp(0f, 1f, t);
            if (turnOff.isPlaying)
                turnOff.volume = Mathf.Lerp(startTurnOffVol, 0f, t);
            await UniTask.Yield(PlayerLoopTiming.Update, ct);
        }

        target.volume = 1f;
        if (turnOff.isPlaying)
        {
            turnOff.Stop();
            turnOff.clip = null;
            turnOff.volume = 1f;
        }
    }

    public void StopBGM()
    {
        bgmFadeCts?.Cancel();
        foreach (var source in bgmSources)
        {
            source.Stop();
            source.clip = null;
            source.volume = 1f;
        }
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

    private void OnDestroy()
    {
        bgmFadeCts?.Cancel();
        bgmFadeCts?.Dispose();
        bgmFadeCts = null;
    }
}