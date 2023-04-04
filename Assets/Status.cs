using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Status : MonoBehaviour
{
    private float      _health   = 100;
    private float      maxHealth = 100;
    private float      _mana     = 100;
    private float      maxMana   = 100;
    public  GameObject healthBar;
    public  GameObject manaBar;
    public  GameObject menu;
    public  string     savedProgram;

    public float health
    {
        get => _health;
        set
        {
            _health = value;
            if (_health > maxHealth)
                _health = maxHealth;
            if (_health < 0)
                _health = 0;
            if (healthBar != null)
                healthBar.transform.localScale = new(_health / maxHealth, 1, 1);
        }
    }
    
    public float mana
    {
        get => _mana;
        set
        {
            _mana = value;
            if (_mana > maxMana)
                _mana = maxMana;
            if (_mana < 0)
            {
                health += _mana;
                _mana = 0;
            }

            if (manaBar != null)
                manaBar.transform.localScale = new(_mana / maxMana, 1, 1);
        }
    }
    void OnMenu()
    {
        menu.SetActive(!menu.activeSelf);
        // reverse cursor lock
        Cursor.lockState = Cursor.lockState == CursorLockMode.Locked ? CursorLockMode.None : CursorLockMode.Locked;
    }
    public void OnFire()
    {
        GetComponent<Compiler>().Compile(savedProgram);
    }
}
#if UNITY_EDITOR
[CustomEditor(typeof(Status))]
public class StatusEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        Status status = (Status) target;
        status.health = EditorGUILayout.FloatField("Health", status.health);
        status.mana = EditorGUILayout.FloatField("Mana", status.mana);
        
    }
}
#endif