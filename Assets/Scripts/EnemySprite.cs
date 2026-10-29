using UnityEngine;

// swaps the 3D body for a flat sprite that always turns to face the camera
// Can put this on an enemy and then drag a sprite
public class EnemySprite : MonoBehaviour
{
    public Sprite sprite;                 // the picture to show
    public float height = 2f;             // how tall the enemy looks in the game, in Unity units
    public float heightOffset = 0f;       // raise (positive) or lower (negative) the sprite if it floats or sinks into the floor
    public Color tint = Color.white;      // darken this if the sprite looks too bright in a dark room
    public bool hideOriginalBody = true;  // turns off the old capsule or model, colliders and movement are not touched

    private Transform spriteObject;
    private Camera cam;

    void Start()
    {
        if (sprite == null) return; // nothing assigned, old body stays visible

        if (hideOriginalBody)
        {
            foreach (Renderer r in GetComponentsInChildren<Renderer>())
            {
                if (r is MeshRenderer || r is SkinnedMeshRenderer) r.enabled = false;
            }
        }

        // find where the feet are so the sprite stands on the ground instead of floating at the enemy's center
        float feetY = transform.position.y;
        Collider col = GetComponentInChildren<Collider>();
        if (col != null) feetY = col.bounds.min.y;

        GameObject visual = new GameObject("EnemySpriteVisual");
        spriteObject = visual.transform;
        spriteObject.SetParent(transform, true);

        SpriteRenderer sr = visual.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.color = tint;

        // scale the sprite to the wanted height whatever its pixel size is, and keep it right even if the enemy object is scaled
        float scale = height / sprite.bounds.size.y;
        Vector3 parentScale = transform.lossyScale;
        spriteObject.localScale = new Vector3(scale / parentScale.x, scale / parentScale.y, scale / parentScale.z);

        // lift the sprite so the bottom edge of the picture sits on the ground, this works for any pivot setting
        float pivotY = feetY - sprite.bounds.min.y * scale + heightOffset;
        spriteObject.position = new Vector3(transform.position.x, pivotY, transform.position.z);
    }

    // turns the sprite toward the camera every frame, left and right only so it never tilts
    void LateUpdate()
    {
        if (spriteObject == null) return;

        if (cam == null) cam = Camera.main;
        if (cam == null) return;

        Vector3 facing = cam.transform.forward;
        facing.y = 0f;

        if (facing.sqrMagnitude > 0.0001f)
        {
            spriteObject.rotation = Quaternion.LookRotation(facing);
        }
    }
}