using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InputFieldTouchScreenKeyboard : InputField {
    private TouchScreenKeyboard overlayKeyboard;
    // public static string inputText = "";
    private static TouchScreenKeyboardType _touchScreenKeyboardType = TouchScreenKeyboardType.Default;

    public override void OnSelect(BaseEventData eventData) {
        base.OnSelect(eventData);
        // Debug.Log("OnSelect(); -try- eventData:" + eventData);
        // Debug.Log("OnSelect(); -- TouchScreenKeyboard.area:" + TouchScreenKeyboard.area);
        // Debug.Log("OnSelect(); -- TouchScreenKeyboard.visible:" + TouchScreenKeyboard.visible);
        // Debug.Log("OnSelect(); -- TouchScreenKeyboard.hideInput:" + TouchScreenKeyboard.hideInput);
        // Debug.Log("OnSelect(); -- TouchScreenKeyboard.isSupported:" + TouchScreenKeyboard.isSupported);
        // Debug.Log("OnSelect(); -- TouchScreenKeyboard.isInPlaceEditingAllowed:" + TouchScreenKeyboard.isInPlaceEditingAllowed);
        if (float.TryParse(text, out float fResult)) {
            overlayKeyboard = TouchScreenKeyboard.Open("", TouchScreenKeyboardType.DecimalPad);
        } else if (int.TryParse(text, out int iResult)) {
            overlayKeyboard = TouchScreenKeyboard.Open("", TouchScreenKeyboardType.NumberPad);
        } else {
            overlayKeyboard = TouchScreenKeyboard.Open("", _touchScreenKeyboardType);
        }
        if (overlayKeyboard == null) {
            overlayKeyboard = TouchScreenKeyboard.Open("");
        }
        Debug.Log("OnSelect(); -- overlayKeyboard:" + overlayKeyboard);
        if (overlayKeyboard != null) {
            // Debug.Log("OnSelect(); -- overlayKeyboard.active:" + overlayKeyboard.active);
            // Debug.Log("OnSelect(); -- overlayKeyboard.selection:" + overlayKeyboard.selection);
            // Debug.Log("OnSelect(); -- overlayKeyboard.status:" + overlayKeyboard.status);
            // Debug.Log("OnSelect(); -- overlayKeyboard.text:" + overlayKeyboard.text);
            // Debug.Log("OnSelect(); -- overlayKeyboard.type:" + overlayKeyboard.type);
            // Debug.Log("OnSelect(); -- overlayKeyboard.characterLimit:" + overlayKeyboard.characterLimit);
            // Debug.Log("OnSelect(); -- overlayKeyboard.targetDisplay:" + overlayKeyboard.targetDisplay);
            // Debug.Log("OnSelect(); -- overlayKeyboard.canGetSelection:" + overlayKeyboard.canGetSelection);
            // Debug.Log("OnSelect(); -- overlayKeyboard.canSetSelection:" + overlayKeyboard.canSetSelection);
            overlayKeyboard.text = text;
            // Debug.Log("OnSelect(); -- text:" + text);
            if (TouchScreenKeyboard.isSupported) {
                StartCoroutine(StartWait());
            }
        }
    }

    private IEnumerator StartWait() {
        while (overlayKeyboard.active) {
            // Debug.Log("StartWait(); -- overlayKeyboard:" + overlayKeyboard.ToString());
            // Debug.Log("StartWait(); -- overlayKeyboard.active:" + overlayKeyboard.active);
            // Debug.Log("StartWait(); -- overlayKeyboard.selection:" + overlayKeyboard.selection);
            // Debug.Log("StartWait(); -- overlayKeyboard.status:" + overlayKeyboard.status);
            // Debug.Log("StartWait(); -- overlayKeyboard.text:" + overlayKeyboard.text);
            // Debug.Log("StartWait(); -- overlayKeyboard.type:" + overlayKeyboard.type);
            // Debug.Log("StartWait(); -- overlayKeyboard.characterLimit:" + overlayKeyboard.characterLimit);
            // Debug.Log("StartWait(); -- overlayKeyboard.targetDisplay:" + overlayKeyboard.targetDisplay);
            // Debug.Log("StartWait(); -- overlayKeyboard.canGetSelection:" + overlayKeyboard.canGetSelection);
            // Debug.Log("StartWait(); -- overlayKeyboard.canSetSelection:" + overlayKeyboard.canSetSelection);
            text = overlayKeyboard.text;
            caretPosition = text.Length;
            yield return 0;
        }
        // overlayKeyboard = null;
        Debug.Log("StartWait(); -- text:" + text);
    }
}
