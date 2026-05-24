using UnityEngine;

/// <summary>
/// 마을 모드에서 MinimapCamera가 플레이어를 따라가도록 갱신합니다.
/// </summary>
public class VillageMinimapSync : MonoBehaviour
{
    Transform player;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    void LateUpdate()
    {
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player")?.transform;
            if (player == null)
                return;
        }

        var pos = player.position;
        transform.position = new Vector3(pos.x, transform.position.y, pos.z);
    }
}
