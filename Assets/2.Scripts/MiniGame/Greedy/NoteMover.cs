using UnityEngine;

public class NoteMover
{
    private float speed = 100f;
    public bool IsMoving { get; private set; } = false;
    private Vector3 direction = Vector3.left;
    private readonly Transform transform;

    public NoteMover(Transform transform)
    {
        this.transform = transform;
    }

    public void Update()
    {
        if (IsMoving)
        {
            transform.Translate(direction * speed * Time.deltaTime);
        }
    }

    public void StartMoving()
    {
        IsMoving = true;
    }

    public void SetSpeed(float newSpeed)
    {
        speed = newSpeed;
    }
}
