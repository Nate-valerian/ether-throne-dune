// ClaudeControlServer — runs an HTTP server inside the Unity Editor so
// Claude Code can build the scene, wire Inspector fields, and control
// Play mode without any manual clicking.
//
// Default port: 7777  (change PORT below if needed)
// All endpoints accept and return JSON.
//
// Endpoints
//   GET  /ping                  — liveness check
//   GET  /hierarchy             — dump full scene GameObject tree
//   GET  /logs                  — last captured console entries
//   POST /create                — create a named GameObject (optionally as child)
//   POST /add-component         — add a C# component by type name
//   POST /set-active            — set GameObject active/inactive
//   POST /set-field             — set a serialised Inspector field by name
//   POST /set-reference         — wire an object reference field
//   POST /create-prefab-instance — instantiate a prefab from Assets/
//   POST /play                  — enter Play mode
//   POST /stop                  — exit Play mode
//   POST /save-scene            — save the open scene

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public static class ClaudeControlServer
{
    const int PORT = 7777;

    static HttpListener _listener;
    static Thread _listenerThread;
    static readonly ConcurrentQueue<(HttpListenerContext ctx, Func<string> handler)> _queue = new();
    static readonly List<string> _logBuffer = new();
    static bool _running;

    // ── Initialise ────────────────────────────────────────────────────────────

    static ClaudeControlServer()
    {
        Application.logMessageReceived += CaptureLog;
        EditorApplication.update       += DrainQueue;
        EditorApplication.quitting     += Stop;
        Start();
    }

    static void Start()
    {
        if (_running) return;
        _listener = new HttpListener();
        _listener.Prefixes.Add($"http://localhost:{PORT}/");
        try
        {
            _listener.Start();
            _running = true;
            _listenerThread = new Thread(Listen) { IsBackground = true };
            _listenerThread.Start();
            Debug.Log($"[Claude] Control server listening on http://localhost:{PORT}/");
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[Claude] Could not start control server: {e.Message}");
        }
    }

    static void Stop()
    {
        _running = false;
        try { _listener?.Stop(); } catch { }
    }

    // ── Background listener thread ─────────────────────────────────────────────

    static void Listen()
    {
        while (_running)
        {
            try
            {
                var ctx = _listener.GetContext();
                RouteRequest(ctx);
            }
            catch (HttpListenerException) { break; }
            catch (Exception e) { Debug.LogError($"[Claude] Listener error: {e}"); }
        }
    }

    static void RouteRequest(HttpListenerContext ctx)
    {
        var method = ctx.Request.HttpMethod.ToUpper();
        var path   = ctx.Request.Url.AbsolutePath.TrimEnd('/').ToLower();

        // GET endpoints that don't need the main thread
        if (method == "GET" && path == "/ping")
        {
            Respond(ctx, 200, "{\"status\":\"alive\"}");
            return;
        }
        if (method == "GET" && path == "/logs")
        {
            var logs = string.Join(",", _logBuffer.TakeLast(50).Select(l => $"\"{EscapeJson(l)}\""));
            Respond(ctx, 200, $"{{\"logs\":[{logs}]}}");
            return;
        }

        // All other endpoints need the main thread — enqueue with a result slot
        string result = null;
        var done = new ManualResetEventSlim(false);

        _queue.Enqueue((ctx, () =>
        {
            try   { result = Dispatch(method, path, ReadBody(ctx)); }
            catch (Exception e) { result = Error(e.Message); }
            done.Set();
            return result;
        }));

        // Wait up to 10 s for the main thread to process
        if (!done.Wait(10_000))
            Respond(ctx, 504, Error("Main thread timeout"));
        else
            Respond(ctx, 200, result ?? Error("No result"));
    }

    // ── Main-thread queue drain ────────────────────────────────────────────────

    static void DrainQueue()
    {
        while (_queue.TryDequeue(out var item))
        {
            var (ctx, handler) = item;
            var result = handler();
            Respond(ctx, 200, result);
        }
    }

    // ── Dispatch ──────────────────────────────────────────────────────────────

    static string Dispatch(string method, string path, string body)
    {
        if (method == "GET"  && path == "/hierarchy")       return GetHierarchy();
        if (method == "POST" && path == "/create")          return CreateObject(body);
        if (method == "POST" && path == "/add-component")   return AddComponent(body);
        if (method == "POST" && path == "/set-active")      return SetActive(body);
        if (method == "POST" && path == "/set-field")       return SetField(body);
        if (method == "POST" && path == "/set-reference")   return SetReference(body);
        if (method == "POST" && path == "/create-prefab-instance") return InstantiatePrefab(body);
        if (method == "POST" && path == "/play")            return EnterPlay();
        if (method == "POST" && path == "/stop")            return ExitPlay();
        if (method == "POST" && path == "/save-scene")      return SaveScene();
        if (method == "POST" && path == "/import-tmp")      return ImportTMP();
        return Error($"Unknown route: {method} {path}");
    }

    // ── Handlers ──────────────────────────────────────────────────────────────

    static string GetHierarchy()
    {
        var roots = SceneManager.GetActiveScene().GetRootGameObjects();
        var lines = new List<string>();
        foreach (var go in roots)
            AppendHierarchy(go.transform, 0, lines);
        var items = string.Join(",", lines.Select(l => $"\"{EscapeJson(l)}\""));
        return $"{{\"hierarchy\":[{items}]}}";
    }

    static void AppendHierarchy(Transform t, int depth, List<string> lines)
    {
        lines.Add(new string(' ', depth * 2) + t.gameObject.name + (t.gameObject.activeSelf ? "" : " [inactive]"));
        foreach (Transform child in t)
            AppendHierarchy(child, depth + 1, lines);
    }

    static string CreateObject(string body)
    {
        var d        = ParseJson(body);
        var name     = d.GetValueOrDefault("name", "NewObject");
        var parent   = d.GetValueOrDefault("parent", "");
        var tag      = d.GetValueOrDefault("tag", "");

        var go = new GameObject(name);
        if (!string.IsNullOrEmpty(tag)) go.tag = tag;

        if (!string.IsNullOrEmpty(parent))
        {
            var parentGo = GameObject.Find(parent);
            if (parentGo != null) go.transform.SetParent(parentGo.transform, false);
        }

        Undo.RegisterCreatedObjectUndo(go, $"Create {name}");
        EditorUtility.SetDirty(go);
        return Ok($"Created '{name}'");
    }

    static string AddComponent(string body)
    {
        var d         = ParseJson(body);
        var goName    = d.GetValueOrDefault("gameObject", "");
        var typeName  = d.GetValueOrDefault("type", "");

        var go = FindGO(goName);
        if (go == null) return Error($"GameObject '{goName}' not found");

        var type = FindType(typeName);
        if (type == null) return Error($"Type '{typeName}' not found");

        Undo.AddComponent(go, type);
        EditorUtility.SetDirty(go);
        return Ok($"Added {typeName} to '{goName}'");
    }

    static string SetActive(string body)
    {
        var d      = ParseJson(body);
        var goName = d.GetValueOrDefault("gameObject", "");
        var active = d.GetValueOrDefault("active", "true").ToLower() == "true";

        var go = FindGO(goName);
        if (go == null) return Error($"GameObject '{goName}' not found");

        Undo.RecordObject(go, $"Set active {active}");
        go.SetActive(active);
        EditorUtility.SetDirty(go);
        return Ok($"'{goName}' active={active}");
    }

    static string SetField(string body)
    {
        var d         = ParseJson(body);
        var goName    = d.GetValueOrDefault("gameObject", "");
        var compName  = d.GetValueOrDefault("component", "");
        var fieldName = d.GetValueOrDefault("field", "");
        var value     = d.GetValueOrDefault("value", "");

        var go   = FindGO(goName);
        if (go == null) return Error($"GameObject '{goName}' not found");

        var comp = FindComponent(go, compName);
        if (comp == null) return Error($"Component '{compName}' not found on '{goName}'");

        var so   = new SerializedObject(comp);
        var prop = so.FindProperty(fieldName);
        if (prop == null) return Error($"Field '{fieldName}' not found on {compName}");

        SetSerializedValue(prop, value);
        so.ApplyModifiedProperties();
        return Ok($"Set {compName}.{fieldName} = {value}");
    }

    static string SetReference(string body)
    {
        var d           = ParseJson(body);
        var goName      = d.GetValueOrDefault("gameObject", "");
        var compName    = d.GetValueOrDefault("component", "");
        var fieldName   = d.GetValueOrDefault("field", "");
        var targetName  = d.GetValueOrDefault("target", "");
        var targetComp  = d.GetValueOrDefault("targetComponent", "");

        var go   = FindGO(goName);
        if (go == null) return Error($"GameObject '{goName}' not found");

        var comp = FindComponent(go, compName);
        if (comp == null) return Error($"Component '{compName}' not found");

        var targetGo = FindGO(targetName);
        if (targetGo == null) return Error($"Target '{targetName}' not found");

        UnityEngine.Object targetObj = targetGo;
        if (!string.IsNullOrEmpty(targetComp))
        {
            var tc = FindComponent(targetGo, targetComp);
            if (tc == null) return Error($"Target component '{targetComp}' not found");
            targetObj = tc;
        }

        var so   = new SerializedObject(comp);
        var prop = so.FindProperty(fieldName);
        if (prop == null) return Error($"Field '{fieldName}' not found");

        prop.objectReferenceValue = targetObj;
        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(go);
        return Ok($"Wired {compName}.{fieldName} → {targetName}{(string.IsNullOrEmpty(targetComp) ? "" : "/" + targetComp)}");
    }

    static string InstantiatePrefab(string body)
    {
        var d          = ParseJson(body);
        var assetPath  = d.GetValueOrDefault("prefab", "");
        var parentName = d.GetValueOrDefault("parent", "");
        var name       = d.GetValueOrDefault("name", "");

        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
        if (prefab == null) return Error($"Prefab not found at '{assetPath}'");

        Transform parentT = null;
        if (!string.IsNullOrEmpty(parentName))
        {
            var parentGo = GameObject.Find(parentName);
            if (parentGo != null) parentT = parentGo.transform;
        }

        var instance = PrefabUtility.InstantiatePrefab(prefab, parentT) as GameObject;
        if (!string.IsNullOrEmpty(name)) instance.name = name;
        Undo.RegisterCreatedObjectUndo(instance, $"Instantiate {prefab.name}");
        return Ok($"Instantiated '{prefab.name}' as '{instance.name}'");
    }

    static string EnterPlay()
    {
        EditorApplication.EnterPlaymode();
        return Ok("Entering play mode");
    }

    static string ExitPlay()
    {
        EditorApplication.ExitPlaymode();
        return Ok("Exiting play mode");
    }

    static string SaveScene()
    {
        EditorSceneManager.SaveOpenScenes();
        return Ok("Scene saved");
    }

    static string ImportTMP()
    {
        string pkg = "Packages/com.unity.ugui/Package Resources/TMP Essential Resources.unitypackage";
        if (!System.IO.File.Exists(pkg))
        {
            // Try alternate path used in older ugui versions
            pkg = "Packages/com.unity.textmeshpro/Package Resources/TMP Essential Resources.unitypackage";
        }
        if (!System.IO.File.Exists(pkg))
            return Error($"TMP package not found at expected path");

        AssetDatabase.ImportPackage(pkg, false);
        AssetDatabase.Refresh();
        return Ok("TMP Essential Resources imported");
    }

    // ── Helpers ────────────────────────────────────────────────────────────────

    static GameObject FindGO(string name)
    {
        if (string.IsNullOrEmpty(name)) return null;
        // Active objects first (fast path)
        var go = GameObject.Find(name);
        if (go != null) return go;
        // Include inactive objects
        return Resources.FindObjectsOfTypeAll<GameObject>()
            .FirstOrDefault(g => g.name == name && g.scene.IsValid());
    }

    static Component FindComponent(GameObject go, string typeName)
    {
        if (string.IsNullOrEmpty(typeName)) return null;
        // Scan all components by name — avoids GetComponent(Type) Unity C++ check
        foreach (var c in go.GetComponents<Component>())
        {
            if (c == null) continue;
            var t = c.GetType();
            if (t.Name == typeName || t.FullName == typeName) return c;
        }
        return null;
    }

    static Type FindType(string name)
    {
        if (string.IsNullOrEmpty(name)) return null;

        // Explicit map for common Unity UI types — avoids System.Windows.Forms ambiguity
        switch (name)
        {
            case "Button":               return typeof(UnityEngine.UI.Button);
            case "Image":                return typeof(UnityEngine.UI.Image);
            case "Slider":               return typeof(UnityEngine.UI.Slider);
            case "ScrollRect":           return typeof(UnityEngine.UI.ScrollRect);
            case "VerticalLayoutGroup":  return typeof(UnityEngine.UI.VerticalLayoutGroup);
            case "HorizontalLayoutGroup":return typeof(UnityEngine.UI.HorizontalLayoutGroup);
            case "ContentSizeFitter":    return typeof(UnityEngine.UI.ContentSizeFitter);
            case "CanvasScaler":         return typeof(UnityEngine.UI.CanvasScaler);
            case "GraphicRaycaster":     return typeof(UnityEngine.UI.GraphicRaycaster);
            case "RectTransform":        return typeof(RectTransform);
            case "Transform":            return typeof(Transform);
            case "Canvas":               return typeof(Canvas);
            case "EventSystem":          return typeof(UnityEngine.EventSystems.EventSystem);
            case "StandaloneInputModule":return typeof(UnityEngine.EventSystems.StandaloneInputModule);
        }

        // Try fully qualified name
        foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
        {
            var t = asm.GetType(name);
            if (t != null) return t;
        }

        // Short name — prefer UnityEngine / TMPro namespaces
        Type fallback = null;
        foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
        {
            foreach (var t in asm.GetTypes())
            {
                if (t.Name != name) continue;
                if (t.Namespace != null && t.Namespace.StartsWith("UnityEngine")) return t;
                if (t.Namespace != null && t.Namespace.StartsWith("TMPro"))       return t;
                fallback ??= t;
            }
        }
        return fallback;
    }

    static void SetSerializedValue(SerializedProperty prop, string value)
    {
        switch (prop.propertyType)
        {
            case SerializedPropertyType.String:  prop.stringValue  = value; break;
            case SerializedPropertyType.Boolean: prop.boolValue    = bool.Parse(value); break;
            case SerializedPropertyType.Integer: prop.intValue     = int.Parse(value); break;
            case SerializedPropertyType.Float:   prop.floatValue   = float.Parse(value); break;
            case SerializedPropertyType.Enum:    prop.enumValueIndex = int.Parse(value); break;
        }
    }

    static void CaptureLog(string message, string stackTrace, LogType type)
    {
        _logBuffer.Add($"[{type}] {message}");
        if (_logBuffer.Count > 200) _logBuffer.RemoveAt(0);
    }

    // ── JSON mini-parser (no external deps) ───────────────────────────────────

    static Dictionary<string, string> ParseJson(string json)
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (string.IsNullOrWhiteSpace(json)) return result;
        json = json.Trim().Trim('{', '}');
        foreach (var pair in json.Split(','))
        {
            var kv = pair.Split(new[] { ':' }, 2);
            if (kv.Length != 2) continue;
            var key = kv[0].Trim().Trim('"');
            var val = kv[1].Trim().Trim('"');
            result[key] = val;
        }
        return result;
    }

    static string ReadBody(HttpListenerContext ctx)
    {
        using var sr = new StreamReader(ctx.Request.InputStream, ctx.Request.ContentEncoding);
        return sr.ReadToEnd();
    }

    static void Respond(HttpListenerContext ctx, int code, string json)
    {
        try
        {
            var bytes = Encoding.UTF8.GetBytes(json);
            ctx.Response.StatusCode        = code;
            ctx.Response.ContentType       = "application/json";
            ctx.Response.ContentLength64   = bytes.Length;
            ctx.Response.OutputStream.Write(bytes, 0, bytes.Length);
            ctx.Response.OutputStream.Close();
        }
        catch { }
    }

    static string Ok(string msg)    => $"{{\"ok\":true,\"msg\":\"{EscapeJson(msg)}\"}}";
    static string Error(string msg) => $"{{\"ok\":false,\"error\":\"{EscapeJson(msg)}\"}}";

    static string EscapeJson(string s) =>
        s.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n").Replace("\r", "");
}
