using UnityEngine;

public class ItemMove : MonoBehaviour
{
    private float speed = 2f;      // 위아래로 왕복하는 속도
    private float height = 2f;   // 움직일 최대 높이 (반경)

    private Vector3 startPosition;                  // 게임 시작 시점의 최초 위치

    private void Start()
    {
        // 오브젝트가 배치된 처음 기준 위치를 기억해 둡니다.
        startPosition = transform.localPosition;
    }

    private void Update()
    {
        // Time.time(시간)이 흐름에 따라 Mathf.Sin은 -1에서 1 사이를 부드럽게 반복합니다.
        float newY = Mathf.Sin(Time.time * speed) * height;

        // 기준점이 되는 startPosition의 Y값에 실시간 변동 폭(newY)을 더해줍니다.
        transform.localPosition = new Vector3(
            startPosition.x,
            startPosition.y + newY,
            startPosition.z
        );
    }
}
