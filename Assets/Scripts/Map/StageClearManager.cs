using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

public class StageClearManager : MonoBehaviour
{
    [SerializeField] private GameObject jobChoiceUI;
    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            Debug.Log("clear");
            if(!GameManager.instance.tutorialClear)
            {
                Time.timeScale = 0;
                jobChoiceUI.SetActive(true);
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

            GameObject.FindGameObjectWithTag("Player").transform.position = UIManager.Instance.villagePos.position;
            GameManager.instance.mapState = MapState.Village;
            DungeonMapService.Instance?.FlushSave();
            UIManager.Instance.virtualCamera.GetComponent<CinemachineConfiner3D>().BoundingVolume
                = UIManager.Instance.villageCollider;
            DayManager.instance.sunLight.transform.rotation
                = Quaternion.Euler(DayManager.instance.nightSunRotation);
            DayManager.instance.curDay = Day.night;
            DayManager.instance.NightIconAppear();
            GameObject map = GameObject.FindGameObjectWithTag("Map");
            Destroy(map);
        }
    }
}
