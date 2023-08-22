using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

namespace Trivia_Swipe
{
    public class GameLogic : MonoBehaviour
    {
        public byte score;
        public float totalTime;
        private bool gameStarted;

        [Header("Touch Controls")]
        [SerializeField] private float minDiff;
        private Vector3 startPoint, touchPoint, offset;     //, endPoint
        private bool touchedScreen;  //For test Serialized, disableTouch
        //[SerializeField] private Vector3 shrinkSize;      //(3.6f, 5.5f, 1f)

        [Header("Card Controls")]
        [SerializeField] private GameObject cardHolder;
        [SerializeField] private float cardSpeedMultiplier;
        [SerializeField] private Transform[] cards;
        private byte currentCard = 0, cardCount = 0, totalCardCount = 10;
        [SerializeField] private LayerMask raycastLayerMask;
        [SerializeField] private string[] questions;
        [SerializeField] private bool[] responses;

        [Header("Actions")]
        public Action OnTimerOver, OnQuestionAnswered, OnGameOver, OnGameReset, OnGameStart;

        private void OnEnable()
        {
            EnhancedTouchSupport.Enable();
            OnGameReset += ResetLogic;
            OnTimerOver += MoveCardIfNoResponse;
            OnGameStart += StartGame;
        }

        private void OnDisable()
        {
            EnhancedTouchSupport.Disable();
            OnGameReset -= ResetLogic;
            OnTimerOver -= MoveCardIfNoResponse;
            OnGameStart -= StartGame;
        }

        private void StartGame()
        {
            cardHolder.SetActive(true);
            Invoke(nameof(turnOnGameStarted), 1f);
        }

        //To prevent touch input while starting game.
        private void turnOnGameStarted()
        {
            gameStarted = true;
        }

        // Update is called once per frame
        private void Update()
        {
            //if (disableTouch)
            //    return;

            if (Touch.activeFingers.Count > 0)
            {
                Touch touch = Touch.activeFingers[0].currentTouch;
                touchPoint = Camera.main.ScreenToWorldPoint(touch.screenPosition);
                //Debug.Log($"touchPoint : {touchPoint}, position : {touch.screenPosition}");

                RaycastHit2D hit = Physics2D.Raycast(touchPoint, touch.screenPosition, 20f, raycastLayerMask);
                if (hit.collider != null)
                {
                    if (!touchedScreen)
                    {
                        //Check Offset of finger and center of card
                        touchedScreen = true;
                        startPoint = touchPoint;
                        startPoint.z = 0f;
                        offset = cards[currentCard].position - startPoint;

                        //We only need to move in the x-direction
                        offset.z = 0f;
                        offset.y = 0f;
                    }

                    touchPoint.z = 0f;
                    touchPoint.y = 0f;
                    cards[currentCard].position = touchPoint + offset;// + new Vector3(0f, 0f, 10f);
                    //Debug.Log($"Hit Something : {hit.collider.name}");
                }
            }
            else
            {
                //endPoint = touchPoint;        //no need, touchPOint will be the last anyways

                //Calculate the diff, if greater than set diff, and the player releases their touch
                //, swipe away the card from screen
                if (touchedScreen && gameStarted)
                {
                    float touchDiff = Vector3.Distance(startPoint, touchPoint);
                    bool leftSwipe = (startPoint.x - touchPoint.x) > 0;
                    if (Mathf.Abs(touchDiff) >= minDiff)
                    {
                        if (leftSwipe == responses[cardCount] || cardCount == 4)
                            score++;
                        cardCount++;

                        //disableTouch = true;
                        if (cardCount < totalCardCount)
                            UpdateQuestionInNextCard();
                        _ = StartCoroutine(MoveCardAwayScreen(leftSwipe));
                        //Debug.Log($"Swiped, diff : {Vector3.Distance(startPoint, touchPoint)}");
                    }
                    else
                        cards[currentCard].position = Vector3.zero;
                }
                touchedScreen = false;
            }
        }

        #region CardTransformManipulation
        private void MoveCardIfNoResponse()
        {
            //Debug.Log("Moving Card No Response");
            cardCount++;

            //disableTouch = true;
            if (cardCount < totalCardCount)
                UpdateQuestionInNextCard();
            _ = StartCoroutine(MoveCardAwayScreen(true));
        }

        private IEnumerator MoveCardAwayScreen(bool leftSWipe)
        {
            float t = 0;
            float xVal = leftSWipe ? -5f : 5f;
            Vector3 finalPos = new Vector3(xVal, 0f, 0f);
            Vector3 startPos = cards[currentCard].position;
            while (t < 1f)             //time taken to move away
            {
                t += cardSpeedMultiplier * Time.deltaTime;

                cards[currentCard].position = Vector3.Lerp(startPos, finalPos, t);
                yield return null;
            }

            if (cardCount < (totalCardCount - 1))
                RepositionCard(currentCard);

            currentCard = (currentCard <= 0) ? ++currentCard : (byte)0;
            //Debug.Log($"Finished Moving Away, currentCard :{currentCard}");

            if (cardCount < totalCardCount)
                _ = StartCoroutine(MoveCardToFront());
            else
                OnGameOver?.Invoke();
        }

        private IEnumerator MoveCardToFront()
        {
            float t = 0;
            Vector3 finalSize = new Vector3(4f, 6f, 1f);
            Vector3 startSize = cards[currentCard].localScale;
            Vector3 startPos = cards[currentCard].position;
            while (t < 1f)             //time taken to move away
            {
                t += (cardSpeedMultiplier / 2) * Time.deltaTime;

                cards[currentCard].localScale = Vector3.Lerp(startSize, finalSize, t);
                cards[currentCard].position = Vector3.Lerp(startPos, Vector3.zero, t);
                yield return null;
            }

            OnQuestionAnswered?.Invoke();
            //Debug.Log("Finished Moving Front");
        }

        private void RepositionCard(int cardIndex)
        {
            cards[cardIndex].position = new Vector3(0f, 0f, 0.5f);
            cards[cardIndex].localScale = new Vector3(3.6f, 5.5f, 1f);
        }
        #endregion CardTransformManipulation

        private void UpdateQuestionInNextCard()
        {
            //Current card should be +1
            byte cardIndex = (currentCard == 0) ? (byte)(currentCard + 1) : (byte)0;

            string question = questions[cardCount];
            question = question.Replace("\\n", "\n");               //To get a new line
            cards[cardIndex].GetChild(0).GetChild(0).GetComponent<TMPro.TMP_Text>().text = question;
        }

        private void ResetLogic()
        {
            cardCount = 0;
            currentCard = 1;            //For card 0
            UpdateQuestionInNextCard();
            currentCard = 0;

            cards[0].position = new Vector3(0f, 0f, 0f);
            cards[0].localScale = new Vector3(4f, 6f, 1f);
            RepositionCard(1);

            touchedScreen = false;
        }
    }
}