using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class DialogPanel : UIBase
{
    [Header("Chat")]
    [SerializeField] private ScrollRect chatScrollRect;
    [SerializeField] private RectTransform chatContent;
    [SerializeField] private float chatHeightPerLine = 50f;

    [Header("ChatPrefab")]
    private ChatPanel chatPanel;

    async UniTaskVoid Awake()
    {
        // 임시 -> ResourceManager로 이전 (시트에서 불러오기)
        chatPanel = Resources.Load<ChatPanel>("Images/UI/Chat/ChatPanel");
        for (int i = 0; i < 5; i++)
        {
            await ShowChatAsync(CharacterType.Bernard, "\"…좋아. 이건 내 위장을 채우진 못해도, 최소한 빈 손으로 돌아가는 꼴은 면하게 해주겠지. \n받아간다, 장사꾼. 오늘은 네가 날 이겼어.\"");
        }
    }

    /// <summary>
    /// 캐릭터 타입과 메시지를 받아 채팅을 생성하고, 높이를 반환받아 Content 크기를 조정
    /// </summary>
    /// <param name="characterType"></param>
    /// <param name="message"></param>
    /// <returns></returns>
    public async UniTask ShowChatAsync(CharacterType characterType, string message)
    {
        ChatPanel newChat = Instantiate(chatPanel, chatContent);
        float height = await newChat.SetChatTextAndGetHeightAsync($"<b>{characterType}</b>- {message}", chatHeightPerLine);

        UpdateContentSize(height);
    }

    /// <summary>
    /// Content 크기를 업데이트합니다.
    /// </summary>
    /// <param name="height"></param>
    private void UpdateContentSize(float height)
    {
        RectTransform contentRect = chatContent.GetComponent<RectTransform>();
        contentRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, contentRect.sizeDelta.y + height);
    }
}
