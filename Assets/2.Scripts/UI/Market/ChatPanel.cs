using System;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChatPanel : UIBase
{
    private RectTransform rect;
    [SerializeField] private TextMeshProUGUI chatText;

    void Awake()
    {
        rect = GetComponent<RectTransform>();
    }

    /// <summary>
    /// 채팅 텍스트를 설정하고, 총 높이를 반환합니다.
    /// </summary>
    /// <param name="text"></param>
    /// <param name="heightPerLine"></param>
    /// <returns></returns>
    public async UniTask<float> SetChatTextAndGetHeightAsync(string text, float heightPerLine)
    {
        chatText.text = text;

        float totalHeight = await GetTotalHeightAsync(heightPerLine);
        rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, totalHeight);
        return totalHeight;
    }

    /// <summary>
    /// 주어진 높이당 라인 수에 따라 총 높이를 계산합니다.
    /// </summary>
    /// <param name="heightPerLine"></param>
    /// <returns></returns>
    public async UniTask<float> GetTotalHeightAsync(float heightPerLine)
    {
        int lineCount = await TMPUtils.GetLineCount(chatText);
        float totalHeight = lineCount * heightPerLine;
        return totalHeight;
    }
}
