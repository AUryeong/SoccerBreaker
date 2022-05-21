using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Block : MonoBehaviour
{
    public byte x;
    public byte y;

    public void BlockMove()
    {
        transform.DOLocalMove(new Vector3(x * 1.8f, y * -1.23f - 1.23f), 0.5f);
    }
}
