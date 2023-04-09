using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
public sealed class TerrainGenerator : MonoBehaviour
{
	public enum Material
	{
		Air,
		Grass,
		Dirt,
		Stone,
		Wood,
		Leaves
	}
	private const int CHUNK_SIZE = 16;
	private const int MAX_HEIGHT = 64;
	// heightmap
	public UnityEngine.Material[] materials;
	[Tooltip("The lower the value the higher the terrain will be.")]
	public float riseSpeed = 32;
	public           int             pregen = 2;
	private readonly List<Block[,,]> maps   = new();

	private void Start()
	{
		for (int _x = -pregen / 4; _x <= pregen / 4; _x++)
		{
			for (int _z = -pregen / 4; _z <= pregen / 4; _z++)
			{
				GenerateChunk(_x               * CHUNK_SIZE, _z * CHUNK_SIZE, out int _index);
				CreateMesh(Material.Grass,  _x * CHUNK_SIZE, _z * CHUNK_SIZE, _index);
				CreateMesh(Material.Dirt,   _x * CHUNK_SIZE, _z * CHUNK_SIZE, _index);
				CreateMesh(Material.Stone,  _x * CHUNK_SIZE, _z * CHUNK_SIZE, _index);
				CreateMesh(Material.Wood,   _x * CHUNK_SIZE, _z * CHUNK_SIZE, _index);
				CreateMesh(Material.Leaves, _x * CHUNK_SIZE, _z * CHUNK_SIZE, _index);
			}
		}
	}
	private void GenerateChunk(int _xOffset, int _zOffset, out int _mapIndex)
	{
		_mapIndex = maps.Count;
		maps.Add(new Block[CHUNK_SIZE, MAX_HEIGHT + 1, CHUNK_SIZE]);
		for (int _x = 0; _x < CHUNK_SIZE; _x++)
		{
			for (int _z = 0; _z < CHUNK_SIZE; _z++)
			{
				float _height = Mathf.PerlinNoise((_x + _xOffset) / riseSpeed, (_z + _zOffset) / riseSpeed) * MAX_HEIGHT * .5f;
				_height = Mathf.Clamp((int)_height, 0, MAX_HEIGHT);
				// set heightmap
				for (int _y = 0; _y < _height; _y++)
				{
					Material _material = Material.Grass;
					if (_y < _height - 1)
						_material = Material.Dirt;
					if (_y < _height - 3)
						_material = Material.Stone;
					maps[_mapIndex][_x, _y, _z] = new()
					{
						material = _material
					};
				}
				// chance to spawn tree
				if (Random.Range(0, 100) < 1)
					GenerateTree(new(_x, _height, _z), _mapIndex);
			}
		}
	}

	private void GenerateTree(Vector3 _position, int _index)
	{
		int _height = Random.Range(3, 6);
		for (int _y = 0; _y < _height; _y++)
		{
			// if out of bounds
			if ((int)_position.x      < 0 || (int)_position.x      >= CHUNK_SIZE ||
			    (int)_position.y + _y < 0 || (int)_position.y + _y >= MAX_HEIGHT ||
			    (int)_position.z      < 0 || (int)_position.z      >= CHUNK_SIZE)
				continue;
			maps[_index][(int)_position.x, (int)_position.y + _y, (int)_position.z] = new()
			{
				material = Material.Wood
			};
		}
		for (int _x = -2; _x <= 2; _x++)
		{
			for (int _z = -2; _z <= 2; _z++)
			{
				for (int _y = -2; _y <= 2; _y++)
				{
					if (Mathf.Abs(_x) + Mathf.Abs(_y) + Mathf.Abs(_z) > 3)
						continue;
					// if out of bounds
					if ((int)_position.x + _x < 0 || (int)_position.x + _x >= CHUNK_SIZE ||
					    (int)_position.y + _height                    + _y < 0           || (int)_position.y + _height + _y >= MAX_HEIGHT ||
					    (int)_position.z + _z                              < 0           || (int)_position.z + _z           >= CHUNK_SIZE)
						continue;
					maps[_index][(int)_position.x + _x, (int)_position.y + _height + _y, (int)_position.z + _z] = new()
					{
						material = Material.Leaves
					};
				}
			}
		}
	}
	// if you don't understand this function , me neither
	private void CreateMesh(Material _material, int _xOffset, int _zOffset, int _mapIndex)
	{
		Mesh          _mesh      = new();
		List<Vector3> _vertices  = new();
		List<int>     _triangles = new();
		List<Vector2> _uvs       = new();
		List<Vector3> _normals   = new();
		for (int _x = 0; _x < CHUNK_SIZE; _x++)
		{
			for (int _z = 0; _z < CHUNK_SIZE; _z++)
			{
				for (int _y = 0; _y < MAX_HEIGHT + 1; _y++)
				{

					// top
					if (maps[_mapIndex][_x, _y, _z].material != _material)
						continue;
					// if there is no block on the top side then draw top side
					if (_y == MAX_HEIGHT || maps[_mapIndex][_x, _y + 1, _z].material == Material.Air)
					{
						_vertices.Add(new(_x, _y, _z));                 // bottom left
						_vertices.Add(new(_x         + 1, _y, _z));     // bottom right
						_vertices.Add(new(_x         + 1, _y, _z + 1)); // top right
						_vertices.Add(new(_x, _y, _z + 1));             // top left
						_triangles.Add(_vertices.Count - 4);            // 0
						_triangles.Add(_vertices.Count - 2);            // 2
						_triangles.Add(_vertices.Count - 3);            // 1
						_triangles.Add(_vertices.Count - 4);            // 0
						_triangles.Add(_vertices.Count - 1);            // 3
						_triangles.Add(_vertices.Count - 2);            // 2
						// tiling 1, 0.5 , offset 0, .5
						_uvs.Add(new(0f, .5f));
						_uvs.Add(new(1f, .5f));
						_uvs.Add(new(1f, 1f));
						_uvs.Add(new(0f, 1f));
						// normals
						_normals.Add(Vector3.up);
						_normals.Add(Vector3.up);
						_normals.Add(Vector3.up);
						_normals.Add(Vector3.up);
					}
					// if there is no block on the left side then draw left side
					if (_x == 0 || maps[_mapIndex][_x - 1, _y, _z].material == Material.Air)
					{
						_vertices.Add(new(_x, _y - 1, _z));         // bottom left
						_vertices.Add(new(_x, _y, _z));             // top left
						_vertices.Add(new(_x, _y, _z + 1));         // top right
						_vertices.Add(new(_x, _y     - 1, _z + 1)); // bottom right
						_triangles.Add(_vertices.Count - 4);        // 0
						_triangles.Add(_vertices.Count - 2);        // 2
						_triangles.Add(_vertices.Count - 3);        // 1
						_triangles.Add(_vertices.Count - 4);        // 0
						_triangles.Add(_vertices.Count - 1);        // 3;
						_triangles.Add(_vertices.Count - 2);        // 2
						// tiling 1, 0.5 , offset 0, 0
						_uvs.Add(new(0f, 0f));
						_uvs.Add(new(0f, .5f));
						_uvs.Add(new(1f, .5f));
						_uvs.Add(new(1f, 0f));
						// normals
						_normals.Add(Vector3.left);
						_normals.Add(Vector3.left);
						_normals.Add(Vector3.left);
						_normals.Add(Vector3.left);
					}

					// if there is no block on the right side then draw right side
					if (_x == CHUNK_SIZE - 1 || maps[_mapIndex][_x + 1, _y, _z].material == Material.Air)
					{
						_vertices.Add(new(_x + 1, _y     - 1, _z));     // bottom left
						_vertices.Add(new(_x + 1, _y     - 1, _z + 1)); // bottom right
						_vertices.Add(new(_x + 1, _y, _z + 1));         // top right
						_vertices.Add(new(_x + 1, _y, _z));             // top left
						_triangles.Add(_vertices.Count - 4);            // 0
						_triangles.Add(_vertices.Count - 2);            // 2
						_triangles.Add(_vertices.Count - 3);            // 1
						_triangles.Add(_vertices.Count - 4);            // 0
						_triangles.Add(_vertices.Count - 1);            // 3
						_triangles.Add(_vertices.Count - 2);            // 2
						// tiling 1, 0.5 , offset 0, 0
						_uvs.Add(new(0f, 0f));
						_uvs.Add(new(1f, 0f));
						_uvs.Add(new(1f, .5f));
						_uvs.Add(new(0f, .5f));
						// normals
						_normals.Add(Vector3.right);
						_normals.Add(Vector3.right);
						_normals.Add(Vector3.right);
						_normals.Add(Vector3.right);
					}

					// if there is no block on the front side then draw front side
					if (_z == 0 || maps[_mapIndex][_x, _y, _z - 1].material == Material.Air)
					{
						_vertices.Add(new(_x + 1, _y - 1, _z)); // top right
						_vertices.Add(new(_x + 1, _y, _z));     // bottom right
						_vertices.Add(new(_x, _y, _z));         // bottom left
						_vertices.Add(new(_x, _y - 1, _z));     // top left
						_triangles.Add(_vertices.Count - 4);    // 0
						_triangles.Add(_vertices.Count - 2);    // 2
						_triangles.Add(_vertices.Count - 3);    // 1
						_triangles.Add(_vertices.Count - 4);    // 0
						_triangles.Add(_vertices.Count - 1);    // 3
						_triangles.Add(_vertices.Count - 2);    // 2
						// tiling 1, 0.5 , offset 0, 0
						_uvs.Add(new(0f, 0f));
						_uvs.Add(new(0f, .5f));
						_uvs.Add(new(1f, .5f));
						_uvs.Add(new(1f, 0f));
						// normals
						_normals.Add(Vector3.forward);
						_normals.Add(Vector3.forward);
						_normals.Add(Vector3.forward);
						_normals.Add(Vector3.forward);
					}

					// if there is no block on the back side then draw back side
					if (_z == CHUNK_SIZE - 1 || maps[_mapIndex][_x, _y, _z + 1].material == Material.Air)
					{
						_vertices.Add(new(_x, _y     - 1, _z + 1));             // top left
						_vertices.Add(new(_x, _y, _z + 1));                     // bottom left
						_vertices.Add(new(_x         + 1, _y, _z + 1));         // bottom right
						_vertices.Add(new(_x         + 1, _y     - 1, _z + 1)); // top right
						_triangles.Add(_vertices.Count - 4);                    // 0
						_triangles.Add(_vertices.Count - 2);                    // 2
						_triangles.Add(_vertices.Count - 3);                    // 1
						_triangles.Add(_vertices.Count - 4);                    // 0
						_triangles.Add(_vertices.Count - 1);                    // 3
						_triangles.Add(_vertices.Count - 2);                    // 2
						// tiling 1, 0.5 , offset 0, 0
						_uvs.Add(new(1f, 0f));
						_uvs.Add(new(1f, .5f));
						_uvs.Add(new(0f, .5f));
						_uvs.Add(new(0f, 0f));
						// normals
						_normals.Add(Vector3.back);
						_normals.Add(Vector3.back);
						_normals.Add(Vector3.back);
						_normals.Add(Vector3.back);

					}

					// if there is no block on the bottom side then draw bottom side
					if (_y == 0 || maps[_mapIndex][_x, _y - 1, _z].material == Material.Air)
					{
						_vertices.Add(new(_x     + 1, _y - 1, _z + 1)); // top right
						_vertices.Add(new(_x     + 1, _y - 1, _z));     // bottom right
						_vertices.Add(new(_x, _y - 1, _z));             // bottom left
						_vertices.Add(new(_x, _y - 1, _z + 1));         // top left
						_triangles.Add(_vertices.Count - 4);            // 0
						_triangles.Add(_vertices.Count - 2);            // 2
						_triangles.Add(_vertices.Count - 3);            // 1
						_triangles.Add(_vertices.Count - 4);            // 0
						_triangles.Add(_vertices.Count - 1);            // 3
						_triangles.Add(_vertices.Count - 2);            // 2
						// tiling 1, 0.5 , offset 0, 0
						_uvs.Add(new(0f, 0f));
						_uvs.Add(new(0f, .5f));
						_uvs.Add(new(1f, .5f));
						_uvs.Add(new(1f, 0f));
						// normals
						_normals.Add(Vector3.down);
						_normals.Add(Vector3.down);
						_normals.Add(Vector3.down);
						_normals.Add(Vector3.down);
					}
				}
			}
		}
		_mesh.vertices  = _vertices.ToArray();
		_mesh.triangles = _triangles.ToArray();
		_mesh.uv        = _uvs.ToArray();
		_mesh.normals   = _normals.ToArray();
		_mesh.RecalculateNormals();
		_mesh.RecalculateBounds();
		_mesh.Optimize();
		// create gameobject
		GameObject _gameObject = new(_material.ToString());
		_gameObject.transform.SetParent(transform);
		_gameObject.transform.position                      = new(_xOffset, 0, _zOffset);
		_gameObject.name                                    = _mapIndex + "_" + (int)_material;
		_gameObject.layer                                   = LayerMask.NameToLayer("Terrain");
		_gameObject.AddComponent<MeshFilter>().mesh         = _mesh;
		_gameObject.AddComponent<MeshRenderer>().material   = materials[(int)_material - 1];
		_gameObject.AddComponent<MeshCollider>().sharedMesh = _mesh;
	}
	internal bool RemoveCube(Vector3 _pos, int _mapIndex)
	{
		// round the position
		int _x = Mathf.FloorToInt(_pos.x);
		int _y = Mathf.CeilToInt(_pos.y);
		int _z = Mathf.FloorToInt(_pos.z);
		if (maps[_mapIndex][_x, _y, _z].material != Material.Air)
		{
			maps[_mapIndex][_x, _y, _z].material = Material.Air;
			// recreate the chunk foreach material
			foreach (Material _material in Enum.GetValues(typeof(Material)))
				if (_material != Material.Air)
				{
					GameObject _gameObject = transform.Find(_mapIndex + "_" + (int)_material).gameObject;
					int        _xOffset    = (int)_gameObject.transform.position.x;
					int        _zOffset    = (int)_gameObject.transform.position.z;
					// destroy the old chunk
					Destroy(_gameObject);
					// create the new chunk
					CreateMesh(_material, _xOffset, _zOffset, _mapIndex);
				}

		}
		return false;
	}
	public void AddCube(Vector3 _point, int _index, Material _mat)
	{
		// round the position
		int _x = Mathf.FloorToInt(_point.x);
		int _y = Mathf.CeilToInt(_point.y);
		int _z = Mathf.FloorToInt(_point.z);
		// check that there is nothing in the way on all layers except terrain
		int _terrainLayer = LayerMask.GetMask("Terrain");
		int _layerMask    = ~_terrainLayer;
		if (Physics.CheckBox(new(_x, _y, _z), Vector3.one * .5f, Quaternion.identity, _layerMask))
			return;

		if (maps[_index][_x, _y, _z].material == Material.Air)
		{
			maps[_index][_x, _y, _z].material = _mat;
			// recreate the chunk foreach material
			foreach (Material _material in Enum.GetValues(typeof(Material)))
				if (_material != Material.Air)
				{
					GameObject _gameObject = transform.Find(_index + "_" + (int)_material).gameObject;
					int        _xOffset    = (int)_gameObject.transform.position.x;
					int        _zOffset    = (int)_gameObject.transform.position.z;
					// destroy the old chunk
					Destroy(_gameObject);
					// create the new chunk
					CreateMesh(_material, _xOffset, _zOffset, _index);
				}
		}
	}
	private struct Block
	{
		public Material material;
	}
}