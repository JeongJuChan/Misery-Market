using PrimeTween;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// SpriteRenderer 위에 마우스 호버 시 Active 스프라이트로 전환 / 해제 시 Inactive 스프라이트로 전환.
/// 전환 시 페이드 아웃 -> 스프라이트 교체 -> 페이드 인 애니메이션.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Collider2D))] // OnMouseEnter/Exit 동작을 위해 필요
public class BaseCampToggleAnimation : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite inActiveSprite;
    [SerializeField] private Sprite activeSprite;

    [Header("Animation Settings")]
    [SerializeField] private float fadeOutDuration = 0.15f;
    [SerializeField] private float fadeInDuration = 0.15f;
    [SerializeField] private Ease fadeEase = Ease.OutCubic;
    [SerializeField] private bool startAsInactive = true;

    private bool isHover;               // 현재 호버 상태
    private Sequence seq;               // 진행 중 시퀀스 (중복 방지)
    private Color baseColor = Color.white;

    private void Reset()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        var col = GetComponent<Collider2D>();
        if (col is BoxCollider2D box)
        {
            if (box.size == Vector2.zero && spriteRenderer != null && spriteRenderer.sprite != null)
            {
                box.size = spriteRenderer.sprite.bounds.size;
            }
        }
    }

    private void Awake()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            baseColor = spriteRenderer.color;
            spriteRenderer.sprite = startAsInactive ? inActiveSprite : activeSprite;
            spriteRenderer.color = new Color(baseColor.r, baseColor.g, baseColor.b, 1f);
        }
    }

    private void OnMouseEnter()
    {
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            return; // UI 클릭 중이므로 3D 오브젝트 클릭 로직 스킵
        }

        SetHover(true);
    }

    private void OnMouseExit()
    {
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            return; // UI 클릭 중이므로 3D 오브젝트 클릭 로직 스킵
        }

        SetHover(false);
    }

    /// <summary>
    /// 호버 상태에 따라 스프라이트를 전환하고 페이드 애니메이션 수행
    /// </summary>
    /// <param name="hover">true=Active, false=Inactive</param>
    public void SetHover(bool hover)
    {
        if (spriteRenderer == null) return;
        if (isHover == hover && !seq.isAlive) return; // 이미 상태 동일 & 애니 없음

        isHover = hover;

        // 기존 시퀀스 중지
        if (seq.isAlive)
            seq.Stop();

        Color current = spriteRenderer.color;
        Color fadeOutTarget = new Color(current.r, current.g, current.b, 0f);
        Color fadeInTarget = new Color(baseColor.r, baseColor.g, baseColor.b, 1f);

        seq = Sequence.Create()
            .Chain(Tween.Color(spriteRenderer, fadeOutTarget, fadeOutDuration, fadeEase))
            .ChainCallback(() =>
            {
                spriteRenderer.sprite = hover ? activeSprite : inActiveSprite;
            })
            .Chain(Tween.Color(spriteRenderer, fadeInTarget, fadeInDuration, fadeEase));
    }
}
