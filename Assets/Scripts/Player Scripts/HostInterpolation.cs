using UnityEngine;

public class HostInterpolation : Extrapolation
{
    private GameObject InterpolatedVersion;
    private SpriteRenderer sprite;

    // Makes the target invisiable and creates a fake Visual effect of the player to do smoothing on
    protected override void PostStart()
    {
        // Grab the original sprite
        SpriteRenderer OriginalRenderer = gameObject.GetComponent<SpriteRenderer>();
        // Create a Gameobject that is just a sprite renderer
        InterpolatedVersion = new GameObject("Visual");
        sprite = InterpolatedVersion.AddComponent<SpriteRenderer>();
        // Copy all visuals to new object
        sprite.sprite = OriginalRenderer.sprite;
        InterpolatedVersion.layer = 2; // layer is ignore raycast
        InterpolatedVersion.tag = gameObject.tag;
        InterpolatedVersion.transform.position = transform.position;
        // disable the old renderer
        OriginalRenderer.enabled = false;
        // Tell the interpolator what to target
        interpolationTarget = InterpolatedVersion.transform;
    }
}
