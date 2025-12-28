using UnityEngine;
using ProjectScript.Enums;


#if UNITY_EDITOR
using UnityEditor;
[CustomEditor(typeof(DrawPileManager))]
public class DrawPileManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        DrawPileManager drawPileManager = (DrawPileManager)target;
 

        if (GUILayout.Button("Draw Next Card"))
        {
            GameSetupStart.playerBlue.drawPile.DrawCard(1,FieldPlace.Hand);
            GameSetupStart.playerRed.drawPile.DrawCard(1, FieldPlace.Hand);
        }

        
    }
}
#endif