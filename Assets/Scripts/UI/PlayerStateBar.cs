using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStateBar : MonoBehaviour
{
    public Image healthBar;
    public Image healthDelayBar;
    public Image powerBar;
    /// <summary>
    /// Updates the health bar fill amount based on the given health percentage.
    /// </summary>
    /// <param name="healthPercent">The health percentage to set the fill amount to.</param>

    public void OnHealthChanged(float healthPercent)
    {
        healthBar.fillAmount = healthPercent;
    }
    private void Update()
    {
        if (healthDelayBar.fillAmount > healthBar.fillAmount)
        {
            healthDelayBar.fillAmount -= Time.deltaTime;
            Debug.Log("Delay Bar: " + healthDelayBar.fillAmount);
        }
        else
        {
            healthDelayBar.fillAmount = healthBar.fillAmount;
        }
    }
}
