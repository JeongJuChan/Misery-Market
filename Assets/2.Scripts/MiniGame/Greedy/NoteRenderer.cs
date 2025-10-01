using UnityEngine;
using UnityEngine.UI;

public class NoteRenderer
{
    private readonly Image image;

    public NoteRenderer(Image image)
    {
        this.image = image;
    }

    public void SetColor(Color color)
    {
        if (image != null)
        {
            image.color = color;
        }
    }
}
