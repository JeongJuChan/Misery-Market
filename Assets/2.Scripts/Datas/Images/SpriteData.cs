using System;
using UnityEngine;

[Serializable]
public struct SpriteData
{
    [field: SerializeField] public string key;
    [field: SerializeField] public string spriteName;
    [field: SerializeField] public string Path;
}
