using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;


public class Brick : MonoBehaviour
{
    [SerializeField] TextMeshPro hpText;

    public int hp;
    public int x;
    public int y;


    public void Hit()
    {
        hp--;
        ShowHp();
    }

    public void BrickMove()
    {
        transform.DOLocalMove(new Vector3(x * 1.8f, y * -1.23f), 0.5f);
    }
    public void ShowHp()
    {
        if(hp <= 0)
        {
            gameObject.SetActive(false);
            return;
        }
        hpText.text = hp.ToString();
    }
}
