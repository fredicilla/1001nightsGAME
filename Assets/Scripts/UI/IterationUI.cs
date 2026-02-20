using UnityEngine;
using TMPro;
using GeniesGambit.Core;

namespace GeniesGambit.UI
{
    public class IterationUI : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI iterationText;
        [SerializeField] TextMeshProUGUI roleText;

        void Update()
        {
            if (IterationManager.Instance == null)
            {
                if (iterationText != null)
                    iterationText.text = "";
                if (roleText != null)
                    roleText.text = "";
                return;
            }

            int iteration = IterationManager.Instance.CurrentIteration;

            if (iterationText != null)
            {
                iterationText.text = $"Iteration: {iteration}/3";
            }

            if (roleText != null)
            {
                if (iteration == 1)
                    roleText.text = "You control: HERO - Reach the flag!";
                else
                    roleText.text = "You control: ENEMY - Catch the ghost!";
            }
        }
    }
}
