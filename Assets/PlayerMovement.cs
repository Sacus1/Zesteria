using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.InputSystem;
[RequireComponent(typeof(CharacterController))]
public sealed class PlayerMovement : MonoBehaviour
{
	public   float               speed            = 5f;
	public   float               jumpForce        = 5f;
	public   float               mouseSensitivity = 10f;
	private  Vector3             moveDirection;
	internal Camera              cam;
	internal float               camMax = 90;
	internal float               camMin = -90;
	private  CharacterController controller;
	private const float          GRAVITY = 9.81f;
	private void Start()
	{
		cam        = GetComponentInChildren<Camera>();
		// lock cursor
		Cursor.lockState = CursorLockMode.Locked;
		controller = GetComponent<CharacterController>();
	}

	private void FixedUpdate()
	{
		controller.Move(transform.TransformDirection(moveDirection) * (speed * Time.deltaTime));
		// apply gravity
		moveDirection.y -= GRAVITY * Time.deltaTime;
	}

	private void OnMove(InputValue _value)
	{
		Vector2 _input = _value.Get<Vector2>();
		Vector3 _move  = new(_input.x, 0, _input.y);
		moveDirection = _move;
	}

	private void OnJump()
	{
		if (controller.isGrounded)
		{
			moveDirection.y = jumpForce;
		}
	}

	private void OnLook(InputValue _value)
	{
		// don't rotate if cursor is not locked
		if (Cursor.lockState != CursorLockMode.Locked) return;

		Vector2 _input = _value.Get<Vector2>();
		transform.Rotate(Vector3.up, _input.x);
		float _angle  = cam.transform.localEulerAngles.x - _input.y * Time.deltaTime * mouseSensitivity;
		// convert from 0-360 to -180-180
		_angle                          = _angle > 180 ? _angle - 360 : _angle;
		_angle                          = Mathf.Max(Mathf.Min(_angle, camMax), camMin);
		cam.transform.localEulerAngles = new(_angle, 0, 0);
	}
}