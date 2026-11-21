using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Linq;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;

    public GameObject transitionContainer;
    private SceneTransition[] transitions;
    private bool isLoading = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if (transitionContainer != null)
        {
            transitions = transitionContainer.GetComponentsInChildren<SceneTransition>();
        }
        else
        {
            transitions = FindObjectsByType<SceneTransition>(FindObjectsSortMode.None);
        }
    }

    public void LoadScene(string sceneName, string transitionName)
    {
        if (isLoading) return;
        StartCoroutine(LoadSceneAsync(sceneName, transitionName));
    }

    private IEnumerator LoadSceneAsync(string sceneName, string transitionName)
    {
        isLoading = true;

        if (transitions == null || transitions.Length == 0)
        {
            if (transitionContainer != null)
                transitions = transitionContainer.GetComponentsInChildren<SceneTransition>();
            else
                transitions = FindObjectsByType<SceneTransition>(FindObjectsSortMode.None);
        }

        SceneTransition transition = transitions != null ? transitions.FirstOrDefault(t => t.name == transitionName) : null;
        if (transition == null && transitions != null && transitions.Length > 0)
        {
            transition = transitions[0];
        }

        AsyncOperation scene = SceneManager.LoadSceneAsync(sceneName);
        scene.allowSceneActivation = false;

        if (transition != null)
        {
            yield return transition.AnimateTransitionIn();
        }
 
        scene.allowSceneActivation = true;

        if (transition != null)
        {
            yield return transition.AnimateTransitionOut();
        }

        isLoading = false;
    }
}
