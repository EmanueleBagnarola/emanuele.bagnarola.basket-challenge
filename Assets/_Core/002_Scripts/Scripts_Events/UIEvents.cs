using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class UIEvents
{
    public delegate void OnShowNotificationHandler(NotificationType notificationType);
    public static event OnShowNotificationHandler OnShowNotification;

    /// <summary>
    /// Called to show the notification UI on specific events (Backboard bonus active or Fireball bonus active)
    /// </summary>
    /// <param name="notificationType"></param>
    public static void TriggerShowNotification(NotificationType notificationType)
    {
        OnShowNotification?.Invoke(notificationType);
    }
}
