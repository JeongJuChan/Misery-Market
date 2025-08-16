using UnityEngine;

[CreateAssetMenu(fileName = "TableData", menuName = "ScriptableObjects/TableData", order = 1)]
public class TableData : ScriptableObject
{
    public bool isResourceFolderUsed = false;
    public string tableName;    // 테이블 이름 (예: "Sheet1")
    public string gameDataName; // GameData의 이름
    public string range;        // 데이터를 가져올 범위 (예: "A1:D100")
}
