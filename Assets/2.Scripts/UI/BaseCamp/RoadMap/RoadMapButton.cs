using UnityEngine;
using UnityEngine.UI;

public class RoadMapButton : MonoBehaviour
{
    [SerializeField] private Button loadMapButton;

    private void OnButtonClicked()
    {
        UIManager.Instance.Show(UIId.RoadMapPanel);
    }
}
