using UnityEngine;

// plays one footstep sound every time the player has walked a set distance, so the steps match how fast the player moves
public class PlayerFootsteps : MonoBehaviour
{
    public AudioClip footstepClip;       // the single step sound
    public float stepDistance = 1.8f;    // how far the player walks between steps, lower = faster steps
    public float volume = 0.6f;          // how loud the steps are
    public float pitchVariation = 0.1f;  // small random pitch change so every step doesn't sound identical

    private AudioSource audioSource;
    private CharacterController controller; // only used to check if the player is on the ground, stays empty if there isn't one
    private Vector3 lastPosition;
    private float distanceSinceLastStep;

    void Start()
    {
        // gets its own AudioSource so it never touches any other sound settings on the player
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f; // 2D sound, the player's ears are the listener

        controller = GetComponent<CharacterController>();
        lastPosition = transform.position;
    }

    void Update()
    {
        Vector3 moved = transform.position - lastPosition;
        moved.y = 0f; // ignore falling and jumping, only count walking along the ground
        lastPosition = transform.position;

        if (footstepClip == null) return;

        // no steps in the air, and ignore teleports and levels loading
        bool grounded = controller == null || controller.isGrounded;
        if (!grounded || moved.magnitude > 1f) return;

        distanceSinceLastStep += moved.magnitude;

        if (distanceSinceLastStep >= stepDistance)
        {
            distanceSinceLastStep = 0f;
            audioSource.pitch = 1f + Random.Range(-pitchVariation, pitchVariation);
            audioSource.PlayOneShot(footstepClip, volume);
        }
    }
}