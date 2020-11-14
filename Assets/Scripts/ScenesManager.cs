using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class ScenesManager : MonoBehaviour
{
    private static ScenesManager _instance = default;
    static string[] scenesName ={
        "ClockScene",
        "GraphScene",
        "MovementScene",
        "MovementPhysicScene",
        "MovementSlopesScene"
    };

    [SerializeField]
    Button nextButton = default, prevButton = default;

    private int currentSceneIndex;
    int getCurrentSceneIndex()
    {
        string currenSceneName = SceneManager.GetActiveScene().name;
        for (int i = 0; i < scenesName.Length; ++i)
        {
            if (currenSceneName.Equals(scenesName[i]))
            {
                return i;
            }
        }
        return -1;
    }

    void Awake()
    {
        if (_instance != null)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        setButtonListener();
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        checkButtonState();
    }

    void setButtonListener()
    {
        nextButton.onClick.AddListener(NextScene);
        prevButton.onClick.AddListener(PrevScene);
    }

    void checkButtonState()
    {
        currentSceneIndex = getCurrentSceneIndex();
        if (currentSceneIndex >= scenesName.Length - 1)
        {
            nextButton.gameObject.SetActive(false);
        }
        else
        {
            nextButton.gameObject.SetActive(true);
        }
        if (currentSceneIndex <= 0)
        {
            prevButton.gameObject.SetActive(false);
        }
        else
        {
            prevButton.gameObject.SetActive(true);
        }
    }
    void NextScene()
    {
        currentSceneIndex = getCurrentSceneIndex();
        if (currentSceneIndex < scenesName.Length - 1)
        {
            LoadSceneAsync(currentSceneIndex + 1);
        }
    }

    void PrevScene()
    {
        currentSceneIndex = getCurrentSceneIndex();
        if (currentSceneIndex > 0)
        {
            LoadSceneAsync(currentSceneIndex - 1);
        }
    }

    private void LoadSceneAsync(int index)
    {
        StartCoroutine(LoadLevelAsync(index));
    }

    private IEnumerator LoadLevelAsync(int index)
    {
        yield return SceneManager.LoadSceneAsync(scenesName[index]);
        checkButtonState();
    }
}
