using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// UI(EventSystem)가 WASD를 가로채 플레이어 이동이 멈추는 것을 방지합니다.
/// </summary>
public static class GameplayInputUtility
{
    public static void ReleaseUiFocus()
    {
        if (EventSystem.current == null)
            return;

        EventSystem.current.SetSelectedGameObject(null);
    }

    public static bool IsGameplayInputAllowed(InventoryMain inventory)
    {
        if (Time.timeScale <= 0f)
            return false;

        if (inventory != null && inventory.currentUI != UIType.None)
            return false;

        return true;
    }
}
