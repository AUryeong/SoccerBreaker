using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class GameManager : Singleton<GameManager>
{
    [SerializeField]
    Transform[] Clouds;
    
    [Header("공 관리")]
    [SerializeField]
    Ball defaultBall; // 기본 공
    [SerializeField]
    Transform ballParent; // 공 부모
    BallState State; // 현재 상태
    Vector3 clickPos; // 공 발사 기능

    int BallCount = 1;
    int ShootingBallCount = 0;

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
        CheckTouch();
    }

    #region 볼 관리
    void CheckTouch() // 볼 슈팅
    {
        if (Input.touchCount > 0 && State == BallState.Wait)
        {
            Touch touch = Input.GetTouch(0);
            switch (touch.phase)
            {
                case TouchPhase.Began:
                    clickPos = Camera.main.ScreenToWorldPoint(touch.position);
                    break;
                case TouchPhase.Moved:
                    Vector3 mousePos = Camera.main.ScreenToWorldPoint(touch.position);
                    float angle = Mathf.Atan2(mousePos.y - clickPos.y, mousePos.x - clickPos.x) * Mathf.Rad2Deg;
                    if (angle + 90 <= 70 && angle + 90 >= -70)
                        defaultBall.transform.rotation = Quaternion.AngleAxis(angle + 90, Vector3.forward);
                    break;
                case TouchPhase.Ended:
                    State = BallState.Shooting;
                    StartCoroutine(ShootBall(BallCount));
                    break;
            }
        }
    }

    public void DisableBall(Ball ball)
    {
        ball.gameObject.SetActive(false);
        ShootingBallCount--;
        if(ShootingBallCount == 0) // 모든 공이 다 튕겨서 땅에 도착했을때
        {
            State = BallState.Wait;
            defaultBall.gameObject.SetActive(true);
            defaultBall.transform.position = ball.transform.position;
            defaultBall.transform.rotation = Quaternion.identity;
        }
    }

    IEnumerator ShootBall(int ballCount) // 공 쏘기
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
