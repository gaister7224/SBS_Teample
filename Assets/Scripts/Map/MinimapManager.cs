using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class MinimapManager : MonoBehaviour
{
    public static MinimapManager instance;

    public GameObject MinimapImage;
    public Camera MinimapCamera;
    public Canvas cv;

    public HashSet<Vector2Int> MapStagePositions = new HashSet<Vector2Int>();
    public Vector2Int curSelectedStage;
    private Transform player;

    StageManager stageManager;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        stageManager = GameObject.Find("StageManager").GetComponent<StageManager>();
    }

    void Update()
    {
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player").transform;
        }

        if (Input.GetKey(KeyCode.Tab))
        {
            MinimapCamera.orthographicSize = 200f;
            MinimapCamera.transform.position = new Vector3(0, 30, 0);

            MinimapImage.transform.localScale = new Vector3(3.5f, 3.5f, 1);
            MinimapImage.transform.localPosition = new Vector3(0, 0, 0);
        }
        else
        {
            MinimapCamera.orthographicSize = 50f;
            MinimapCamera.transform.position = new Vector3(player.position.x, transform.position.y, player.position.z);

            MinimapImage.transform.localScale = new Vector3(1, 1, 1);
            MinimapImage.transform.localPosition = new Vector3(800, 400, 0);
        }
    }
}
