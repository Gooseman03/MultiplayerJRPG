using UnityEngine;
using UnityEngine.UIElements;
using System;
using System.IO;
using Microsoft.Win32.SafeHandles;
using System.Linq;
using Unity.Collections;
public class ConsoleLog
{
    public string Title { get; private set; }
    public string Description { get; private set; }
    public StyleSheet addedStyle;
    public VisualElement UIElement { get; private set; }

    public ConsoleLog(string Title, string Description, StyleSheet style)
    {
        this.Title = Title;
        this.Description = Description;
        this.addedStyle = style;
        string filestuff = DateTime.Now.ToString("[h:mm:ss]") + " " + Title;
        string directory = Application.persistentDataPath;
        directory = directory + "/YourLogs";

        System.IO.Directory.CreateDirectory(directory);
        // that command means "create if not already there, otherwise leave it alone"

        string filename = directory + "/log.txt";

        try
        {
            System.IO.File.AppendAllText(filename, filestuff + "\n");
        }
        catch
        {
            // careful not to create a loop of logging!
        }
    }

    public VisualElement CreateUIElement(VisualTreeAsset template)
    {
        UIElement = template.CloneTree();
        UIElement.styleSheets.Add(addedStyle);
        BindUI(UIElement);
        return UIElement;
    }

    public void BindUI(VisualElement element)
    {
        var titleLabel = element.Q<Label>("Log");
        if (titleLabel != null)
            titleLabel.text = DateTime.Now.ToString("[h:mm:ss]") + " " + Title;
        if (Description != "")
        {
            var description = new Label(Description)
            {
                bindingPath = "description"
            };
            description.styleSheets.Add(addedStyle);
            description.AddToClassList("log_description");
            element.Add(description);
        }
        titleLabel.styleSheets.Add(addedStyle);
    }
}