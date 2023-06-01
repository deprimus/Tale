#if UNITY_EDITOR
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEditor;
using TMPro;

namespace TaleUtil
{
    public partial class Editor
    {
        public class CreateTransitionDialog : EditorWindow
        {
            public new string name;

            void OnGUI()
            {
                EditorGUILayout.LabelField("Enter the transition name:");
                name = EditorGUILayout.TextField(name);

                if (GUILayout.Button("OK") || Event.current.keyCode == KeyCode.Return)
                {
                    CreateTaleTransition(FindTaleMaster(), name);
                    Close();
                }
            }
        }
    }
}
#endif