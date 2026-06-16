using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public enum SceneNames { TitleScene = 0, MainScene }

public class SceneLoader : MonoBehaviour
{
    public static SceneLoader instance;

    [SerializeField] private GameObject loadingScreen; //로딩 화면
    [SerializeField] private Image loadingBackground; //배경 이미지
    [SerializeField] private Slider loadingProgress; //로딩 진행도
    [SerializeField] private TextMeshProUGUI textProgress; //로딩 진행도 텍스트

    private WaitForSeconds waitChangeDelay; //씬 변경 지연 시간

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
            waitChangeDelay = new WaitForSeconds(0.5f);

            DontDestroyOnLoad(gameObject);

            SceneManager.sceneLoaded += OnSceneLoaded;
        }
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    public void LoadScene(string name)
    {
        loadingScreen.SetActive(true);

        loadingProgress.value = 0f;

        StartCoroutine(LoadSceneAsync(name));
    }

    public void LoadScene(SceneNames name)
    {
        LoadScene(name.ToString());
    }

    private IEnumerator LoadSceneAsync(string name)
    {
        yield return null;

        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(name); //비동기 작업 상태

        asyncOperation.allowSceneActivation = false;

        //비동기 작업(씬 불러오기)이 완료될 때까지 반복
        float timer = 0f;
        while (!asyncOperation.isDone)
        {
            yield return null;

            timer += Time.unscaledDeltaTime; // timeScale = 0 상태여도 굴러가도록 처리

            float targetProgress = asyncOperation.progress >= 0.9f ? 1f : asyncOperation.progress;

            loadingProgress.value = Mathf.MoveTowards(loadingProgress.value, targetProgress, timer * 0.05f);
            textProgress.text = $"{Mathf.RoundToInt(loadingProgress.value * 100f)}%";

            if (loadingProgress.value >= 1f && asyncOperation.progress >= 0.9f)
            {
                yield return waitChangeDelay;
                asyncOperation.allowSceneActivation = true;
                
            }
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if(loadingScreen != null)
        {
            loadingScreen.SetActive(false);
        }
    }
}

