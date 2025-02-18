public abstract class State<T>
{
    protected T owner;
    protected bool isInterrupted = false;

    public State(T owner)
    {
        this.owner = owner;
    }

    public virtual void Enter() { }
    public virtual void Exit() { }
    public virtual void Update()
    {
        if (isInterrupted) return;
    }
    public virtual void Interrupt()
    {
        if (isInterrupted) return;
        isInterrupted = true;
    }
}
