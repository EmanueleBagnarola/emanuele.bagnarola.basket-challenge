using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameModeUIManager : MonoBehaviour
{
    [SerializeField] private NotificationsSettings _notificationsSettings;

    private void Awake()
    {
        UIServices.NotificationSettings = _notificationsSettings;
    }
}
