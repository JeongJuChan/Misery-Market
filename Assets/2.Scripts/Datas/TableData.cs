using UnityEngine;

[CreateAssetMenu(fileName = "TableData", menuName = "ScriptableObjects/TableData", order = 1)]
public class TableData : ScriptableObject
{
    [Header("리소스 폴더 사용 여부")]
    public bool isResourceFolderUsed = false;
    [Header("추가 경로")]
    public string additionalPath = "";
    [Header("테이블 정보")]
    public string tableName;    // 테이블 이름 (예: "Sheet1")
    public string range;        // 데이터를 가져올 범위 (예: "A1:D100")
    [Header("생성될 게임 데이터 이름")]
    public string gameDataName; // GameData의 이름
}
