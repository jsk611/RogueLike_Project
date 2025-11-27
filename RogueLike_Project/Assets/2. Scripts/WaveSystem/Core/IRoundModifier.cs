using System.Collections.Generic;
using UnityEngine;

namespace WaveSystem
{
    /// <summary>
    /// 라운드 변화 타입 정의
    /// </summary>
    public enum RoundModifierType
    {
        FireTrap,           // 불장판 기믹
        NewEnemy,           // 신규 적 추가
        RandomChange,       // 랜덤 변화
        NewMission,         // 신규 미션
        BlockDeletion,      // 블럭 삭제 기믹
        None
    }

    /// <summary>
    /// 라운드 변화를 적용하는 인터페이스
    /// 각 변화 타입은 이 인터페이스를 구현하여 독립적으로 동작
    /// </summary>
    public interface IRoundModifier
    {
        /// <summary>
        /// 변화 타입
        /// </summary>
        RoundModifierType ModifierType { get; }

        /// <summary>
        /// 변화 적용 (라운드 시작 시)
        /// </summary>
        void ApplyModifier(RoundContext context);

        /// <summary>
        /// 변화 제거 (라운드 종료 시, 필요한 경우)
        /// </summary>
        void RemoveModifier(RoundContext context);

        /// <summary>
        /// 변화가 현재 활성화되어 있는지
        /// </summary>
        bool IsActive { get; }

        /// <summary>
        /// 변화 검증 (에디터용)
        /// </summary>
        bool Validate(out string errorMessage);
    }

    /// <summary>
    /// 라운드 컨텍스트 - 라운드 실행 정보 전달
    /// </summary>
    public class RoundContext
    {
        public int currentRound;
        public int currentStage;
        public GameObject waveManagerObject;
        public Transform mapParent;

        // 동적으로 추가될 수 있는 데이터
        private Dictionary<string, object> additionalData = new Dictionary<string, object>();

        public RoundContext(int round, int stage, GameObject waveManager, Transform map)
        {
            currentRound = round;
            currentStage = stage;
            waveManagerObject = waveManager;
            mapParent = map;
        }

        public void SetData(string key, object value)
        {
            additionalData[key] = value;
        }

        public T GetData<T>(string key, T defaultValue = default)
        {
            if (additionalData.TryGetValue(key, out object value) && value is T typedValue)
            {
                return typedValue;
            }
            return defaultValue;
        }

        public bool HasData(string key)
        {
            return additionalData.ContainsKey(key);
        }
    }
}