using UnityEngine;

/// <summary>
/// VillageDemo 등 단독 실행 씬에서 GameManager가 없을 때 최소 인스턴스를 생성합니다.
/// </summary>
[DefaultExecutionOrder(-100)]
public class DemoSceneBootstrap : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void EnsureOnSceneLoad()
    {
        var sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        if (!sceneName.Contains("Village") && !sceneName.Contains("Vilage"))
            return;

        EnsureGameManager();
    }

    void Awake()
    {
        EnsureGameManager();
    }

    public static void EnsureGameManager()
    {
        if (GameManager.instance != null)
            return;

        var go = new GameObject("GameManager");
        var gm = go.AddComponent<GameManager>();
        gm.nickName = "Player";
        gm.level = 1;
        gm.curLevel = 0;
        gm.mapState = MapState.Village;
        gm.maxActCount = 10;
        gm.curActCount = 10;
        gm.possibleDungeon = new bool[11];
    }
}
