using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 마을 HUD에서 던전 입장 포탈 UI(DungeonPortal)만 제어합니다. InfoUI(스테이터스)와 분리합니다.
/// </summary>
public static class VillageDungeonEntryUi
{
    const string DungeonPortalPanelName = "DungeonPortal";
    const string InfoUiPanelName = "InfoUI";

    public static void ApplyVillageExplorationHud()
    {
        SetPanelActive(InfoUiPanelName, false);
        SetPanelActive(DungeonPortalPanelName, false);
    }

    public static bool ShowDungeonEntryMenu()
    {
        var portal = FindPanel(DungeonPortalPanelName);
        if (portal == null)
            return false;

        SetPanelActive(InfoUiPanelName, false);
        portal.SetActive(true);
        return true;
    }

    public static void HideDungeonEntryMenu()
    {
        SetPanelActive(DungeonPortalPanelName, false);
        SetPanelActive(InfoUiPanelName, false);
    }

    static void SetPanelActive(string panelName, bool active)
    {
        var panel = FindPanel(panelName);
        if (panel != null)
            panel.SetActive(active);
    }

    static GameObject FindPanel(string panelName)
    {
        var scene = SceneManager.GetActiveScene();
        foreach (var root in scene.GetRootGameObjects())
        {
            foreach (var transform in root.GetComponentsInChildren<Transform>(true))
            {
                if (transform.name == panelName)
                    return transform.gameObject;
            }
        }

        return null;
    }
}
