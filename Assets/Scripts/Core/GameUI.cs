using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Trivia_Swipe
{
    public class GameUI : MonoBehaviour
    {
        private Coroutine timerCoroutine;

        [Header("UI")]
        [SerializeField] private Slider timer;
        [SerializeField] private GameObject gameOverPanel, mainMenuPanel, gameplayPanel;
        [SerializeField] private TMPro.TMP_Text scoreTxt, answerStatusTxt;

        [Header("Script References")]
        [SerializeField] private GameLogic localGameLogic;

        private void OnEnable()
        {
            localGameLogic.OnNextQuestion += RestartTimer;
            localGameLogic.OnGameOver += ShowGameOverUI;
            localGameLogic.OnQuestionAnswered += ShowAnswerStatus;
        }

        private void OnDisable()
        {
            localGameLogic.OnNextQuestion -= RestartTimer;
            localGameLogic.OnGameOver -= ShowGameOverUI;
            localGameLogic.OnQuestionAnswered -= ShowAnswerStatus;
        }

        // Start is called before the first frame update
        void Start()
        {
            timer.maxValue = localGameLogic.totalTime;
            timer.value = localGameLogic.totalTime;
            //InvokeRepeating(nameof(CountDown), 0f, 1f);
        }

        #region Timer
        private void RestartTimer()
        {
            //StopCoroutine(timerCoroutine);
            timer.value = localGameLogic.totalTime;
            timerCoroutine = StartCoroutine(CountDown());
        }

        private IEnumerator CountDown()
        {
            float t = 0;
            float totalTime = localGameLogic.totalTime;

            while(t <= totalTime)
            {
                t += Time.unscaledDeltaTime;

                timer.value = Mathf.Lerp(totalTime, 0, t / totalTime);
                yield return null;
            }

            localGameLogic.OnTimerOver?.Invoke(true);
            //Debug.Log("Timer has reached zero");
        }
        #endregion Timer

        private void ShowGameOverUI()
        {
            StopCoroutine(timerCoroutine);
            gameOverPanel.SetActive(true);
            scoreTxt.text = localGameLogic.score.ToString() + "/10";
        }

        private void ShowAnswerStatus(bool status)
        {
            StopCoroutine(timerCoroutine);
            CancelInvoke(nameof(AnswerStatusToDefault));            //If the player swipes early
            AnswerStatusToDefault();

            if (status)
            {
                answerStatusTxt.text = "CORRECT";
                answerStatusTxt.color = Color.green;
            }
            else
            {
                answerStatusTxt.text = "WRONG";
                answerStatusTxt.color = Color.red;
            }

            Invoke(nameof(AnswerStatusToDefault), 1.5f);
        }

        private void AnswerStatusToDefault()
        {
            answerStatusTxt.text = "";
            answerStatusTxt.color = Color.grey;
        }

        #region Buttons
        //On the Restart button,under StatsCard/GameOverPanel/GameplayCanvas
        public void RestartGame()
        {
            localGameLogic.OnGameReset?.Invoke();
            gameOverPanel.SetActive(false);
            RestartTimer();
        }

        //On the Exit button,under MainMenuCanvas
        public void ExitGame()
        {
            Application.Quit();
        }

        //On the Play button,under MainMenuCanvas
        public void StartGame()
        {
            mainMenuPanel.SetActive(false);
            gameplayPanel.SetActive(true);

            localGameLogic.OnGameStart?.Invoke();
            timerCoroutine = StartCoroutine(CountDown());
        }
        #endregion Buttons
    }
}