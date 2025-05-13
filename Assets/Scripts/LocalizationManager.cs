using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LocalizationManager : MonoBehaviour
{
    public static LocalizationManager Instance;
    public TMP_Dropdown languageDropdown;

    private Dictionary<string, string> localizedText;

    [System.Serializable]
    public class LanguageFile
    {
        public string languageCode;
        public TextAsset jsonFile;
    }

    public List<LanguageFile> languageFiles;

    void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); }
    }

    void Start()
    {
        LoadLocalizedText(languageFiles[0].languageCode);


        if (languageDropdown != null)
        {
            languageDropdown.ClearOptions();
            List<string> options = new List<string>();
            foreach (var lang in languageFiles) options.Add(lang.languageCode);
            languageDropdown.AddOptions(options);
            languageDropdown.onValueChanged.AddListener(delegate { OnLanguageSelected(languageDropdown.value); });
        }
    }

    private void OnLanguageSelected(int index)
    {
        LoadLocalizedText(languageFiles[index].languageCode);
        foreach (var mb in FindObjectsOfType<MonoBehaviour>(true))
        {
            if (mb is ILocalizable loc) loc.UpdateText();
        }

    }

    public void LoadLocalizedText(string languageCode)
    {
        TextAsset langAsset = languageFiles.Find(l => l.languageCode == languageCode)?.jsonFile;
        if (langAsset != null)
        {
            localizedText = JsonUtility.FromJson<LocalizationData>(langAsset.text).ToDictionary();
            Debug.Log("LocalizationManager: Loaded " + languageCode);
        }
        else
        {
            Debug.Log("LocalizationManager: Language not found: " + languageCode);
        }
    }

    public string GetText(string key)
    {
        if (localizedText == null) return $"[null:{key}]";
        if (localizedText.ContainsKey(key)) return localizedText[key];
        return $"[missing:{key}]";
    }

}

public interface ILocalizable
{
    void UpdateText();
}

[System.Serializable]
public class LocalizationData
{
    public LocalizedItem[] items;
    public Dictionary<string, string> ToDictionary()
    {
        var dict = new Dictionary<string, string>();
        foreach (var item in items) dict[item.key] = item.value;
        return dict;
    }
}

[System.Serializable]
public class LocalizedItem
{
    public string key;
    public string value;
}
