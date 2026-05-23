using Unity.Cinemachine;
using UnityEngine;

public class GameState : MonoBehaviour
{
    [SerializeField] private GameObject player;
    [SerializeField] private Transform playerSpawnPoint;

    [SerializeField] private CinemachineCamera playerCamera;

    [Header("´ë»ç ÇÁ¸®ÆÕ")]
    [SerializeField] private DialogueGroup introStory;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        GameObject newPlayer = Instantiate(player, new Vector3(0, 1.9f, 0), Quaternion.identity);
        if (newPlayer != null)
        {
            playerCamera.Follow = newPlayer.transform;
            playerCamera.LookAt = newPlayer.transform;
        }
    }

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
