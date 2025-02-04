using UnityEngine;

public abstract class BossPhaseBase<T> : State<T>
{
    // 생성자
    public BossPhaseBase(T owner) : base(owner)
    {
    }

    // 필요하다면, 공통되는 페이즈 로직을 여기에 작성
    public override void Enter()
    {
        // 페이즈 진입 시
        Debug.Log($"Enter BossPhaseBase<{typeof(T).Name}>");
    }

    public override void Update()
    {
        // 상위(페이즈)에서 공통으로 처리할 로직
        // 예: HP 체크, 페이즈 전환 조건 등
    }

    public override void Exit()
    {
        Debug.Log($"Exit BossPhaseBase<{typeof(T).Name}>");
    }
}
