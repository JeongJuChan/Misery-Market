using System;
using System.Collections.Generic;
using UnityEngine;

public class DataManager
{
    public static DataManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new DataManager();
            }
            return instance;
        }
    }
    private static DataManager instance;

    private GameData marketData;
    private Dictionary<MarketPlace, MarketSpriteData> marketSpriteDict = new Dictionary<MarketPlace, MarketSpriteData>();

    private DataManager()
    {
        marketData = Resources.Load<GameData>("ScriptableObjects/GameData/MarketImageKeyData");
    }
    
    
}
