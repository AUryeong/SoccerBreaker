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
    bool noCrash = false;
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
    void OnCollisionExit2D(Collision2D collision)
    {
        if(collision.collider != null && State == BallState.Shooting)
            noCrash = false;
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
                case "Brick":
                    if (!noCrash)
                    {
                        Vector3 ballPos = transform.position;
                        float angle = Mathf.Atan2(ballPos.y - collision.transform.position.y, ballPos.x - collision.transform.position.x) * Mathf.Rad2Deg - 90;
                        if ((angle <= 60 && angle >= -60) || (angle >= 120 || angle <= -120))
                            euler = (euler > 180) ? 540 - euler : 180 - euler;
                        else
                            euler = 360 - euler;
                    }
                    collision.collider.GetComponent<Brick>().Hit();
                    noCrash = true;
                    break;
            }
            transform.localRotation = Quaternion.Euler(0, 0, euler);
        }
    }
}
