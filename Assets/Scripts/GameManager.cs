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
    Transform[] clouds;
    [SerializeField]
    TextMeshProUGUI scoreWindow;
    
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
    SpriteRenderer charSprite; // ĳ���� �̹���
    [SerializeField]
    float ballGravity;
    [SerializeField]
    float ballShootDelay;


    public BallState State; // ���� ����
    float ballShootCooltime;
    Vector3 clickPos;
    Vector3 ballDirection;

    [Header("���� ����")]
    [SerializeField]
    Brick defaultBrick;
    [SerializeField]
    Block defaultAddBall;
    [SerializeField]
    Transform blockParent;
    public List<Block> blocks = new List<Block>();

    [SerializeField]
    float BallSpeed = 4;
    int ShotBallCount = 0;
    int ShootBallCount = 0;
    public int Score = 1;
    public int MaxScore = 1;
    public int BallCount = 1;

    // Start

    void Start()
    {
        SetResolution(); // �ʱ⿡ ���� �ػ� ����
        ScoreWindowUpdate();
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
        if(ShootBallCount > 0 && State == BallState.Shooting)
           ShootingBall(deltaTime);
    }

    void ScoreWindowUpdate()
    {
        scoreWindow.text = (Score >= MaxScore ? "<#ff0000>" : "")+ "���� : " + Score.ToString() + "\n" + "��� : " + MaxScore.ToString();
    }

    public void AddBall(GameObject addBall)
    {
        addBall.SetActive(false);
    }

    void GameOver()
    {
        foreach (Block block in blocks)
        {
            block.gameObject.SetActive(false);
        }
        Score = 1;
        defaultBall.transform.localPosition = Vector3.zero;
        BlockMoveAndCreate();
        ScoreWindowUpdate();
    }
    #region �� ����
    public void GetNewAddBall(byte x, byte y)
    {
        Block copyAddBall = PoolManager.Instance.Init(defaultAddBall.gameObject).GetComponent<Block>();
        copyAddBall.transform.SetParent(blockParent);
        copyAddBall.transform.localScale = defaultAddBall.transform.localScale;
        copyAddBall.transform.localPosition = new Vector3(x * 1.8f, 1.23f);
        copyAddBall.x = x;
        copyAddBall.y = y;
        copyAddBall.BlockMove();
        if (!blocks.Contains(copyAddBall))
        {
            blocks.Add(copyAddBall);
        }
    }
    public void GetNewBrick(byte x, byte y, int hp)
    {
        Brick copyBrick = PoolManager.Instance.Init(defaultBrick.gameObject).GetComponent<Brick>();
        copyBrick.transform.SetParent(blockParent);
        copyBrick.transform.localScale = defaultBrick.transform.localScale;
        copyBrick.transform.localPosition = new Vector3(x * 1.8f, 1.23f);
        copyBrick.x = x;
        copyBrick.y = y;
        copyBrick.hp = hp;
        copyBrick.BlockMove();
        copyBrick.ShowHp();
        if (!blocks.Contains(copyBrick))
        {
            blocks.Add(copyBrick);
        }
    }
    void BlockMoveAndCreate()
    {
        foreach (Block block in blocks)
        {
            block.y++;
            block.BlockMove();
            if (block.gameObject.activeSelf && block.y >= 6)
            {
                GameOver();
                return;
            }
        }
        int brickCount = Random.Range(3, 5);
        List<int> indexList = new List<int>(6) { 0,1,2,3,4,5 };
        int ballAddIndex = Random.Range(0, brickCount);
        for (int i = 0; i < brickCount; i++)
        {

            int index = Random.Range(0, indexList.Count);
            if (ballAddIndex == i)
                GetNewAddBall((byte)indexList[index], 0);
            else
                GetNewBrick((byte)indexList[index], 0, Score);
            indexList.RemoveAt(index);
        }
            
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

    void ShootingBall(float deltaTime)
    {
        ballShootCooltime += deltaTime;
        if (ballShootCooltime < ballShootDelay)
        {
            return;
        }
        Ball copyBall = PoolManager.Instance.Init(defaultBall.gameObject).GetComponent<Ball>();
        copyBall.State = BallState.Shooting;
        copyBall.transform.SetParent(ballParent);
        copyBall.transform.position = defaultBall.transform.position;
        copyBall.transform.localScale = defaultBall.transform.localScale;
        copyBall.GetComponent<Rigidbody2D>().AddForce(ballDirection * 100 * BallSpeed);
        copyBall.RB.gravityScale = ballGravity;
        ballShootCooltime -= ballShootDelay;
        ShootBallCount--;
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
        ShotBallCount = BallCount;
        ShootBallCount = BallCount;
        defaultBall.gameObject.SetActive(false);
        ballDirection = direction;
        
    }
    void EndShootingBall()
    {
        State = BallState.Wait;
        Score++;
        if (MaxScore < Score)
            MaxScore = Score;
        touchSenser.gameObject.SetActive(true);
        ScoreWindowUpdate();
        BlockMoveAndCreate();
    }

    public void DisableBall(Ball ball)
    {
        ball.gameObject.SetActive(false);
        
        ShotBallCount--;
        if (ShotBallCount == BallCount - 1) // ù ��° ��
        {
            defaultBall.gameObject.SetActive(true);
            defaultBall.transform.position = ball.transform.position;
            defaultBall.transform.rotation = Quaternion.identity;
        }
        if (ShotBallCount == 0) // ��� ���� �� ƨ�ܼ� ���� ����������
        {
            EndShootingBall();
        }
    }
    
    #endregion

    #region ����
    void MoveCloud(float deltaTime) // ���� �̵�
    {
        float CLOUD_MAX_DISTANCE = 14.34f; // ���� �ִ� �Ÿ�
        float CLOUD_SPEED = 0.2f; // ���� �̵� �ӵ�
        foreach(Transform cloudObj in clouds)
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
