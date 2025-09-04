using Cysharp.Threading.Tasks;
using UnityEngine;

public interface IInitializable
{
    UniTask InitializeAsync();
}
