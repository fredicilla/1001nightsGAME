using UnityEditor;
using UnityEngine;

namespace BossFight.Editor
{
    [InitializeOnLoad]
    public static class TagCreator
    {
        static TagCreator()
        {
            CreateTag("Projectile");
            CreateTag("Monster");
        }

        private static void CreateTag(string tagName)
        {
            SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            SerializedProperty tagsProp = tagManager.FindProperty("tags");

            if (tagsProp == null)
            {
                Debug.LogError("Could not find 'tags' property in TagManager.asset.");
                return;
            }

            bool found = false;
            for (int i = 0; i < tagsProp.arraySize; i++)
            {
                SerializedProperty t = tagsProp.GetArrayElementAtIndex(i);
                if (t.stringValue.Equals(tagName))
                {
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                tagsProp.InsertArrayElementAtIndex(tagsProp.arraySize);
                SerializedProperty newTag = tagsProp.GetArrayElementAtIndex(tagsProp.arraySize - 1);
                newTag.stringValue = tagName;
                tagManager.ApplyModifiedPropertiesWithoutUndo();
                Debug.Log($"Tag '{tagName}' created successfully.");
            }
        }
    }
}
