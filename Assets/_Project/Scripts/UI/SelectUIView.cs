using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace FMV
{
    /// <summary>
    /// SelectUI 프리팹의 선택지 버튼을 노드 선택지 수에 맞게 켜고 문구를 바꾼다
    /// </summary>
    /// <remarks>
    /// 버튼은 프리팹에 최대 개수만큼 미리 두고, 쓰지 않는 버튼은 꺼서 LayoutGroup 배치에서 빠지게 한다.
    /// 씬의 ActionUIController는 프리팹 복제본의 버튼을 Inspector로 참조할 수 없으므로, 버튼 등록은 프리팹 루트의 이 컴포넌트가 맡는다
    /// </remarks>
    public class SelectUIView : MonoBehaviour
    {
        #region Serialized Fields

        [SerializeField] private List<GameObject> optionButtons = new List<GameObject>();

        #endregion

        #region Private Fields

        private readonly List<TMP_Text> optionLabels = new List<TMP_Text>();

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            // 버튼은 프리팹에서 고정이므로, 문구를 바꿀 때마다 찾지 않도록 한 번만 캐싱한다
            foreach (GameObject button in optionButtons)
            {
                optionLabels.Add(button.GetComponentInChildren<TMP_Text>(true));
            }
        }

        #endregion

        #region Options

        /// <summary>
        /// 선택지 수만큼 버튼을 켜고 문구를 바꾸며, 남는 버튼은 끈다
        /// </summary>
        /// <param name="options"> 표시할 노드 선택지 </param>
        public void SetOptions(JsonNodeOption[] options)
        {
            if (options.Length > optionButtons.Count)
            {
                Debug.LogError($"Not enough option buttons: {options.Length} options, {optionButtons.Count} buttons");
            }

            for (int i = 0; i < optionButtons.Count; i++)
            {
                bool hasOption = i < options.Length;
                optionButtons[i].SetActive(hasOption);

                if (hasOption)
                {
                    optionLabels[i].text = options[i].Label;
                }
            }
        }

        #endregion
    }
}
