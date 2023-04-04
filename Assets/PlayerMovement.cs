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
	public bool isOffset = false;
	private void FixedUpdate()
	{
		controller.Move(transform.TransformDirection(moveDirection) * (speed * Time.deltaTime));
		// apply gravity
		moveDirection.y -= GRAVITY * Time.deltaTime;
		if (!GetSquare(out RaycastHit _hit, out Vector3 _p0, out Vector3 _p1, out Vector3 _p2, out Vector3 _p3))
			return;
		// draw triangle
		Debug.DrawLine(_p0, _p1, Color.red);
		Debug.DrawLine(_p1, _p2, Color.red);
		Debug.DrawLine(_p2, _p3, Color.red);
		Debug.DrawLine(_p3, _p0, Color.red);

	}
	private bool GetSquare(out RaycastHit _hit, out Vector3 _p0, out Vector3 _p1, out Vector3 _p2, out Vector3 _p3)
	{
		_p0 = _p1 = _p2 = _p3 = Vector3.zero;
		if (!Physics.Raycast(cam.ScreenPointToRay(Input.mousePosition), out _hit))
			return false;

		MeshCollider _meshCollider = _hit.collider as MeshCollider;
		if (_meshCollider == null || _meshCollider.sharedMesh == null)
			return false;

		Mesh      _mesh      = _meshCollider.sharedMesh;
		Vector3[] _vertices  = _mesh.vertices;
		int[]     _triangles = _mesh.triangles;
		// triangle are 0,2,1 and 0,3,2
		_p0       = _vertices[_triangles[_hit.triangleIndex * 3 + 0]];
		_p1       = _vertices[_triangles[_hit.triangleIndex * 3 + 2]];
		_p2       = _vertices[_triangles[_hit.triangleIndex * 3 + 1]];
		_p3       = _vertices[_triangles[_hit.triangleIndex * 3 + 4]];
		isOffset = false;
		// check that they form a square , if not offset
		if (Vector3.Distance(_p0, _p1) > 1.1f || Vector3.Distance(_p1, _p2) > 1.1f || Vector3.Distance(_p2, _p3) > 1.1f || Vector3.Distance(_p3, _p0) > 1.1f)
		{
			isOffset = true;
			int _offset = 3;
			_p0 = _vertices[_triangles[_hit.triangleIndex * 3 + 0 + _offset]];
			_p1 = _vertices[_triangles[_hit.triangleIndex * 3 + 2 + _offset]];
			_p2 = _vertices[_triangles[_hit.triangleIndex * 3 + 1 + _offset]];
			_p3 = _vertices[_triangles[_hit.triangleIndex * 3 + 4 + _offset]];
			return false;
		}

		// convert to world space
		_p0 = _meshCollider.transform.TransformPoint(_p0);
		_p1 = _meshCollider.transform.TransformPoint(_p1);
		_p2 = _meshCollider.transform.TransformPoint(_p2);
		_p3 = _meshCollider.transform.TransformPoint(_p3);
		return true;
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