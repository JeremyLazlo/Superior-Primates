using System.Collections.Generic;
using UnityEngine;

// The text of the credits page. Edit the Credits asset (Assets/UI/Menus/Credits.asset) to change it;
[CreateAssetMenu(fileName = "Credits", menuName = "Menus/Credits")]
public class CreditsData : ScriptableObject
{
    [System.Serializable]
    public class Section
    {
        [Tooltip("e.g. Programming, Art, Sound, Special Thanks")]
        public string heading;
        public List<string> names = new List<string>();
    }

    [Tooltip("Shown first, in large letters.")]
    public string title = "Superior Primates";

    public List<Section> sections = new List<Section>();

    [Tooltip("Shown last.")]
    public string closingLine = "Thanks for playing";
}
