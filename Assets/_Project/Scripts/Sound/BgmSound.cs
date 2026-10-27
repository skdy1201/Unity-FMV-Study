using UnityEngine;

namespace FMV
{
    /// <summary>
    /// 같은 오브젝트의 AudioSource로 BGM 한 곡을 반복 재생한다
    /// </summary>
    /// <remarks>
    /// UISound의 PlayOneShot은 반복 재생이 되지 않으므로 BGM은 따로 둔다.
    /// 볼륨은 들어 보며 맞추는 값이라 코드에 두지 않고 AudioSource 인스펙터에서 조절한다
    /// </remarks>
    [RequireComponent(typeof(AudioSource))]
    public class BgmSound : MonoBehaviour
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
            audioSource.clip = clip;

            // 인스펙터에서 Loop를 깜빡 꺼 두어도 BGM은 항상 반복되게 한다
            audioSource.loop = true;

            // AudioSource의 자동 재생이 이 Awake보다 먼저 돌지 보장되지 않아, 클립을 넣기 전에 재생될 수 있으므로 끄고 Start에서 직접 재생한다
            audioSource.playOnAwake = false;
        }

        /// <remarks>
        /// clip이 비어 있으면 재생할 곡이 없으므로 아무것도 하지 않는다
        /// </remarks>
        private void Start()
        {
            if (clip == null)
            {
                return;
            }

            audioSource.Play();
        }

        #endregion
    }
}
