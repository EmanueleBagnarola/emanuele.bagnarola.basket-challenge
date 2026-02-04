using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NotificationsSettings", menuName = "ScriptableObjects/NotificationSettings")]
public class NotificationsSettings : ScriptableObject
{
    [field: SerializeField, NonReorderable] public List<NotificationConfig> NotificationConfigs { get; private set; } = new List<NotificationConfig>();

    public string GetNotificationText(NotificationType notificationType)
    {
        return NotificationConfigs.Find(c => c.NotificationType == notificationType).NotificationText;
    }
}

[System.Serializable]
public class NotificationConfig
{
    public NotificationType NotificationType;
    [TextArea(3, 3)] public string NotificationText;
}

public enum NotificationType
{
    BackboardBonus,
    FireballActive,
}
