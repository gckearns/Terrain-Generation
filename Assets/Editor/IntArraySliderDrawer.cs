using UnityEditor;
using UnityEngine;

// IngredientDrawer
[CustomPropertyDrawer(typeof(IntArraySlider))]
public class IntArraySliderDrawer : PropertyDrawer
{
    // Draw the property inside the given rect
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // Using BeginProperty / EndProperty on the parent property means that
        // prefab override logic works on the entire property.
        EditorGUI.BeginProperty(position, label, property);

        // Save indent to set it back later
        var indent = EditorGUI.indentLevel;

        // First get the attribute since it contains the range for the slider
        IntArraySlider intArraySlider = (IntArraySlider)attribute;
        int[] intArray = intArraySlider.array;
        int arraySize = intArraySlider.array.Length;

        // Calculate rects
        var labelRect = new Rect(position.x, position.y, EditorGUIUtility.labelWidth, position.height);
        var chunkSizeRect = new Rect(position.x + EditorGUIUtility.labelWidth, position.y, 30, position.height);
        var sliderRect = new Rect(position.x + EditorGUIUtility.labelWidth + 30, position.y, position.width - (EditorGUIUtility.labelWidth + 30), position.height);

        // Draw label
        EditorGUI.PrefixLabel(labelRect, label);

        // Don't make chunkSize field be indented
        EditorGUI.indentLevel = 0;

        // Draw fields - passs GUIContent.none to each so they are drawn without labels
        EditorGUI.SelectableLabel(chunkSizeRect, string.Format("{0}", intArray[property.intValue]));
        property.intValue = EditorGUI.IntSlider(sliderRect, property.intValue, 0, arraySize - 1);

        // Set indent back to what it was
        EditorGUI.indentLevel = indent;

        EditorGUI.EndProperty();
    }
}