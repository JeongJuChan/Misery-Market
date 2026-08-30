using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public interface IButtonActionAsync
{
    UniTask OnButtonClickedAsync(CancellationToken cancellationToken);
}
