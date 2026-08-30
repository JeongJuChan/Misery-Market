using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class MarketSpriteRendererSetter : MonoBehaviour
{
    [SerializeField] private SpriteRenderer backgroundRenderer;
    [SerializeField] private SpriteRenderer customerRenderer;

    [SerializeField] private MarketPlace marketPlace;

    private int[] characterKeys;

    private async UniTaskVoid Start()
    {
        await UniTask.WaitUntil(() => ManagerBootstrapper.Instance != null && ManagerBootstrapper.Instance.IsInitializationComplete());

        MarketSpriteData marketSpriteData = DataManager.Instance.GetMarketSpriteData(marketPlace);
        characterKeys = marketSpriteData.CharacterKeys;
        backgroundRenderer.sprite = ResourceManager.Instance.GetSprite($"{Consts.MARKET_MAPPING_KEY}{marketSpriteData.BackgroundKey}");
        UpdateCustomerSprite(0);
    }

    public void UpdateCustomerSprite(int characterIndex)
    {
        if (characterIndex < 0 || characterIndex >= characterKeys.Length)
        {
            Debug.LogWarning($"Character index {characterIndex} is out of bounds for market place {marketPlace}");
            return;
        }

        int characterKey = characterKeys[characterIndex];
        Sprite newSprite = ResourceManager.Instance.GetSprite($"{Consts.CHARACTER_MAPPING_KEY}{characterKey}");
        if (newSprite != null)
        {
            customerRenderer.sprite = newSprite;
        }
        else
        {
            Debug.LogWarning($"Sprite not found for character key: {characterKey}");
        }
    }
}
