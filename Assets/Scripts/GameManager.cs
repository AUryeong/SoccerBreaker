using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    [SerializeField]
    Transform[] Clouds;

    void Update()
    {
        float deltaTime = Time.deltaTime;
        MoveCloud(deltaTime);
    }

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
}
