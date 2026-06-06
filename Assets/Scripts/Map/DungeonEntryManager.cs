using System.Collections;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class DungeonEntryManager : MonoBehaviour, IPointerClickHandler
{
    public DungeonData data;

    public GameObject spawnedDungeonInstance;

    [SerializeField] private GameObject ui;
    private GameObject player;
    private PlayerProfile playerProfile;
    private Image backGroundImage;

    private const float MAP_SPACING = 500f; //맵 끼리 안 겹치게 하는 고정 간격

    private void OnEnable()
    {
        backGroundImage = GetComponent<Image>();

        if(data == null || GameManager.instance == null)
        {
            Debug.Log("필요한 instance 없음");
            return;
        }

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
            GameManager.instance.spawnedDungeon = this;
            if (spawnedDungeonInstance == null)
            {
                Vector3 autoSpawnPosition = new Vector3(data.dungeonNumber * MAP_SPACING, 0f, 0f);

                spawnedDungeonInstance = Instantiate(data.mapPrefab, autoSpawnPosition, Quaternion.identity);
            }

            Time.timeScale = 1f;
            
            GameManager.instance.curDungeonNumber = data.dungeonNumber;
            GameManager.instance.curDungeonFloorNumber = data.floor;
            
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
            
            if(child.CompareTag("NoneStage"))
            {
                Transform entryChild = child.GetComponentInChildren<Transform>();
                foreach (Transform _child in entryChild)
                {
                    if (_child.CompareTag("DungeonEntry"))
                    {
                        entryTransform = _child;
                        break;
                    }
                }
                    
            }
        }

        if(entryTransform != null )
        {
            player.transform.position = entryTransform.position;
        }
        else
        {
            Debug.Log("entryTransform null");
        }

        yield return null;

        const int maxFrames = 30;
        for (var frame = 0; frame < maxFrames; frame++)
        {
            if (DungeonMapLayoutResolver.CollectStagePositions().Count > 0)
                break;

            yield return null;
        }

        DungeonMapLayoutResolver.SyncAfterLayoutChange(clearVisibility: true);

        UIManager.Instance.inventory.currentUI = UIType.None;
        UIManager.Instance.inventory.playerProfile.SetActive(true);
        UIManager.Instance.inventory.playerAttack.uiClicking = false;
        StoreManager.Instance.ReFreshShop();
        playerProfile.AnimationReset();
        DayManager.instance.sunLight.transform.rotation
                = Quaternion.Euler(DayManager.instance.nightSunRotation);
        ui.SetActive(false);
    }
}
