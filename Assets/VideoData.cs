using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

[CreateAssetMenu(fileName = "NewVideoData", menuName = "Video/VideoData")]
public class VideoData : ScriptableObject
{
    public string videoName; // Nom de la vidéo
    public VideoClip videoClip; // Vidéo principale
    public float choiceTime; // Temps d'apparition des choix (en secondes)

    public float trimStartSeconds = 0f; // Nombre de secondes à couper au début
    public float trimEndSeconds = 0f; // Nombre de secondes à couper à la fin
    public bool pivot;
    public bool IsEndVideo = false;
    public VideoData simpleNextVideo; // Vidéo à jouer automatiquement si aucun choix n'est disponible

    public List<string> variablesToAdd; // Variables à ajouter lorsque cette vidéo est jouée
    public List<string> variablesToRemove; // Variables à supprimer lorsque cette vidéo est jouée
    
    public List<QteData> qtes;

    [System.Serializable]
    public class VideoChoice
    {
        public string choiceText; // Texte affiché pour ce choix
        public VideoData nextVideo; // Vidéo normale à jouer (optionnelle)
        public List<ConditionalVideo> conditionalVideos; // Liste des vidéos conditionnelles

        public string eventToAdd;
        public List<string> conditionEvent;
        public int emotionScore;
    }

    [System.Serializable]
    public class ConditionalVideo
    {
        public string requiredVariable; // Variable nécessaire pour jouer cette vidéo
        public VideoData videoToPlay; // Vidéo à jouer si la variable est présente
    }

    public List<VideoChoice> choices; // Liste des choix disponibles
}
