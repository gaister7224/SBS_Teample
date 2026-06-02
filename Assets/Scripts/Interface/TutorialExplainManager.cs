using TMPro;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public struct TitleExplainData
{
    public string title;
    public Sprite keyboard;
}

public class TutorialExplainManager : MonoBehaviour
{
    public static TutorialExplainManager instance;

    public TitleExplainData[] data;

    private Animator ani;

    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private Image keyboardImage;

    [SerializeField] private int count;

    private void Awake()
    {
        instance = this;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ani = GetComponent<Animator>();
        DialogueManager.instance.OnDialogueComplete += Appear;
    }

    public void Appear()
    {
        if(count == 8)
        {
            this.gameObject.SetActive(false);
        }
        titleText.text = data[count].title;
        keyboardImage.sprite = data[count].keyboard;
        ani.SetBool("Appear", true);
        ani.SetBool("Back", false);

        if(count > 0)
        {
            keyboardImage.gameObject.GetComponent<RectTransform>().localScale = new Vector3(1.8f, 1.8f, 1.8f);
            keyboardImage.gameObject.GetComponent<RectTransform>().localPosition = new Vector3(-73, 150, 0);
        }
    }

    public void Back()
    {
        ani.SetBool("Appear", false);
        ani.SetBool("Back", true);

        count++;

        if(count <= 2)
        {
            Invoke("Appear", 1);
        }
    }
}
