using UnityEngine;

// 실시간 단순 회전 스크립트 : 아이템에 적용
public class Rotator : MonoBehaviour
{
    public float rotationSpeed = 60f; // 회전 속도

    private void Update() 
    {
        // y축 기준으로 회전 : 제자리 빙글빙글
        transform.Rotate(0f, rotationSpeed*Time.deltaTime, 0f);
    }
}
