using PrimeTween;
using UnityEngine;

public class BaseCampToggleAnimation
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite inActiveSprite;
    [SerializeField] private Sprite activeSprite;

    private bool isOriginSprite = true;
    private Sequence seq; // 실행 중 중복 방지용

    public BaseCampToggleAnimation(SpriteRenderer spriteRenderer, Sprite inActiveSprite, Sprite activeSprite)
    {
        this.spriteRenderer = spriteRenderer;
        this.inActiveSprite = inActiveSprite;
        this.activeSprite = activeSprite;
    }

    void Awake()
    {
        spriteRenderer.sprite = inActiveSprite;
        var c = spriteRenderer.color;
        spriteRenderer.color = new Color(c.r, c.g, c.b, 1f);
    }

    public void Fade()
    {
        if (seq.isAlive) return; // 진행 중이면 무시

        var c0 = spriteRenderer.color;
        var fadeOut = new Color(c0.r, c0.g, c0.b, 0f);
        var fadeIn = new Color(c0.r, c0.g, c0.b, 1f);

        seq = Sequence.Create()
            .Chain(Tween.Color(spriteRenderer, fadeOut, 0.25f))   // 페이드 아웃
            .ChainCallback(() =>
            {
                spriteRenderer.sprite = isOriginSprite ? activeSprite : inActiveSprite;
                isOriginSprite = !isOriginSprite;
            });
    }
}
