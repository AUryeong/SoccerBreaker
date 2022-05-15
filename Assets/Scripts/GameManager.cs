using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class GameManager : Singleton<GameManager>
{
    [SerializeField]
    Transform[] Clouds;
    
    [Header("�� ����")]
    [SerializeField]
    Ball defaultBall; // �⺻ ��
    [SerializeField]
    Transform ballParent; // �� �θ�
    BallState State; // ���� ����
    Vector3 clickPos; // �� �߻� ���

    int BallCount = 1;
    int ShootingBallCount = 0;

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
        CheckTouch();
    }

    #region �� ����
    void CheckTouch() // �� ����
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
        if(ShootingBallCount == 0) // ��� ���� �� ƨ�ܼ� ���� ����������
        {
            State = BallState.Wait;
            defaultBall.gameObject.SetActive(true);
            defaultBall.transform.position = ball.transform.position;
            defaultBall.transform.rotation = Quaternion.identity;
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
