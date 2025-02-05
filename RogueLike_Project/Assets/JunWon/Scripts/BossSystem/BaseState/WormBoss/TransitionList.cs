

using static UnityEngine.GraphicsBuffer;
using UnityEngine;
using System.Collections.Generic;

public abstract class TransitionList<T>
{
    protected T owner;
    protected List<Transition<T>> transitions = new List<Transition<T>>();
    protected StateMachine<T> fsm;

    public TransitionList(T owner)
    {
        this.owner = owner;
    }
    public StateMachine<T> returnMachine()
    {
        return fsm;
    }
    protected abstract void AddTransition();
}
