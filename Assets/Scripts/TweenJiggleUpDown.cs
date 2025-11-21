using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class TweenJiggleUpDown : MonoBehaviour
{
    private void Awake()
    {
        float initialY = transform.localPosition.y;

        transform.DOLocalMoveY(initialY + 0.1f, 0.5f).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo);
    }
}
