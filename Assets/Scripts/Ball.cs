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
    public float Speed;
    void Start()
    {
        
    }

    void FixedUpdate()
    {
        Shooting();
    }
    void Shooting()
    {
        if(State == BallState.Shooting)
           transform.Translate(Vector2.up * Speed);
    }

    void OnCollisionEnter2D(Collision2D collision) // 벽돌 또는 벽에 충돌할 경우
    {
        if (collision.collider != null && State == BallState.Shooting)
        {
            float euler = transform.localRotation.eulerAngles.z;
            switch (collision.transform.tag)
            {
                case "Bottom Wall":
                    if (euler <= 270 && euler >= 90)
                    {
                        GameManager.Instance.DisableBall(this);
                    }
                    break;
                case "Side Wall":
                    euler = 360 - euler;
                    break;
                case "Top Wall":
                    euler = (euler <= 180) ? 180 - euler : 540 - euler;
                    break;
            }
            transform.localRotation = Quaternion.Euler(0, 0, euler);
        }
    }
}
