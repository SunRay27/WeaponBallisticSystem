using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Weapon))]
public class WeaponEditor : Editor {
    public Texture aTexture;
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
      //  EditorGUILayout.ObjectField()
       // GUI.DrawTexture(new Rect(10, 10, 60, 60), aTexture, ScaleMode.ScaleToFit, true, 10.0F);
    }
}
