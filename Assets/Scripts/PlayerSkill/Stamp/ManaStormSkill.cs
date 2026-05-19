using UnityEngine;

public class ManaStormSkill : MonoBehaviour
{
    [SerializeField] private float radius;
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private Vector3 center;

    private PlayerProfile playerProfile;

    private float damage;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerProfile = GameObject.FindWithTag("Player").GetComponent<PlayerProfile>();

        if (playerProfile != null)
        {
            bool critical = playerProfile.CriticalProbability();
            if (critical)
                damage = playerProfile.CriticalBuff(playerProfile.ATK(75));
            else
                damage = playerProfile.ATK(75);
        }

        InvokeRepeating("CheckAttack", 0.3f, 0.3f);
        Invoke("ObjectDestroy", 3);
    }

    private void ObjectDestroy()
    {
        playerProfile.SkillStart = false;
        Destroy(gameObject);
    }

    void CheckAttack()
    {
        playerProfile.ShakeCamera(0.2f, 1.0f, 15.0f);
        Vector3 finalCenter = (transform.position)
                          + (transform.forward * center.z)
                          + (transform.up * center.y) + (transform.right * center.x);

        Collider[] hitEnemies = Physics.OverlapSphere(finalCenter, radius, enemyLayer);

        DrawDebugSphere(finalCenter, radius, transform.rotation, Color.yellow, 5f);

        foreach (Collider enemy in hitEnemies)
        {
            Vector3 hitPoint = enemy.ClosestPoint(transform.position);
            playerProfile.BowSkillHit(hitPoint);
            if (enemy.CompareTag("Boss"))
            {
                Debug.Log("스킬 : 마력 폭풍" + enemy.gameObject.name + "을(를) 공격했습니다!" + "damage = " + damage);
                enemy.gameObject.GetComponent<BossStatus>().GetDamage(damage);
            }
            else if (enemy.CompareTag("Enemy"))
            {
                Debug.Log("스킬 : 마력 폭풍" + enemy.gameObject.name + "을(를) 공격했습니다!" + "damage = " + damage);
                if (enemy.gameObject.GetComponent<MonsterBehavior>() != null)
                    enemy.gameObject.GetComponent<MonsterBehavior>().TakeDamage(damage);
                if (enemy.gameObject.GetComponent<SealStoneManager>() != null)
                    enemy.gameObject.GetComponent<SealStoneManager>().Damage(damage);
            }
            //끌어당기기
            Vector3 dist = enemy.transform.position - transform.position;
            dist.Normalize();

            enemy.transform.position -= dist * 1;
            if (playerProfile.BloodHeal)
                playerProfile.BloodHealHp(10, damage);
        }
    }

    private void OnDrawGizmos()
    {
        // 1. 게임 실행 중이 아닐 때도 위치를 미리 보기 위해 계산
        Vector3 finalCenter = transform.position
                              + (transform.forward * center.z)
                              + (transform.up * center.y)
                              + (transform.right * center.x);

        Gizmos.color = Color.green;

        Matrix4x4 rotationMatrix = Matrix4x4.TRS(finalCenter, transform.rotation, Vector3.one);
        Gizmos.matrix = rotationMatrix;

        Gizmos.DrawSphere(Vector3.zero, radius);
    }

    void DrawDebugSphere(Vector3 center, float radius, Quaternion orientation, Color color, float duration)
    {
        float angleStep = 10f; // 원을 얼마나 부드럽게 그릴지 결정 (낮을수록 부드러움)

        // 이전 프레임의 점들을 저장할 변수
        Vector3 prevX = Vector3.zero;
        Vector3 prevY = Vector3.zero;
        Vector3 prevZ = Vector3.zero;

        // 0도부터 360도까지 돌면서 원 그리기
        for (float i = 0; i <= 360; i += angleStep)
        {
            float rad = i * Mathf.Deg2Rad;
            float c = Mathf.Cos(rad) * radius;
            float s = Mathf.Sin(rad) * radius;

            // 1. 로컬 좌표계에서 3개의 원(평면) 좌표 계산
            Vector3 nextX = new Vector3(0, c, s); // YZ 평면을 도는 원
            Vector3 nextY = new Vector3(c, 0, s); // XZ 평면을 도는 원
            Vector3 nextZ = new Vector3(c, s, 0); // XY 평면을 도는 원

            // 2. 회전(orientation)과 위치(center) 적용
            // (기존 코드의 Matrix4x4 대신 벡터 연산이 더 가벼워서 변경했습니다)
            nextX = center + (orientation * nextX);
            nextY = center + (orientation * nextY);
            nextZ = center + (orientation * nextZ);

            // 3. 첫 번째 점이 아닐 때만 선 긋기
            if (i > 0)
            {
                Debug.DrawLine(prevX, nextX, color, duration);
                Debug.DrawLine(prevY, nextY, color, duration);
                Debug.DrawLine(prevZ, nextZ, color, duration);
            }

            // 현재 점을 이전 점으로 저장
            prevX = nextX;
            prevY = nextY;
            prevZ = nextZ;
        }
    }
}
