using System;

public class Transition<T>
{
    public State<T> From { get; private set; }
    public State<T> To { get; private set; }
    public Func<bool> Condition { get; private set; }

    public Transition(State<T> from, State<T> to, Func<bool> condition)
    {
        From = from;
        To = to;
        Condition = condition;
    }
}
