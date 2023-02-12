using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor (typeof (EnemyAI))]
public class FieldOfViewEditor : Editor
{
    private void OnSceneGUI() {
        EnemyAI fov = (EnemyAI)target;
    }
}
