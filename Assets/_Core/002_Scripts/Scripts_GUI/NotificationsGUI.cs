using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class NotificationsGUI : MonoBehaviour
{
    [Header("config")]
    [SerializeField] private Transform _notificationsContainer;
    [SerializeField] private GameObject _notificationLabelPrefab;
    
    private void Awake()
    {
        UIEvents.OnShowNotification += OnShowNotification;
    }

    private void OnDestroy()
    {
        UIEvents.OnShowNotification -= OnShowNotification;
    }

    private void OnShowNotification(NotificationType notificationType)
    {
        GameObject notificationLabelObj = Instantiate(_notificationLabelPrefab, _notificationsContainer);
        NotificationLabel notificationLabel = notificationLabelObj.GetComponent<NotificationLabel>();
        if (notificationLabel != null)
        {
            notificationLabel.SetNotificationText(UIServices.NotificationSettings.GetNotificationText(notificationType));
        }
    }

    [Button]
    public void Debug_Show()
    {
        OnShowNotification(NotificationType.FireballActive);
    }
}
