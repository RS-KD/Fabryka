using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(FabricGenerator))]
public class FactoryGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Draw the default inspector
        DrawDefaultInspector();

        // Reference the FactoryGenerator script
        FabricGenerator factoryGenerator = (FabricGenerator)target;

        // Add a space
        EditorGUILayout.Space();

      
        // Add Generate button
        if (GUILayout.Button("Generate Factory"))
        {
            factoryGenerator.GenerateFactory();
        }

        // Add Clear button
        if (GUILayout.Button("Clear Factory"))
        {
            factoryGenerator.ClearFactory();
        }
    }
}