using UnityEngine;

namespace GeniesGambit.Level
{
    [CreateAssetMenu(
        fileName = "CaptchaData_New",
        menuName = "GeniesGambit/Captcha Data",
        order = 2)]
    public class CaptchaData : ScriptableObject
    {
        [Header("Challenge Settings")]
        [TextArea(1, 2)]
        public string promptText = "Select all images with coins";

        [Header("Images")]
        public Sprite[] allImages;
        
        [Header("Correct Answers")]
        [Tooltip("Indices of correct images in the allImages array")]
        public int[] correctIndices;

        [Header("Difficulty")]
        [Range(2, 5)]
        public int gridSize = 3;
        
        [Range(10f, 30f)]
        public float timeLimit = 20f;

        public bool ValidateAnswer(int[] selectedIndices)
        {
            if (selectedIndices.Length != correctIndices.Length)
                return false;

            System.Array.Sort(selectedIndices);
            System.Array.Sort(correctIndices);

            for (int i = 0; i < correctIndices.Length; i++)
            {
                if (selectedIndices[i] != correctIndices[i])
                    return false;
            }

            return true;
        }
    }
}
