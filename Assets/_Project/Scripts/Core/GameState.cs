namespace FMV
{
    /// <summary>
    /// 현재 단계, 최대 단계, 실패 횟수 같은 진행 기록을 save.json 데이터로 읽고 바꾼다.
    /// 저장은 같은 객체를 들고 있는 SaveController가 맡는다.
    /// 분기 안의 진행 단계는 저장하지 않는 GamePhase로 따로 갖고 있다
    /// </summary>
    /// <remarks>
    /// GameController가 소유하는 일반 클래스라 씬 오브젝트 없이 쓴다.
    /// 기록은 SaveController가 불러온 JsonSaveData와 같은 객체를 GameController가 넘겨주므로, 여기서 바꾼 값이 곧 저장될 값이다.
    /// 단계는 분기 id("2a" 같은 값)로 들고 있어 Graph·GameData와 같은 값을 쓴다
    /// </remarks>
    public class GameState
    {
        #region Private Fields

        private GamePhase gamePhase = new GamePhase();
        private JsonSaveData gameSave;

        #endregion

        #region Properties

        /// <summary>
        /// 분기 안의 진행 단계. 저장하지 않는다
        /// </summary>
        public GamePhase GamePhase => gamePhase;

        /// <summary>
        /// 저장 기록 전체. GameController가 Init 전에 SaveController가 불러온 기록을 넣는다
        /// </summary>
        public JsonSaveData GameSave
        {
            get => gameSave;
            set => gameSave = value;
        }

        /// <summary>
        /// 지금까지 가장 멀리 간 분기 id. 기록이 없으면 빈 문자열
        /// </summary>
        public string MaxGrade => gameSave.Progress.MaxGradeId;

        /// <summary>
        /// 마지막으로 도달한 분기 id. 기록이 없으면 빈 문자열
        /// </summary>
        public string NewestGrade => gameSave.Progress.NewestId;

        /// <summary>
        /// 최근에 들어간 Action(선택지·QTE) 분기 id. 기록이 없으면 빈 문자열
        /// </summary>
        /// <remarks>
        /// 배드 엔딩은 선택에서 갈라지므로, 실패로 끝났을 때 이 분기로 되돌아간다
        /// </remarks>
        public string CheckpointGrade
        {
            get => gameSave.Progress.LastSelect;
            set => gameSave.Progress.LastSelect = value;
        }

        /// <summary>
        /// 누적 실패 횟수
        /// </summary>
        public int FallCount => gameSave.Progress.FallCount;

        #endregion

        #region Progress

        /// <summary>
        /// 현재 단계를 도달한 분기 id로 바꾸고, 방문한 분기로 기록한다.
        /// 최대 단계보다 앞선 단계면 최대 단계도 갱신한다
        /// </summary>
        /// <remarks>
        /// 같은 단계 안의 갈래("2a", "2b")는 앞서고 뒤서는 관계가 없으므로, 먼저 기록된 쪽을 유지한다.
        /// 빈 문자열은 엔딩 뒤 처음부터 시작하라는 뜻이라 방문 기록에 넣지 않는다
        /// </remarks>
        /// <param name="id"> 도달한 분기 id </param>
        public void UpdateGrade(string id)
        {
            JsonProgressInfo progress = gameSave.Progress;

            progress.NewestId = id;

            if (!string.IsNullOrEmpty(id) && !progress.Visited.Contains(id))
            {
                progress.Visited.Add(id);
            }

            if (ParseStage(id) > ParseStage(progress.MaxGradeId))
            {
                progress.MaxGradeId = id;
            }
        }

        /// <summary>
        /// 실패 횟수를 1 올린다
        /// </summary>
        public void AddFall()
        {
            ++gameSave.Progress.FallCount;
        }

        #endregion

        #region Helpers

        /// <remarks>
        /// "2a"는 2, "10"은 10처럼 앞쪽 숫자만 읽는다
        /// </remarks>
        /// <returns> 숫자로 시작하지 않으면 0 </returns>
        private static int ParseStage(string id)
        {
            int stage = 0;

            foreach (char c in id)
            {
                if (!char.IsDigit(c))
                {
                    break;
                }

                stage = stage * 10 + (c - '0');
            }

            return stage;
        }

        #endregion
    }
}
