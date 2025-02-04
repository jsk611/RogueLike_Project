using System.Collections.Generic;

public abstract class CompositeState<T> : State<T>
{
    protected State<T> currentSubState;
    protected List<Transition<T>> subTransitions = new List<Transition<T>>();

    public CompositeState(T owner) : base(owner) { }

    // 하위 상태를 변경하는 메서드
    public void ChangeSubState(State<T> newSubState)
    {
        currentSubState?.Exit();
        currentSubState = newSubState;
        currentSubState.Enter();
    }

    // CompositeState 자체의 Update -> 하위 상태의 Update도 호출
    public override void Update()
    {
        base.Update();     // 상위 상태(CompositeState)에서 공통으로 처리할 로직
        currentSubState?.Update();

        // 하위 상태 전환 조건 확인
        foreach (var transition in subTransitions)
        {
            if (transition.From == currentSubState && transition.Condition())
            {
                ChangeSubState(transition.To);
                break;
            }
        }
    }
}