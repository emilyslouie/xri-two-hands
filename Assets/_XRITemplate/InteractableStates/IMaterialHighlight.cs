using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.SpatialFramework.Rendering
{
    public enum MaterialHighlightMode
    {
        /// <summary>Adds a new material to the renderers materials array</summary>
        Layer,
        /// <summary>Replace the renderers materials with materials</summary>
        Replace,
    }

    public interface IMaterialHighlight
    {

        /// <summary>
        /// How a new material will be applied to the renderer's material array.
        /// </summary>
        MaterialHighlightMode HighlightMode { get; set; }

        /// <summary>
        /// Material to use for highlighting
        /// </summary>
        Material HighlightMaterial { get; }

        /// <summary>
        /// Used to set up any initial values or materials
        /// </summary>
        void Initialize();

        /// <summary>
        /// Used to remove any persistent objects
        /// </summary>
        void Deinitialize();

        /// <summary>
        /// Raised when a highlight operations has completed
        /// </summary>
        void OnHighlight();

        /// <summary>
        /// Raised when a un-highlight operations has completed
        /// </summary>
        /// <returns>A requested delay to transition out the highlight</returns>
        float OnUnhighlight();
    }
}
