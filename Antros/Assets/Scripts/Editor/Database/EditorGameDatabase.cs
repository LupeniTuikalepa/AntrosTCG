using System;
using System.Collections.Generic;
using ATCG.Databases;
using UnityEditor;
using UnityEngine;

public static class EditorGameDatabase
{
    private static Dictionary<Guid, GameDatabaseObject> existings = new();

    [InitializeOnLoadMethod]
    [MenuItem("Tools/ATCG/Ensure unique GUIDs")]
    private static void Initialize()
    {
        existings.Clear();
        string[] guids = AssetDatabase.FindAssets($"t:{nameof(GameDatabaseObject)}");
        for (int i = 0; i < guids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);
            GameDatabaseObject obj = AssetDatabase.LoadAssetAtPath<GameDatabaseObject>(path);
            Register(obj);
        }

        AssetDatabase.SaveAssets();
    }

    private static void Register(GameDatabaseObject gameDatabaseObject)
    {
        if (existings.TryGetValue(gameDatabaseObject.ID, out GameDatabaseObject existing))
        {
            if(existing == gameDatabaseObject)
                return;

            Debug.LogWarning($"Found duplicated ID {gameDatabaseObject.ID}. Changing {gameDatabaseObject.name} ID's.");
            using (SerializedObject so = new SerializedObject(gameDatabaseObject))
            {
                SerializedProperty property = so.FindProperty("guidText");
                property.boxedValue = Guid.NewGuid().ToString();

                so.ApplyModifiedPropertiesWithoutUndo();

            }
        }

        existings[gameDatabaseObject.ID] = gameDatabaseObject;
    }

}