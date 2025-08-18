using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TableDataCollection", menuName = "ScriptableObjects/TableDataCollection", order = 2)]
public class TableDataCollection : ScriptableObject
{
    public List<TableData> tables; // 여러 테이블의 메타데이터를 관리하는 리스트
}