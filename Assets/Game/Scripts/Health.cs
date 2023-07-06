using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour
{
    public int MaxHealth;
    public int CurrentHealth;
    private Character cc;

    private void Awake()
    {
        CurrentHealth = MaxHealth;
        cc = GetComponent<Character>();
    }

    public void ApplyDamage(int damage)
    {
        CurrentHealth -= damage;
        Debug.Log(gameObject.name + " took" +  damage + " damage.");
        Debug.Log(gameObject.name + " current health: " + CurrentHealth);

        CheckHealth();
    }

    private void CheckHealth()
    {
        if (CurrentHealth <= 0)
        {
            cc.SwitchStateTo(Character.CharacterState.Dead);
        }
    }

    public void AddHealth (int health)
    {
        CurrentHealth += health;

        if (CurrentHealth > MaxHealth)
        {
            CurrentHealth = MaxHealth;
        }
    }
}
