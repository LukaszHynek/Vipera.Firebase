using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Vipera
{
    [CustomEditor(typeof(RemoteConfig))]
    public class RemoteConfigEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            ShowAssignAllButton();
            base.OnInspectorGUI();
        }

        void ShowAssignAllButton()
        {
            if (GUILayout.Button("AssignAll"))
            {
                ((RemoteConfig)target).AssignRemoteSettings();
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
            }
        }
    }
}
