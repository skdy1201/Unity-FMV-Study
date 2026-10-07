using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SceneChangeEffect : MonoBehaviour
{
    #region Serialize Fields

    [SerializeField] private Image fadeImage;
    [SerializeField] private float fadeDuration = 0.4f;
    [SerializeField] private EventSystem eventSystem;

    #endregion

    #region Fade

    public IEnumerator FadeIn() => FadeRoutine(1f, 0f);

    private IEnumerator FadeRoutine(float from, float to)
    {
        eventSystem.enabled = false;
        Color Imagecolor = fadeImage.color;

        float elapsed = 0f;

        while(elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;

            Imagecolor.a = Mathf.Lerp(from, to, elapsed/fadeDuration);
            fadeImage.color = Imagecolor;

            yield return null;
        }

        Imagecolor.a = to;
        fadeImage.color = Imagecolor;

        eventSystem.enabled = true;
    }

    #endregion

    private void Start()
    {
        StartCoroutine(FadeIn());
    }
}
