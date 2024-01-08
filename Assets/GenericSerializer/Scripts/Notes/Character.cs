using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;

public class Character : MonoBehaviour
{

    public string name;

    public int speed;
    public int health;

    public GameObject weapon;

    private int currentHealth;




    [ContextMenu("Copy With Reflection")]
    public void copyWithReflection()
    {
        CharacterToCopyFrom = this;
        Type characterType = typeof(Character);
        FieldInfo[] characterFields = characterType.GetFields(BindingFlags.Public | BindingFlags.Instance);

        fieldToCopy = characterFields;
        Debug.Log($"Copying: {characterFields.Length}");
    }
    [ContextMenu("Paste With Reflection")]
    public void PasteWithReflection()
    {
        foreach (FieldInfo field in fieldToCopy)
        {
            object value = field.GetValue(CharacterToCopyFrom);
            field.SetValue(this, value);
        }
    }

    private static Character CharacterToCopyFrom;
    private static FieldInfo[] fieldToCopy;
}
