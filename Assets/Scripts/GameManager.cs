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
    SpriteRenderer touchArrow;
    [SerializeField]
    SpriteRenderer ballArrow;
    public BallState State; // ���� ����
    Vector3 clickPos;
    [Header("���� ����")]
    [SerializeField]
    Brick defaultBrick;
    [SerializeField]
    Transform brickParent;
    public List<Brick> bricks = new List<Brick>();

    public int Score;
    public int BallCount = 1;
    public int ShootingBallCount = 0;

    // Start

    void Start()
    {
        SetResolution(); // �ʱ⿡ ���� �ػ� ����
        for(int i = 0; i < 5; i++)
        {
            bricks.Add(GetNewBrick(i, 0, 5));
        }
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
    public Brick GetNewBrick(int x = 0, int y = 0, int hp = 0)
    {
        GameObject copyBrick = PoolManager.Instance.Init(defaultBrick.gameObject);
        copyBrick.transform.SetParent(brickParent);
        copyBrick.transform.localScale = defaultBrick.transform.localScale;
        Brick brick = copyBrick.GetComponent<Brick>();
        brick.x = x;
        brick.y = y;
        brick.hp = hp;
        brick.BrickMove();
        brick.ShowHp();
        return brick;
    }
    #endregion

    #region �� ����
    public void StartDrag(PointerEventData data)
    {
        touchArrow.gameObject.SetActive(true);
        ballArrow.gameObject.SetActive(true);
        clickPos = Camera.main.ScreenToWorldPoint(data.position);
    }
    public void UpdateDrag(PointerEventData data)
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(data.position);
        float angle = Mathf.Atan2(mousePos.y - clickPos.y, mousePos.x - clickPos.x) * Mathf.Rad2Deg - 90;
        touchArrow.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        touchArrow.transform.position = new Vector3((mousePos.x + clickPos.x)/2, (mousePos.y + clickPos.y) / 2, 0);


        Vector3 distance = mousePos - clickPos;
        touchArrow.size = new Vector2 (0.07f, distance.magnitude / 1.25f);
        if (angle > 80 || angle < -80)
        {
            if(angle > 0 || angle < -180)
                angle = 80;
            else
                angle = -80;
        }
        Quaternion angle2 = Quaternion.AngleAxis(angle, Vector3.forward);
        distance.Normalize();
        RaycastHit2D raycastHit2D = Physics2D.CircleCast(defaultBall.transform.position, 10f, distance, Mathf.Infinity, LayerMask.GetMask("Wall"));
        if(raycastHit2D.collider != null)
        {
            ballArrow.transform.rotation = angle2;
            ballArrow.transform.position = new Vector2((defaultBall.transform.position.x + raycastHit2D.point.x)/2, (defaultBall.transform.position.y + raycastHit2D.point.y)/2);
            ballArrow.size = new Vector2(0.07f, raycastHit2D.distance/ 1.25f);
        }
        defaultBall.transform.rotation = angle2;
    }
    public void EndDrag(PointerEventData data)
    {
        State = BallState.Shooting;
        touchSenser.gameObject.SetActive(false);
        touchArrow.gameObject.SetActive(false);
        ballArrow.gameObject.SetActive(false);
        StartCoroutine(ShootBall(BallCount));
    }
    void EndShootingBall(Ball endBall)
    {
        State = BallState.Wait;
        Score++;
        touchSenser.gameObject.SetActive(true);
        defaultBall.gameObject.SetActive(true);
        defaultBall.transform.position = endBall.transform.position;
        defaultBall.transform.rotation = Quaternion.identity;
        foreach(Brick brick in bricks)
        {
            brick.y++;
            brick.BrickMove();
        }
    }

    public void DisableBall(Ball ball)
    {
        ball.gameObject.SetActive(false);
        ShootingBallCount--;
        if(ShootingBallCount == 0) // ��� ���� �� ƨ�ܼ� ���� ����������
        {
            EndShootingBall(ball);
        }
    }

    IEnumerator ShootBall(int ballCount) // �� ���
    {
        ShootingBallCount = ballCount;
        defaultBall.gameObject.SetActive(false);
        Vector3 pos = defaultBall.transform.position;
        Quaternion rotation = defaultBall.transform.rotation;
        Vector3 scale = defaultBall.transform.localScale;
        while(ballCount > 0)
        {
            Ball copyBall = PoolManager.Instance.Init(defaultBall.gameObject).GetComponent<Ball>();
            copyBall.State = BallState.Shooting;
            copyBall.transform.SetParent(ballParent);
            copyBall.transform.position = pos;
            copyBall.transform.rotation = rotation;
            copyBall.transform.localScale = scale;
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
