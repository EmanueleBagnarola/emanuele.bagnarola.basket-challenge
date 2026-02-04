using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class UIEvents
{
    public delegate void OnShowNotificationHandler(NotificationType notificationType);
    public static event OnShowNotificationHandler OnShowNotification;

    public static void TriggerShowNotification(NotificationType notificationType)
    {
        OnShowNotification?.Invoke(notificationType);
    }
}
