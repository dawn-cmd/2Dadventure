using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Character : MonoBehaviour
{
    [Header("Basic Properties")]
    public float maxHealth;
    public float currentHealth;

    [Header("Invincible frame wounded")]
    public float invulnerableDuration;
    private float invulnerableCounter;
    public void SetInvulnerableCounter(float counter)
    {
        invulnerableCounter = counter;
    }
    public bool invulnerable;
    public UnityEvent<Character> OnHealthChanged;
    public UnityEvent<Transform> OnTakeDamage;
    public UnityEvent OnDie;
    private void Start()
    {
        currentHealth = maxHealth;
        OnHealthChanged?.Invoke(this);
    }
    private void Update()
    {
        if (invulnerable) invulnerableCounter -= Time.deltaTime;
        if (invulnerableCounter <= 0) invulnerable = false;
    }
    public void TakeDamage(Attack attacker)
    {
        if (invulnerable) return;
        Debug.Log(attacker.damage);
        if (currentHealth >= attacker.damage)
        {
            currentHealth -= attacker.damage;
            TriggerInvulnerable();
            // Take Hurt
            if (attacker.transform is not null && attacker.transform is UnityEngine.Transform)
            {
                Debug.Log("Is Transform");
                OnTakeDamage?.Invoke(attacker.transform);
            }
        }
        else
        {
            currentHealth = 0;
            OnDie?.Invoke();
            // Dead
        }
        OnHealthChanged?.Invoke(this);
    }
    private void TriggerInvulnerable()
    {
        if (invulnerable) return;
        invulnerable = true;
        invulnerableCounter = invulnerableDuration;
    }
}
