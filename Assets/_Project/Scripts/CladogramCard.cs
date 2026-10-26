using UnityEngine;

namespace FMV
{
    /// <summary>
    /// 분기도에 놓이는 카드 한 장. 분기 id와 썸네일을 가진다
    /// </summary>
    /// <remarks>
    /// 전개도를 코드로 생성할지, 씬에 배치해 둘지 정해지기 전이라 값은 setter로만 넣는다
    /// </remarks>
    public class CladogramCard : MonoBehaviour
    {
        #region Serialized Fields

        [SerializeField] private string id;
        [SerializeField] private Sprite thumbnail;

        #endregion

        #region Setter

        /// <summary>
        /// 카드가 가리킬 분기 id를 정한다
        /// </summary>
        public void SetId(string id)
        {
            this.id = id;
        }

        /// <summary>
        /// 카드에 보일 썸네일을 정한다
        /// </summary>
        public void SetThumbnail(Sprite thumbnail)
        {
            this.thumbnail = thumbnail;
        }

        #endregion
    }
}
