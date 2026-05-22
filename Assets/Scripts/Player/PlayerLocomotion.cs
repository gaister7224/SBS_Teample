using UnityEngine;

/// <summary>
/// Rigidbody 플레이어 텔레포트·이동 보조.
/// </summary>
public static class PlayerLocomotion
{
    public static void Teleport(Transform player, Vector3 worldPosition)
    {
        if (player == null)
            return;

        var rb = player.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.position = worldPosition;
            Physics.SyncTransforms();
            rb.WakeUp();
        }
        else
        {
            player.position = worldPosition;
        }
    }

    public static void TeleportOffset(Transform player, Vector3 offset)
    {
        if (player == null)
            return;

        Teleport(player, player.position + offset);
    }

    public static PlayerProfile GetProfile(Component from)
    {
        if (from == null)
            return null;

        return from.GetComponentInParent<PlayerProfile>();
    }

    public static PlayerProfile GetProfile(GameObject from)
    {
        if (from == null)
            return null;

        return from.GetComponentInParent<PlayerProfile>();
    }

    public static GameObject ResolvePlayerObject()
    {
        if (StageManager.instance != null && StageManager.instance.Player != null)
            return StageManager.instance.Player;

        return GameObject.FindWithTag("Player");
    }
}
