using System.Collections;
using UnityEngine;

// Opens the story popup when the level starts: the player's first thoughts after breaking out of the tube.
public class IntroPopup : MonoBehaviour
{
    [Tooltip("Heading above the text. Leave empty for none.")]
    [SerializeField] private string title = "Awake";

    [TextArea(6, 16)]
    [SerializeField] private string message =
        "There's glass everywhere. It looks like whatever I was in somehow shattered. The floor is so cold, and I can't help myself from shaking. " +
        "How long was I in that thing?\n\n" +
        "I remember seeing faces on the other side of the glass. Those faces were something beyond human. Always observing, always taking notes. " +
        "It looks like their tests finally failed, but I fear they are still near.\n\n" +
        "I need to escape this nightmare, whatever it takes. First, I should look around to see if there's anything that could aid my escape. ";

    [Tooltip("Real seconds after the level appears before the popup opens, so the player sees the room first.")]
    [SerializeField] private float delay = 0.1f;

    [Tooltip("Popups with the same ID only show once per play session.")]
    [SerializeField] private string popupId = "intro";

    private IEnumerator Start()
    {
        // Wait a frame so every other Start has run: the map is built, the player is placed,
        // and PauseMenu / InventoryToggle have set up time and the cursor
        yield return null;

        if (delay > 0f)
        {
            yield return new WaitForSecondsRealtime(delay);
        }

        // Don't open on top of the pause menu or the inventory: wait until the player is actually playing
        while (Time.timeScale == 0f || PauseMenu.IsGameplayFrozen)
        {
            yield return null;
        }

        DialoguePopup.ShowOnce(popupId, message, title);
    }
}
