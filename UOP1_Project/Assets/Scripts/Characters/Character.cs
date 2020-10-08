﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{
	private CharacterController characterController;

	[Tooltip("Horizontal XZ plane speed multiplier")] public float speed = 8f;
	[Tooltip("Smoothing for rotating the character to their movement direction")] public float turnSmoothTime = 0.2f;
	[Tooltip("General multiplier for gravity (affects jump and freefall)")] public float gravityMultiplier = 5f;
	[Tooltip("The initial upwards push when pressing jump. This is injected into verticalMovement, and gradually cancelled by gravity")] public float initialJumpForce = 10f;
	[Tooltip("How long can the player hold the jump button")] public float jumpInputDuration = .4f;
	[Tooltip("Represents how fast gravityContributionMultiplier will go back to 1f. The higher, the faster")] public float gravityComebackMultiplier = 15f;
	[Tooltip("The maximum speed reached when falling (in units/frame)")] public float maxFallSpeed = 50f;
	[Tooltip("Each frame while jumping, gravity will be multiplied by this amount in an attempt to 'cancel it' (= jump higher)")] public float gravityDivider = .6f;
	[Tooltip("Adjust the friction of the slope")] public float slideFriction = 0.3f;
	[Tooltip("Starting vertical movement when falling from a platform")] public float fallingVerticalMovement = -5f;


	private float gravityContributionMultiplier = 0f; //The factor which determines how much gravity is affecting verticalMovement
	private bool isJumping = false; //If true, a jump is in effect and the player is holding the jump button
	private float jumpBeginTime = -Mathf.Infinity; //Time of the last jump
	private float turnSmoothSpeed; //Used by Mathf.SmoothDampAngle to smoothly rotate the character to their movement direction
	private float verticalMovement = 0f; //Represents how much a player will move vertically in a frame. Affected by gravity * gravityContributionMultiplier
	private float currentSlope;
	private Vector3 hitNormal; // ground normal
	private bool shouldSlide; // Should player slide?
	private Vector3 inputVector; //Initial input horizontal movement (y == 0f)
	private Vector3 movementVector; //Final movement vector
	private StateMachine stateMachine; // handles different states and transitions between them

	private const float ROTATION_TRESHOLD = .02f; // Used to prevent NaN result causing rotation in a non direction

	private void Awake()
	{
		characterController = GetComponent<CharacterController>();
		OrchestrateStateMachine();
	}

	private void Update()
	{
		// calls tick on the currently active state
		stateMachine.Tick();
	}

	private void OrchestrateStateMachine()
	{
		stateMachine = new StateMachine();

		// create all states and pass in references to this character
		IdleState idleState = new IdleState();
		WalkingState walkingState = new WalkingState(this);
		JumpingState jumpingState = new JumpingState(this);
		FallingState fallingState = new FallingState(this);

		// setup all explicit transitions
		idleState.AddTransition(jumpingState, PerformedJumpAction());
		idleState.AddTransition(walkingState, IsMovingAndGrounded());
		walkingState.AddTransition(jumpingState, PerformedJumpAction());
		fallingState.AddTransition(idleState, IsCharacterNotMovingAndGrounded());
		fallingState.AddTransition(walkingState, IsMovingAndGrounded());

		stateMachine.AddAnyTransition(fallingState, IsFallingAndNotJumping());
		stateMachine.AddAnyTransition(idleState, IsCharacterNotMovingAndGrounded());

		// now, initialize the state machine and start ticking
		stateMachine.SetState(idleState);
	}

	private void OnControllerColliderHit(ControllerColliderHit hit)
	{
		hitNormal = hit.normal;
		bool isMovingUpwards = verticalMovement > 0f;
		if (isMovingUpwards)
		{
			// Making sure the collision is near the top of the head
			float permittedDistance = characterController.radius / 2f;
			float topPositionY = transform.position.y + characterController.height;
			float distance = Mathf.Abs(hit.point.y - topPositionY);
			if (distance <= permittedDistance)
			{
				// Stopping any upwards movement
				// and having the player fall back down
				isJumping = false;
				gravityContributionMultiplier = 1f;
				verticalMovement = 0f;
			}
		}
	}

	private void CalculateFinalAirMovement()
	{
		//Less control in mid-air, conserving momentum from previous frame
		movementVector = inputVector * speed;

		//The character is either jumping or in freefall, so gravity will add up
		gravityContributionMultiplier = Mathf.Clamp01(gravityContributionMultiplier);
		verticalMovement += Physics.gravity.y * gravityMultiplier * Time.deltaTime * gravityContributionMultiplier; //Add gravity contribution
																													//Note that even if it's added, the above value is negative due to Physics.gravity.y

		//Cap the maximum so the player doesn't reach incredible speeds when freefalling from high positions
		verticalMovement = Mathf.Clamp(verticalMovement, -maxFallSpeed, 100f);
	}

	private bool IsThereInput()
	{
		return inputVector != Vector3.zero;
	}

	//---- PREDICATES TO DEFINE STATE TRANSITIONS ----

	private Func<bool> IsMovingAndGrounded() => () => IsThereInput() && characterController.isGrounded;

	private Func<bool> IsCharacterNotMovingAndGrounded() => () => characterController.isGrounded && !IsThereInput();

	private Func<bool> PerformedJumpAction() => () => isJumping;

	private Func<bool> IsFallingAndNotJumping() => () => !characterController.isGrounded && !isJumping;

	//---- METHODS USED TO CONTROL CHARACTER STATE ----

	public void ApplyMovementAndRotate()
	{
		UpdateSlide();

		// apply movement vector based
		movementVector = inputVector * speed;

		//Apply the result and move the character in space
		movementVector.y = verticalMovement;
		characterController.Move(movementVector * Time.deltaTime);

		//Rotate to the movement direction
		movementVector.y = 0f;
		if (movementVector.sqrMagnitude >= .02f)
		{
			float targetRotation = Mathf.Atan2(movementVector.x, movementVector.z) * Mathf.Rad2Deg;
			transform.eulerAngles = Vector3.up * Mathf.SmoothDampAngle(
				transform.eulerAngles.y,
				targetRotation,
				ref turnSmoothSpeed,
				turnSmoothTime);
		}

		if (!characterController.isGrounded)
		{
			CalculateFinalAirMovement();
		}
	}

	public void ResetVerticalMovement()
	{
		verticalMovement = -5f;
		gravityContributionMultiplier = 0f;
	}

	public void ApplyGravityComeback()
	{
		//Raises the multiplier to how much gravity will affect vertical movement when in mid-air
		//This is 0f at the beginning of a jump and will raise to maximum 1f
		gravityContributionMultiplier += Time.deltaTime * gravityComebackMultiplier;
	}

	public void ReduceGravityEffect()
	{
		gravityContributionMultiplier *= gravityDivider;
	}

	public void ResetGravityContributorMultiplier()
	{
		gravityContributionMultiplier = 1f;
	}

	public void InitJumpingValues()
	{
		jumpBeginTime = Time.time;
		verticalMovement = initialJumpForce; //This is the only place where verticalMovement is set to a positive value
		gravityContributionMultiplier = 0f;
	}

	public void SetJumpingState(bool isJumping)
	{
		this.isJumping = isJumping;
	}

	public bool IsJumpingTooLong(float deltaTime) => deltaTime >= jumpBeginTime + jumpInputDuration;

	//---- COMMANDS ISSUED BY OTHER SCRIPTS ----
	public void Move(Vector3 movement)
	{
		inputVector = movement;
	}

	public void Jump()
	{
		if (characterController.isGrounded && !shouldSlide)
		{
			SetJumpingState(true);
		}
	}

	public void CancelJump()
	{
		SetJumpingState(false); //This will stop the reduction to the gravity, which will then quickly pull down the character
	}

	private void UpdateSlide()
	{
		// if player has to slide then add sideways speed to make it go down
		if (shouldSlide)
		{
			movementVector.x += (1f - hitNormal.y) * hitNormal.x * (speed - slideFriction);
			movementVector.z += (1f - hitNormal.y) * hitNormal.z * (speed - slideFriction);
		}
		// check if the controller is grounded and above slope limit
		// if player is grounded and above slope limit
		// player has to slide
		if (characterController.isGrounded)
		{
			currentSlope = Vector3.Angle(Vector3.up, hitNormal);
			shouldSlide = currentSlope >= characterController.slopeLimit;
		}
	}
}
