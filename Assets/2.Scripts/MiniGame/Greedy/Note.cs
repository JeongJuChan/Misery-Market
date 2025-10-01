using UnityEngine;
using UnityEngine.UI;

public class Note : MonoBehaviour
{
    private Transform myTransform;
    [SerializeField] private Image image;
    [field: SerializeField] public NoteMover noteMover { get; private set; }
    [field: SerializeField] public NoteRenderer noteRenderer { get; private set; }

    void Awake()
    {
        myTransform = GetComponent<Transform>();
        noteMover = new NoteMover(myTransform);
        noteRenderer = new NoteRenderer(image);
    }
}
