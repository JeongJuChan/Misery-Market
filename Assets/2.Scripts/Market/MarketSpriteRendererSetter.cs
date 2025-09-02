using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MarketSpriteRendererSetter : MonoBehaviour
{
    [SerializeField] private SpriteRenderer backgroundRenderer;
    [SerializeField] private SpriteRenderer customerRenderer;

    [SerializeField] private MarketPlace marketPlace;

    void Awake()
    {
        // 임시
        if (marketPlace == MarketPlace.Subway)
        {
            backgroundRenderer.sprite = ResourceManager.Instance.GetSprite($"{Consts.MARKET_MAPPING_KEY}");
        }
    }
}
