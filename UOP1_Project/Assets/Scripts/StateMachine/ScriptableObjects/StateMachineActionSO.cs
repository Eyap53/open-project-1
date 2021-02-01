namespace UOP1.StateMachine.ScriptableObjects
{
	using UnityEngine;
	using UOP1.StateMachine;
	using System;
	using System.Collections.Generic;

	[CreateAssetMenu(fileName = "StateMachine", menuName = "State Machines/Actions/StateMachine")]
	public class StateMachineActionSO : StateActionSO<StateMachineAction>
	{
		[Tooltip("Set the initial state of this StateMachine")]
		public TransitionTableSO _transitionTableSO = default;
	}

	public class StateMachineAction : StateAction, IStateMachine
	{
		private new StateMachineActionSO _originSO => (StateMachineActionSO)base.OriginSO; // The SO this StateAction spawned from
		private readonly Dictionary<Type, Component> _cachedComponents = new Dictionary<Type, Component>();
		internal State _currentState;

		private IStateMachine _parentStateMachine = default;
		private State _initialState = default;

		public override void Awake(IStateMachine stateMachine)
		{
			_parentStateMachine = stateMachine;
			_initialState = _originSO._transitionTableSO.GetInitialState(this);
		}

		public override void OnStateEnter()
		{
			_currentState = _initialState;
			_currentState.OnStateEnter();
		}

		public override void OnUpdate()
		{
			if (_currentState.TryGetTransition(out var transitionState))
				Transition(transitionState);

			_currentState.OnUpdate();
		}

		public override void OnStateExit()
		{
			_currentState.OnStateExit();
		}

		#region IStateMachine (component methods)
		public Transform transform { get { return _parentStateMachine.transform; } }
		public GameObject gameObject { get { return _parentStateMachine.gameObject; } }
		public string tag { get { return _parentStateMachine.tag; } set { _parentStateMachine.tag = value; } }

		public bool CompareTag(string tag) { return _parentStateMachine.CompareTag(tag); }
		public T GetComponent<T>() { return _parentStateMachine.GetComponent<T>(); }
		public T GetComponentInChildren<T>(bool includeInactive) { return _parentStateMachine.GetComponentInChildren<T>(includeInactive); }
		public T GetComponentInChildren<T>() { return _parentStateMachine.GetComponentInChildren<T>(); }
		public T GetComponentInParent<T>() { return _parentStateMachine.GetComponentInParent<T>(); }
		public void GetComponents<T>(List<T> results) { _parentStateMachine.GetComponents<T>(results); }
		public T[] GetComponents<T>() { return _parentStateMachine.GetComponents<T>(); }

		public void GetComponentsInChildren<T>(List<T> results) { _parentStateMachine.GetComponentsInChildren<T>(results); }
		public T[] GetComponentsInChildren<T>() { return _parentStateMachine.GetComponentsInChildren<T>(); }
		public void GetComponentsInChildren<T>(bool includeInactive, List<T> result) { _parentStateMachine.GetComponentsInChildren<T>(includeInactive, result); }
		public T[] GetComponentsInChildren<T>(bool includeInactive) { return _parentStateMachine.GetComponentsInChildren<T>(includeInactive); }

		public T[] GetComponentsInParent<T>() { return _parentStateMachine.GetComponentsInParent<T>(); }
		public T[] GetComponentsInParent<T>(bool includeInactive) { return _parentStateMachine.GetComponentsInParent<T>(includeInactive); }
		public void GetComponentsInParent<T>(bool includeInactive, List<T> results) { _parentStateMachine.GetComponentsInParent<T>(includeInactive, results); }

		public bool TryGetComponent<T>(out T component) { return _parentStateMachine.TryGetComponent<T>(out component); }
		#endregion

		private void Transition(State transitionState)
		{
			_currentState.OnStateExit();
			_currentState = transitionState;
			_currentState.OnStateEnter();
		}
	}
}
