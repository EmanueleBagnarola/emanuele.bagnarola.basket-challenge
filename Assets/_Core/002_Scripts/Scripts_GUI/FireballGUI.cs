using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FireballGUI : MonoBehaviour
{
    [SerializeField] private Slider _fireballBarSlider;
    [SerializeField] private TMP_Text _multiplierText;
    [SerializeField] private bool _isHumanPlayer;

    [Header("Color config")]
    [SerializeField, NonReorderable] private List<FireballGUIElementConfig> _fireballGUIElementConfigs =  new List<FireballGUIElementConfig>();

    private void Awake()
    {
        GameModeEvents.OnUpdateFireballScore += OnUpdateFireballScore;
        GameModeEvents.OnSetFireballScoreActive += SetFireballScoreActive;
    }

    private void Start()
    {
        InitSlider();
        ActivateGraphicElements(false);

        _multiplierText.text = $"x{RuntimeServices.GameModeService.GameModeSettings.FireballScoreMultiplier}";
    }
    
    private void OnDestroy()
    {
        GameModeEvents.OnUpdateFireballScore -= OnUpdateFireballScore;
        GameModeEvents.OnSetFireballScoreActive -= SetFireballScoreActive;
    }

    private void InitSlider()
    {
        _fireballBarSlider.maxValue = RuntimeServices.GameModeService.GameModeSettings.FireballMaxScore;
        _fireballBarSlider.value = 0;
    }

    private void OnUpdateFireballScore(float fireballScore, bool isHumanPlayer)
    {
        if(_isHumanPlayer != isHumanPlayer)
            return;
        
        DOVirtual.Float(
            _fireballBarSlider.value, 
            fireballScore, 
            0.1f,
            (v) =>
            {
                _fireballBarSlider.value = v;
            });
    }

    private void SetFireballScoreActive(bool active, bool isHumanPlayer)
    {
        if(_isHumanPlayer != isHumanPlayer)
            return;
        
        ActivateGraphicElements(active);
    }

    private void ActivateGraphicElements(bool active)
    {
        foreach (var elementConfig in _fireballGUIElementConfigs)
        {
            elementConfig.SetActive(active);
        }
    }
}

[System.Serializable]
public class FireballGUIElementConfig
{
    public MaskableGraphic GraphicElement;
    public Color DefaultColor;
    public Color ActiveColor;

    public void SetActive(bool active)
    {
        GraphicElement.color = active ? ActiveColor : DefaultColor;
    }
}
