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
            LogType.Error => Resources.Load<StyleSheet>("StyleError"),
            LogType.Assert => Resources.Load<StyleSheet>("StyleAssert"),
            LogType.Warning => Resources.Load<StyleSheet>("StyleWarning"),
            LogType.Log => Resources.Load<StyleSheet>("StyleLog"),
            LogType.Exception => Resources.Load<StyleSheet>("StyleException"),
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
        InputSystem.actions.FindAction("consoleToggle").performed += ToggleConsoleVisablity;
    }
    private void Start()
    {
        var inputField = root.Q<TextField>("Input");

        inputField.RegisterCallback<KeyDownEvent>(evt =>
        {
            if (evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.KeypadEnter)
            {
                userInput = inputField.value;
                inputField.value = "";
                AddLog(userInput);
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
        // Find ListView from UI
        LogView = root.Q<ScrollView>("LogView");
        

        //LogView.makeItem = () =>
        //{
        //    var template = LogItemTemplate;
        //    var element = template.CloneTree();
        //    return element;
        //};
        //
        //LogView.bindItem = (element, index) =>
        //{
        //    var log = logs[index];
        //    log.BindUI(element);
        //};
    }

    public void AddLog(string Title, string Description = "", LogType type = LogType.Log)
    {
        var logItem = new ConsoleLog(Title, Description, GetStyleSheet(type));
        var template = LogItemTemplate.Instantiate();
        var visual = logItem.CreateUIElement(template.templateSource);
        LogView.Add(visual);
    }
    public void RemoveLog(ConsoleLog logItem)
    {
        //if (LogView.Contains(logItem))
        //{
        //    LogView.Remove(logItem);
        //}
    }
    private void OnUnityLogReceived(string condition, string stackTrace, LogType type)
    {
        // Create a new ConsoleLog representing this Unity log
        string title = condition;
        string message = stackTrace;
        AddLog(title, message, type);
    }
    private void ToggleConsoleVisablity(InputAction.CallbackContext context)
    {
        if (context.ReadValueAsButton() == true)
        {
            root.visible = !root.visible;
            root.focusable = !root.focusable;
        }
    }
}
