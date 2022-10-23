using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
[CustomEditor(typeof(MidiSync))]
public class MidiSyncEditor : Editor
{
    SerializedProperty mov;
    SerializedProperty source;
    SerializedProperty autoplayLineWithMidi;
    SerializedProperty midiFilePath;
    SerializedProperty timingsFound;
    SerializedProperty timingIndex;
    private void OnEnable()
    {
        mov = serializedObject.FindProperty("mov");
        source = serializedObject.FindProperty("source");
        autoplayLineWithMidi = serializedObject.FindProperty("autoplayLineWithMidi");
        midiFilePath = serializedObject.FindProperty("midiFilePath");
        timingsFound = serializedObject.FindProperty("timingsFound");
        timingIndex = serializedObject.FindProperty("timingIndex");
    }
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.LabelField("Instances", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(mov);
        EditorGUILayout.PropertyField(source);
        EditorGUILayout.LabelField("MIDI", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(autoplayLineWithMidi);
        EditorGUILayout.PropertyField(midiFilePath);
        EditorGUILayout.LabelField("Results", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(timingsFound);
        if (timingsFound.isExpanded)
        {
            var timingChilds = GetChildren(timingsFound);
            foreach (var children in timingChilds)
            {
                EditorGUILayout.PropertyField(children);
            }
        }
        EditorGUILayout.PropertyField(timingIndex);
        EditorGUILayout.LabelField("Toolkit", EditorStyles.boldLabel);
        if (GUILayout.Button("Generate Lines")) ((MidiSync)target).GenerateLines();
        if (GUILayout.Button("Reset Timings")) ((MidiSync)target).timingsFound = new float[0];
        serializedObject.ApplyModifiedProperties();
    }
    public static IEnumerable<SerializedProperty> GetChildren(SerializedProperty property)
    {
        property = property.Copy();
        var nextElement = property.Copy();
        bool hasNextElement = nextElement.NextVisible(false);
        if (!hasNextElement)
        {
            nextElement = null;
        }

        property.NextVisible(true);
        while (true)
        {
            if ((SerializedProperty.EqualContents(property, nextElement)))
            {
                yield break;
            }

            yield return property;

            bool hasNext = property.NextVisible(false);
            if (!hasNext)
            {
                break;
            }
        }
    }
}