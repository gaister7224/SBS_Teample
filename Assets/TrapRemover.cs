using UnityEngine;

public class TrapRemover : MonoBehaviour
{
    StageManager stageManager;

    void Start()
    {
        stageManager = GetComponentInParent<StageManager>();
    }

    void Update()
    {
        if (stageManager.curStagePos.x * stageManager.spacing != transform.localPosition.x || stageManager.curStagePos.y * stageManager.spacing != transform.localPosition.y)
        {
            Destroy(gameObject);
        }
    }
}
