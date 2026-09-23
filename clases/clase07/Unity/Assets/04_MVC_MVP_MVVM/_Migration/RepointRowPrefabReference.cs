using System;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Clase07.EditorTools
{
    // Herramienta de una sola vez para la migración a carpetas autocontenidas: cuando
    // se copia InventoryItemRow.prefab a una carpeta nueva, Unity le asigna un GUID
    // distinto al importar, y la referencia serializada de la escena al prefab viejo
    // queda rota. Esto reasigna esa referencia al prefab nuevo y guarda la escena, sin
    // abrir el Editor de forma interactiva. Se borra junto con el resto de esta carpeta
    // (`_Migration/`) cuando las tres implementaciones de MVx ya la usaron.
    public static class RepointRowPrefabReference
    {
        public static void Run()
        {
            var args = Environment.GetCommandLineArgs();
            var scenePath = GetArg(args, "-scenePath");
            var componentTypeName = GetArg(args, "-componentType");
            var fieldName = GetArg(args, "-fieldName");
            var newPrefabPath = GetArg(args, "-newPrefabPath");

            var newPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(newPrefabPath);
            if (newPrefab == null)
            {
                Debug.LogError($"No se encontró el prefab en {newPrefabPath}");
                EditorApplication.Exit(1);
                return;
            }

            EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

            var componentType = Type.GetType(componentTypeName);
            if (componentType == null)
            {
                Debug.LogError($"No se pudo resolver el tipo {componentTypeName}");
                EditorApplication.Exit(1);
                return;
            }

            var target = UnityEngine.Object.FindObjectOfType(componentType);
            if (target == null)
            {
                Debug.LogError($"No se encontró ningún componente de tipo {componentTypeName} en {scenePath}");
                EditorApplication.Exit(1);
                return;
            }

            var so = new SerializedObject(target);
            var prop = so.FindProperty(fieldName);
            if (prop == null)
            {
                Debug.LogError($"No se encontró el campo serializado {fieldName} en {componentTypeName}");
                EditorApplication.Exit(1);
                return;
            }

            // El campo serializado puede esperar el GameObject raíz del prefab o un
            // Component específico (p.ej. una vista de fila). Asignar el tipo
            // equivocado hace que Unity trate la referencia como null en silencio, así
            // que resolvemos el tipo declarado del campo por reflexión y extraemos el
            // Object correcto del prefab nuevo antes de asignarlo.
            var fieldInfo = componentType.GetField(
                fieldName,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (fieldInfo == null)
            {
                Debug.LogError($"No se pudo resolver por reflexión el campo {fieldName} en {componentTypeName}");
                EditorApplication.Exit(1);
                return;
            }

            UnityEngine.Object valueToAssign = newPrefab;
            if (fieldInfo.FieldType != typeof(GameObject))
            {
                valueToAssign = newPrefab.GetComponent(fieldInfo.FieldType);
                if (valueToAssign == null)
                {
                    Debug.LogError(
                        $"El prefab en {newPrefabPath} no tiene un componente de tipo {fieldInfo.FieldType}");
                    EditorApplication.Exit(1);
                    return;
                }
            }

            prop.objectReferenceValue = valueToAssign;
            so.ApplyModifiedProperties();

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
            EditorApplication.Exit(0);
        }

        private static string GetArg(string[] args, string name)
        {
            for (var i = 0; i < args.Length - 1; i++)
            {
                if (args[i] == name) return args[i + 1];
            }
            throw new ArgumentException($"Falta el argumento de línea de comandos {name}");
        }
    }
}
