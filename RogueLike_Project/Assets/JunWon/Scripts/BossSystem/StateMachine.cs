using System.Collections.Generic;
using System.Diagnostics;

public class StateMachine<T>
{
    public State<T> CurrentState { get; private set; }
    private List<Transition<T>> transitions = new List<Transition<T>>();

    public StateMachine(State<T> initialState)
    {
        CurrentState = initialState;
        CurrentState.Enter();
    }

    public void AddTransition(Transition<T> transition)
    {
        transitions.Add(transition);
    }

    public void ForcedTransition(State<T> state)
    {
        if(state!=null)
        {
            CurrentState.Exit();
            CurrentState = state;
            CurrentState.Enter();
        }
    }

    public void Update()
    {
        // 상위 FSM 전환 조건 확인
        foreach (var transition in transitions)
        {
            if (transition.From == null ||
                (transition.From == CurrentState && transition.Condition()))
            {
                CurrentState.Exit();
                CurrentState = transition.To;
                CurrentState.Enter();
                break;
            }
        }

        // 현재 상태의 Update (CompositeState라면, 내부에서 SubState도 처리)
        CurrentState.Update();
        Debug.WriteLine(CurrentState.ToString());
    }
}