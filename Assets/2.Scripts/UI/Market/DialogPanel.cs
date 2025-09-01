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

    void Awake()
    {
        // 임시 -> ResourceManager로 이전 (시트에서 불러오기)
        chatPanel = Resources.Load<ChatPanel>("Images/UI/Chat/ChatPanel");
        for (int i = 0; i < 5; i++)
        {
            ShowChat(CharacterType.Bernard, "\"…좋아. 이건 내 위장을 채우진 못해도, 최소한 빈 손으로 돌아가는 꼴은 면하게 해주겠지. \n받아간다, 장사꾼. 오늘은 네가 날 이겼어.\"");
        }
    }

    public void ShowChat(CharacterType characterType, string message)
    {
        ChatPanel newChat = Instantiate(chatPanel, chatContent);
        newChat.SetChatText($"<b>{characterType}</b>- {message}", chatHeightPerLine);
    }
}
