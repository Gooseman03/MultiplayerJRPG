using UnityEngine;

public class HostInterpolation : ClientInterpolation
{
    private GameObject InterpolatedVersion;
    private SpriteRenderer sprite;

    protected override void PostStart()
    {
        InterpolatedVersion = new GameObject("PlayerVisual");
        SpriteRenderer OriginalRenderer = gameObject.GetComponent<SpriteRenderer>();
        sprite = InterpolatedVersion.AddComponent<SpriteRenderer>();
        sprite.sprite = OriginalRenderer.sprite;
        InterpolatedVersion.layer = 6;
        InterpolatedVersion.tag = "Player";
        OriginalRenderer.enabled = false;
        interpolationTarget = InterpolatedVersion.transform;
    }
}
