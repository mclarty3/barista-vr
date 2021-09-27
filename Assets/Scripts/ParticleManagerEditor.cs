using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ParticleManager))]
public class ParticleManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if (GUILayout.Button("Toggle pour"))
        {
            if (target.GetType() == typeof(ParticleManager))
            {
                ParticleManager pm = (ParticleManager)target;
                pm.isPouring = !pm.isPouring;
            }
        }
    }
}
