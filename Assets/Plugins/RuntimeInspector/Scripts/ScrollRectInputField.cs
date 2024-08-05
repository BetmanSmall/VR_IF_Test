using System;
using System.Globalization;
using System.Threading;
using RuntimeInspectorNamespace;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ScrollRectInputField : ScrollRect {
    [SerializeField] public BoundInputField boundInputField;

    // The offset from handle position to mouse down position
    private Vector2 m_PointerStartLocalCursor1 = Vector2.zero;
    private Vector2 m_ContentStartPosition1 = Vector2.zero;
    private bool m_Dragging1;
    private MovementType m_MovementType1 = MovementType.Elastic;
    private Bounds m_ViewBounds1;
    private bool m_Horizontal1 = true;
    private bool m_Vertical1 = true;

    public override bool IsActive() {
        if (boundInputField == null) {
            boundInputField = gameObject?.transform?.parent?.parent?.Find(gameObject.name[gameObject.name.Length - 1] + "InputField")?.GetComponent<BoundInputField>();
        }
        return base.IsActive();
    }

    public override void OnBeginDrag(PointerEventData eventData) {
        // base.OnBeginDrag(eventData);
        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        if (!IsActive())
            return;

        UpdateBounds();

        m_PointerStartLocalCursor1 = Vector2.zero;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(viewRect, eventData.position, eventData.pressEventCamera, out m_PointerStartLocalCursor1);
        m_ContentStartPosition1 = content.anchoredPosition;
        m_Dragging1 = true;
    }

    public override void OnDrag(PointerEventData eventData) {
        if (!m_Dragging1)
            return;

        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        if (!IsActive())
            return;

        Vector2 localCursor;
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(viewRect, eventData.position, eventData.pressEventCamera, out localCursor))
            return;

        UpdateBounds();

        var pointerDelta = localCursor - m_PointerStartLocalCursor1;
        Vector2 position = m_ContentStartPosition1 + pointerDelta;

        // Offset to get content into place in the view.
        Vector2 offset = CalculateOffset(position - content.anchoredPosition);
        position += offset;
        if (m_MovementType1 == MovementType.Elastic) {
            if (offset.x != 0)
                position.x = position.x - RubberDelta(offset.x, m_ViewBounds1.size.x);
            if (offset.y != 0)
                position.y = position.y - RubberDelta(offset.y, m_ViewBounds1.size.y);
        }

        SetContentAnchoredPosition(position);

        if (offset != Vector2.zero) {
            // Debug.Log(" -4- boundInputField.Text:" + boundInputField.Text);
            double fieldText = ConvertToDouble(boundInputField.Text);// double.Parse(boundInputField.Text);
            // int intText = int.Parse(boundInputField.Text);
            fieldText -= offset.x * 0.01f;
            fieldText -= offset.y * 0.01f;
            // intText -= (int)offset.x;
            // intText -= (int)offset.y;
            boundInputField.Text = fieldText.ToString("F");
            // boundInputField.Text = intText.ToString();
            // boundInputField.InputFieldValueChanged(boundInputField.Text);
            boundInputField.InputFieldValueSubmitted(boundInputField.Text);
        }
    }

    private double ConvertToDouble(string s) {
        char systemSeparator = Thread.CurrentThread.CurrentCulture.NumberFormat.CurrencyDecimalSeparator[0];
        double result = 0;
        try {
            if (s != null)
                if (!s.Contains(","))
                    result = double.Parse(s, CultureInfo.InvariantCulture);
                else
                    result = Convert.ToDouble(s.Replace(".", systemSeparator.ToString()).Replace(",", systemSeparator.ToString()));
        }
        catch (Exception e) {
            try {
                result = Convert.ToDouble(s);
            }
            catch {
                try {
                    result = Convert.ToDouble(s.Replace(",", ";").Replace(".", ",").Replace(";", "."));
                }
                catch {
                    throw new Exception("Wrong string-to-double format e:" + e);
                }
            }
        }
        return result;
    }

    private static float RubberDelta(float overStretching, float viewSize) {
        return (1 - (1 / ((Mathf.Abs(overStretching) * 0.55f / viewSize) + 1))) * viewSize * Mathf.Sign(overStretching);
    }

    private Vector2 CalculateOffset(Vector2 delta) {
        return InternalCalculateOffset(ref m_ViewBounds1, ref m_ContentBounds, m_Horizontal1, m_Vertical1, m_MovementType1, ref delta);
    }

    internal static Vector2 InternalCalculateOffset(ref Bounds viewBounds, ref Bounds contentBounds, bool horizontal, bool vertical, MovementType movementType, ref Vector2 delta) {
        Vector2 offset = Vector2.zero;
        if (movementType == MovementType.Unrestricted)
            return offset;

        Vector2 min = contentBounds.min;
        Vector2 max = contentBounds.max;

        // min/max offset extracted to check if approximately 0 and avoid recalculating layout every frame (case 1010178)

        if (horizontal) {
            min.x += delta.x;
            max.x += delta.x;

            float maxOffset = viewBounds.max.x - max.x;
            float minOffset = viewBounds.min.x - min.x;

            if (minOffset < -0.001f)
                offset.x = minOffset;
            else if (maxOffset > 0.001f)
                offset.x = maxOffset;
        }

        if (vertical) {
            min.y += delta.y;
            max.y += delta.y;

            float maxOffset = viewBounds.max.y - max.y;
            float minOffset = viewBounds.min.y - min.y;

            if (maxOffset > 0.001f)
                offset.y = maxOffset;
            else if (minOffset < -0.001f)
                offset.y = minOffset;
        }

        return offset;
    }
}
