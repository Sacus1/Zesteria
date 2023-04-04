using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
public sealed class Creator : MonoBehaviour
{
	public           GameObject                     template;
	public           GameObject                     storage;
	public           ScrollRect                     main;
	public           Button                         newLine;
	[FormerlySerializedAs("remove")]
	public           Button                         removeButton;
	public           Button                         saveButton;
	public           Compiler                       compiler;
	public           string[]                       commands;
	public           Sprite[]                       sprites;
	private readonly List<List<GameObject>>         lines = new();
	private          int                            screenWidth, screenHeight;
	private          int                            maxCol;
	private          int                            storageRow, storageCol, mainRow, mainCol;
	private          float                          height,     width;
	private          Dictionary<string, GameObject> storageContent = new();
	private void ChangeSize([NotNull] RectTransform _rect)
	{
		_rect.sizeDelta     = new(25f / (Screen.width / 800f), 25f / (Screen.height / 600f));
		Rect _transformRect = _rect.rect;
		width               = _transformRect.width;
		height              = _transformRect.height;
		maxCol              = (int)(screenWidth / (width + width / 2f) - 3);
		_rect.localPosition = new(width * storageCol + width / 2 * (storageCol + 2), -height * storageRow - height / 2 * (storageRow + 2), 0);
		storageCol++; // increase col
		if (!(storageCol >= maxCol))
			return;

		storageCol = 0; // reset col
		storageRow++;   // increase row
	}
	private void Start()
	{
		// create a button for each command
		if (commands == null)
			return;

		if (lines == null) return;

		lines.Add(new());
		// get screen size
		screenWidth  = Screen.width;
		screenHeight = Screen.height;
		if (commands == null) return;

		for (int _i = 0; _i < commands.Length; _i++)
		{
			if (storage == null) continue;
			// create button
			storageContent                                                        ??= new();
			storageContent[commands[_i] ?? throw new InvalidOperationException()] =   Instantiate(template, storage.transform);
			storageContent[commands[_i]].name                                    =   commands[_i] ?? string.Empty;
			storageContent[commands[_i]].SetActive(true);
			ChangeSize(storageContent[commands[_i]].GetComponent<RectTransform>());
			Image _image = storageContent[commands[_i]].GetComponent<Button>()?.image;
			if (_image != null && sprites != null)
				_image.sprite = sprites[_i];
			int _i1 = _i;

			void Call()
			{
				if (mainCol >= maxCol)
					return;
				// create a new button in main
				if (commands == null)
					return;

				GameObject _newButton = Instantiate(storageContent[commands[_i1] ?? string.Empty], main.content.transform);
				// get size of the button
				RectTransform _rect = _newButton.GetComponent<RectTransform>();
				// set the position of the button in main using row and col
				_rect.localPosition = new(width * mainCol + width / 2, -height * mainRow - width / 2, 0);
				// store the command in the compiler
				compiler.code += commands[_i1] == "space" ? " " : commands[_i1];
				// if the command is not a number add " " to the command to separate it from the next command
				if (!(commands[_i1] == "1" || commands[_i1] == "2" || commands[_i1] == "3" || commands[_i1] == "0"))
					compiler.code += " ";
				// add the button to the list
				lines[^1].Add(_newButton);
				// rename the button
				_newButton.name = commands[_i1] ?? string.Empty;
				// increment row and col
				mainCol++;
			}

			storageContent[commands[_i]].GetComponent<Button>().onClick?.AddListener(Call);
		}
		storageCol = 0;
		storageRow = 0;
		newLine.onClick.AddListener(() =>
		{
			mainRow++;
			mainCol       =  0;
			compiler.code += "\n";
			lines.Add(new());
			// change main content size
			Vector2 _sizeDelta = main.content.sizeDelta;
			_sizeDelta             = new(_sizeDelta.x, _sizeDelta.y + 50);
			main.content.sizeDelta = _sizeDelta;
		});
		removeButton.onClick.AddListener(() =>
		{
			if (lines.Count == 0)
				return;
			// get commands to remove
			if (lines[^1].Count == 0)
			{
				lines.RemoveAt(lines.Count - 1);
				mainRow--;
				mainCol = lines[^1].Count;
			}
			string _command = lines[^1][^1].name;
			_command = _command == "space" ? " " : _command;
			// remove the last command
			compiler.code = compiler.code.Remove(compiler.code.LastIndexOf(_command, StringComparison.Ordinal));
			// remove the last button
			Destroy(lines[^1][^1]);
			lines[^1].RemoveAt(lines[^1].Count - 1);
			// decrement row and col
			mainCol--;
		});
		saveButton.onClick.AddListener(Save);
		Load();
	}
	private void Save()
	{
		compiler.GetComponent<Status>().savedProgram = compiler.code;
		// store the code in PlayerPrefs
		PlayerPrefs.SetString("code", compiler.code);
		PlayerPrefs.Save();
	}
	private void Load()
	{
		// load the code from PlayerPrefs
		compiler.code = PlayerPrefs.GetString("code", "");
		// check if the code is empty
		if (compiler.code == "")
			return;
		// split the code into lines
		string[] _lines = compiler.code.Split('\n');
		// for each line
		foreach (string _line in _lines)
		{
			// split the line into commands
			string[] _commands = _line.Split(' ');
			// for each command
			foreach (string _s in _commands)
			{
				// if the command is empty continue
				string _command = _s;
				if (_s == string.Empty)
				{
					// if last command is a number don't add a space
					if (lines[^1][^1].name is "1" or "2" or "3" or "0")
						continue;

					_command = "space";
				}
				// create a new button in main
				GameObject _newButton = Instantiate(storageContent[_command], main.content.transform);
				// get size of the button
				RectTransform _rect = _newButton.GetComponent<RectTransform>();
				// set the position of the button in main using row and col
				_rect.localPosition = new(width * mainCol + width / 2, -height * mainRow - width / 2, 0);
				// add the button to the list
				lines[^1].Add(_newButton);
				// rename the button
				_newButton.name = _command;
				// increment row and col
				mainCol++;
			}
			// change main content size
			Vector2 _sizeDelta = main.content.sizeDelta;
			_sizeDelta             = new(_sizeDelta.x, _sizeDelta.y + 50);
			main.content.sizeDelta = _sizeDelta;
			mainRow++;
			mainCol = 0;
		}
	}
	private void Update()
	{
		if (screenHeight == Screen.height && screenWidth == Screen.width)
			return;
		storageCol = -1; 
		storageRow = 0;
		foreach (Transform _child in storage.transform)
			ChangeSize(_child.GetComponent<RectTransform>());
		screenHeight = Screen.height;
		screenWidth  = Screen.width;
	}
}