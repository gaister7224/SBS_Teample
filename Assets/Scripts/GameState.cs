using Unity.Cinemachine;
using UnityEngine;

public class GameState : MonoBehaviour
{
    [SerializeField] private GameObject player;
    [SerializeField] private Transform playerSpawnPoint;

    [SerializeField] private CinemachineCamera playerCamera;

    [Header("ùù?ùùùùùù")]
    [SerializeField] private DialogueGroup introStory;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        MinimapManager.BootstrapForMainScene();
        if (player == null)
        {
            Debug.LogError("GameState: player prefab is not assigned.");
            return;
        }

        var spawnPosition = ResolveSpawnPosition();
        var newPlayer = Instantiate(player, spawnPosition, Quaternion.identity);
        if (newPlayer == null)
            return;

        if (playerCamera != null)
        {
            playerCamera.Follow = newPlayer.transform;
            playerCamera.LookAt = newPlayer.transform;
        }
        else
        {
            Debug.LogError("GameState: playerCamera is not assigned.");
        }

        var mainFollow = Object.FindFirstObjectByType<MainFollowPC>();
        if (mainFollow != null)
            mainFollow.BindPlayerCamera(playerCamera != null ? playerCamera.transform : null);
    }

    static Vector3 ResolveSpawnPosition(Transform spawnPoint)
    {
        if (spawnPoint != null)
            return spawnPoint.position;

        var entry = GameObject.FindGameObjectWithTag("DungeonEntry");
        if (entry != null)
            return entry.transform.position;

        var spawnByName = GameObject.Find("PlayerSpawnPoint")
            ?? GameObject.Find("PlayerSpawnPoint (1)");
        if (spawnByName != null)
            return spawnByName.transform.position;

        return new Vector3(0f, 1.9f, 0f);
    }

    Vector3 ResolveSpawnPosition() => ResolveSpawnPosition(playerSpawnPoint);

    private void Start()
    {
        GameManager.instance.mapState = MapState.Stage;
        Invoke("StartDialogue", 0.2f);
    }

    private void StartDialogue()
    {
        if (!DialogueManager.instance.start)
        {
            DialogueManager.instance.OnDialogue(introStory);
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
