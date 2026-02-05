using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DifficultySelectionGUI : MonoBehaviour
{
    [SerializeField, NonReorderable] private List<DifficultySelectionButton> _buttons = new List<DifficultySelectionButton>();

    private void Awake()
    {
        foreach (var difficultySelectionButton in _buttons)
        {
            difficultySelectionButton.Button.onClick.AddListener(()=> OnDifficultyButtonClicked(difficultySelectionButton));
        }
    }

    private void Start()
    {
        OnDifficultyButtonClicked(_buttons[0]);
    }

    private void OnDifficultyButtonClicked(DifficultySelectionButton difficultySelectionButton)
    {
        difficultySelectionButton.Button.interactable = false;
        RuntimeServices.GameModeService.AIDifficulty = difficultySelectionButton.Difficulty;

        foreach (var button in _buttons)
        {
            if(button !=  difficultySelectionButton)
                button.Button.interactable = true;
        }
    }
}

[System.Serializable]
public class DifficultySelectionButton
{
    public Button Button;
    public AIDifficulty Difficulty;
}
