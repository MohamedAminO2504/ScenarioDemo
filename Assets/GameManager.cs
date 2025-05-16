using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;
using TMPro;

public class VideoManager : MonoBehaviour
{

    public bool isWeb;

    public VideoPlayer videoPlayer;
    public VideoData currentVideo;

    [Header("UI Elements")]
    public GameObject choicesPanel;
    public Button[] choiceButtons;

    public EmotionManager emotionManager;

    private static HashSet<string> gameVariables = new HashSet<string>();
    private VideoData nextVideoToPlay = null;

    public List<string> currentEvent;

    public bool canBeShow = true;

    public Image progressionImg;

    private bool displayProgression;

    public float progress = 0;

    public bool choiceAlreadyShowed;

    public QTEManager qTEManager;

    void Start()
    {
        videoPlayer.loopPointReached += OnVideoFinished;
        if (currentVideo != null)
        {
            PlayVideo(currentVideo);
        }
    }


    public void DisplayProgression(){
        displayProgression = true;
        progress = 10f;
    }

    public void PlayVideo(VideoData videoData)
    {
        canBeShow = true;
        choiceAlreadyShowed = false;
        qTEManager.Reset(videoData);
        if (!videoData.pivot)
        {
            StartCoroutine(PlayWithTrim(videoData));
        }
        else
        {
            int indexAleatoire = Random.Range(0, videoData.choices.Count);
            VideoData video = videoData.choices[indexAleatoire].nextVideo;
            StartCoroutine(PlayWithTrim(video));
        }
    }

    public VideoData Pivot(VideoData videoData)
    {
        Debug.Log("is Pivot");
        List<VideoData.VideoChoice> choixPossible = new List<VideoData.VideoChoice>();
        for (int i = 0; i < videoData.choices.Count; i++){
            bool toAdd = true;
            if (videoData.choices[i].conditionEvent.Count == 0){
                choixPossible.Add(videoData.choices[i]);
            }
            foreach (string e in videoData.choices[i].conditionEvent){
                string[] tableau = e.Split(',');
                toAdd = true;
                foreach (string t in tableau)
                {
                    if (!currentEvent.Contains(t)){
                        toAdd = false;
                    }
                }
                if (toAdd){
                    choixPossible.Add(videoData.choices[i]);
                    continue;
                }
            }
        }
        foreach (var item in choixPossible)
        {
                    Debug.Log(item.nextVideo.name);

        }
        Debug.Log("");
        int indexAleatoire = Random.Range(0, choixPossible.Count);
        VideoData video = choixPossible[indexAleatoire].nextVideo;
        Debug.Log(video.name);
        return video;
    }

    IEnumerator PlayWithTrim(VideoData videoData)
    {

        currentVideo = videoData;

        string url = Application.streamingAssetsPath + "/AI/"+currentVideo.videoClip.name+".mp4";
            videoPlayer.clip = currentVideo.videoClip;

        if (isWeb)
        {
            videoPlayer.url = url;
        }
        


        // Attendre que la vidéo soit prête avant de la lancer
        videoPlayer.Prepare();
        while (!videoPlayer.isPrepared) yield return null;

        // Définir le point de départ avec un cast en float
        videoPlayer.time = Mathf.Clamp(currentVideo.trimStartSeconds, 0, (float)videoPlayer.clip.length);
        videoPlayer.Play();

        // Calculer la durée restante avant de couper la vidéo avec un cast en float
        float duration = Mathf.Max(0, (float)videoPlayer.clip.length - currentVideo.trimStartSeconds - currentVideo.trimEndSeconds);

        // Ajouter les variables de cette vidéo
        foreach (string variable in videoData.variablesToAdd) gameVariables.Add(variable);
        foreach (string variable in videoData.variablesToRemove) gameVariables.Remove(variable);

        // Cacher les choix au début
        choicesPanel.SetActive(false);
        UpdateChoiceButtons();

        // Attendre la fin de la vidéo après le temps ajusté
        yield return new WaitForSeconds(duration);

        // Déclencher la fin manuellement si la vidéo est toujours en cours
        if (videoPlayer.isPlaying)
        {
            videoPlayer.Stop();
            OnVideoFinished(videoPlayer);
        }
    }


    void Update()
    {
        // Vérifie si la vidéo atteint le moment où afficher les choix
        if (!choiceAlreadyShowed && videoPlayer.isPlaying && videoPlayer.time >= currentVideo.choiceTime)
        {
            ShowChoices();
        }
         if(displayProgression){
            progress -= Time.deltaTime;
            progressionImg.fillAmount = (1f) - (progress / 10f);
            if(progress < 0){
                displayProgression = false;
                choicesPanel.SetActive(false);
                choiceAlreadyShowed = true;
                 progressionImg.fillAmount = 0;
            }
        }
        ShowQTE();
    }

    public void ShowQTE(){
        foreach (var qte in currentVideo.qtes)
        {
            if(!qte.played && videoPlayer.isPlaying && videoPlayer.time >= qte.time){
                qte.played = true;
                qTEManager.DisplayQTE(qte);
            }
        }
    }

    void OnVideoFinished(VideoPlayer vp)
    {
        if(currentVideo.IsEndVideo){
            Application.Quit();
        }
        if (nextVideoToPlay != null)
        {
            PlayVideo(nextVideoToPlay);
            nextVideoToPlay = null;
        }
        else if (currentVideo.simpleNextVideo != null)
        {
            // Si aucun choix n'est disponible, jouer la vidéo suivante définie dans simpleNextVideo
            PlayVideo(currentVideo.simpleNextVideo);
        }
    }

    // Met à jour les boutons avec les nouveaux choix
    public void UpdateChoiceButtons()
    {
        // Désactiver tous les boutons au cas où il y a moins de 3 choix
        foreach (Button btn in choiceButtons)
        {
            btn.gameObject.SetActive(false);
        }

        List<VideoData.VideoChoice> choixPossible = new List<VideoData.VideoChoice>();

        if (currentVideo.choices.Count > 2)
        {
            for (int i = 0; i < currentVideo.choices.Count; i++)
            {
                bool toAdd = true;
                if (currentVideo.choices[i].conditionEvent.Count == 0)
                {
                    choixPossible.Add(currentVideo.choices[i]);
                }
                foreach (string e in currentVideo.choices[i].conditionEvent)
                {
                    string[] tableau = e.Split(',');
                    toAdd = true;
                    foreach (string t in tableau)
                    {
                        if (!currentEvent.Contains(t))
                        {
                            toAdd = false;
                        }
                    }
                    if (toAdd)
                    {
                        choixPossible.Add(currentVideo.choices[i]);
                        continue;
                    }
                }
            }
        }
        else
        {
            choixPossible = new List<VideoData.VideoChoice>(currentVideo.choices);
        }

        for (int i = 0; i < choixPossible.Count; i++)
        {
            choiceButtons[i].gameObject.SetActive(true); // Activer le bouton

            // Récupérer TMP_Text au lieu de Text
            TMP_Text buttonText = choiceButtons[i].GetComponentInChildren<TMP_Text>();
            if (buttonText != null)
            {
                buttonText.text = choixPossible[i].choiceText; // Mettre à jour le texte
            }

            VideoData.VideoChoice choice = choixPossible[i];
            choiceButtons[i].onClick.RemoveAllListeners(); // Nettoyer les anciens events
            choiceButtons[i].onClick.AddListener(() => OnChoiceSelected(choice));
            if (i > 1)
                continue;
        }
    }

    // Afficher le panel des choix si des choix sont disponibles
    void ShowChoices()
    {
        if (choicesPanel.activeSelf) return; // Ne pas afficher plusieurs fois

        if (canBeShow && currentVideo.choices.Count > 0)
        {
            choicesPanel.SetActive(true);
            DisplayProgression();
        }
    }

    // Gérer le choix sélectionné (stocke la prochaine vidéo mais ne la joue pas tout de suite)
    void OnChoiceSelected(VideoData.VideoChoice choice)
    {
        Debug.Log("Choix sélectionné : " + choice.choiceText);
        canBeShow = false;
        choicesPanel.SetActive(false);
        currentEvent.Add(choice.eventToAdd);
        emotionManager.UpEmotion(choice.emotionScore);
        // Récupérer la vidéo à jouer en fonction des variables
        nextVideoToPlay = GetNextVideo(choice);
    }

    // Vérifie quelle vidéo doit être jouée en fonction des variables globales
    VideoData GetNextVideo(VideoData.VideoChoice choice)
    {
        // Vérifie si une vidéo conditionnelle correspond aux variables existantes
        foreach (VideoData.ConditionalVideo conditionalVideo in choice.conditionalVideos)
        {
            if (gameVariables.Contains(conditionalVideo.requiredVariable))
            {
                Debug.Log("Vidéo conditionnelle jouée : " + conditionalVideo.videoToPlay.videoName);
                return conditionalVideo.videoToPlay;
            }
        }

        // Sinon, retourne la vidéo par défaut
        return choice.nextVideo;
    }


}
