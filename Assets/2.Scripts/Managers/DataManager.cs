using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class DataManager : IInitializable
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

    public async UniTask InitializeAsync()
    {
        var request = Resources.LoadAsync("ScriptableObjects/GameData/MarketData/MarketImageKeyData");
        await request;
        marketData = request.asset as GameData;

        var rows = marketData.GetDataRows();

        for (int i = 0; i < rows.Count; i++)
        {
            List<string> datas = rows[i].rowData;

            MarketPlace marketPlaceKey = (MarketPlace)Enum.Parse(typeof(MarketPlace), datas[0]);
            int backgroundKey = int.Parse(datas[1]);
            string[] iconKeys = datas[2].Split();
            int[] iconKeyArr = new int[iconKeys.Length];

            for (int j = 0; j < iconKeys.Length; j++)
            {
                iconKeyArr[j] = int.Parse(iconKeys[j]);
            }

            if (!marketSpriteDict.ContainsKey(marketPlaceKey))
            {
                marketSpriteDict.Add(marketPlaceKey, new MarketSpriteData(backgroundKey, iconKeyArr));
            }
        }
    }

    public MarketSpriteData GetMarketSpriteData(MarketPlace marketPlace)
    {
        return marketSpriteDict[marketPlace];
    }
}
