using UnityEngine;

public class ReleaseOfSwordSkill : MonoBehaviour
{
    [SerializeField] private Vector3 boxSize;
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private Vector3 center;
    PlayerProfile playerProfile;

    private float moveSpeed = 10.0f;

    private float damage;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerProfile = GameObject.FindWithTag("Player").GetComponent<PlayerProfile>();
        if (playerProfile != null)
        {
            playerProfile.UseMP(1);
            bool critical = playerProfile.CriticalProbability();
            if (critical)
                damage = playerProfile.CriticalBuff(playerProfile.ATK(500f));
            else
                damage = playerProfile.ATK(500f);
        }
    }

    // Update is called once per frame
    void Update()
    {
        transform.position += transform.forward * moveSpeed * Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Wall") || other.CompareTag("Storage"))
        {
            playerProfile.SkillStart = false;
            Destroy(gameObject);
        }
        if (other.CompareTag("Enemy") || other.CompareTag("Boss"))
        {
            Vector3 hitPoint = other.ClosestPoint(transform.position);
            playerProfile.SwordSkillHit(hitPoint);
            if (other.CompareTag("Boss"))
            {
                Debug.Log("스킬 : 검기방출" + other.gameObject.name + "을(를) 공격했습니다!" + "damage = " + damage);
                other.gameObject.GetComponent<BossStatus>().GetDamage(damage);
            }
            else if (other.CompareTag("Enemy"))
            {
                Debug.Log("스킬 : 검기방출" + other.gameObject.name + "을(를) 공격했습니다!" + "damage = " + damage);
                if (other.gameObject.GetComponent<MonsterBehavior>() != null)
                    other.gameObject.GetComponent<MonsterBehavior>().TakeDamage(damage);
                if (other.gameObject.GetComponent<SealStoneManager>() != null)
                    other.gameObject.GetComponent<SealStoneManager>().Damage(damage);
                if (other.gameObject.GetComponent<SealedStone>() != null)
                    other.gameObject.GetComponent<SealedStone>().TakeDamage(damage);
            }
            if (playerProfile.BloodHeal)
                playerProfile.BloodHealHp(10, damage);
            //적 hp 감소
        }
    }
    //void CheckAttack()
    //{
    //    Vector3 halfSize = boxSize / 2f;

    //    Vector3 finalCenter = (transform.position)
    //                      + (transform.forward * center.z)
    //                      + (transform.up * center.y) + (transform.right * center.x);

    //    Collider[] hitEnemies = Physics.OverlapBox(finalCenter, halfSize, transform.rotation, enemyLayer);

    //    DrawDebugBox(finalCenter, halfSize, transform.rotation, Color.yellow, 5f);

    //    foreach (Collider enemy in hitEnemies)
    //    {
    //        if (enemy.CompareTag("Wall"))
    //        {
    //            Destroy(gameObject);
    //        }
    //        if (enemy.CompareTag("Enemy"))
    //        {
    //            Debug.Log("스킬 : 검기방출" + enemy.gameObject.name + "을(를) 공격했습니다!");
    //            //적 hp 감소
    //        }
    //    }
    //}

    //private void OnDrawGizmos()
    //{
    //    // 1. 게임 실행 중이 아닐 때도 위치를 미리 보기 위해 계산
    //    // transform.position(현재 오브젝트 위치)을 더해줘야 따라옵니다.
    //    Vector3 finalCenter = transform.position
    //                          + (transform.forward * center.z)
    //                          + (transform.up * center.y)
    //                          + (transform.right * center.x);

    //    Gizmos.color = Color.red;

    //    // 2. 회전값 적용 (플레이어가 회전할 때 빨간 박스도 같이 회전하게 함)
    //    // Matrix를 사용하면 위치, 회전, 스케일을 한 번에 기즈모에 입힐 수 있습니다.
    //    Matrix4x4 rotationMatrix = Matrix4x4.TRS(finalCenter, transform.rotation, Vector3.one);
    //    Gizmos.matrix = rotationMatrix;

    //    Gizmos.DrawWireCube(Vector3.zero, boxSize);
    //}

    //void DrawDebugBox(Vector3 center, Vector3 halfExtents, Quaternion orientation, Color color, float duration)
    //{
    //    // 위치, 회전, 크기를 계산하는 행렬 생성
    //    Matrix4x4 gMatrix = Matrix4x4.TRS(center, orientation, Vector3.one);

    //    // 박스의 8개 정점 계산
    //    Vector3[] vertices = new Vector3[8];
    //    vertices[0] = gMatrix.MultiplyPoint(new Vector3(-halfExtents.x, -halfExtents.y, -halfExtents.z));
    //    vertices[1] = gMatrix.MultiplyPoint(new Vector3(halfExtents.x, -halfExtents.y, -halfExtents.z));
    //    vertices[2] = gMatrix.MultiplyPoint(new Vector3(halfExtents.x, -halfExtents.y, halfExtents.z));
    //    vertices[3] = gMatrix.MultiplyPoint(new Vector3(-halfExtents.x, -halfExtents.y, halfExtents.z));
    //    vertices[4] = gMatrix.MultiplyPoint(new Vector3(-halfExtents.x, halfExtents.y, -halfExtents.z));
    //    vertices[5] = gMatrix.MultiplyPoint(new Vector3(halfExtents.x, halfExtents.y, -halfExtents.z));
    //    vertices[6] = gMatrix.MultiplyPoint(new Vector3(halfExtents.x, halfExtents.y, halfExtents.z));
    //    vertices[7] = gMatrix.MultiplyPoint(new Vector3(-halfExtents.x, halfExtents.y, halfExtents.z));

    //    // 각 정점을 연결하여 박스 형태 선 그리기 (총 12개 선)
    //    Debug.DrawLine(vertices[0], vertices[1], color, duration);
    //    Debug.DrawLine(vertices[1], vertices[2], color, duration);
    //    Debug.DrawLine(vertices[2], vertices[3], color, duration);
    //    Debug.DrawLine(vertices[3], vertices[0], color, duration);
    //    Debug.DrawLine(vertices[4], vertices[5], color, duration);
    //    Debug.DrawLine(vertices[5], vertices[6], color, duration);
    //    Debug.DrawLine(vertices[6], vertices[7], color, duration);
    //    Debug.DrawLine(vertices[7], vertices[4], color, duration);
    //    Debug.DrawLine(vertices[0], vertices[4], color, duration);
    //    Debug.DrawLine(vertices[1], vertices[5], color, duration);
    //    Debug.DrawLine(vertices[2], vertices[6], color, duration);
    //    Debug.DrawLine(vertices[3], vertices[7], color, duration);
    //}
}
