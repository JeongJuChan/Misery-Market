using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unity.VisualScripting;
using UnityEngine;

public class LocalizationManager : MonoBehaviour
{
    public static LocalizationManager Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject go = new GameObject("@LocalizationManager");
                LocalizationManager localizationManager = go.AddComponent<LocalizationManager>();
                instance = localizationManager;
            }

            return instance;
        }
    }
    private static LocalizationManager instance;

    //private Dictionary<>

    [field: SerializeField] public Language CurrentLanguage { get; private set; } = Language.Ko;

    [SerializeField] private GameData localizationTextData;

    public event Action<Language> OnLanguageChanged;

    public event Action OnUpdateTextsByLanguageChanged;

    private Dictionary<string, string[]> localizationTextDict = new Dictionary<string, string[]>();

    [field: SerializeField] public Language[] Languages { get; private set; }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            InitLocalization();
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void InitLocalization()
    {
        if (Languages != null && Languages.Length > 0)
        {
            return;
        }

        Languages = (Language[])Enum.GetValues(typeof(Language));

        if(PlayerPrefs.HasKey("SavedLanguage"))
        {
            CurrentLanguage = (Language)PlayerPrefs.GetInt("SavedLanguage");
        }

        InitLocalizationTextDict(Languages);
    }

    private void InitLocalizationTextDict(Language[] languages)
    {
        if (localizationTextData == null)
        {
            localizationTextData = Resources.Load<GameData>("ScriptableObjects/GameData/LocalizationText");
        }

        var rows = localizationTextData.GetDataRows();

        for (int i = 0; i < rows.Count; i++)
        {
            List<string> datas = rows[i].rowData;

            string localizationKey = datas[0];

            if (!localizationTextDict.ContainsKey(localizationKey))
            {
                localizationTextDict.Add(localizationKey, new string[languages.Length]);
            }

            for (int j = 0; j < languages.Length; j++)
            {
                localizationTextDict[localizationKey][j] = datas[j + 1]; // 데이터 0번은 컬럼 키
            }
        }
    }

    public void UpdateLanguage(Language language)
    {
        if (CurrentLanguage == language)
        {
            return;
        }

        CurrentLanguage = language;

        PlayerPrefs.SetInt("SavedLanguage", (int)language);
        PlayerPrefs.Save();

        // FireBaseManager.Instance.SendLanguageToJsBridge(language);

        OnLanguageChanged?.Invoke(language);
        OnUpdateTextsByLanguageChanged?.Invoke();
    }

    public string GetLocalizedText(string localizationKey)
    {
        if (Languages == null || Languages.Length == 0)
        {
            InitLocalization();
        }

        if (string.IsNullOrEmpty(localizationKey))
        {
            Debug.LogWarning("[LocalizationManager] Empty localization key provided");
            return "[MISSING_KEY]";
        }

        if (!localizationTextDict.TryGetValue(localizationKey, out string[] texts))
        {
            Debug.LogWarning($"[LocalizationManager] Missing localization key: {localizationKey}");
            return $"[{localizationKey}]"; // 키를 그대로 표시
        }

        int languageIndex = (int)CurrentLanguage;
        if (languageIndex < 0 || languageIndex >= texts.Length)
        {
            Debug.LogWarning($"[LocalizationManager] Invalid language index: {languageIndex} for key: {localizationKey}");
            return texts[0]; // 첫 번째 언어로 폴백
        }

        return texts[languageIndex];
    }

    public string GetLocalizedText(string localizationKey, int targetCount)
    {
        string abilityInfoText = GetLocalizedText(localizationKey);
        return abilityInfoText.Replace("{n}", $"{targetCount}");
    }

    // public Sprite GetLocalizedSprite(string key)
    // {
    //     if (Languages == null || Languages.Length == 0)
    //     {
    //         InitLocalization();
    //     }

    //     return ResourceManager.Instance.GetLocalizedSprite(key);
    // }

    // rawKey → C# enum 식별자 변환
    private static string ToSafeEnumName(string rawKey)
    {
        if (string.IsNullOrEmpty(rawKey)) return null;
        string s = rawKey/*.ToUpperInvariant()*/
            .Select(c => (char.IsLetterOrDigit(c) || c == '_') ? c : '_')
            .Aggregate("", (acc, c) => acc + c);
        while (s.Contains("__")) s = s.Replace("__", "_");
        s = s.Trim('_');
        if (string.IsNullOrEmpty(s)) return null;
        if (char.IsDigit(s[0])) s = "_" + s;
        return s;
    }
}