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
	public        bool                isOffset;
	public        float               selectDistance = 5f;
	public        bool                isRunning;
	public        float               runMultiplier = 2f;
	private       Camera              cam;
	private       CharacterController controller;
	private       Vector3             moveDirection;
	private void Start()
	{
		cam = GetComponentInChildren<Camera>();
		// lock cursor
		Cursor.lockState = CursorLockMode.Locked;
		controller       = GetComponent<CharacterController>();
	}
	private void FixedUpdate()
	{
		controller.Move(transform.TransformDirection(moveDirection) * (speed * Time.deltaTime * (isRunning ? runMultiplier : 1f)));
		// apply gravity
		moveDirection.y -= GRAVITY * Time.fixedDeltaTime;
		DrawCube();

	}
	private void DrawCube()
	{
		if (!GetCube(out RaycastHit _hit, out Vector3[] _points, selectDistance))
			return;
		// draw cube outline 
		Color _color = Color.black;
		Debug.DrawLine(_points[0], _points[1], _color);
		Debug.DrawLine(_points[1], _points[2], _color);
		Debug.DrawLine(_points[2], _points[3], _color);
		Debug.DrawLine(_points[3], _points[0], _color);
		Debug.DrawLine(_points[0], _points[4], _color);
		Debug.DrawLine(_points[1], _points[5], _color);
		Debug.DrawLine(_points[2], _points[6], _color);
		Debug.DrawLine(_points[3], _points[7], _color);
		Debug.DrawLine(_points[4], _points[5], _color);
		Debug.DrawLine(_points[5], _points[6], _color);
		Debug.DrawLine(_points[6], _points[7], _color);
		Debug.DrawLine(_points[7], _points[4], _color);
	}
	private bool GetSquare(out RaycastHit _hit, out Vector3 _point0, out Vector3 _point1, out Vector3 _point2, out Vector3 _point3, float distance = Mathf.Infinity)
	{
		_point0 = _point1 = _point2 = _point3 = Vector3.zero;
		if (!Physics.Raycast(cam.ScreenPointToRay(Input.mousePosition), out _hit, distance))
			return false;

		MeshCollider _meshCollider = _hit.collider as MeshCollider;
		if (_meshCollider == null || _meshCollider.sharedMesh == null)
			return false;

		Mesh      _mesh      = _meshCollider.sharedMesh;
		Vector3[] _vertices  = _mesh.vertices;
		int[]     _triangles = _mesh.triangles;
		// triangle are 0,2,1 and 0,3,2
		_point0  = _vertices[_triangles[_hit.triangleIndex * 3 + 0]];
		_point1  = _vertices[_triangles[_hit.triangleIndex * 3 + 2]];
		_point2  = _vertices[_triangles[_hit.triangleIndex * 3 + 1]];
		_point3  = _vertices[_triangles[_hit.triangleIndex * 3 + 4]];
		isOffset = false;
		// check that they form a square , if not offset
		if (Vector3.Distance(_point0, _point1) > 1.1f || Vector3.Distance(_point1, _point2) > 1.1f || Vector3.Distance(_point2, _point3) > 1.1f || Vector3.Distance(_point3, _point0) > 1.1f)
		{
			isOffset = true;
			const int _offset = -3;
			_point0 = _vertices[_triangles[_hit.triangleIndex * 3 + 0 + _offset]];
			_point1 = _vertices[_triangles[_hit.triangleIndex * 3 + 2 + _offset]];
			_point2 = _vertices[_triangles[_hit.triangleIndex * 3 + 1 + _offset]];
			_point3 = _vertices[_triangles[_hit.triangleIndex * 3 + 4 + _offset]];
		}

		// convert to world space
		_point0 = _meshCollider.transform.TransformPoint(_point0);
		_point1 = _meshCollider.transform.TransformPoint(_point1);
		_point2 = _meshCollider.transform.TransformPoint(_point2);
		_point3 = _meshCollider.transform.TransformPoint(_point3);
		return true;
	}
	private bool GetCube(out RaycastHit _hit, out Vector3[] _points, float distance)
	{
		if (!GetSquare(out _hit, out Vector3 _p0, out Vector3 _p1, out Vector3 _p2, out Vector3 _p3, distance))
		{
			_points = null;
			return false;
		}
		_points = new Vector3[8];

		// get the normal of the square
		Vector3 _normal = Vector3.Cross(_p1 - _p0, _p3 - _p0).normalized;

		// if face is top
		if (_normal == Vector3.down)
		{
			_points[0] = _p0;
			_points[1] = _p1;
			_points[2] = _p2;
			_points[3] = _p3;
			_points[4] = _p0 - Vector3.up;
			_points[5] = _p1 - Vector3.up;
			_points[6] = _p2 - Vector3.up;
			_points[7] = _p3 - Vector3.up;
		}

		// if face is left
		if (_normal == Vector3.right)
		{
			_points[0] = _p0;
			_points[1] = _p1;
			_points[2] = _p2;
			_points[3] = _p3;
			_points[4] = _p0 + Vector3.right;
			_points[5] = _p1 + Vector3.right;
			_points[6] = _p2 + Vector3.right;
			_points[7] = _p3 + Vector3.right;
		}

		// if face is right
		if (_normal == Vector3.left)
		{
			_points[0] = _p0;
			_points[1] = _p1;
			_points[2] = _p2;
			_points[3] = _p3;
			_points[4] = _p0 + Vector3.left;
			_points[5] = _p1 + Vector3.left;
			_points[6] = _p2 + Vector3.left;
			_points[7] = _p3 + Vector3.left;
		}

		// if face is front
		if (_normal == Vector3.back)
		{
			_points[0] = _p0;
			_points[1] = _p1;
			_points[2] = _p2;
			_points[3] = _p3;
			_points[4] = _p0 + Vector3.back;
			_points[5] = _p1 + Vector3.back;
			_points[6] = _p2 + Vector3.back;
			_points[7] = _p3 + Vector3.back;
		}

		// if face is back
		if (_normal == Vector3.forward)
		{
			_points[0] = _p0;
			_points[1] = _p1;
			_points[2] = _p2;
			_points[3] = _p3;
			_points[4] = _p0 + Vector3.forward;
			_points[5] = _p1 + Vector3.forward;
			_points[6] = _p2 + Vector3.forward;
			_points[7] = _p3 + Vector3.forward;
		}

		// if face is bottom TODO: je peut pas test ca car je ne peux pas placer un cube sur le sol

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
	private void OnRun(InputValue _value)
	{
		isRunning = _value.isPressed;
		// if running increase fov and decrease mouse sensitivity
		if (isRunning)
		{
			cam.fieldOfView  *= 1.2f;
			mouseSensitivity /= 1.2f;
		}
		else
		{
			cam.fieldOfView  /= 1.2f;
			mouseSensitivity *= 1.2f;
		}
	}
}