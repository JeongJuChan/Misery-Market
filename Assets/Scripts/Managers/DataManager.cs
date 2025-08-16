using System;
using UnityEngine;

public class DataManager : MonoBehaviour
{
    public static DataManager Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject go = new GameObject("@DataManager");
                DataManager dataManager = go.AddComponent<DataManager>();
                instance = dataManager;
            }
            return instance;
        }
    }
    private static DataManager instance;

    private bool isMobile; // 모바일인지 여부
    public string userDocId; // 유저 id
    public string language; // 언어 설정

    [System.Serializable]
    public class DataObject
    {
        public bool isMobile;
        public string userDocId;
        public string language;
    }

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
    }

    public void receive_data_from_web(string jsonData)
    {
        DataObject receivedData = JsonUtility.FromJson<DataObject>(jsonData);
        //Debug.Log("isMobile: " + receivedData.isMobile);
        //Debug.Log("userDocId: " + receivedData.userDocId);
        //Debug.Log("language: " + receivedData.language);


        // 받은 데이터를 클래스 내부 변수에 저장
        isMobile = receivedData.isMobile;
        userDocId = receivedData.userDocId;
        language = receivedData.language;

        LocalizationManager.Instance.UpdateLanguage((Language)Enum.Parse(typeof(Language), language));

    }


    public string GetUserId()
    {
        return userDocId == null || userDocId == "" ? "nobody" : userDocId;
    }
}
