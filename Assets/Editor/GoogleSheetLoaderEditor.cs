using UnityEditor;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using Google.Apis.Sheets.v4;
using System.IO;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Unity.Jobs;

public class GoogleSheetLoaderEditor : EditorWindow
{
    private const string SPREADSHEET_ID = "17ua_9qSl1Pimh-XCrpxJCsC4iFTUxHBrO96OKWxHbI4"; // Google Sheets 문서의 ID
    private SheetsService service;
    private TableDataCollection tableDataCollection;
    private TableData selectedTableData;
    private int selectedIndex = 0;
    private string[] columnNames;
    private string[,] data;
    private Vector2 scrollPosition;

    [MenuItem("Tools/Google Sheets Loader")]
    public static void ShowWindow()
    {
        GetWindow<GoogleSheetLoaderEditor>("Google Sheets Loader");
    }

    private void OnGUI()
    {
        GUILayout.Label("Google Sheets Loader", EditorStyles.boldLabel);

        // TableDataCollection을 에디터에서 연결할 수 있도록 제공
        tableDataCollection = (TableDataCollection)EditorGUILayout.ObjectField("Table Data Collection", tableDataCollection, typeof(TableDataCollection), false);

        if (tableDataCollection != null && tableDataCollection.tables != null && tableDataCollection.tables.Count > 0)
        {
            string[] tableNames = tableDataCollection.tables.Select(t => t.gameDataName).ToArray();

            // 현재 선택된 인덱스가 범위를 벗어나지 않는지 확인
            if (selectedIndex >= tableNames.Length)
            {
                selectedIndex = 0; // 인덱스가 유효하지 않다면 0으로 설정
            }

            selectedIndex = EditorGUILayout.Popup("Select Table", selectedIndex, tableNames);
            selectedTableData = tableDataCollection.tables[selectedIndex];
        }
        else
        {
            GUILayout.Label("No table data available. Please create or load a TableDataCollection.");
        }

        // if (GUILayout.Button("Initialize Google Sheets API"))
        // {
        //     InitializeService();
        // }

        if (GUILayout.Button("Load Data from Google Sheets"))
        {
            if (selectedTableData != null)
            {
                LoadDataFromGoogleSheet(selectedTableData);
            }
            else
            {
                Debug.LogWarning("No table selected.");
            }
        }

        if (GUILayout.Button("Create or Update GameData"))
        {
            if (selectedTableData != null)
            {
                SaveOrUpdateGameData(selectedTableData);
            }
            else
            {
                Debug.LogWarning("No table selected.");
            }
        }

        if (GUILayout.Button("Load All Tables and Save"))
        {
            LoadAllTablesAndSave();
        }

        // 불러온 데이터 표시
        if (columnNames != null && data != null)
        {
            GUILayout.Label("Loaded Data", EditorStyles.boldLabel);
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            EditorGUILayout.BeginHorizontal();
            foreach (var colName in columnNames)
            {
                GUILayout.Label(colName, GUILayout.Width(100));
            }
            EditorGUILayout.EndHorizontal();

            for (int i = 0; i < data.GetLength(0); i++)
            {
                EditorGUILayout.BeginHorizontal();
                for (int j = 0; j < data.GetLength(1); j++)
                {
                    GUILayout.Label(data[i, j], GUILayout.Width(100));
                }
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndScrollView();
        }
    }

    private void InitializeService()
    {
        // 사용자 홈 디렉토리 경로를 가져옵니다.
        //string homeDirectory = System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile);

        Debug.Log(System.Environment.SpecialFolder.UserProfile);

        // 홈 디렉토리를 기반으로 credentials.json 경로 설정
        //string credentialsPath = Path.Combine(homeDirectory, "path/to/credentials.json");
        string credentialsPath = Application.streamingAssetsPath + "/miserymarket.json";

        if (string.IsNullOrEmpty(credentialsPath) || !File.Exists(credentialsPath))
        {
            Debug.LogError("환경 변수 'GOOGLE_CREDENTIALS_PATH'가 설정되지 않았거나 파일이 존재하지 않습니다.");
            return;
        }

        GoogleCredential credential;
        try
        {
            using (var stream = new FileStream(credentialsPath, FileMode.Open, FileAccess.Read))
            {
                credential = GoogleCredential.FromStream(stream).CreateScoped(SheetsService.Scope.SpreadsheetsReadonly);
            }

            service = new SheetsService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "Unity Google Sheets Integration"
            });

            Debug.Log("Google Sheets API 서비스가 성공적으로 초기화되었습니다.");
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Google Sheets API 초기화 중 오류 발생: " + ex.Message);
        }
    }

    private void LoadDataFromGoogleSheet(TableData tableData)
    {
        InitializeService();

        if (service == null)
        {
            Debug.LogError("Google Sheets Service is not initialized.");
            return;
        }

        // 여러 범위를 쉼표로 구분하여 처리
        var ranges = tableData.range.Split(',').Select(r => $"{tableData.tableName}!{r.Trim()}").ToList();

        var request = service.Spreadsheets.Values.BatchGet(SPREADSHEET_ID);
        request.Ranges = ranges;
        var response = request.Execute();

        List<string> allColumnNames = new List<string>();
        List<List<string>> allDataRows = new List<List<string>>();

        foreach (var rangeResponse in response.ValueRanges)
        {
            IList<IList<object>> values = rangeResponse.Values;

            if (values != null && values.Count > 0)
            {
                int totalColumns = values[0].Count;

                // 첫 번째 행 (컬럼 이름)에서 유효한 컬럼 수 계산 및 컬럼 이름 배열 초기화
                List<int> validColumns = new List<int>();
                for (int i = 0; i < totalColumns; i++)
                {
                    string columnName = values[0][i].ToString();

                    // 컬럼 이름이 #으로 시작하면 무시
                    if (columnName.StartsWith("#"))
                    {
                        continue;
                    }

                    // 컬럼 이름이 중복되지 않게 추가
                    if (!allColumnNames.Contains(columnName))
                    {
                        allColumnNames.Add(columnName);
                    }

                    validColumns.Add(i);
                }

                // 유효한 컬럼의 데이터만 저장
                for (int i = 1; i < values.Count; i++) // 첫 번째 행은 컬럼 이름이므로 제외
                {
                    if (allDataRows.Count < i)
                    {
                        // 아직 해당 행이 추가되지 않은 경우 새로 추가
                        allDataRows.Add(new List<string>(new string[allColumnNames.Count]));
                    }

                    List<string> rowData = allDataRows[i - 1];
                    for (int j = 0; j < values[i].Count; j++)
                    {
                        rowData[allColumnNames.IndexOf(values[0][j].ToString())] = values[i][j].ToString();
                    }
                }
            }
            else
            {
                Debug.Log("데이터가 없습니다.");
            }
        }

        // 수집한 데이터를 배열로 변환하여 필드에 저장
        columnNames = allColumnNames.ToArray();
        int rows = allDataRows.Count;
        int columns = allColumnNames.Count;
        data = new string[rows, columns];

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                data[i, j] = allDataRows[i][j];
            }
        }

        Debug.Log($"{tableData.tableName}의 모든 범위에서 데이터가 성공적으로 로드되었습니다.");
    }

    private void LoadAllTablesAndSave()
    {
        InitializeService();

        if (service == null)
        {
            Debug.LogError("Google Sheets Service is not initialized.");
            return;
        }

        foreach (var tableData in tableDataCollection.tables)
        {
            LoadDataFromGoogleSheet(tableData);
            SaveOrUpdateGameData(tableData);
        }

        Debug.Log("모든 테이블이 성공적으로 불러오고 저장되었습니다.");
    }

    private void SaveOrUpdateGameData(TableData tableData)
    {
        string folderPath = tableData.isResourceFolderUsed ? "Assets/Resources/ScriptableObjects/GameData" : "Assets/3.Prefabs/ScriptableObjects/GameData";
        string assetPath = $"{folderPath}/{tableData.gameDataName}.asset";

        GameData gameData = AssetDatabase.LoadAssetAtPath<GameData>(assetPath);

        if (gameData == null)
        {
            gameData = ScriptableObject.CreateInstance<GameData>();
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
                AssetDatabase.Refresh(); // Unity에게 새 폴더가 생겼음을 알림
            }
            AssetDatabase.CreateAsset(gameData, assetPath);
            Debug.Log($"{tableData.gameDataName} 이름으로 새로운 GameData가 생성되었습니다.");
        }
        else
        {
            Debug.Log($"{tableData.gameDataName} GameData가 업데이트되었습니다.");
        }

        if (columnNames != null && data != null)
        {
            gameData.ApplyData(columnNames, data);
            EditorUtility.SetDirty(gameData);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"{tableData.gameDataName}에 데이터가 성공적으로 적용되었습니다.");
        }
        else
        {
            Debug.LogWarning("불러온 데이터가 없습니다. 데이터를 적용할 수 없습니다.");
        }
    }
}