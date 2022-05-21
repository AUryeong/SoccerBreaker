using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;


public class Brick : Block
{
    [SerializeField] TextMeshPro hpText;
    public int hp;

    public void Hit()
    {
        hp--;
        ShowHp();
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
