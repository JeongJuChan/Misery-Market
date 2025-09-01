using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class ButtonBase : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private MonoBehaviour actionProvider;

    private IButtonAction buttonAction;
    private IButtonActionAsync buttonActionAsync;
    private bool isRunning;
    private CancellationToken cancellationToken;

    private void Awake()
    {
        buttonAction = actionProvider as IButtonAction;
        buttonActionAsync = actionProvider as IButtonActionAsync;
        cancellationToken = this.GetCancellationTokenOnDestroy();
        button.onClick.AddListener(() => TryExecute().Forget());
    }

    private async UniTaskVoid TryExecute()
    {
        if (isRunning)
        {
            return;
        }

        isRunning = true;
        button.interactable = false;

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        try
        {
            if (buttonActionAsync != null)
            {
                await buttonActionAsync.OnButtonClickedAsync(cts.Token);
            }
            else if (buttonAction != null)
            {
                buttonAction.OnButtonClicked();
            }
            else
            {
                Debug.LogWarning("No action defined for the button.");
            }
        }
        catch (OperationCanceledException e)
        {
            Debug.Log($"Operation was canceled: {e.Message}");
        }
        catch (Exception e)
        {
            Debug.LogError($"An error occurred while executing button action: {e.Message}");
        }
        finally
        {
            isRunning = false;
            if (this != null)
            {
                button.interactable = true;
            }
        }
    }
}
