using System;
using System.Collections.Generic;
using UnityEngine;

public class StateMachine
{
	/// <summary>
	/// Store the current state
	/// </summary>
	/// <value></value>
	public State currentState { get; private set; }
	/// <summary>
	/// Called on state change
	/// </summary>
	public Action stateChanged;

	private readonly List<Transition> _transitions = new List<Transition>();

	public void Tick()
	{
		Transition transition = GetTransitionIfAvailable(currentState);
		if (transition != null)
		{
			SetState(transition.to);
		}

		currentState?.Tick();
	}

	public void SetState(State state)
	{
		if (state == currentState)
		{
			return;
		}

		currentState?.OnExit();
		currentState = state;
		if (stateChanged != null)
		{
			stateChanged();
		}

		currentState.OnEnter();
	}

	public void AddTransition(State fromState, State toState, Func<bool> predicate)
	{
		if (toState == null)
		{
			throw new ArgumentException();
		}
		if (fromState == toState)
		{
			throw new ArgumentException();
		}
		_transitions.Add(new Transition(fromState, toState, predicate));
	}

	private Transition GetTransitionIfAvailable(State currentState)
	{
		foreach (Transition transition in _transitions)
		{
			if (((transition.from == null && transition.to != currentState) || transition.from == currentState) && transition.condition())
			{
				return transition;
			}
		}

		return null;
	}
}
