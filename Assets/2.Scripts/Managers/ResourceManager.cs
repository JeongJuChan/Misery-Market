using System.Collections.Generic;
using UnityEngine;

public class ResourceManager : MonoBehaviour
{
    public static ResourceManager Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject go = new GameObject("@ResourceManager");
                ResourceManager resourceManager = go.AddComponent<ResourceManager>();
                instance = resourceManager;
            }

            return instance;
        }
    }
    private static ResourceManager instance;

    [Header("Game Datas")]
    [SerializeField] private GameData localizationSpriteData;
    [SerializeField] private GameData[] spriteDatas;
    [SerializeField] private GameData soundGroupData;

    private Dictionary<string, Sprite[]> localizationSpriteDict = new Dictionary<string, Sprite[]>();
    private Dictionary<string, Sprite> spriteDict = new Dictionary<string, Sprite>();
    private Dictionary<SFXName, MixerGroup> sfxMixerGroupDict = new Dictionary<SFXName, MixerGroup>();

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        LocalizationManager.Instance.InitLocalization();
        InitResourceDict();
    }

    private void InitResourceDict()
    {
        Language[] languages = LocalizationManager.Instance.Languages;
        InitSpriteDict();
    }

    public Sprite GetSprite(string key)
    {
        if (spriteDict.TryGetValue(key, out Sprite sprite))
        {
            return sprite;
        }

        Debug.LogWarning($"Sprite not found: {key}");
        return null;
    }

    private void InitSpriteDict()
    {
        if (spriteDatas == null || spriteDatas.Length == 0)
        {
            spriteDatas = Resources.LoadAll<GameData>("ScriptableObjects/GameData/Images/Market");
        }

        foreach (var spriteData in spriteDatas)
        {
            var rows = spriteData.GetDataRows();

            for (int i = 0; i < rows.Count; i++)
            {
                List<string> datas = rows[i].rowData;
                string spriteKey = datas[0];
                Sprite sprite = Resources.Load<Sprite>(datas[2]);
                spriteDict[spriteKey] = sprite;
            }
        }
    }

    private void InitLocalizationSpriteDict(Language[] languages)
    {
        if (localizationSpriteData == null)
        {
            localizationSpriteData = Resources.Load<GameData>("ScriptableObjects/GameData/LocalizationSprite");
        }

        var rows = localizationSpriteData.GetDataRows();

        for (int i = 0; i < rows.Count; i++)
        {
            List<string> datas = rows[i].rowData;

            string localizationKey = datas[0];

            if (!localizationSpriteDict.ContainsKey(localizationKey))
            {
                localizationSpriteDict.Add(localizationKey, new Sprite[languages.Length]);
            }

            for (int j = 0; j < languages.Length; j++)
            {
                localizationSpriteDict[localizationKey][j] = Resources.Load<Sprite>(datas[j + 1]); // 데이터 0번은 컬럼 키
            }
        }
    }

    public Sprite GetLocalizedSprite(string localizationKey)
    {
        return localizationSpriteDict[localizationKey][(int)LocalizationManager.Instance.CurrentLanguage];
    }

    public MixerGroup GetMixerGroupBySFXName(SFXName sfxName)
    {
        if (soundGroupData == null || sfxMixerGroupDict.Count == 0)
        {
            InitMixerGroupDict();
        }

        return sfxMixerGroupDict[sfxName];
    }

    private void InitMixerGroupDict()
    {
        if (soundGroupData == null)
        {
            soundGroupData = Resources.Load<GameData>("ScriptableObjects/GameData/SoundGroupData");
        }

        var rows = soundGroupData.GetDataRows();
        for (int i = 0; i < rows.Count; i++)
        {
            List<string> datas = rows[i].rowData;
            SFXName sfxNameKey = (SFXName)System.Enum.Parse(typeof(SFXName), datas[0]);
            MixerGroup mixerGroup = (MixerGroup)System.Enum.Parse(typeof(MixerGroup), datas[1]);
            if (!sfxMixerGroupDict.ContainsKey(sfxNameKey))
            {
                sfxMixerGroupDict.Add(sfxNameKey, mixerGroup);
            }
        }
    }
}
