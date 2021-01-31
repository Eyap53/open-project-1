using System;
using System.Collections.Generic;
using UnityEngine;

namespace UOP1.StateMachine
{
	public interface IStateMachine
	{
		//
        // Summary:
        //     The Transform attached to this GameObject.
        public Transform transform { get; }
        //
        // Summary:
        //     The game object this component is attached to. A component is always attached
        //     to a game object.
        public GameObject gameObject { get; }
        //
        // Summary:
        //     The tag of this game object.
        public string tag { get; set; }

        //
        // Summary:
        //     Is this game object tagged with tag ?
        //
        // Parameters:
        //   tag:
        //     The tag to compare.
        public bool CompareTag(string tag);

        //
        // Summary:
        //     Returns the component of Type type if the GameObject has one attached, null if
        //     it doesn't. Will also return disabled components.
        //
        // Parameters:
        //   T:
        //     The type of Component to retrieve.
        public T GetComponent<T>();

        //
        // Summary:
        //     Returns the component of Type type in the GameObject or any of its children using
        //     depth first search.
        //
        // Parameters:
        //   t:
        //     The type of Component to retrieve.
        //
        // Returns:
        //     A component of the matching type, if found.
        public T GetComponentInChildren<T>(bool includeInactive);
        public T GetComponentInChildren<T>();
        //
        // Summary:
        //     Returns the component of Type type in the GameObject or any of its parents.
        //
        // Parameters:
        //   t:
        //     The type of Component to retrieve.
        //
        // Returns:
        //     A component of the matching type, if found.
        public T GetComponentInParent<T>();
        //
        // Summary:
        //     Returns all components of Type type in the GameObject.
        //
        // Parameters:
        //   type:
        //     The type of Component to retrieve.
        public void GetComponents<T>(List<T> results);
        public T[] GetComponents<T>();
        //
        // Summary:
        //     Returns all components of Type type in the GameObject or any of its children.
        //
        // Parameters:
        //   t:
        //     The type of Component to retrieve.
        //
        //   includeInactive:
        //     Should Components on inactive GameObjects be included in the found set? includeInactive
        //     decides which children of the GameObject will be searched. The GameObject that
        //     you call GetComponentsInChildren on is always searched regardless.
        public void GetComponentsInChildren<T>(List<T> results);
        public T[] GetComponentsInChildren<T>();
        public void GetComponentsInChildren<T>(bool includeInactive, List<T> result);
        public T[] GetComponentsInChildren<T>(bool includeInactive);
        //
        // Summary:
        //     Returns all components of Type type in the GameObject or any of its parents.
        //
        // Parameters:
        //   T:
        //     The type of Component to retrieve.
        //
        //   includeInactive:
        //     Should inactive Components be included in the found set?
        public T[] GetComponentsInParent<T>();
        public T[] GetComponentsInParent<T>(bool includeInactive);
        public void GetComponentsInParent<T>(bool includeInactive, List<T> results);

        public bool TryGetComponent<T>(out T component);
	}
}
