using System;
using System.Collections.Generic;
using UnityEngine;

namespace UOP1.StateMachine
{
	public class StateMachine : MonoBehaviour, IStateMachine
	{
		[Tooltip("Set the initial state of this StateMachine")]
		[SerializeField] private ScriptableObjects.TransitionTableSO _transitionTableSO = default;

#if UNITY_EDITOR
		[Space]
		[SerializeField]
		internal Debugging.StateMachineDebugger _debugger = default;
#endif

		internal State _currentState;

		private void Awake()
		{
			_currentState = _transitionTableSO.GetInitialState(this);
#if UNITY_EDITOR
			_debugger.Awake(this);
#endif
		}

		void Start()
		{
			_currentState.OnStateEnter();
		}

		void Update()
		{
			if (_currentState.TryGetTransition(out var transitionState))
				Transition(transitionState);

			_currentState.OnUpdate();
		}

		void OnDestroy()
		{
			_currentState.OnStateExit();
		}

		private void Transition(State transitionState)
		{
			_currentState.OnStateExit();
			_currentState = transitionState;
			_currentState.OnStateEnter();
		}
	}
}
