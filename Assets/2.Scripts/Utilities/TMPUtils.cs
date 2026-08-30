using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public static class TMPUtils
{
    public static async UniTask<int> GetLineCount(TextMeshProUGUI text)
    {
        if (text == null || text.text == null || text.text.Length == 0)
            return 0;

        LayoutRebuilder.ForceRebuildLayoutImmediate(text.rectTransform.parent as RectTransform); // 레이아웃 그룹 쓰면 부모 쪽을 갱신
        text.ForceMeshUpdate(true, true);

        await UniTask.WaitForEndOfFrame();

        return text.textInfo.lineCount;
    }
}
