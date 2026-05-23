using System.Collections;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DungeonEntryManager : MonoBehaviour, IPointerClickHandler
{
    public DungeonData data;

    public GameObject spawnedDungeonInstance; //이미 생성된 던진이 있는지 체크

    //[SerializeField] private GameObject map;
    [SerializeField] private GameObject ui;
    //[SerializeField] private Transform mapSpawnPos;
    //[SerializeField] private int dungeonNumber;
    private GameObject player;
    private PlayerProfile playerProfile;
    private Image backGroundImage;

    private void OnEnable()
    {
        backGroundImage = GetComponent<Image>();
        if (GameManager.instance.possibleDungeon[data.dungeonNumber - 1])
        {
            backGroundImage.color = Color.white;
        }
        else
        {
            backGroundImage.color = Color.gray;
        }
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        if (GameManager.instance.possibleDungeon[data.dungeonNumber - 1] && GameManager.instance.dayEnd)
        {
            if(spawnedDungeonInstance == null)
            {
                spawnedDungeonInstance = Instantiate(data.mapPrefab, data.spawnOffset, Quaternion.identity);
            }

            Time.timeScale = 1f;
            
            GameManager.instance.curDungeonNumber = data.dungeonNumber;
            
            GameManager.instance.dayEnd = false;
            GameManager.instance.itemGetAll = false;
            GameManager.instance.mapState = MapState.Stage;
            MinimapManager.ApplyMainSceneMinimapMode();

            if (DungeonMapService.Instance == null)
            {
                var serviceObject = new GameObject("DungeonMapService");
                serviceObject.AddComponent<DungeonMapService>();
            }
            DungeonMapService.Instance.LoadDungeon(data.dungeonNumber);
            DungeonMapUiInstaller.EnsureMapBoardUi();

            GameManager.instance.spawnedDungeon = this;

            StartCoroutine(PlayerMove(0.5f));
        }
    }

    private IEnumerator PlayerMove(float delay)
    {
        yield return new WaitForSeconds(delay);

        player = GameObject.FindGameObjectWithTag("Player");
        playerProfile = player.GetComponent<PlayerProfile>();

        Transform entryTransform = null;

        foreach(Transform child in spawnedDungeonInstance.GetComponentInChildren<Transform>())
        {
            if(child.CompareTag("DungeonEntry"))
            {
                entryTransform = child;
                break;
            }
        }

        if(entryTransform != null )
        {
            player.transform.position = entryTransform.position;
        }
        else
        {
            Debug.Log("입구 없음");
        }

        UIManager.Instance.inventory.currentUI = UIType.None;
        UIManager.Instance.inventory.playerProfile.SetActive(true);
        UIManager.Instance.inventory.playerAttack.uiClicking = false;
        playerProfile.AnimationReset();
        ui.SetActive(false);
    }
}
