using UnityEngine;

namespace Unity.SpatialFramework.Rendering
{
    /// <summary>
    /// Used to change the materials array of an object when highlighted. Can either add the highlight material to the
    /// renderers materials array or replace the renderers materials with the highlight material.
    /// </summary>
    /// <remarks>
    /// Layering a material will only draw the layer on the last sub mesh in a mesh renderer.
    /// </remarks>
    public class MaterialHighlight : MonoBehaviour, IMaterialHighlight
    {
        [SerializeField]
        [Tooltip("How the highlight material will be applied to the renderer's material array.")]
        MaterialHighlightMode m_HighlightMode = MaterialHighlightMode.Replace;

        [SerializeField, Tooltip("Material to use for highlighting")]
        Material m_HighlightMaterial;

        /// <summary>
        /// How the highlight material will be applied to the renderer's material array.
        /// </summary>
        public MaterialHighlightMode HighlightMode
        {
            get => m_HighlightMode;
            set => m_HighlightMode = value;
        }

        /// <summary>
        /// Material to use for highlighting
        /// </summary>
        public Material HighlightMaterial
        {
            get => m_HighlightMaterial;
            set => m_HighlightMaterial = value;
        }

        void IMaterialHighlight.Initialize() { }

        void IMaterialHighlight.Deinitialize() { }

        void IMaterialHighlight.OnHighlight() { }

        float IMaterialHighlight.OnUnhighlight() { return 0.0f; }
    }
}
