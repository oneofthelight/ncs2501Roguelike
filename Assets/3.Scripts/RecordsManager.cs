using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.UIElements; // UI Toolkit 사용을 위해 추가

public class RecordsManager : MonoBehaviour
{
    // 저장할 기록의 최대 개수
    private const int MAX_RECORDS = 6;
    
    // PlayerPrefs 키의 접두사
    private const string LEVEL_KEY_PREFIX = "HighestLevel_"; 
    private const string NAME_KEY_PREFIX = "RecordName_"; 

    /// <summary>
    /// 단일 플레이어의 최고 기록 정보를 담는 구조체
    /// </summary>
    public struct RecordEntry
    {
        public int Level;
        public string Name;
    }

    #region Private UI Fields
    private TextField m_NameInputTextField; 
    private Button m_SaveRecordButton;
    private int m_CurrentLevelToSave = 0; // 저장할 레벨 값 임시 저장
    #endregion

    /// <summary>
    /// GameManager에서 게임 오버 시 호출하여 기록 입력 UI를 초기화합니다.
    /// </summary>
    /// <param name="nameField">이름을 입력받을 TextField</param>
    /// <param name="saveButton">기록 저장을 실행할 Button</param>
    /// <param name="currentLevel">현재 도달한 레벨 값</param>
 

    public void InitRecordInputUI(TextField nameField, Button saveButton, int currentLevel)
    {
        m_NameInputTextField = nameField;
        m_SaveRecordButton = saveButton;
        m_CurrentLevelToSave = currentLevel;

        // 1. 이름 필드 초기화
        m_NameInputTextField.value = "Player"; // 기본값 설정
        
        // 2. 저장 버튼 리스너 연결 (중복 방지를 위해 기존 리스너 제거 후 다시 연결)
        m_SaveRecordButton.clicked -= OnSaveRecordButtonClicked;
        m_SaveRecordButton.clicked += OnSaveRecordButtonClicked;
        
        // 3. 기록 저장 버튼 활성화
        m_SaveRecordButton.SetEnabled(true);

        // Text Field에 포커스를 주어 바로 입력을 받을 수 있도록 합니다.
        m_NameInputTextField.Focus();

        Debug.Log($"기록 입력 UI 초기화 완료. 레벨: {currentLevel}");
    }

    /// <summary>
    /// 기록 저장 버튼이 눌렸을 때 호출되는 함수
    /// </summary>
    private void OnSaveRecordButtonClicked()
    {
        string playerName = m_NameInputTextField.value;
        
        // 입력된 이름과 레벨로 기록 저장 로직 호출
        AddNewRecord(m_CurrentLevelToSave, playerName);
        
        // 기록이 저장되었으므로 버튼을 비활성화하여 중복 저장 방지
        m_SaveRecordButton.SetEnabled(false);
        
        // (추가 작업 필요 시: UI 닫기, 하이 스코어 목록 표시 등을 GameManager에 알릴 수 있습니다.)
    }

    /// <summary>
    /// 현재 플레이어가 달성한 레벨과 이름을 기록 목록에 추가하고 순위를 업데이트합니다.
    /// </summary>
    /// <param name="currentLevel">현재 게임에서 도달한 레벨 수치</param>
    /// <param name="playerName">기록을 달성한 플레이어의 이름</param>
    public void AddNewRecord(int currentLevel, string playerName)
    {
        // 1. 현재 저장된 모든 기록을 불러옵니다.
        List<RecordEntry> currentRecords = LoadAllRecords();

        // 2. 새로운 기록을 목록에 추가합니다.
        currentRecords.Add(new RecordEntry
        {
            Level = currentLevel,
            Name = string.IsNullOrEmpty(playerName) ? "Unknown" : playerName 
        });
        
        // 3. 기록을 내림차순 (높은 레벨이 먼저)으로 정렬합니다.
        List<RecordEntry> sortedRecords = currentRecords
                                            .OrderByDescending(entry => entry.Level)
                                            .ToList();

        // 4. 최대 개수(6개)까지만 남깁니다.
        int recordsToSaveCount = Mathf.Min(sortedRecords.Count, MAX_RECORDS);
        
        // 5. 정렬된 기록을 PlayerPrefs에 레벨과 이름 쌍으로 저장합니다.
        for (int i = 0; i < recordsToSaveCount; i++)
        {
            PlayerPrefs.SetInt(LEVEL_KEY_PREFIX + i, sortedRecords[i].Level);
            PlayerPrefs.SetString(NAME_KEY_PREFIX + i, sortedRecords[i].Name);
        }
        
        // 6. 남은 공간의 이전 기록들을 지워줍니다.
        for (int i = recordsToSaveCount; i < MAX_RECORDS; i++)
        {
            PlayerPrefs.DeleteKey(LEVEL_KEY_PREFIX + i);
            PlayerPrefs.DeleteKey(NAME_KEY_PREFIX + i);
        }

        // PlayerPrefs의 변경 사항을 디스크에 저장합니다.
        PlayerPrefs.Save();
        
        Debug.Log($"새로운 기록 저장 완료 - 이름: {playerName}, 레벨: {currentLevel}");
        PrintAllRecords(); // 저장 후 전체 기록을 확인합니다.
    }


    /// <summary>
    /// PlayerPrefs에 저장된 모든 레벨 기록을 이름과 함께 불러옵니다.
    /// </summary>
    /// <returns>저장된 기록 엔트리들의 리스트</returns>
    public List<RecordEntry> LoadAllRecords()
    {
        List<RecordEntry> records = new List<RecordEntry>();
        
        for (int i = 0; i < MAX_RECORDS; i++)
        {
            string levelKey = LEVEL_KEY_PREFIX + i;
            string nameKey = NAME_KEY_PREFIX + i;
            
            // 레벨 키와 이름 키가 모두 존재해야 유효한 기록으로 판단합니다.
            if (PlayerPrefs.HasKey(levelKey) && PlayerPrefs.HasKey(nameKey))
            {
                records.Add(new RecordEntry
                {
                    Level = PlayerPrefs.GetInt(levelKey),
                    Name = PlayerPrefs.GetString(nameKey)
                });
            }
            else
            {
                break;
            }
        }
        
        return records;
    }

    /// <summary>
    /// 현재 저장된 모든 기록을 디버그 로그에 출력합니다. (확인용)
    /// </summary>
    public void PrintAllRecords()
    {
        List<RecordEntry> records = LoadAllRecords();
        Debug.Log("--- 현재 최고 레벨 기록 (Top 6) ---");
        
        if (records.Count == 0)
        {
            Debug.Log("저장된 기록이 없습니다.");
            return;
        }

        for (int i = 0; i < records.Count; i++)
        {
            Debug.Log($"순위 {i + 1}: 레벨 {records[i].Level}, 이름: {records[i].Name}");
        }
    }
    
    // (선택 사항) 모든 기록을 초기화하는 함수
    public void ClearAllRecords()
    {
        for (int i = 0; i < MAX_RECORDS; i++)
        {
            if (PlayerPrefs.HasKey(LEVEL_KEY_PREFIX + i))
            {
                PlayerPrefs.DeleteKey(LEVEL_KEY_PREFIX + i);
            }
            if (PlayerPrefs.HasKey(NAME_KEY_PREFIX + i))
            {
                PlayerPrefs.DeleteKey(NAME_KEY_PREFIX + i);
            }
        }
        PlayerPrefs.Save();
        Debug.Log("모든 레벨 기록이 초기화되었습니다.");
    }
}