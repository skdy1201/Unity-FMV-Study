using UnityEngine;

namespace FMV
{
    /// <summary>
    /// 같은 오브젝트의 AudioSource로 UI 효과음 하나를 재생한다
    /// </summary>
    /// <remarks>
    /// 프리팹은 씬 오브젝트를 참조할 수 없으므로, 소리를 내는 오브젝트가 자기 AudioSource를 직접 들고 있게 한다
    /// </remarks>
    [RequireComponent(typeof(AudioSource))]
    public class UISound : MonoBehaviour
    {
        #region Serialized Fields

        [SerializeField] private AudioClip clip;

        #endregion

        #region Private Fields

        private AudioSource audioSource;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();

            // 효과음은 Play를 부를 때만 내야 하므로, 오브젝트가 켜질 때 저절로 재생되지 않게 한다
            audioSource.playOnAwake = false;
        }

        #endregion

        #region Sound

        /// <summary>
        /// 지정한 효과음을 한 번 재생한다
        /// </summary>
        public void Play()
        {
            Play(clip);
        }

        /// <summary>
        /// 넘겨받은 효과음을 한 번 재생한다
        /// </summary>
        /// <remarks>
        /// 결과마다 소리가 다른 ActionUIController처럼 클립을 여러 개 쓰는 곳에서 부른다.
        /// PlayOneShot은 앞 소리를 끊지 않고 겹쳐 재생하므로, 빠르게 연속으로 눌러도 소리가 잘리지 않는다.
        /// clip이 비어 있으면 PlayOneShot이 에러를 내므로 아무것도 하지 않는다
        /// </remarks>
        /// <param name="playClip"> 재생할 효과음 </param>
        public void Play(AudioClip playClip)
        {
            if (playClip == null)
            {
                return;
            }

            audioSource.PlayOneShot(playClip);
        }

        #endregion
    }
}
