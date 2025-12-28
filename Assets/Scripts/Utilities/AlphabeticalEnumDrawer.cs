using UnityEditor;
using System;
using System.Linq;
using UnityEngine;
using ProjectScript.Enums;

[CustomPropertyDrawer(typeof(DigimonType))]
public class AlphabeticalEnumDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var enumType = fieldInfo.FieldType;
        var names = Enum.GetNames(enumType).OrderBy(n => n).ToArray();
        var values = names.Select(n => (int)Enum.Parse(enumType, n)).ToArray();

        int index = Array.IndexOf(values, property.intValue);
        index = EditorGUI.Popup(position, label.text, index, names);
        property.intValue = values[index];
    }
}

