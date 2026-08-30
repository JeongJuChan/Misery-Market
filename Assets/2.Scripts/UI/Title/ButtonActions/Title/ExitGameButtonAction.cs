using UnityEngine;

public class ExitGameButtonAction : MonoBehaviour, IButtonAction
{
    public void OnButtonClicked()
    {
        Application.Quit();
    }
}
