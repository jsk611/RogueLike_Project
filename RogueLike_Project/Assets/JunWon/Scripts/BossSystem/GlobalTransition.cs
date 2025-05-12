using System;
using System.Collections;
using System.Collections.Generic;

public class GlobalTransition<T>
{
    public State<T> To { get; private set; }
    public Func<bool> Condition { get; private set; }
    public List<State<T>> ExceptStates { get; private set; } // 제외할 상태 목록

    public GlobalTransition(State<T> to, Func<bool> condition, List<State<T>> exceptStates = null)
    {
        To = to;
        Condition = condition;
        ExceptStates = exceptStates ?? new List<State<T>>();
    }

    public bool IsStateExcepted(State<T> state)
    {
        return ExceptStates.Contains(state);
    }
}
