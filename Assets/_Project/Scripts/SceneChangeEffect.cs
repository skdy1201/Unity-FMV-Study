using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 화면 전체를 덮는 Image의 알파값으로 Fade 연출
/// </summary>
/// <remarks>
/// 페이드 중에는 fadeImage가 레이캐스트를 받아 뒤의 UI 클릭을 막는다.
/// EventSystem 전체를 끄지 않는 이유는 페이드와 무관한 입력까지 막지 않기 위해서다
/// </remarks>
public class SceneChangeEffect : MonoBehaviour
{
    #region Serialized Fields

    [SerializeField] private Image fadeImage;
    [SerializeField] private float fadeDuration = 1.5f;

    #endregion

    #region Unity Lifecycle

    private void Start()
    {
        StartCoroutine(FadeIn());
    }

    #endregion

    #region Fade

    /// <summary>
    /// 검은 화면에서 밝아지는 Fade in
    /// </summary>
    public IEnumerator FadeIn() => FadeRoutine(1f, 0f);

    private IEnumerator FadeRoutine(float from, float to)
    {
        if (fadeImage == null)
        {
            Debug.LogWarning($"{name}: fadeImage가 연결되지 않아 페이드를 건너뜀", this);
            yield break;
        }

        fadeImage.raycastTarget = true;
        Color imageColor = fadeImage.color;

        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            imageColor.a = Mathf.Lerp(from, to, elapsed / fadeDuration);
            fadeImage.color = imageColor;
            yield return null;
        }

        imageColor.a = to;
        fadeImage.color = imageColor;

        // 완전히 투명해졌을 때만 클릭을 통과시킨다
        fadeImage.raycastTarget = to > 0f;
    }

    #endregion
}
