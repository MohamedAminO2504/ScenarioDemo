using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EmotionManager : MonoBehaviour
{
    public int scoreEmotion;
    public TextMeshProUGUI libelle;
    public Color colorTriste;
    public Color colorContent;
    public Color colorNeutre;
    public Color colorDeprime;
    public Color colorHeureu;

    public Image back;

    public void Start(){
        UpdateEmotion();
    }

    public void UpEmotion(int i){
        scoreEmotion += i;
        UpdateEmotion();
    }

    public void UpdateEmotion(){
        if(scoreEmotion < -5){
            libelle.text = "Deprimé";
            back.color = colorDeprime; // application de la couleur

        }else if(scoreEmotion < 0){
            libelle.text = "Triste";
            back.color = colorTriste; // application de la couleur

        }else if(scoreEmotion < 5){
            libelle.text = "Neutre";
            back.color = colorNeutre; // application de la couleur
        }else if(scoreEmotion < 10){
            libelle.text = "Joyeux";
            back.color = colorContent; // application de la couleur
        }else {
            libelle.text = "Heureux";
            back.color = colorHeureu; // application de la couleur
        }
    }
}
