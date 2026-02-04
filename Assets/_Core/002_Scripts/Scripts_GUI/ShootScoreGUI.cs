using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class ShootScoreGUI : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private TMP_Text _playerShootScoreText;
    [SerializeField] private TMP_Text _aiShootScoreText;
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
        SetText("", true);
        SetText("", false);
    }

    private void OnDestroy()
    {
        GameModeEvents.OnShootScore -= OnShootScore;
    }

    private void OnShootScore(ShootResult result, int score)
    {
        if (score <= 0)
            return;
        
        StartCoroutine(ShowScoreText(string.Format(_scoreTextFormat, score), result.IsHumanPlayer));
    }

    private IEnumerator ShowScoreText(string scoreText, bool isHumanPlayer)
    {
        SetText(scoreText, isHumanPlayer);
     
        yield return new WaitForSeconds(_messageDuration);

        SetText("", isHumanPlayer);
    }

    private void SetText(string text, bool isHumanPlayer)
    {
        if (isHumanPlayer)
        {
            _playerShootScoreText.text = text;
            Punch(_playerShootScoreText.transform);
        }
        else
        {
            _aiShootScoreText.text = text;
            Punch(_aiShootScoreText.transform);
        }
        
    }

    [Button]
    public void Punch(Transform textComponentTransform)
    {
        textComponentTransform.transform.DOPunchScale(textComponentTransform.transform.localScale * _punchAnimationDuration, _punchAnimationDuration, _punchAnimationVibrato, _punchAnimationElasticity);
    }
}
