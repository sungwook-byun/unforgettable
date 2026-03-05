using System;
using System.Text.RegularExpressions;
using UnityEngine;

public enum Languages
{
    Korean,
    English,
    German,
    Turkish
}

public class LocalizationManager : Singleton<LocalizationManager>
{
    private Languages _currentLanguage = Languages.Korean;
    
    public static event Action OnLanguageChanged;

    protected override void Awake()
    {
        base.Awake();
        LoadLanguage();
    }
    
    /// <summary> 스트링 키를 통해 문자열 가져오기 </summary>
    public string GetString(string key)
    {
        if (key.Equals(string.Empty))
        {
            return string.Empty;
        }
        
        return TableManager.Instance.GetString(key, _currentLanguage);
    }
    
    /// <summary> 스트링 키와 매개변수를 통해 문자열 가져오기 </summary>
    public string GetString(string key, params object[] variables)
    {
        if (key.Equals(string.Empty))
        {
            Debug.LogError("스트링 키 값을 입력해주세요");
            return string.Empty;
        }
        
        string value = TableManager.Instance.GetString(key, _currentLanguage);
        string pattern = @"\$\{([^}]+)\}";
        
        var matches = Regex.Matches(value, pattern);
        if (matches.Count != variables.Length)
        {
            Debug.LogError("스트링 키 또는 매개변수 갯수 확인이 필요합니다.");
            return string.Empty;
        }
        
        for (int i = 0; i < matches.Count; i++)
        {
            value = value.Replace(matches[i].Value, variables[i].ToString());
        }
            
        return value;
    }

    /// <summary> 현재 언어 설정 가져오기 </summary>
    public Languages GetLanguage()
    {
        return _currentLanguage;
    }
    
    /// <summary> 언어 설정 변경 시 사용 </summary>
    public void SetLanguage(Languages languages)
    {
        if (_currentLanguage == languages)
            return;
        
        _currentLanguage = languages;
        SaveLanguage();
        
        OnLanguageChanged?.Invoke();
    }

    /// <summary> 언어 설정을 로컬에 저장 </summary>
    private void SaveLanguage()
    {
        PlayerPrefs.SetInt("Language", (int)_currentLanguage);
    }

    /// <summary> 언어 설정을 로컬에서 불러오기 </summary>
    private void LoadLanguage()
    {
        int index = PlayerPrefs.GetInt("Language", (int)_currentLanguage);
        _currentLanguage = (Languages)index; 
    }
}
