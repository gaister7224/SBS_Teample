using Unity.VisualScripting;
using UnityEngine;

public class DungeonEntryPortal : MonoBehaviour
{
    [SerializeField] private GameObject ui;
    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player") && DayManager.instance.curDay == Day.day)
        {
            Time.timeScale = 0f;
            UIManager.Instance.inventory.currentUI = UIType.DungeonEntry;
            UIManager.Instance.inventory.playerProfile.SetActive(false);
            UIManager.Instance.inventory.playerAttack.uiClicking = true;
            ui.SetActive(true);
        }
    }

    public void Back()
    {
        Time.timeScale = 1f;
        UIManager.Instance.inventory.currentUI = UIType.None;
        UIManager.Instance.inventory.playerProfile.SetActive(true);
        UIManager.Instance.inventory.playerAttack.uiClicking = false;
        ui.SetActive(false);
    }
}
