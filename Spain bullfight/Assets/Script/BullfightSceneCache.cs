using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class BullfightSceneCache
{
    private static readonly Dictionary<Type, UnityEngine.Object> typeCache = new Dictionary<Type, UnityEngine.Object>();
    private static readonly Dictionary<Type, object> sceneListCache = new Dictionary<Type, object>();
    private static readonly Dictionary<string, UnityEngine.Object> namedCache = new Dictionary<string, UnityEngine.Object>();
    private static readonly Dictionary<string, GameObject> taggedCache = new Dictionary<string, GameObject>();
    private static bool initialized;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        EnsureInitialized();
        Clear();
    }

    public static void Clear()
    {
        typeCache.Clear();
        sceneListCache.Clear();
        namedCache.Clear();
        taggedCache.Clear();
    }

    public static T FindObject<T>(bool includeInactive = true) where T : UnityEngine.Object
    {
        EnsureInitialized();

        Type key = typeof(T);
        if (typeCache.TryGetValue(key, out UnityEngine.Object cached) && IsSceneObjectValid(cached))
            return cached as T;

        T resolved = UnityEngine.Object.FindObjectOfType<T>(includeInactive);
        if (IsSceneObjectValid(resolved))
            typeCache[key] = resolved;
        else
            typeCache.Remove(key);

        return resolved;
    }

    public static T FindSceneObjectByName<T>(string objectName) where T : UnityEngine.Object
    {
        EnsureInitialized();

        string key = typeof(T).FullName + "|" + objectName;
        if (namedCache.TryGetValue(key, out UnityEngine.Object cached) && IsSceneObjectValid(cached))
            return cached as T;

        List<T> sceneObjects = GetSceneObjects<T>();
        for (int i = 0; i < sceneObjects.Count; i++)
        {
            T candidate = sceneObjects[i];
            if (!IsSceneObjectValid(candidate) || !string.Equals(candidate.name, objectName, StringComparison.Ordinal))
                continue;

            namedCache[key] = candidate;
            return candidate;
        }

        namedCache.Remove(key);
        return null;
    }

    public static List<T> GetSceneObjects<T>() where T : UnityEngine.Object
    {
        EnsureInitialized();

        Type key = typeof(T);
        if (sceneListCache.TryGetValue(key, out object cachedObj) && cachedObj is List<T> cachedList)
        {
            PruneInvalidEntries(cachedList);
            return cachedList;
        }

        List<T> resolved = new List<T>();
        T[] allObjects = Resources.FindObjectsOfTypeAll<T>();
        for (int i = 0; i < allObjects.Length; i++)
        {
            T candidate = allObjects[i];
            if (!IsSceneObjectValid(candidate))
                continue;

            resolved.Add(candidate);
        }

        sceneListCache[key] = resolved;
        return resolved;
    }

    public static GameObject FindSceneObjectWithTag(string tag)
    {
        EnsureInitialized();

        if (string.IsNullOrWhiteSpace(tag))
            return null;

        if (taggedCache.TryGetValue(tag, out GameObject cached) && IsSceneObjectValid(cached))
            return cached;

        GameObject resolved;
        try
        {
            resolved = GameObject.FindGameObjectWithTag(tag);
        }
        catch (UnityException)
        {
            resolved = null;
        }

        if (IsSceneObjectValid(resolved))
            taggedCache[tag] = resolved;
        else
            taggedCache.Remove(tag);

        return resolved;
    }

    public static Transform FindTransformWithTag(string tag)
    {
        GameObject taggedObject = FindSceneObjectWithTag(tag);
        return taggedObject != null ? taggedObject.transform : null;
    }

    public static T GetLocalOrScene<T>(Component owner) where T : Component
    {
        if (owner != null)
        {
            T local = owner.GetComponent<T>();
            if (IsSceneObjectValid(local))
                return local;
        }

        return FindObject<T>();
    }

    private static void EnsureInitialized()
    {
        if (initialized)
            return;

        SceneManager.sceneLoaded += HandleSceneLoaded;
        initialized = true;
    }

    private static void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Clear();
    }

    private static void PruneInvalidEntries<T>(List<T> values) where T : UnityEngine.Object
    {
        for (int i = values.Count - 1; i >= 0; i--)
        {
            if (!IsSceneObjectValid(values[i]))
                values.RemoveAt(i);
        }
    }

    private static bool IsSceneObjectValid(UnityEngine.Object candidate)
    {
        if (candidate == null)
            return false;

        switch (candidate)
        {
            case Component component:
                return component.gameObject.scene.IsValid();
            case GameObject gameObject:
                return gameObject.scene.IsValid();
            default:
                return true;
        }
    }
}
