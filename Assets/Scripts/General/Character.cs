using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Character : MonoBehaviour, ISavable
{
    [Header("事件监听")]
    public VoidEventSO newGameEvent;
    [Header("Basic Properties")]
    public float maxHealth;
    public float currentHealth;
    public float maxPower;
    public float currentPower;
    public float powerRecoverSpeed;

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
    private void NewGame()
    {
        currentHealth = maxHealth;
        currentPower = maxPower;
        OnHealthChanged?.Invoke(this);
    }
    private void OnEnable()
    {
        newGameEvent.OnEventRaised += NewGame;
        ISavable savable = this;
        savable.RegisterSaveDate();
    }
    private void OnDisable()
    {
        newGameEvent.OnEventRaised -= NewGame;
        ISavable savable = this;
        savable.UnRegisterSaveDate();
    }
    private void Start()
    {
        currentHealth = maxHealth;
    }
    private void Update()
    {
        if (invulnerable) invulnerableCounter -= Time.deltaTime;
        if (invulnerableCounter <= 0) invulnerable = false;
        if (currentPower < maxPower)
        {
            currentPower += powerRecoverSpeed * Time.deltaTime;
            OnHealthChanged?.Invoke(this);
        }
    }
    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Water"))
        {
            currentHealth = 0;
            OnDie?.Invoke();
            OnHealthChanged?.Invoke(this);
        }
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
    public void OnConsumePower(int cost)
    {
        currentPower -= cost;
        OnHealthChanged?.Invoke(this);
    }

    public DataDefinition GetDataID()
    {
        return GetComponent<DataDefinition>();
    }

    public void GetSaveData(Data data)
    {
        if (data.characterPosDist.ContainsKey(GetDataID().ID))
        {
            data.characterPosDist[GetDataID().ID] = transform.position;
            data.floatSavedData[GetDataID().ID + "health"] = currentHealth;
            data.floatSavedData[GetDataID().ID + "power"] = currentPower;
        }
        else
        {
            data.characterPosDist.Add(GetDataID().ID, transform.position);
            data.floatSavedData.Add(GetDataID().ID + "health", currentHealth);
            data.floatSavedData.Add(GetDataID().ID + "power", currentPower);
        }
    }

    public void LoadData(Data data)
    {
        if (data.characterPosDist.ContainsKey(GetDataID().ID))
        {
            transform.position = data.characterPosDist[GetDataID().ID];
            currentHealth = data.floatSavedData[GetDataID().ID + "health"];
            currentPower = data.floatSavedData[GetDataID().ID + "power"];
            OnHealthChanged?.Invoke(this);
        }
    }
}
