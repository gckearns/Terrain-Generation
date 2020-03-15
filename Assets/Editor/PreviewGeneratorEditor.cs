using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SpherePreviewGenerator))]
public class PreviewGeneratorEditor : Editor
{

    public override void OnInspectorGUI()
    {
        base.DrawDefaultInspector();
        (target as SpherePreviewGenerator).UpdateReferencePlanes();
        (target as SpherePreviewGenerator).UpdateReferenceSphere();
        if (GUILayout.Button("Generate"))
        {
            (target as SpherePreviewGenerator).GeneratePreview();
        }

    }
}
