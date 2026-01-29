using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class ShootScoreGUI : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private TMP_Text _shootScoreText;
    [SerializeField] private string _scoreTextFormat = "{0} pts!";
    [SerializeField] private float _messageDuration;

    [Header("Animation")]
    [SerializeField] private float _punchAnimationDuration;
    [SerializeField] private float _punchAnimationScale;
    [SerializeField] private int _punchAnimationVibrato;
    [SerializeField] private int _punchAnimationElasticity;
    
    private void Awake()
    {
        GameModeEvents.OnShootScore += OnShootScore;
    }

    private void Start()
    {
        SetText("");
    }

    private void OnDestroy()
    {
        GameModeEvents.OnShootScore -= OnShootScore;
    }

    private void OnShootScore(int score)
    {
        StartCoroutine(ShowScoreText(string.Format(_scoreTextFormat, score)));
    }

    private IEnumerator ShowScoreText(string scoreText)
    {
        SetText(scoreText);
        Punch();
     
        yield return new WaitForSeconds(_messageDuration);

        SetText("");
    }

    private void SetText(string text)
    {
        _shootScoreText.text = text;
    }

    [Button]
    public void Punch()
    {
        _shootScoreText.transform.DOPunchScale(_shootScoreText.transform.localScale * _punchAnimationDuration, _punchAnimationDuration, _punchAnimationVibrato, _punchAnimationElasticity);
    }
}
