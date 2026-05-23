using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

public class StageClearManager : MonoBehaviour
{
    [SerializeField] private GameObject jobChoiceUI;

    void Start()
    {
        jobChoiceUI = GameObject.Find("JobChoiceUI");
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            Debug.Log("clear");
            if(!GameManager.instance.tutorialClear)
            {
                Time.timeScale = 0;
                UIManager.Instance.jobChoiceUI.SetActive(true);
                other.GetComponent<PlayerAttack>().uiClicking = true;
                UIManager.Instance.inventory.playerProfile.SetActive(false);
                GameManager.instance.possibleDungeon[0] = true;
                GameManager.instance.tutorialClear = true;
                GameManager.instance.statusPoint++;
                GameManager.instance.curLevel++;
                GameObject tMap = GameObject.FindGameObjectWithTag("Map");
                Destroy(tMap);
                DayManager.instance.sunLight.transform.rotation
                = Quaternion.Euler(DayManager.instance.nightSunRotation);
                DayManager.instance.curDay = Day.night;
                DayManager.instance.NightIconAppear();
                return;
            }
            other.gameObject.GetComponent<PlayerProfile>().BuffStoneRelease();
            GameManager.instance.possibleDungeon[GameManager.instance.curDungeonNumber] = true;
            GameManager.instance.statusPoint++;
            GameManager.instance.curLevel++;
            
            GameManager.instance.mapState = MapState.Village;
            MinimapManager.ApplyMainSceneMinimapMode();
            DungeonMapService.Instance?.FlushSave();
            UIManager.Instance.virtualCamera.GetComponent<CinemachineConfiner3D>().BoundingVolume
                = UIManager.Instance.villageCollider;
            DayManager.instance.sunLight.transform.rotation
                = Quaternion.Euler(DayManager.instance.nightSunRotation);
            DayManager.instance.curDay = Day.night;
            DayManager.instance.NightIconAppear();
            //GameObject map = GameObject.FindGameObjectWithTag("Map");
            //Destroy(map);
            GameManager.instance.spawnedDungeon = null;
            GameManager.instance.spawnedDungeon.spawnedDungeonInstance = null;
            MapDestroy();
            GameObject.FindGameObjectWithTag("Player").transform.position = UIManager.Instance.villagePos.position;
        }
    }

    private void MapDestroy()
    {
        GameObject[] mapObjs = GameObject.FindGameObjectsWithTag("Map");
        GameObject nearestEntry = null;
        float minDistance = Mathf.Infinity;
        Vector3 currentPos = transform.position;
        foreach (GameObject map in mapObjs)
        {
            float distance = Vector3.Distance(map.transform.position, currentPos);
            if (distance < minDistance)
            {
                minDistance = distance;
                nearestEntry = map;
            }
        }
        if (nearestEntry != null)
        {
            Destroy(nearestEntry);
        }
    }
}
