using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
[RequireComponent(typeof(CharacterController))]
public sealed class PlayerMovement : MonoBehaviour
{
	private const float               CAM_MAX          = 90;
	private const float               CAM_MIN          = -90;
	private const float               GRAVITY          = 9.81f / 2;
	public        float               speed            = 5f;
	public        float               jumpForce        = 5f;
	public        float               mouseSensitivity = 10f;
	public        bool                isRunning;
	public        float               runMultiplier = 2f;
	private       Camera              cam;
	private       CharacterController controller;
	private       GameObject          hand;
	private       Vector3             moveDirection;
	private void Start()
	{
		cam  = GetComponentInChildren<Camera>();
		hand = GameObject.Find("Hand");
		// lock cursor
		Cursor.lockState = CursorLockMode.Locked;
		controller       = GetComponent<CharacterController>();
	}
	private void FixedUpdate()
	{
		controller.Move(transform.TransformDirection(moveDirection) * (speed * Time.deltaTime * (isRunning ? runMultiplier : 1f)));
		// apply gravity
		moveDirection.y -= GRAVITY * Time.fixedDeltaTime;
		
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
			moveDirection.y = jumpForce;
	}

	private void OnLook(InputValue _value)
	{
		// don't rotate if cursor is not locked
		if (Cursor.lockState != CursorLockMode.Locked) return;

		Vector2 _input = _value.Get<Vector2>();
		transform.Rotate(Vector3.up, _input.x * Time.deltaTime * mouseSensitivity);
		float _angle = cam.transform.localEulerAngles.x - _input.y * Time.deltaTime * mouseSensitivity;
		// convert from 0-360 to -180-180
		_angle                         = _angle > 180 ? _angle - 360 : _angle;
		_angle                         = Mathf.Max(Mathf.Min(_angle, CAM_MAX), CAM_MIN);
		cam.transform.localEulerAngles = new(_angle, 0, 0);
	}
	private IEnumerator ChangeFOV(float _target, float _duration)
	{
		float _start = cam.fieldOfView;
		float _time  = 0;
		while (_time < _duration)
		{
			_time           += Time.deltaTime * 2;
			cam.fieldOfView =  Mathf.Lerp(_start, _target, _time);
			yield return null;
		}
		if (!(_target > _start))
			yield break;
		isRunning = true;
		StartCoroutine(Run());
	}
	private IEnumerator Run()
	{
		// rotate camera to increase sense of speed while running
		Vector3 _lastPos      = transform.position;
		Vector3 _startCamPos  = cam.transform.localPosition;
		Vector3 _startHandPos = hand.transform.localPosition;
		while (isRunning)
		{
			cam.transform.Rotate(Vector3.right, Mathf.Sin(Time.time * 10) * 0.5f);
			cam.transform.Translate(Vector3.up                            * (Mathf.Sin(Time.time * 10) * 0.01f));
			// rotate hand as well
			hand.transform.Rotate(Vector3.right, Mathf.Sin(Time.time * 10) * -2f);
			yield return new WaitForSeconds(0.05f);
			float        _distanceSquared = (_lastPos - transform.position).sqrMagnitude;
			const double MIN_DISTANCE     = 0.05f;
			// if not sprinting stop running
			if (_distanceSquared < MIN_DISTANCE)
			{
				isRunning = false;
				StartCoroutine(ChangeFOV(cam.fieldOfView / 1.2f, 0.5f));
				mouseSensitivity *= 1.2f;
				break;
			}
			_lastPos = transform.position;
		}
		// reset camera rotation
		cam.transform.localPosition  = _startCamPos;
		hand.transform.localPosition = _startHandPos;
	}
	private void OnRun(InputValue _value)
	{
		// if running increase fov and decrease mouse sensitivity using lerp
		if (!_value.isPressed || !(moveDirection.magnitude > 0.5f))
			return;

		if (isRunning) return;

		mouseSensitivity /= 1.2f;
		StartCoroutine(ChangeFOV(cam.fieldOfView * 1.2f, 0.5f));
	}

}