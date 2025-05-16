using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class QteData
{
    public float time;

    public float duration;
    public string expectedDirection;
    public QteType type;
    public int buttonsToShow;
    public bool played = false;
}

public enum QteType{
    BUTTON, SWIPE
}
