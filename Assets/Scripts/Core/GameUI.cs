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
        [SerializeField] private TMPro.TMP_Text scoreTxt;

        [Header("Script References")]
        [SerializeField] private GameLogic localGameLogic;

        private void OnEnable()
        {
            localGameLogic.OnQuestionAnswered += RestartTimer;
            localGameLogic.OnGameOver += ShowGameOverUI;
        }

        private void OnDisable()
        {
            localGameLogic.OnQuestionAnswered -= RestartTimer;
            localGameLogic.OnGameOver -= ShowGameOverUI;
        }

        // Start is called before the first frame update
        void Start()
        {
            timer.maxValue = localGameLogic.totalTime;
            timer.value = localGameLogic.totalTime;
            //InvokeRepeating(nameof(CountDown), 0f, 1f);
        }

        private void RestartTimer()
        {
            StopCoroutine(timerCoroutine);
            timer.value = localGameLogic.totalTime;
            timerCoroutine = StartCoroutine(CountDown());
        }

        private IEnumerator CountDown()
        {
            Debug.Log("Beforeb Timer has reached zero");
            float t = 0;
            float totalTime = localGameLogic.totalTime;

            while(t <= totalTime)
            {
                t += Time.unscaledDeltaTime;

                timer.value = Mathf.Lerp(totalTime, 0, t / totalTime);
                yield return null;
            }

            localGameLogic.OnTimerOver?.Invoke();
            Debug.Log("Timer has reached zero");
        }

        private void ShowGameOverUI()
        {
            StopCoroutine(timerCoroutine);
            gameOverPanel.SetActive(true);
            scoreTxt.text = localGameLogic.score.ToString() + "/10";
        }

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
    }
}