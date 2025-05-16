using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class QTEManager : MonoBehaviour
{
    public Button[] qteButtons;
    public float timeLimit = 1.5f;
    public float swipeTimeLimit = 2f; // Durée spécifique pour le swipe QTE

    private float timer;
    private float swipeTimer;
    private float timeSinceQTEStart;
    private float qteStartDelay = 0.1f;

    private bool qteActive = false;
    private bool qteSwipeActive = false;
    private bool qteInProgress = false;

    private List<Button> activeButtons = new List<Button>();

    public GameObject successPanel;
    public GameObject failPanel;
    public TextMeshProUGUI instructionText;

    private Vector2 startTouchPos;
    private string expectedDirection = "Right";

    public GameObject leftSwipe;
    public GameObject rightSwipe;
    public GameObject upSwipe;
    public GameObject downSwipe;


    public EmotionManager emotionManager;

    void Update()
    {
        if (qteActive && qteInProgress)
        {
            timer -= Time.deltaTime;
            if (timer <= 0)
            {
                ResultQTE(false);
            }
        }

        ShowTimerQTE();

        if (qteSwipeActive && qteInProgress)
        {
            swipeTimer -= Time.deltaTime;
            timeSinceQTEStart += Time.deltaTime;

            if (swipeTimer <= 0)
            {
                ResultQTE(false);
                return;
            }

            if (timeSinceQTEStart < qteStartDelay)
                return;

            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);

                if (touch.phase == TouchPhase.Began)
                    startTouchPos = touch.position;

                else if (touch.phase == TouchPhase.Ended)
                {
                    Vector2 endTouchPos = touch.position;
                    Vector2 swipe = endTouchPos - startTouchPos;

                    if (swipe.magnitude > 50f)
                    {
                        string direction = GetSwipeDirection(swipe);
                        ResultQTE(direction == expectedDirection);
                    }
                }
            }

#if UNITY_EDITOR
            if (Input.GetMouseButtonDown(0))
                startTouchPos = Input.mousePosition;

            if (Input.GetMouseButtonUp(0))
            {
                Vector2 endTouchPos = Input.mousePosition;
                Vector2 swipe = endTouchPos - startTouchPos;

                if (swipe.magnitude > 50f)
                {
                    string direction = GetSwipeDirection(swipe);
                    ResultQTE(direction == expectedDirection);
                }
            }
#endif
        }
    }

    public void DisplayQTE(QteData qte)
    {
        if (qte.type == QteType.BUTTON)
        {
            StartQTE(qte);
        }
        else
        {
            // Utilise la durée personnalisée si définie, sinon fallback sur swipeTimeLimit
            float duration = qte.duration > 0 ? qte.duration : swipeTimeLimit;
            StartSwipeQTE(qte.expectedDirection, duration);
        }
    }

    public void StartQTE(QteData qte)
    {
        qteActive = true;
        qteSwipeActive = false;
        qteInProgress = true;
        timer = timeLimit;

        activeButtons.Clear();

        int buttonsToShow = qte.buttonsToShow;
        List<int> indices = new List<int>();

        while (indices.Count < buttonsToShow)
        {
            int index = Random.Range(0, qteButtons.Length);
            if (!indices.Contains(index))
                indices.Add(index);
        }

        foreach (Button btn in qteButtons)
        {
            btn.gameObject.SetActive(false);
            btn.onClick.RemoveAllListeners();
        }

        foreach (int i in indices)
        {
            Button btn = qteButtons[i];
            btn.gameObject.SetActive(true);
            btn.GetComponent<Image>().fillAmount = 1f;
            btn.onClick.AddListener(() => ClickBtn(btn));
            activeButtons.Add(btn);
        }

        instructionText.text = "Appuie vite sur un des boutons !";
        instructionText.gameObject.SetActive(true);
        successPanel?.SetActive(false);
        failPanel?.SetActive(false);
    }

    public void ClickBtn(Button btn){
        
        btn.gameObject.SetActive(false);
        foreach (var item in activeButtons)
        {
            if(item.gameObject.active == true)
                return;
        }
        ResultQTE(true);
    }
    public void StartSwipeQTE(string direction, float duration)
    {
        SetExpectedDirection(direction);
        LaunchSwipeQTE(duration);
    }

    public void LaunchSwipeQTE(float duration)
    {
        qteActive = false;
        qteSwipeActive = true;
        qteInProgress = true;
        swipeTimer = duration;
        timeSinceQTEStart = 0f;

        instructionText.text = $"Fais un swipe vers {expectedDirection} !";
        if(expectedDirection == "Left"){
            leftSwipe.SetActive(true);
        }
        if(expectedDirection == "Right"){
            rightSwipe.SetActive(true);
        }
        if(expectedDirection == "Up"){
            upSwipe.SetActive(true);
        }
        if(expectedDirection == "Down"){
            downSwipe.SetActive(true);
        }

        instructionText.gameObject.SetActive(true);
        successPanel?.SetActive(false);
        failPanel?.SetActive(false);

        foreach (Button btn in qteButtons)
            btn.gameObject.SetActive(false);
    }

    private void ResultQTE(bool result)
    {
        if (!qteInProgress) return;

        qteInProgress = false;
        qteActive = false;
        qteSwipeActive = false;

        instructionText.text = result ? "Bravo !" : "Trop tard...";
        instructionText.gameObject.SetActive(true);
        successPanel?.SetActive(result);
        failPanel?.SetActive(!result);
        if(result){
            emotionManager.UpEmotion(1);
        }else{
            emotionManager.UpEmotion(-1);
        }

        foreach (var btn in activeButtons)
            btn.onClick.RemoveAllListeners();

        activeButtons.Clear();

        StartCoroutine(hidePanelQTE());
    }

    IEnumerator hidePanelQTE()
    {
        foreach (var item in qteButtons)
        {
            item.gameObject.SetActive(false);
        }

        leftSwipe.SetActive(false);
        rightSwipe.SetActive(false);
        upSwipe.SetActive(false);
        downSwipe.SetActive(false);
        instructionText.gameObject.SetActive(false);

        yield return new WaitForSeconds(2f);
        successPanel?.SetActive(false);
        failPanel?.SetActive(false);
    }

    public void ShowTimerQTE()
    {
        foreach (var btn in activeButtons)
        {
            btn.GetComponent<Image>().fillAmount -= Time.deltaTime / timeLimit;
        }
    }

    private string GetSwipeDirection(Vector2 swipe)
    {
        if (Mathf.Abs(swipe.x) > Mathf.Abs(swipe.y))
            return swipe.x > 0 ? "Right" : "Left";
        else
            return swipe.y > 0 ? "Up" : "Down";
    }

    public void SetExpectedDirection(string dir)
    {
        expectedDirection = dir;
    }

    public void Reset(VideoData videoData)
    {
        foreach (var qte in videoData.qtes)
        {
            qte.played = false;
        }
        activeButtons = new List<Button>();
    }
}
