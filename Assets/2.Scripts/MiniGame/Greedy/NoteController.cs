using System;
using System.Collections.Generic;
using UnityEngine;

public class NoteController : MonoBehaviour
{
    [SerializeField] private Note notePrefab;
    private RectTransform noteRectTransform;
    [SerializeField] private int createCount = 7;
    [SerializeField] private List<Note> notes;
    private Note correctNote;
    [SerializeField] private float noteSpeed = 100f;

    [SerializeField] private Transform lineTransform;

    [field: SerializeField] public GreedyGameState CurrentGameState { get; private set; } = GreedyGameState.Ready;

    void Awake()
    {
        InitNoteOriginSettings();
        CreateNotes(createCount);
        InitNotes();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.S) && CurrentGameState == GreedyGameState.Ready)
        {
            SetNotesMoving();
        }

        if (Input.GetKeyDown(KeyCode.Space) && CurrentGameState == GreedyGameState.Playing)
        {
            CheckNoteAlignWithLine();
        }

        UpdateNotesMove();
    }

    private void UpdateNotesMove()
    {
        foreach (var note in notes)
        {
            if (note != null)
            {
                note.noteMover.Update();
            }
        }
    }

    private void InitNoteOriginSettings()
    {
        notePrefab = Resources.Load<Note>("MiniGame/Greedy/Note");
        noteRectTransform = notePrefab.GetComponent<RectTransform>();
    }

    private void CreateNotes(int createCount)
    {
        for (int i = 0; i < createCount; i++)
        {
            Vector3 notePos = new Vector3(transform.position.x + i * noteRectTransform.sizeDelta.x, transform.position.y, 0);
            var note = Instantiate(notePrefab, notePos, Quaternion.identity, transform);
            notes.Add(note);
        }
    }

    private void InitNotes()
    {
        if (notes.Count > 0 && notes[0] != null)
        {
            correctNote = notes[3]; // 네 번째 노트가 정답 노트라고 가정
            // 1초에 하나씩 노트가 지나가도록 속도 설정
            noteSpeed = correctNote.GetComponent<RectTransform>().sizeDelta.x;
        }

        foreach (var note in notes)
        {
            note.noteMover.SetSpeed(noteSpeed);
            if (note == correctNote)
            {
                note.noteRenderer.SetColor(Color.green); // 정답 노트는 초록색으로 표시
            }
            else
            {
                note.noteRenderer.SetColor(Color.red); // 나머지 노트는 빨간색으로 표시
            }
        }
    }

    private void CheckNoteAlignWithLine()
    {
        CurrentGameState = GreedyGameState.Ready;

        if (correctNote.transform.position.x >= lineTransform.position.x - noteSpeed * 0.5f &&
            correctNote.transform.position.x <= lineTransform.position.x + noteSpeed * 0.5f)
        {
            Debug.Log("정확히 맞췄습니다!");
        }
        else
        {
            Debug.Log("놓쳤습니다.");
        }
    }

    private void SetNotesMoving()
    {
        CurrentGameState = GreedyGameState.Playing;
        // 모든 노트 이동 시작

        foreach (var note in notes)
        {
            if (note != null)
            {
                // 각 NoteMover의 Update 메서드를 호출하여 노트 이동 처리
                note.noteMover.StartMoving();
            }
        }
    }
}
