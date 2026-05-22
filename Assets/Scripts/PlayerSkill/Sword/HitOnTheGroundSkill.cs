using System.Collections;
using UnityEngine;

public class HitOnTheGroundSkill : MonoBehaviour
{
    [SerializeField] private float radius;
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private Vector3 center;
    PlayerProfile playerProfile;

    private float debugDuration = 5f; // 디버그 박스가 유지될 시간

    private float damage1;
    private float damage2;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerProfile = GameObject.FindWithTag("Player").GetComponent<PlayerProfile>();
        if (playerProfile != null)
        {
            playerProfile.UseMP(1);
            bool critical = playerProfile.CriticalProbability();
            if (critical)
            {
                damage1 = playerProfile.CriticalBuff(playerProfile.ATK(650f));
                damage2 = playerProfile.CriticalBuff(playerProfile.ATK(200f));
            }
            else
            {
                damage1 = playerProfile.ATK(650f);
                damage2 = playerProfile.ATK(200f);
            }
        }
        Invoke("CheckAttack", 0.1f);
        Invoke("ObjectDestory", 5);
    }

    void ObjectDestory()
    {
        playerProfile.SkillStart = false;
        Destroy(gameObject);
    }
    void CheckAttack()
    {
        playerProfile.ShakeCamera(0.2f, 5.0f, 15.0f);
        Vector3 finalCenter = (transform.position)
                          + (transform.forward * center.z)
                          + (transform.up * center.y) + (transform.right * center.x);

        Collider[] hitEnemies = Physics.OverlapSphere(finalCenter, radius, enemyLayer);

        //DrawDebugBox(finalCenter, halfSize, transform.rotation, Color.yellow, 5f);
        DrawDebugSphere(finalCenter, radius, transform.rotation, Color.yellow, 5f);

        foreach (Collider enemy in hitEnemies)
        {
            float dist = Vector3.Distance(transform.position, enemy.transform.position);
            if (dist <= 2)
            {
                Vector3 hitPoint = enemy.ClosestPoint(transform.position);
                playerProfile.SwordSkillHit(hitPoint);
                if (enemy.CompareTag("Boss"))
                {
                    Debug.Log("스킬 : 지면강타" + enemy.gameObject.name + "을(를) 공격했습니다!" + "damage1 = " + damage1);
                    enemy.gameObject.GetComponent<BossStatus>().GetDamage(damage1);
                }
                else if (enemy.CompareTag("Enemy"))
                {
                    Debug.Log("스킬 : 지면강타" + enemy.gameObject.name + "을(를) 공격했습니다!" + "damage1 = " + damage1);
                    if (enemy.gameObject.GetComponent<MonsterBehavior>() != null)
                        enemy.gameObject.GetComponent<MonsterBehavior>().TakeDamage(damage1);
                    if (enemy.gameObject.GetComponent<SealStoneManager>() != null)
                        enemy.gameObject.GetComponent<SealStoneManager>().Damage(damage1);
                    if (enemy.gameObject.GetComponent<SealedStone>() != null)
                        enemy.gameObject.GetComponent<SealedStone>().TakeDamage(damage1);
                }
                if (playerProfile.BloodHeal)
                    playerProfile.BloodHealHp(10, damage1);
            }
            else if (dist > 2 && dist <= 5)
            {
                StartCoroutine(TimeAttack(enemy, enemy.GetComponent<Rigidbody>()));
            }
        }
    }

    IEnumerator TimeAttack(Collider enemy, Rigidbody enemyRb)
    {
        //시간 차 공격
        yield return new WaitForSeconds(1);
        Vector3 hitPoint = enemy.ClosestPoint(transform.position);
        playerProfile.SwordSkillHit(hitPoint);
        if (enemy.CompareTag("Boss"))
        {
            Debug.Log("스킬 : 지면강타" + enemy.gameObject.name + "을(를) 공격했습니다!" + "damage2 = " + damage2);
            enemy.gameObject.GetComponent<BossStatus>().GetDamage(damage2);
        }
        else if (enemy.CompareTag("Enemy"))
        {
            Debug.Log("스킬 : 지면강타" + enemy.gameObject.name + "을(를) 공격했습니다!" + "damage2 = " + damage2);
            if (enemy.gameObject.GetComponent<MonsterBehavior>() != null)
                enemy.gameObject.GetComponent<MonsterBehavior>().TakeDamage(damage2);
            if (enemy.gameObject.GetComponent<SealStoneManager>() != null)
                enemy.gameObject.GetComponent<SealStoneManager>().Damage(damage2);
            if (enemy.gameObject.GetComponent<SealedStone>() != null)
                enemy.gameObject.GetComponent<SealedStone>().TakeDamage(damage2);

            //넉백
            enemyRb.linearVelocity = Vector3.zero;
            Vector3 dist = enemy.transform.position - transform.position;
            enemyRb.AddForce(dist * 2, ForceMode.Impulse);
            yield return new WaitForSeconds(0.5f);
            enemyRb.linearVelocity = Vector3.zero;
        }
        if (playerProfile.BloodHeal)
            playerProfile.BloodHealHp(10, damage2);
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
