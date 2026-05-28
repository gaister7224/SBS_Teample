using UnityEngine;

public class TrapRemover : MonoBehaviour
{
    StageManager stageManager;

    void Start()
    {
        stageManager = GameObject.Find("StageManager").GetComponent<StageManager>();
    }

    void Update()
    {
        //if (stageManager.curStagePos.x * stageManager.spacing != transform.localPosition.x || stageManager.curStagePos.y * stageManager.spacing != transform.localPosition.z)
        //{
        //    Destroy(gameObject);
        //}

        if(stageManager.curStageType != StageType.Trap)
        {
            Destroy(gameObject);
        }
    }
}
