using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using System;

public class Console : MonoBehaviour
{
    public StyleSheet GetStyleSheet(LogType type)
    {
        return type switch
        {
            LogType.Error => Resources.Load<StyleSheet>("StyleSheets/Console/StyleError"),
            LogType.Assert => Resources.Load<StyleSheet>("StyleSheets/Console/StyleAssert"),
            LogType.Warning => Resources.Load<StyleSheet>("StyleSheets/Console/StyleWarning"),
            LogType.Log => Resources.Load<StyleSheet>("StyleSheets/Console/StyleLog"),
            LogType.Exception => Resources.Load<StyleSheet>("StyleSheets/Console/StyleException"),
            _ => null,
        };
    }
    public VisualTreeAsset LogItemTemplate; // Assign via inspector
    public ScrollView LogView;
    private VisualElement root;
    [SerializeField] private UIDocument document;
    public UnityEvent OnUserInputChanged;
    private string userInput;
    public string UserInput 
    { 
        get => userInput;
        private set
        {
            userInput = value;
            OnUserInputChanged.Invoke();
        }
    }
    void Awake()
    {
        DontDestroyOnLoad(this);
        root = document.rootVisualElement;
        SetupListView();
        Application.logMessageReceivedThreaded += OnUnityLogReceived;
        InputSystem.actions.FindAction("consoleToggle").performed += OnConsoleToggle;
    }
    private void Start()
    {
        ToggleConsoleVisablity();
        var inputField = root.Q<TextField>("Input");

        inputField.RegisterCallback<KeyDownEvent>(evt =>
        {
            if (evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.KeypadEnter)
            {
                userInput = inputField.value;
                inputField.value = "";
                AddLog(userInput);
                inputField.Focus();
            }
        },
        TrickleDown.TrickleDown
        );
    }
    void OnDestroy()
    {
        // Unsubscribe when destroyed
        Application.logMessageReceived -= OnUnityLogReceived;
    }
    private void SetupListView()
    {
        LogView = root.Q<ScrollView>("LogView");
    }

    public void AddLog(string Title, string Description = "", LogType type = LogType.Log)
    {
        var logItem = new ConsoleLog(Title, Description, GetStyleSheet(type));
        var template = LogItemTemplate.Instantiate();
        var visual = logItem.CreateUIElement(template.templateSource);
        LogView.Add(visual);
    }
    private void OnUnityLogReceived(string condition, string stackTrace, LogType type)
    {
        // Create a new ConsoleLog representing this Unity log
        string title = condition;
        string message = stackTrace;
        AddLog(title, message, type);
    }
    private void OnConsoleToggle(InputAction.CallbackContext _)
    {
        ToggleConsoleVisablity();
    }
    private void ToggleConsoleVisablity()
    {
        root.visible = !root.visible;
        root.focusable = !root.focusable;
    }
}
