#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

[AttributeUsage(AttributeTargets.Class)]
public class FSMHelpboxAttribute : PropertyAttribute
{
    public MessageType MessageType;
    public string Text;
    public FSMHelpboxAttribute(string text, MessageType messageType)
    {
        this.Text = text;
        this.MessageType = messageType;
    }
}
#endif