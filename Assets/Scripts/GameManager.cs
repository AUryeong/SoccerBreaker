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
    
    [Header("공 관리")]
    [SerializeField]
    Ball defaultBall; // 기본 공
    [SerializeField]
    Transform ballParent; // 공 부모
    [SerializeField]
    TouchSenser touchSenser; // 터치 센서


    [SerializeField]
    SpriteRenderer touchRoute; // 터치 줄
    [SerializeField]
    SpriteRenderer ballRoute; // 공 줄
    [SerializeField]
    SpriteRenderer ballEx; // 공 예상 경로

    [SerializeField]
    float ballGravity;
    public BallState State; // 현재 상태
    Vector3 clickPos;
    [Header("벽돌 관리")]
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
        SetResolution(); // 초기에 게임 해상도 고정
    }
    #region 해상도 변경
    public void SetResolution()
    {
        int WIDTH = 1080; // 원하는 넓이
        int HEIGHT = 1920; // 원하는 높이

        int DEVICE_WIDTH = Screen.width; // 현재 기기 넓이 저장
        int DEVICE_HEIGHT = Screen.height; // 현재 기기 높이 저장

        float RATIO = (float)WIDTH / HEIGHT;
        float DEVICE_RATIO = (float)DEVICE_WIDTH / DEVICE_HEIGHT;

        Screen.SetResolution(WIDTH, (int)(((float)DEVICE_HEIGHT / DEVICE_WIDTH) * WIDTH), true);
        // 넓이는 고정, 하지만 높이는 디바이스의 해상도 비율에 따라 달라지게.

        if (RATIO < DEVICE_RATIO) // 기기의 해상도 비에서 넓이가 더 큰 경우
        {
            //넓이를 조정한다
            float newWidth = RATIO / DEVICE_RATIO; // 내가 원하는 비율이 될려면 얼만큼 [넓이를] 검정색 처리해야하는지
            Camera.main.rect = new Rect((1f - newWidth) / 2f, 0f, newWidth, 1f); // 차이만큼 [넓이] 부분을 삭제하여 내가 원하는 해상도 이외에 부분은 까맣게 표기함
            //Rect란? 0에서 1사이의 실수를 넣으면 일부분을 삭제해서 송출해줌.
        }
        else // 기기의 해상도 비에서 넓이가 작거나 같은 경우
        {
            //높이를 조정한다
            float newHeight = DEVICE_RATIO / RATIO; // 내가 원하는 비율이 될려면 얼만큼 [높이를] 검정색 처리해야하는지
            Camera.main.rect = new Rect(0f, (1f - newHeight) / 2f, 1f, newHeight); // 차이만큼 [높이] 부분을 삭제하여 내가 원하는 해상도 이외에 부분은 까맣게 표기함
        }
    }
    #endregion


    // Update

    void Update()
    {
        float deltaTime = Time.deltaTime;
        MoveCloud(deltaTime);
    }
    #region 벽돌 관리
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

    #region 볼 관리
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
        if (ShootingBallCount == BallCount - 1) // 첫 번째 공
        {
            defaultBall.gameObject.SetActive(true);
            defaultBall.transform.position = ball.transform.position;
            defaultBall.transform.rotation = Quaternion.identity;
        }
        if (ShootingBallCount == 0) // 모든 공이 다 튕겨서 땅에 도착했을때
        {
            EndShootingBall();
        }
    }

    IEnumerator ShootBall(int ballCount, Vector2 direciton) // 공 쏘기
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

    #region 구름
    void MoveCloud(float deltaTime) // 구름 이동
    {
        float CLOUD_MAX_DISTANCE = 14.34f; // 구름 최대 거리
        float CLOUD_SPEED = 0.2f; // 구름 이동 속도
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
