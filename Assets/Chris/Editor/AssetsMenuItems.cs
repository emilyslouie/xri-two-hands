using System;
using JetBrains.Annotations;
using UnityEditor;

namespace UnityEngine.XR.Interaction.InternalTools
{
    /// <summary>
    /// Additional menu items under Assets.
    /// </summary>
    [UsedImplicitly]
    static class AssetsMenuItems
    {
        /// <summary>
        /// Writes all unsaved asset changes to disk.
        /// </summary>
        [MenuItem("Assets/Save Assets to Disk &s"), UsedImplicitly]
        internal static void SaveAssets()
        {
            AssetDatabase.SaveAssets();
            Debug.Log($"Saved assets to disk at {DateTime.Now}.");
        }

        [MenuItem("Assets/Force Reserialize Assets"), UsedImplicitly]
        internal static void ForceReserializeAssets()
        {
            // ReSharper disable BitwiseOperatorOnEnumWithoutFlags
            var assets = Selection.GetFiltered<Object>(SelectionMode.TopLevel | SelectionMode.Assets | SelectionMode.DeepAssets);
            // ReSharper restore BitwiseOperatorOnEnumWithoutFlags

            if (assets.Length == 0)
                return;

            var assetPaths = new string[1];
            try
            {
                for (var index = 0; index < assets.Length; ++index)
                {
                    var assetPath = AssetDatabase.GetAssetPath(assets[index]);

                    const string title = "Force Reserialize Assets (asset + .meta)";
                    var info = $"({index + 1}/{assets.Length}) {assetPath}";
                    var progress = index / (float)assets.Length;
                    if (EditorUtility.DisplayCancelableProgressBar(title, info, progress))
                    {
                        // Cancelled, skip remaining
                        return;
                    }

                    assetPaths[0] = assetPath;
                    AssetDatabase.ForceReserializeAssets(assetPaths);
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }

            Debug.Log($"Reserialized {assets.Length} assets at {DateTime.Now}.");
        }
    }
}
