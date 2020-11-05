using System;
using System.Collections.Generic;

public abstract class State
{
	public abstract void Tick();
	public abstract void OnEnter();
	public abstract void OnExit();
}
