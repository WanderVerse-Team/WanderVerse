using UnityEngine;
using UnityEditor;
using System;
using WanderVerse.Framework.Utilities;
using WanderVerse.Framework.Data;

namespace WanderVerse.EditorScripts
{
    // This tells Unity to apply this script to any string with [LevelID]
    [CustomPropertyDrawer(typeof(LevelIDAttribute))]
    public class LevelIDDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType == SerializedPropertyType.String)
            {
                // Get levelIDs from the catalog
                string[] options = CourseCatalog.AllLevels;

                // Find which one is currently selected
                int currentIndex = Array.IndexOf(options, property.stringValue);
                if (currentIndex < 0) currentIndex = 0; // Default to the first lesson if empty

                // Draw the dropdown in the Unity Inspector!
                currentIndex = EditorGUI.Popup(position, label.text, currentIndex, options);

                // Save the exact string back to the variable
                property.stringValue = options[currentIndex];
            }
            else
            {
                // Warning if someone tries to use it on an int or float
                EditorGUI.LabelField(position, label.text, "Use [LevelID] on string variables only.");
            }
        }
    }
}