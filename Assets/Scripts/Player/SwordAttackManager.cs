using System.Collections;
using UnityEngine;

public class SwordAttackManager : MonoBehaviour
{
    [SerializeField] private Vector3 boxSize = new Vector3(3f, 2f, 0.5f);
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private Vector3 center;
    PlayerProfile playerProfile;

    [SerializeField] private Vector3 originalSize;

    public float debugDuration = 5f; // 디버그 박스가 유지될 시간

    [SerializeField] private GameObject hitPrefab;

    private float damage;
    private bool skill6Start = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    private void Awake()
    {
        originalSize = boxSize;
    }
    void Start()
    {
        playerProfile = GameObject.FindWithTag("Player").GetComponent<PlayerProfile>();
        playerProfile.SwordAttackCount++;

        bool critical = playerProfile.CriticalProbability();
        if (critical)
        {
            damage = playerProfile.CriticalBuff(playerProfile.BasicATK(100));
        }
        else
        {
            damage = playerProfile.BasicATK(100);
        }

        StartCoroutine(CheckAttackRoutine());
    }

    IEnumerator CheckAttackRoutine()
    {
        // 한 프레임 혹은 고정 물리 프레임(FixedUpdate) 한 번을 기다립니다.
        yield return new WaitForFixedUpdate();

        CheckAttack(); // 여기서 판정 실행

        Destroy(gameObject, 0.5f);
    }

    public void IncreasedColliderSize(float size)
    {
        skill6Start = true;
        if (originalSize == Vector3.zero)
        {
            originalSize = new Vector3(3f, 2f, 0.5f);
        }
        boxSize = originalSize * (1f + (size / 100f));
        skill6Start = true;
    }
    void CheckAttack()
    {
        Vector3 halfSize = boxSize / 2f;

        Vector3 finalCenter = transform.position
                          + (transform.forward * center.z)
                          + (transform.up * center.y);

        Collider[] hitEnemies = Physics.OverlapBox(finalCenter, halfSize, transform.rotation, enemyLayer);

        foreach (Collider enemy in hitEnemies)
        {
            if (enemy.CompareTag("Boss"))
            {
                Debug.Log("검사 기본 공격" + enemy.gameObject.name + "을(를) 공격했습니다!" + "damage = " + damage);
                enemy.gameObject.GetComponent<BossStatus>().GetDamage(damage);
            }
            else if (enemy.CompareTag("Enemy"))
            {
                Debug.Log("검사 기본 공격" + enemy.gameObject.name + "을(를) 공격했습니다!" + "damage = " + damage);
                if (enemy.gameObject.GetComponent<MonsterBehavior>() != null)
                    enemy.gameObject.GetComponent<MonsterBehavior>().TakeDamage(damage);
                if (enemy.gameObject.GetComponent<SealStoneManager>() != null)
                    enemy.gameObject.GetComponent<SealStoneManager>().Damage(damage);
                if (enemy.gameObject.GetComponent<SealedStone>() != null)
                    enemy.gameObject.GetComponent<SealedStone>().TakeDamage(damage);
            }

            Vector3 hitPoint = enemy.ClosestPoint(finalCenter);
            Instantiate(hitPrefab, hitPoint, Quaternion.identity);
            //적 hp 감소
            if (playerProfile.BloodHeal)
                playerProfile.BloodHealHp(10, damage);
        }
    }

    private void OnDrawGizmos()
    {
        // 1. 게임 실행 중이 아닐 때도 위치를 미리 보기 위해 계산
        // transform.position(현재 오브젝트 위치)을 더해줘야 따라옵니다.
        Vector3 finalCenter = transform.position
                              + (transform.forward * center.z)
                              + (transform.up * center.y)
                              + (transform.right * center.x);

        Gizmos.color = Color.red;

        // 2. 회전값 적용 (플레이어가 회전할 때 빨간 박스도 같이 회전하게 함)
        // Matrix를 사용하면 위치, 회전, 스케일을 한 번에 기즈모에 입힐 수 있습니다.
        Matrix4x4 rotationMatrix = Matrix4x4.TRS(finalCenter, transform.rotation, Vector3.one);
        Gizmos.matrix = rotationMatrix;

        Gizmos.DrawWireCube(Vector3.zero, boxSize);
    }

    /// <summary>
    /// OverlapBox와 동일한 영역을 씬 뷰에 그려주는 함수입니다.
    /// </summary>
    void DrawDebugBox(Vector3 center, Vector3 halfExtents, Quaternion orientation, Color color, float duration)
    {
        // 위치, 회전, 크기를 계산하는 행렬 생성
        Matrix4x4 gMatrix = Matrix4x4.TRS(center, orientation, Vector3.one);

        // 박스의 8개 정점 계산
        Vector3[] vertices = new Vector3[8];
        vertices[0] = gMatrix.MultiplyPoint(new Vector3(-halfExtents.x, -halfExtents.y, -halfExtents.z));
        vertices[1] = gMatrix.MultiplyPoint(new Vector3(halfExtents.x, -halfExtents.y, -halfExtents.z));
        vertices[2] = gMatrix.MultiplyPoint(new Vector3(halfExtents.x, -halfExtents.y, halfExtents.z));
        vertices[3] = gMatrix.MultiplyPoint(new Vector3(-halfExtents.x, -halfExtents.y, halfExtents.z));
        vertices[4] = gMatrix.MultiplyPoint(new Vector3(-halfExtents.x, halfExtents.y, -halfExtents.z));
        vertices[5] = gMatrix.MultiplyPoint(new Vector3(halfExtents.x, halfExtents.y, -halfExtents.z));
        vertices[6] = gMatrix.MultiplyPoint(new Vector3(halfExtents.x, halfExtents.y, halfExtents.z));
        vertices[7] = gMatrix.MultiplyPoint(new Vector3(-halfExtents.x, halfExtents.y, halfExtents.z));

        // 각 정점을 연결하여 박스 형태 선 그리기 (총 12개 선)
        Debug.DrawLine(vertices[0], vertices[1], color, duration);
        Debug.DrawLine(vertices[1], vertices[2], color, duration);
        Debug.DrawLine(vertices[2], vertices[3], color, duration);
        Debug.DrawLine(vertices[3], vertices[0], color, duration);
        Debug.DrawLine(vertices[4], vertices[5], color, duration);
        Debug.DrawLine(vertices[5], vertices[6], color, duration);
        Debug.DrawLine(vertices[6], vertices[7], color, duration);
        Debug.DrawLine(vertices[7], vertices[4], color, duration);
        Debug.DrawLine(vertices[0], vertices[4], color, duration);
        Debug.DrawLine(vertices[1], vertices[5], color, duration);
        Debug.DrawLine(vertices[2], vertices[6], color, duration);
        Debug.DrawLine(vertices[3], vertices[7], color, duration);
    }
}
