using System;
using System.Text;
using JetBrains.Annotations;
using UnityEngine;
using UnityEditor;

public class Compiler : MonoBehaviour
{
	/*
	 * This is the compiler class. It is responsible for compiling the code that the user has written ,
	 * the compiled language is based on asm in base 4
	 */
	internal string code = "";
	/**
	0 - 4 : usable registers
	5 - 7 : function parameters 
    */
	internal readonly float[] registers = new float[8];
	private bool isRunning;
	public int complexity = 0;
	// ReSharper disable Unity.PerformanceAnalysis because Log is temporary
	private void Error(int _code)
	{
		switch (_code)
		{
			case 0:
				Debug.Log("Error: Invalid command");
				break;
			case 1:
				Debug.Log("Error: Cannot write to read only register");
				break;
			case 2:
				Debug.Log("Error: Division by zero");
				break;
			case 3:
				Debug.Log("Error: Register not found");
				break;
			case 4:
				Debug.Log("Error: Out of bounds");
				break;
			case 5:
				Debug.Log("Error: Missing instruction");
				break;
		}
		isRunning = false;
	}
	[NotNull]
	internal string RegisterName(int _i)
	{
		if (_i is >= 0 and <= 7)
			return new[]
			{
				"r0", "r1", "r2", "r3", "r4", "r5", "r6", "r7"
			}[_i]!;

		Error(3);
		return "ERROR";
	}
	private void Call(int _callCode)
	{
		switch (_callCode)
		{
			case 1:
				// set f0,f1,f2 to forward vector
				Vector3 _forward = transform.forward;
				if (registers != null)
				{
					registers[5] = _forward.x;
					registers[6] = _forward.y;
					registers[7] = _forward.z;
					complexity += 3;
				}
				else Error(3);
				break;
			case 2:
				// set f0,f1,f2 to position vector
				Vector3 _position = transform.position;
				if (registers != null)
				{
					registers[5] = _position.x;
					registers[6] = _position.y;
					registers[7] = _position.z;
					complexity += 3;
				}
				else Error(3);
				break;
			case 3:
				// move to f0,f1,f2
				if (registers != null)
				{
					transform.position = new(registers[5], registers[6], registers[7]);
					complexity += 3;
				}
				else Error(3);
				break;
		}
	}
	private void Mov(int _register, float _value)
	{
		if (registers != null)
			registers[_register] = _value;
		else Error(3);
	}
	private void Mov(int _register, int _copy)
	{
		if (registers != null)
			Mov(_register, registers[_copy]);
		else Error(3);
	}
	private void Add(int _out, int _in1, int _in2)
	{
		if (registers != null)
			registers[_out] = registers[_in1] + registers[_in2];
		else Error(3);
	}
	private void Sub(int _out, int _in1, int _in2)
	{
		if (registers != null)
			registers[_out] = registers[_in1] - registers[_in2];
		else Error(3);
	}
	private void Mul(int _out, int _in1, int _in2)
	{
		if (registers != null)
			registers[_out] = registers[_in1] * registers[_in2];
		else Error(3);
	}
	private void Div(int _out, int _in1, int _in2)
	{
		if (registers == null)
			Error(3);
		// division by 0
		if (registers[_in2] == 0)
			Error(2);
		registers[_out] = registers[_in1] / registers[_in2];
	}
	internal void Compile()
	{
		string[] _lines = code?.Split('\n');
		if (_lines == null)
			return;

		foreach (string _line in _lines)
		{
			string[] _args = _line?.Split(' ');
			if (_args == null || _args[0] == "") continue;

			if (!isRunning)
			{
				isRunning = true;
				return;
			}
			if (_args[^1] == "")
				_args = _args[..^1];
			switch (_args[0])
			{
				case "call":
					if (_args.Length == 2)
					{
						int _callCode = (int)Convert4(_args[1] ?? "0"); //convert the call code to base 10
						if (_callCode == 0)
							return;

						Call(_callCode);
					}
					else
						Error(0);
					break;
				case { } _x when _x[0] == 'r':
					if (_args.Length == 2)
					{
						int _r0 = _args[0][1] - '0'; //convert the register name to a number
						// if the second argument start by r , f or ret , it's a register
						int _r1 = -1;
						if (_args[1].StartsWith("r", StringComparison.Ordinal))
							_r1 = _args[1][1] - '0';
						if (_r1 != -1)
							Mov(_r0, _r1);
						else
							Mov(_r0, Convert4(_args[1] ?? "0"));
						complexity++;
					}
					else
						Error(5);
					break;
				case "add":
					if (_args.Length == 4)
					{
						int _out = _args[1][1] - '0';
						int _in1 = _args[2][1] - '0';
						int _in2 = _args[3][1] - '0';
						Add(_out, _in1, _in2);
						complexity+=2;
					}
					else
						Error(0);
					break;
				case "sub":
					if (_args.Length == 4)
					{
						int _out = _args[1][1] - '0';
						int _in1 = _args[2][1] - '0';
						int _in2 = _args[3][1] - '0';
						Sub(_out, _in1, _in2);
						complexity+=2;
					}
					else
						Error(0);
					break;
				case "mul":
					if (_args.Length == 4)
					{
						int _out = _args[1][1] - '0';
						int _in1 = _args[2][1] - '0';
						int _in2 = _args[3][1] - '0';
						Mul(_out, _in1, _in2);
						complexity+=2;
					}
					else
						Error(0);
					break;
				case "div":
					if (_args.Length == 4)
					{
						int _out = _args[1][1] - '0';
						int _in1 = _args[2][1] - '0';
						int _in2 = _args[3][1] - '0';
						Div(_out, _in1, _in2);
						complexity+=2;
					}
					else
						Error(0);
					break;
				default:
					Error(0);
					break;
			}
		}
		Error(5); // missing instruction
	}
	internal void Compile([NotNull] string _code)
	{
		code = _code;
		Compile();
		Status _status = GetComponent<Status>();
		if (_status != null)
			_status.mana -= complexity;
	}
	private float Convert4([NotNull] string _value)
	{
		float _result = 0;
		for (int _i = 0; _i < _value.Length; _i++)
		{
			// check if the character is a valid character for the given base
			if (!(_value[_i] >= '0' && _value[_i] <= '0' + 4 - 1))
			{
				Error(4);
				return 0;
			}
			int _digit = _value[_i] - '0';
			_result += _digit * (float)Math.Pow(4, _value.Length - _i - 1);
		}
		return _result;
	}
	[NotNull]
	internal static string Convert10(int value)
	{
		if (value < 4)
			return value.ToString();

		string _result = "";
		while (value > 0)
		{
			_result =  (value % 4) + _result;
			value   /= 4;
		}
		return _result;
	}
}
#if UNITY_EDITOR
[CustomEditor(typeof(Compiler))]
public sealed class CompilerEditor : Editor
{
	public override void OnInspectorGUI()
	{
		Compiler _compiler = (Compiler)target;
		if (_compiler == null)
			return;

		_compiler.code = EditorGUILayout.TextArea(_compiler.code, GUILayout.Height(200));
		if (GUILayout.Button("Compile"))
			_compiler.Compile();
		// draw the registers
		for (int _i = 0; _i < _compiler.registers.Length; _i++)
		{
			EditorGUILayout.LabelField($"Register {_compiler.RegisterName(_i)}", _compiler.registers[_i] + " (" + Compiler.Convert10((int)_compiler.registers[_i]) + ")");
		}
	}
}
#endif