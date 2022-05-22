using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BallState
{
    Wait,
    Shooting
}
public class Ball : MonoBehaviour
{
    public BallState State;
    Rigidbody2D Rb;

    public Rigidbody2D RB
    {
        get
        {
            if (Rb == null)
                Rb = GetComponent<Rigidbody2D>();
            return Rb;
        }
    }

    void OnCollisionEnter2D(Collision2D collision) // ���� �Ǵ� ���� �浹�� ���
    {
        if (collision.collider != null && State == BallState.Shooting)
        {
            switch (collision.transform.tag)
            {
                case "Bottom Wall":
                    GameManager.Instance.DisableBall(this);
                    break;
                case "Brick":
                    collision.collider.GetComponent<Brick>().Hit();
                    break;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject != null && State == BallState.Shooting && collision.transform.tag == "AddBall")
        {
            GameManager.Instance.AddBall(collision.gameObject);
        }
    }
}
