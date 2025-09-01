using System;
using System.Threading.Tasks;
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

    public async Task SetChatText(string text, float heightPerLine)
    {
        chatText.text = text;


        int lineCount = await TMPUtils.GetLineCount(chatText);
        float totalHeight = lineCount * heightPerLine;
        rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, totalHeight);
    }
}
