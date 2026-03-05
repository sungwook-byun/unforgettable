using UnityEditor;
using UnityEngine;
using NicePictures;

namespace NicePictures

{
    public class NicePicturesEditor : EditorWindow
    {
        [MenuItem("Window/NicePictures/Convert materials/Built-In")]
        private static void ConvertMaterialToBuiltIn()
        {
            ConvertMaterials("Built-in");
        }

        [MenuItem("Window/NicePictures/Convert materials/URP")]
        private static void ConvertMaterialToURP()
        {
            ConvertMaterials("URP");
        }

        [MenuItem("Window/NicePictures/Convert materials/HDRP")]
        private static void ConvertMaterialToHDRP()
        {
            ConvertMaterials("HDRP");
        }

        private static void ConvertMaterials(string targetPipeline)
        {
            // Отримання обраних об'єктів в сцені
            GameObject[] selectedObjects = Selection.gameObjects;

            if (selectedObjects.Length == 0)
            {
                Debug.LogWarning("No objects selected.");
                return;
            }

            foreach (GameObject selectedObject in selectedObjects)
            {
                // Отримання всіх дочірніх об'єктів, які є MeshRenderer або SkinnedMeshRenderer
                Renderer[] renderers = selectedObject.GetComponentsInChildren<Renderer>(true);

                // Зміна матеріалів для кожного Renderer
                foreach (Renderer renderer in renderers)
                {
                    if (renderer is MeshRenderer || renderer is SkinnedMeshRenderer)
                    {
                        // Заміна матеріалів відповідно до суфіксу кнопки
                        Material[] newMaterials = new Material[renderer.sharedMaterials.Length];

                        for (int i = 0; i < renderer.sharedMaterials.Length; i++)
                        {
                            newMaterials[i] = GetMaterialWithSuffix(renderer.sharedMaterials[i], targetPipeline);
                        }
                        Undo.RecordObject(renderer, "Change Materials");
                        renderer.sharedMaterials = newMaterials;
                    }
                }
            }

            Debug.Log($"Materials converted to {targetPipeline} for selected objects.");
        }

        private static Material GetMaterialWithSuffix(Material originalMaterial, string targetPipeline)
        {
            string originalMaterialName = originalMaterial.name;
            string newSuffix = string.Empty;

            switch (targetPipeline)
            {
                case "Built-in":
                    newSuffix = "";
                    break;
                case "URP":
                    newSuffix = "_URP";
                    break;
                case "HDRP":
                    newSuffix = "_HDRP";
                    break;
            }

            // Заміна або видалення існуючого суфіксу
            originalMaterialName = originalMaterialName.Replace(" (Instance)", string.Empty);
            originalMaterialName = originalMaterialName.Replace("_URP", string.Empty);
            originalMaterialName = originalMaterialName.Replace("_HDRP", string.Empty);

            // Знаходження матеріалу за новим іменем
            string searchPattern = $"{originalMaterialName}{newSuffix}";
            string[] assetGuids = AssetDatabase.FindAssets(searchPattern, null);
            if (assetGuids.Length > 0)
            {
                // Використання першого знайденого матеріалу
                string assetPath = AssetDatabase.GUIDToAssetPath(assetGuids[0]);
                // Material newMaterial = AssetDatabase.LoadAssetAtPath<Material>(assetPath);
                return AssetDatabase.LoadAssetAtPath<Material>(assetPath); ;
            }
            else
            {
                // Якщо матеріал не знайдено, вивід повідомлення про помилку
                Debug.LogError($"Material with name {originalMaterialName}{newSuffix} not found globally.");
                return originalMaterial;
            }
        }
    }
}