using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using DG.Tweening;

public class NotificationLabel : MonoBehaviour
{
    [SerializeField] private TMP_Text _notificationText;
    
    [Header("Anim settings")]
    [SerializeField] private float _yPositionTarget;
    [SerializeField] private float _animDuration;
    [SerializeField] private float _showDuration;

    public void SetNotificationText(string text)
    {
        _notificationText.text = text;
        
        transform.localPosition = Vector3.zero;
        transform.DOLocalMoveY(_yPositionTarget, _animDuration);

        StartCoroutine(ResetNotificationText());
    }
    
    private IEnumerator ResetNotificationText()
    {
        yield return new WaitForSeconds(_showDuration);
        
        Destroy(gameObject);
    }
}
