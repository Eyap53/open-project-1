using System;

public class Transition
{
	public Func<bool> condition { get; }
	public State from { get; }
	public State to { get; }

	public Transition(State from, State to, Func<bool> condition)
	{
		this.from = from;
		this.to = to;
		this.condition = condition;
	}
}
