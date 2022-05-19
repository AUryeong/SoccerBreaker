using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using DG.Tweening;


public class GameManager : Singleton<GameManager>
{
    [SerializeField]
    Transform[] Clouds;
    
    [Header("�� ����")]
    [SerializeField]
    Ball defaultBall; // �⺻ ��
    [SerializeField]
    Transform ballParent; // �� �θ�
    [SerializeField]
    TouchSenser touchSenser; // ��ġ ����


    [SerializeField]
    SpriteRenderer touchRoute; // ��ġ ��
    [SerializeField]
    SpriteRenderer ballRoute; // �� ��
    [SerializeField]
    SpriteRenderer ballEx; // �� ���� ���

    [SerializeField]
    float ballGravity;
    public BallState State; // ���� ����
    Vector3 clickPos;
    [Header("���� ����")]
    [SerializeField]
    Brick defaultBrick;
    [SerializeField]
    Transform brickParent;
    public List<Brick> bricks = new List<Brick>();

    [SerializeField]
    float BallSpeed = 4;
    public int Score = 1;
    public int BallCount = 1;
    public int ShootingBallCount = 0;

    // Start

    void Start()
    {
        SetResolution(); // �ʱ⿡ ���� �ػ� ����
    }
    #region �ػ� ����
    public void SetResolution()
    {
        int WIDTH = 1080; // ���ϴ� ����
        int HEIGHT = 1920; // ���ϴ� ����

        int DEVICE_WIDTH = Screen.width; // ���� ��� ���� ����
        int DEVICE_HEIGHT = Screen.height; // ���� ��� ���� ����

        float RATIO = (float)WIDTH / HEIGHT;
        float DEVICE_RATIO = (float)DEVICE_WIDTH / DEVICE_HEIGHT;

        Screen.SetResolution(WIDTH, (int)(((float)DEVICE_HEIGHT / DEVICE_WIDTH) * WIDTH), true);
        // ���̴� ����, ������ ���̴� ����̽��� �ػ� ������ ���� �޶�����.

        if (RATIO < DEVICE_RATIO) // ����� �ػ� �񿡼� ���̰� �� ū ���
        {
            //���̸� �����Ѵ�
            float newWidth = RATIO / DEVICE_RATIO; // ���� ���ϴ� ������ �ɷ��� ��ŭ [���̸�] ������ ó���ؾ��ϴ���
            Camera.main.rect = new Rect((1f - newWidth) / 2f, 0f, newWidth, 1f); // ���̸�ŭ [����] �κ��� �����Ͽ� ���� ���ϴ� �ػ� �̿ܿ� �κ��� ��İ� ǥ����
            //Rect��? 0���� 1������ �Ǽ��� ������ �Ϻκ��� �����ؼ� ��������.
        }
        else // ����� �ػ� �񿡼� ���̰� �۰ų� ���� ���
        {
            //���̸� �����Ѵ�
            float newHeight = DEVICE_RATIO / RATIO; // ���� ���ϴ� ������ �ɷ��� ��ŭ [���̸�] ������ ó���ؾ��ϴ���
            Camera.main.rect = new Rect(0f, (1f - newHeight) / 2f, 1f, newHeight); // ���̸�ŭ [����] �κ��� �����Ͽ� ���� ���ϴ� �ػ� �̿ܿ� �κ��� ��İ� ǥ����
        }
    }
    #endregion


    // Update

    void Update()
    {
        float deltaTime = Time.deltaTime;
        MoveCloud(deltaTime);
    }
    #region ���� ����
    public Brick GetNewBrick(int x = 0, int hp = 0)
    {
        GameObject copyBrick = PoolManager.Instance.Init(defaultBrick.gameObject);
        copyBrick.transform.SetParent(brickParent);
        copyBrick.transform.localScale = defaultBrick.transform.localScale;
        copyBrick.transform.localPosition = new Vector3(x * 1.8f, 1.23f);
        Brick brick = copyBrick.GetComponent<Brick>();
        brick.x = x;
        brick.y = 0;
        brick.hp = hp;
        brick.BrickMove();
        brick.ShowHp();
        
        if (!bricks.Contains(brick))
        {
            bricks.Add(brick);
        }
        return brick;
    }
    void BrickMoveAndCreate()
    {
        foreach (Brick brick in bricks)
        {
            brick.y++;
            brick.BrickMove();
        }
        GetNewBrick(Random.Range(0, 6), Score);
    }
    #endregion

    #region �� ����
    public void StartDrag(PointerEventData data)
    {
        touchRoute.gameObject.SetActive(true);
        ballRoute.gameObject.SetActive(true);
        ballEx.gameObject.SetActive(true);
        clickPos = Camera.main.ScreenToWorldPoint(data.position);
    }
    public void UpdateDrag(PointerEventData data)
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(data.position);
        Vector3 distance = mousePos - clickPos;


        float angle = Mathf.Atan2(distance.y, distance.x) * Mathf.Rad2Deg - 90;
        touchRoute.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        touchRoute.transform.position = new Vector3((mousePos.x + clickPos.x)/2, (mousePos.y + clickPos.y) / 2, 0);
        touchRoute.size = new Vector2 (0.07f, distance.magnitude / 1.25f);


        if (angle > 80 || angle < -80)
        {
            if(angle > 0 || angle < -180)
                angle = 80;
            else
                angle = -80;
        }
        Quaternion angle2 = Quaternion.AngleAxis(angle, Vector3.forward);
        distance.Normalize();
        if(distance.y <= 0.178f)
        {
            distance.Set((distance.x < 0) ? -1 : 1, 0.178f, 0);
        }


        RaycastHit2D raycastHit2D = Physics2D.CircleCast(defaultBall.transform.position, 0.18f, distance, Mathf.Infinity, LayerMask.GetMask("Wall"));
        if(raycastHit2D.collider != null)
        {
            ballRoute.transform.rotation = angle2;
            ballRoute.transform.position = new Vector2((defaultBall.transform.position.x + raycastHit2D.point.x)/2, (defaultBall.transform.position.y + raycastHit2D.point.y)/2);
            ballRoute.size = new Vector2(0.07f, raycastHit2D.distance/ 1.25f);

            ballEx.transform.position = raycastHit2D.point + raycastHit2D.normal/10; 
        }
    }
    public void EndDrag(PointerEventData data)
    {
        State = BallState.Shooting;
        touchSenser.gameObject.SetActive(false);
        touchRoute.gameObject.SetActive(false);
        ballRoute.gameObject.SetActive(false);
        ballEx.gameObject.SetActive(false);

        Vector3 direction = Camera.main.ScreenToWorldPoint(data.position) - clickPos;
        direction.Normalize();
        if (direction.y <= 0.178f)
        {
            direction.Set((direction.x < 0) ? -1 : 1, 0.178f, 0);
        }
        StartCoroutine(ShootBall(BallCount, direction));
    }
    void EndShootingBall()
    {
        State = BallState.Wait;
        Score++;
        touchSenser.gameObject.SetActive(true);
        BrickMoveAndCreate();
    }

    public void DisableBall(Ball ball)
    {
        ball.gameObject.SetActive(false);
        
        ShootingBallCount--;
        if (ShootingBallCount == BallCount - 1) // ù ��° ��
        {
            defaultBall.gameObject.SetActive(true);
            defaultBall.transform.position = ball.transform.position;
            defaultBall.transform.rotation = Quaternion.identity;
        }
        if (ShootingBallCount == 0) // ��� ���� �� ƨ�ܼ� ���� ����������
        {
            EndShootingBall();
        }
    }

    IEnumerator ShootBall(int ballCount, Vector2 direciton) // �� ���
    {
        ShootingBallCount = ballCount;
        defaultBall.gameObject.SetActive(false);
        Vector3 pos = defaultBall.transform.position;
        Vector3 scale = defaultBall.transform.localScale;
        while(ballCount > 0)
        {
            Ball copyBall = PoolManager.Instance.Init(defaultBall.gameObject).GetComponent<Ball>();
            copyBall.State = BallState.Shooting;
            copyBall.transform.SetParent(ballParent);
            copyBall.transform.position = pos;
            copyBall.transform.localScale = scale;
            copyBall.GetComponent<Rigidbody2D>().AddForce(direciton * 100 * BallSpeed);
            copyBall.RB.gravityScale = ballGravity;
            ballCount--;
            yield return new WaitForSeconds(0.1f);
        }
    }
    #endregion

    #region ����
    void MoveCloud(float deltaTime) // ���� �̵�
    {
        float CLOUD_MAX_DISTANCE = 14.34f; // ���� �ִ� �Ÿ�
        float CLOUD_SPEED = 0.2f; // ���� �̵� �ӵ�
        foreach(Transform cloudObj in Clouds)
        {
            cloudObj.Translate(deltaTime * Vector3.right * CLOUD_SPEED);
            if(cloudObj.localPosition.x >= CLOUD_MAX_DISTANCE)
            {
                cloudObj.Translate(Vector3.left * CLOUD_MAX_DISTANCE);
            }
        }
    }
    #endregion
}
