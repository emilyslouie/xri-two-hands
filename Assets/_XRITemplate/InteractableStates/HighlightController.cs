using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Unity.SpatialFramework.Rendering
{
    [System.Serializable]
    public class HighlightController
    {
        // Local method use only -- created here to reduce garbage collection. Collections must be cleared before use
        static readonly List<Renderer> k_RendererComponents = new List<Renderer>();

        [SerializeField]
        [Tooltip("Used to set the mode of capturing renderers on an object or to use only manually set renderers.")]
        RendererCaptureDepth m_RendererCaptureDepth = RendererCaptureDepth.AllChildRenderers;

        [SerializeField]
        [Tooltip("Manually set renderers to be affected by the highlight")]
        protected Renderer[] m_ManuallySetRenderers = new Renderer[0];

        int m_MaterialMutations = 0;
        int m_MaterialAdditions = 0;

        List<IMaterialHighlight> m_CacheUsers = new List<IMaterialHighlight>();
        HashSet<Renderer> m_Renderers = new HashSet<Renderer>();
        Dictionary<int, Material[]> m_OriginalMaterials = new Dictionary<int, Material[]>();
        Dictionary<int, Material[]> m_HighlightMaterials = new Dictionary<int, Material[]>();

        bool m_DelayedUnhighlight = false;
        float m_UnhighlightTimer = 0.0f;

        public Transform RendererSource { get; set; }

        public void RegisterCacheUser(IMaterialHighlight cacheUser)
        {
            if (cacheUser.HighlightMode == MaterialHighlightMode.Layer)
                m_MaterialAdditions++;
            else
                m_MaterialMutations++;

            // Set cache user to know about this
            m_CacheUsers.Add(cacheUser);
        }

        public void DeregisterCacheUser(IMaterialHighlight cacheUser)
        {
            if (cacheUser.HighlightMode == MaterialHighlightMode.Layer)
                m_MaterialAdditions--;
            else
                m_MaterialMutations--;

            m_CacheUsers.Remove(cacheUser);
        }


        public void Initialize()
        {
            if (RendererSource == null)
            {
                Debug.LogError("Trying to use a Highlight Controller before setting the root gameobject!");
                return;
            }

            foreach (var cacheUser in m_CacheUsers)
            {
                cacheUser.Initialize();
            }

            // Cache the renderers
            UpdateRendererCache();

            // Generate the original material list and implement the materials from the included highlights
            UpdateMaterialCache();
        }

        public void Deinitialize()
        {
            foreach (var cacheUser in m_CacheUsers)
            {
                if (cacheUser != null)
                    cacheUser.Deinitialize();
            }
        }

        public void Update()
        {
            if (m_DelayedUnhighlight)
            {
                m_UnhighlightTimer -= Time.deltaTime;
                if (m_UnhighlightTimer <= 0.0f)
                {
                    m_DelayedUnhighlight = false;

                    UpdateMaterialCache();
                    foreach (var renderer in m_Renderers)
                    {
                        var rendererID = renderer.GetInstanceID();
                        renderer.materials = m_OriginalMaterials[rendererID];
                    }
                }
            }
        }
        public void Highlight()
        {
            m_DelayedUnhighlight = false;
            UpdateMaterialCache();
            foreach (var renderer in m_Renderers)
            {
                var rendererID = renderer.GetInstanceID();
                renderer.materials = m_HighlightMaterials[rendererID];
            }
            foreach (var cacheUser in m_CacheUsers)
            {
                if (cacheUser != null)
                    cacheUser.OnHighlight();
            }
        }
        public void Unhighlight(bool force = false)
        {
            UpdateMaterialCache();
            
            var maxDelay = 0.0f;
            foreach (var cacheUser in m_CacheUsers)
            {
                if (cacheUser != null)
                    maxDelay = Mathf.Max(cacheUser.OnUnhighlight());
            }

            if (maxDelay <= 0.0f)
            {
                foreach (var renderer in m_Renderers)
                {
                    var rendererID = renderer.GetInstanceID();
                    renderer.materials = m_OriginalMaterials[rendererID];
                }
            }
            else
            {
                m_DelayedUnhighlight = true;
                m_UnhighlightTimer = maxDelay;
            }
        }


        void UpdateRendererCache()
        {
            m_Renderers.Clear();
            m_Renderers.UnionWith(m_ManuallySetRenderers.Where(r => r != null));

            switch (m_RendererCaptureDepth)
            {
                case RendererCaptureDepth.AllChildRenderers:
                    RendererSource.GetComponentsInChildren(true, k_RendererComponents);

                    foreach (var renderer in k_RendererComponents)
                    {
                        if (renderer.GetComponent<TextMesh>() == null)
                            m_Renderers.Add(renderer);
                    }
                    k_RendererComponents.Clear();

                    break;
                case RendererCaptureDepth.CurrentRenderer:
                    RendererSource.GetComponents(k_RendererComponents);

                    foreach (var renderer in k_RendererComponents)
                    {
                        if (renderer.GetComponent<TextMesh>() == null)
                            m_Renderers.Add(renderer);
                    }
                    k_RendererComponents.Clear();
                    break;
                case RendererCaptureDepth.ManualOnly:
                    break;
                default:
                    Debug.LogError($"{RendererSource.name} highlight has an invalid renderer capture mode {m_RendererCaptureDepth}.", RendererSource);
                    break;
            }

            if (m_Renderers.Count == 0)
                Debug.LogWarning($"{RendererSource.name} highlight has no renderers set.", RendererSource);
        }

        void UpdateMaterialCache()
        {
            foreach (var renderer in m_Renderers)
            {
                var rendererID = renderer.GetInstanceID();
                if (m_OriginalMaterials.ContainsKey(rendererID))
                    continue;

                var sharedMaterials = renderer.sharedMaterials;
                var sharedLength = sharedMaterials.Length;
                m_OriginalMaterials[rendererID] = sharedMaterials;

                Material[] highlightMaterials = new Material[sharedLength + m_MaterialAdditions];

                for (var matIndex = 0; matIndex < sharedLength; matIndex++)
                {
                    highlightMaterials[matIndex] = sharedMaterials[matIndex];
                }

                var addOffset = 0;

                for (var i = 0; i < m_CacheUsers.Count; i++)
                {
                    var cacheUser = m_CacheUsers[i];
                    if (cacheUser.HighlightMode == MaterialHighlightMode.Replace)
                    {
                        for (var matIndex = 0; matIndex < sharedLength; matIndex++)
                        {
                            highlightMaterials[matIndex] = cacheUser.HighlightMaterial;
                        }
                    }
                    else
                    {
                        highlightMaterials[sharedLength + addOffset] = cacheUser.HighlightMaterial;
                        addOffset++;
                    }
                }
                m_HighlightMaterials[rendererID] = highlightMaterials;  
            }
        }
    }
}
