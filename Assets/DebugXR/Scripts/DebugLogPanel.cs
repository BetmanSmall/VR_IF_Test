using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshPro))]
public class DebugLogPanel : MonoBehaviour {
    [Header("Visual Feedback")] [Tooltip("Granularity. Sometimes you may not want to see everything being sent to the console.")] [SerializeField]
    LogType LogLevel;

    [Tooltip("Maximum number of messages before deleting the older messages.")] [SerializeField]
    private int maxNumberOfMessages = 40;

    [Tooltip("Check this if you want the stack trace printed after the message.")] [SerializeField]
    private bool includeStackTrace = false;

    [Header("Auditory Feedback")] [Tooltip("Play a sound when the message panel is updated.")] [SerializeField]
    private bool playSoundOnMessage;

    private bool newMessageArrived = false;
    private TextMeshPro debugText;
    private Queue<string> messageQueue;
    private AudioSource messageSound;
    private String lastCondition;

    private void OnEnable() {
        messageQueue = new Queue<string>();
        debugText = GetComponent<TextMeshPro>();
        Application.logMessageReceivedThreaded += Application_logMessageReceivedThreaded;
        messageSound = this.GetComponent<AudioSource>();
    }

    private void Application_logMessageReceivedThreaded(string condition, string stackTrace, LogType type) {
        if (type == LogLevel) {
            if (messageSound != null && playSoundOnMessage) {
                messageSound.Play();
            }
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("\n");
            stringBuilder.Append(condition);
            if (includeStackTrace) {
                stringBuilder.Append("\nStackTrace: ");
                stringBuilder.Append(stackTrace);
            }
            condition = stringBuilder.ToString();
            messageQueue.Enqueue(condition);
            newMessageArrived = true;
            if (messageQueue.Count > maxNumberOfMessages) {
                messageQueue.Dequeue();
            }
        }
    }

    private void OnDisable() {
        Application.logMessageReceivedThreaded -= Application_logMessageReceivedThreaded;
    }

    private void PrintQueue() {
        StringBuilder stringBuilder = new StringBuilder();
        string[] messageList = messageQueue.ToArray();
        for (int i = 0; i < messageList.Length; i++) {
            stringBuilder.Append(messageList[i]);
            stringBuilder.Append("\n");
        }
        string message = stringBuilder.ToString();
        debugText.text = message;
    }

    private void Update() {
        if (newMessageArrived) {
            PrintQueue();
            newMessageArrived = false;
        }
    }

    public void ClearQueue() {
        messageQueue.Clear();
        newMessageArrived = true;
    }
}
