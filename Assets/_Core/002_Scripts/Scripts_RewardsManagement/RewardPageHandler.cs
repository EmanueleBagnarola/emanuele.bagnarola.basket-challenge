using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RewardPageHandler : MonoBehaviour
{
    [SerializeField] private List<GameObject> _elementsToHideOnRewardsPage = new List<GameObject>();
    [SerializeField] private GameObject _rewardPageMainParent;

    private void Awake()
    {
        GameModeEvents.OnShowRewards += ShowRewardsPage;
        ShowGameModeElements(true);
    }

    private void OnDestroy()
    {
        GameModeEvents.OnShowRewards -= ShowRewardsPage;
    }

    private void ShowRewardsPage()
    {
        // Hide all gamemode elements
        ShowGameModeElements(false);
    }

    private void ShowGameModeElements(bool show)
    {
        foreach (var element in _elementsToHideOnRewardsPage)
        {
            element.SetActive(show);
        }
        
        _rewardPageMainParent.SetActive(!show);
    }
} 
