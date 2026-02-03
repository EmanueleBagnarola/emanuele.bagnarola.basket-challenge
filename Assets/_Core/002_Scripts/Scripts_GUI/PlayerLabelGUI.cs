using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerLabelGUI : MonoBehaviour
{
    [SerializeField] private TMP_Text scoreText;

    private void Awake()
    {
        UpdateScore(0);
    }

    public void UpdateScore(int score)
    {
        scoreText.text = score.ToString();
    }
}
