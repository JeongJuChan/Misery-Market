
using System;
using UnityEngine;

[Serializable]
public struct MarketSpriteData
{
    [field: SerializeField] public int BackgroundKey { get; private set; }
    [field: SerializeField] public int[] CharacterKeys { get; private set; }
}
