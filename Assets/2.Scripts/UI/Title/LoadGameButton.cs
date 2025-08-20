using UnityEngine;
using UnityEngine.UI;

public class LoadGameButton : MonoBehaviour
{
    [SerializeField] private Button loadGameButton;

    private void Awake()
    {
        loadGameButton.onClick.AddListener(OnLoadGameButtonClicked);
    }

    private void OnLoadGameButtonClicked()
    {
        
    }

}
