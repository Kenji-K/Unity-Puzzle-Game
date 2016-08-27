using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(Piece))]
public class PieceEditor : Editor {
    public override void OnInspectorGUI() {
        DrawDefaultInspector();

        var piece = (Piece)target;
        if (piece.CurrentCoords != null) {
            EditorGUILayout.LabelField("Current Position", piece.CurrentCoords.ToString());
        }
    }
}
