using UnityEngine;


public abstract class IntroState<T> : State<T>
{
    public IntroState(T owner) : base(owner)
    {
    }

    // 필요하다면, 공통되는 페이즈 로직을 여기에 작성
    public override void Enter()
    {
        // 페이즈 진입 시
        Debug.Log($"Enter IntroPhase<{typeof(T).Name}>");
    }

    public override void Update()
    {
    }

    public override void Exit()
    {
    }
}
