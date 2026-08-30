using UnityEngine;
using UnityEngine.UI;

public class StoragePanel : MonoBehaviour
{
    [Header("Pool")]
    [SerializeField] private StorageItem storageSlotPrefab;
    [SerializeField] private GridLayoutGroup contentGrid;
    [SerializeField] private int initCount = 100;
    [SerializeField] private int maxCount = 500;

    private ObjectPooler<StorageItem> pooler;

    private RectTransform rectTransform;
    private RectTransform contentRectTransform;

    private int columnCount;
    private int rowCount;
    private float spacingY;

    private void Awake()
    {
        Init();
    }

    private void Init()
    {
        rectTransform = GetComponent<RectTransform>();
        pooler = new ObjectPooler<StorageItem>(storageSlotPrefab, contentGrid.transform, initCount, maxCount);

        float spacingX = contentGrid.spacing.x;
        spacingY = contentGrid.spacing.y;

        columnCount = (int)((rectTransform.rect.width + spacingX - (contentGrid.padding.left + contentGrid.padding.right)) /
            (contentGrid.cellSize.x + spacingX));
        rowCount = (int)((rectTransform.rect.height + spacingY - (contentGrid.padding.top + contentGrid.padding.bottom)) /
            (contentGrid.cellSize.y + spacingY));

        int totalCount = columnCount * (rowCount + 1);

        UpdateContentSize(rowCount);
        
        for (int i = 0; i < totalCount; i++)
        {
            pooler.Pool(contentGrid.transform);
        }
    }

    private void UpdateContentSize(int rowCount)
    {
        float contentHeight =
            contentGrid.cellSize.y * (rowCount + 1) + (spacingY * rowCount) + contentGrid.padding.top + contentGrid.padding.bottom;

        contentRectTransform = contentGrid.GetComponent<RectTransform>();
        contentRectTransform.sizeDelta = new Vector2(contentRectTransform.sizeDelta.x, contentHeight);
    }
}
