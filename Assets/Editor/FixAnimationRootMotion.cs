using UnityEngine;
using UnityEditor;

[InitializeOnLoad]
public class FixAnimationRootMotion
{
    static FixAnimationRootMotion()
    {
        EditorApplication.delayCall += FixAllAnimations;
    }

    [MenuItem("Tools/Fix Animation Root Motion")]
    static void FixAllAnimations()
    {
        string[] animationPaths = new string[]
        {
            "Assets/art 3d/Characters/alaa/Neutral Idle.fbx",
            "Assets/art 3d/Characters/alaa/Jump.fbx",
            "Assets/art 3d/Characters/alaa/Running.fbx",
            "Assets/art 3d/Characters/alaa/Throw.fbx"
        };

        bool anyFixed = false;

        foreach (string path in animationPaths)
        {
            ModelImporter importer = AssetImporter.GetAtPath(path) as ModelImporter;
            if (importer == null) continue;

            ModelImporterClipAnimation[] clips = importer.clipAnimations;
            
            if (clips == null || clips.Length == 0)
            {
                clips = importer.defaultClipAnimations;
            }

            if (clips != null && clips.Length > 0)
            {
                bool needsUpdate = false;
                
                for (int i = 0; i < clips.Length; i++)
                {
                    if (!clips[i].lockRootPositionXZ || clips[i].keepOriginalPositionXZ)
                    {
                        clips[i].lockRootPositionXZ = true;
                        clips[i].keepOriginalPositionXZ = false;
                        clips[i].lockRootHeightY = false;
                        clips[i].keepOriginalPositionY = true;
                        clips[i].lockRootRotation = false;
                        clips[i].keepOriginalOrientation = false;
                        needsUpdate = true;
                    }
                }

                if (needsUpdate)
                {
                    importer.clipAnimations = clips;
                    importer.SaveAndReimport();
                    Debug.Log($"✅ Fixed root motion for: {path}");
                    anyFixed = true;
                }
            }
        }

        if (anyFixed)
        {
            Debug.Log("🎉 All animations fixed!");
        }
    }
}
