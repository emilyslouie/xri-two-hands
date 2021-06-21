using UnityEditor;
using UnityEditor.XR.Interaction.Toolkit;

[CustomEditor(typeof(XRCompoundGrabInteractable), true), CanEditMultipleObjects]
public class XRCompoundGrabInteractableEditor : XRGrabInteractableEditor
{
    protected SerializedProperty m_SecondaryInteractable;

    protected override void OnEnable()
    {
        base.OnEnable();
        m_SecondaryInteractable = serializedObject.FindProperty("m_SecondaryInteractable");
    }
    /// <summary>
    /// Draw the property fields related to interaction management.
    /// </summary>
    protected override void DrawInteractionManagement()
    {
        base.DrawInteractionManagement();

        EditorGUILayout.PropertyField(m_SecondaryInteractable);
    }
}
