using UnityEngine;
using UnityEngine.UI;

namespace GeniesGambit.UI
{
    public class QuizUI : MonoBehaviour
    {
        [Header("Answer Buttons")]
        [SerializeField] Button answer1Button;
        [SerializeField] Button answer2Button;
        [SerializeField] Button answer3Button;

        [Header("Which Answer is Correct? (1, 2, or 3)")]
        [SerializeField] int correctAnswerIndex = 1;

        public System.Action OnCorrectAnswer;
        public System.Action OnWrongAnswer;

        void Awake()
        {
            if (answer1Button != null)
                answer1Button.onClick.AddListener(() => CheckAnswer(1));
            
            if (answer2Button != null)
                answer2Button.onClick.AddListener(() => CheckAnswer(2));
            
            if (answer3Button != null)
                answer3Button.onClick.AddListener(() => CheckAnswer(3));
        }

        void CheckAnswer(int selectedIndex)
        {
            if (selectedIndex == correctAnswerIndex)
            {
                Debug.Log("[QuizUI] Correct answer!");
                OnCorrectAnswer?.Invoke();
            }
            else
            {
                Debug.Log("[QuizUI] Wrong answer!");
                OnWrongAnswer?.Invoke();
            }
        }

        public void Show()
        {
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        void OnDestroy()
        {
            if (answer1Button != null)
                answer1Button.onClick.RemoveAllListeners();
            
            if (answer2Button != null)
                answer2Button.onClick.RemoveAllListeners();
            
            if (answer3Button != null)
                answer3Button.onClick.RemoveAllListeners();
        }
    }
}
