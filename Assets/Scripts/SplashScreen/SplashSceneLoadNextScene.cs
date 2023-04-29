using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SplashSceneLoadNextScene : MonoBehaviour
{
    public float delay = 5;
    public float fadeTime = 1;

    CanvasGroup group;

    // Start is called before the first frame update
    void Start()
    {
        group = GetComponent<CanvasGroup>();
        StartCoroutine(LoadNextScene());
    }

    IEnumerator LoadNextScene()
    {
        yield return new WaitForSeconds(delay);

        SceneManager.LoadScene(1,LoadSceneMode.Additive);

        float t = 0;
        while (t < 1)
        {
            t += Time.deltaTime / fadeTime;

            group.alpha = Curves.QuintEaseIn(1, 0, Mathf.Clamp01(t));

            yield return null;
        }

        SceneManager.UnloadSceneAsync(0);
    }
}
