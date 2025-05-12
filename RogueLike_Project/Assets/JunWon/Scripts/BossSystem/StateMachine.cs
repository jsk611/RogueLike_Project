using System;
using System.Collections.Generic;
using System.Diagnostics;

public class StateMachine<T>
{
    public State<T> CurrentState { get; private set; }
    private List<Transition<T>> transitions = new List<Transition<T>>();
    private List<GlobalTransition<T>> globalTransitions = new List<GlobalTransition<T>>();

    public StateMachine(State<T> initialState)
    {
        CurrentState = initialState;
        CurrentState.Enter();
    }

    public void AddTransition(Transition<T> transition)
    {
        transitions.Add(transition);
    }

    public void AddGlobalTransition(State<T> to, Func<bool> condition, List<State<T>> exceptStates = null)
    {
        globalTransitions.Add(new GlobalTransition<T>(to, condition, exceptStates));
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
        // 1. 먼저 global transition 확인
        foreach (var globalTransition in globalTransitions)
        {
            // 현재 상태가 예외 상태 목록에 없고 조건이 만족되면 전환
            if (!globalTransition.IsStateExcepted(CurrentState) && globalTransition.Condition())
            {
                CurrentState.Exit();
                CurrentState = globalTransition.To;
                CurrentState.Enter();
                return; // 전환 완료 후 종료
            }
        }

        // 2. 다음으로 일반 상태 전환 확인
        foreach (var transition in transitions)
        {
            if (transition.From == CurrentState && transition.Condition())
            {
                CurrentState.Exit();
                CurrentState = transition.To;
                CurrentState.Enter();
                return; // 전환 완료 후 종료
            }
        }

        // 전환이 없으면 현재 상태 업데이트
        CurrentState.Update();
        Debug.WriteLine(CurrentState.ToString());
    }
}