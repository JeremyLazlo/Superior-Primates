using UnityEngine;
using UnityEngine.UI;

// Extends the existing partial PlayerHealth class.
// Add this file to the project; do not attach it as a separate component.
public partial class PlayerHealth
{
    [Header("Health Display")]
    public Image healthFill;

    void LateUpdate()
    {
        if (healthFill != null)
        {
            healthFill.fillAmount = maxHealth > 0
                ? Mathf.Clamp01((float)health / maxHealth)
                : 0f;
        }
    }
}
