# Clase07 Demo UI → Inspector Migration Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Replace the code-built UI (`DemoUiFactory`) of the 10 clase07 demo scenes with real Canvas/TextMeshPro hierarchies wired in the Inspector, split into 12 fully isolated scenes (FSM's weapon comparison becomes 3 separate scenes instead of 1 combined one).

**Architecture:** A live Unity Editor (6000.3.21f1) is driven via the `unity-cli` Pipeline connection for every scene/prefab edit — no hand-edited `.unity`/`.prefab` YAML. Each demo `MonoBehaviour` stops building UI in `Awake()`/`Start()` and instead exposes `[SerializeField]` UI references plus public `On*Clicked()` methods that `Button.onClick` invokes as **persistent** listeners (the same serialized result as a human wiring it in the Inspector). Domain logic (FSM/DI/MessageBroker/MVx) is untouched.

**Tech Stack:** Unity 6000.3.21f1, uGUI (`UnityEngine.UI`) + TextMeshPro, VContainer/MessagePipe (existing), `unity-cli` (`unity command eval_file` / `create_scene` / `delete_asset` / `add_scene_to_build` / `recompile` / `editor_play` / `run_tests`) driving the project's `com.unity.pipeline` package.

**Spec:** `docs/superpowers/specs/2026-09-21-clase07-demo-ui-inspector-migration-design.md`

## Global Constraints

- Project root for all Unity paths: `clases/clase07/Unity` (run `unity` CLI commands from there, or pass `--project-path` if ambiguous).
- **Correction from Task 0 (empirically verified against the installed CLI, version 1.0.0-beta.8) — supersedes anything below that says otherwise:**
  - **Never pass `--caller`/`--skill`** to any `unity command ...` invocation in this plan — this CLI build rejects both on every command with `INVALID_COMMAND_ARGS` (exit 2). Every `--caller plugin --skill subagent-driven-development` shown in the task text below must be dropped.
  - **`eval`/`eval_file` C# code cannot use top-level `using` directives** — the CLI compiles the supplied code as statements inside a method body, not as a full compilation unit, so a leading `using X;` is parsed as an invalid using-*statement*. Every `eval_file` script shown in the task text below must have its `using ...;` lines deleted and every short type name it uses fully qualified instead (e.g. `TMPro.TextMeshProUGUI` not `TextMeshProUGUI`, `UnityEngine.UI.Button`/`.Image`/`.VerticalLayoutGroup`/`.HorizontalLayoutGroup`/`.ContentSizeFitter`/`.LayoutElement`/`.RectMask2D`/`.CanvasScaler`/`.GraphicRaycaster`, `UnityEngine.EventSystems.EventSystem`/`.StandaloneInputModule`, `UnityEditor.SerializedObject`, `UnityEditor.Events.UnityEventTools`, `UnityEditor.SceneManagement.EditorSceneManager`, `UnityEditor.AssetDatabase`, `UnityEditor.PrefabUtility`, `UnityEngine.SceneManagement.SceneManager`, `UnityEngine.Events.UnityAction`, and the full namespaced project type for whichever demo component the script attaches, e.g. `Clase07.FSM.Demo.FsmWeaponBaselineDemoView`). `Vector2`/`Color`/`Color32`/`TextAnchor`/`RenderMode`/`LoadSceneMode` etc. also need their `UnityEngine.` prefix. This does **not** change any value, hierarchy, field name, or method name below — only the C# syntax wrapper around them.
  - A command's sole required parameter accepts either a named flag (e.g. `--file "..."`) or a trailing positional argument — both work; prefer whichever the task text already shows.
  - `TMPro.TMP_PackageResourceImporter.ImportResources`'s real third parameter is `interactive`, not `importAudio` (moot after Task 0, noted for completeness).
  - Every implementer subagent for Tasks 1-17 must read `.superpowers/sdd/2026-09-21-clase07-demo-ui-inspector-migration/task-0-report.md` before running its first `unity command`, and apply these corrections rather than the literal `--caller`/`--skill`/`using` syntax shown in its own task's brief text.
- **Additional corrections from Task 1 (empirically verified) — also supersede the brief text below:**
  - `add_scene_to_build` mutates the Editor's in-memory Build Settings but does **not** flush `ProjectSettings/EditorBuildSettings.asset` to disk by itself. Always run `unity command save_all --format json` immediately after, and confirm the on-disk file changed before staging/committing it.
  - A Play-mode functional check that clicks a button and reads a label in the **same** `eval_file` call sees stale text whenever the label is only refreshed inside a `MonoBehaviour.Update()` — the click's state change happens, but that same call returns before the next `Update()` tick runs. Split every such check into two `eval_file` calls (one that clicks and logs, one that reads and logs) with a real-world `sleep 1` between them so the Editor's frame loop advances.
  - `get_scene_hierarchy --format json` is a real command that returns the actual GameObject/component tree of the active scene — use it right after a scene-build `eval_file` call to verify the produced hierarchy structurally, instead of trusting the build script's own "no error" result alone.
  - `recompile_status`'s JSON `result` field is itself a JSON-encoded string, not a nested object — parse it as a string before reading `.status`/`.failed` from it.
- **Additional corrections from Task 4 (empirically verified) — supersede the brief text below wherever it mentions `run_tests`:**
  - `run_tests --filter_type namespace` is rejected outright (`Invalid filterType 'namespace'. Valid options: testName, assembly, category`). Use `--filter_type assembly` with the exact assembly name from the relevant `.asmdef`'s `"name"` field (e.g. `Clase07.FSM.Tests`, `Clase07.DI.Tests`, `Clase07.MessageBroker.Tests`, `Clase07.Mvx.Tests` for EditMode; `Clase07.FSM.Tests.PlayMode` for the one PlayMode assembly).
  - A synchronous `run_tests --mode PlayMode` call fails outright over this CLI's HTTP transport, because entering Play mode triggers a domain reload that drops the request. Always pass `--async_tests true` for any PlayMode run, then poll `unity command test_status --format json` until `status` is no longer `"running"`, then read the pass/fail counts from that final `test_status` result.
- **Additional correction from Task 10 (empirically verified) — supersedes the brief text below wherever it matters:**
  - In `eval`/`eval_file` code, only the bare `UnityEngine` namespace is implicitly available — `UnityEngine.UI` and `TMPro` types must always be fully qualified (`UnityEngine.UI.Button`/`.Image`/`.HorizontalLayoutGroup`/`.LayoutElement` etc., `TMPro.TextMeshProUGUI`/`.TextAlignmentOptions`), same as every `UnityEditor.*` type. This refines (does not contradict) Task 0's "no top-level using" finding.
- **Additional correction from Task 11 (empirically verified) — supersedes any later task's brief text that says `find_assets --type <ComponentTypeName>` to locate a prefab:** `find_assets --type` filters on an asset's AssetDatabase *main* type, which for a prefab is always `GameObject`, never a component type living on it — the check returns 0 results even when the prefab and its wiring are correct. To verify a prefab exists and is wired correctly, use `find_assets --name "<PrefabName>"` (matches by filename) plus direct inspection of the prefab's YAML (or `get_scene_hierarchy`-equivalent for the loaded asset) — not `--type` with a component name.
- **Never hand-edit `.unity`, `.prefab`, or `.asset` files directly** — every scene/prefab change goes through the connected Editor's commands (`eval_file`, `create_scene`, `delete_asset`, etc.).
- All-new/edited demo text uses **TextMeshPro** (`TMPro.TextMeshProUGUI` for labels, `TMPro.TMP_InputField` for input) — never legacy `UnityEngine.UI.Text`/`InputField`.
- Canvas: `RenderMode.ScreenSpaceOverlay`; `CanvasScaler.ScaleMode.ScaleWithScreenSize`; reference resolution `1280x720`; `matchWidthOrHeight = 0.5f`.
- Color palette (used verbatim in every scene): background panel `new Color32(0x1C,0x1E,0x26,0xFF)`, title/body text `new Color32(0xF2,0xF3,0xF5,0xFF)`, primary button `new Color32(0x33,0x88,0xE6,0xFF)`, destructive/remove button `new Color32(0xC0,0x39,0x2B,0xFF)`, button label text `Color.white`.
- Every new/edited `[SerializeField]` is wired via `UnityEditor.SerializedObject` + `FindProperty("_fieldName").objectReferenceValue = ...; ApplyModifiedPropertiesWithoutUndo();` — this is the scripted equivalent of dragging a reference into the Inspector.
- Every `Button.onClick` wired for a demo action uses `UnityEditor.Events.UnityEventTools.AddPersistentListener(button.onClick, new UnityEngine.Events.UnityAction(component.MethodName));` — never a runtime `AddListener` for anything that could be persistent.
- `eval`/`eval_file` code is **top-level C# statements** (Roslyn scripting), not a wrapped class/method — write scripts exactly in that style.
- Scene file paths below are given as full `Assets/...` paths. Before the first `create_scene`/`create_script` call, Task 0 confirms whether the connected Editor's authoring root expects the `Assets/` prefix or a root-relative path — adjust every path in this plan the same way if it differs.

---

## Task 0: Confirm live Editor connection, path conventions, and import TMP Essentials

**Files:** none (infrastructure only).

**Interfaces:**
- Produces: confirmed working `unity command` invocation syntax and path convention used by every later task.

- [ ] **Step 1: Confirm the Editor is connected**

```bash
cd clases/clase07/Unity
unity status --format json
```

Expected: `success: true`, one instance with `"state": "ready"`. If `count: 0`, tell the user their Unity Editor for this project needs to be open and focused (a compile or import in progress can also hide it — check the Editor window), then retry.

- [ ] **Step 2: Confirm authoring root and CLI flag syntax**

```bash
unity command get_authoring_root --caller plugin --skill subagent-driven-development --format json
```

Note the returned root. If it is `Assets` (the expected default), every `path`/`asset` argument in this plan should be passed as the full `Assets/...` string shown in each task. If the CLI rejects a `--path`/`--code`/`--file`/`--asset` flag for a command's sole required parameter, retry passing that value as a trailing positional argument instead (e.g. `unity command create_scene --caller plugin --skill subagent-driven-development "Assets/01_FSM/01_FSM_WeaponBaseline.unity"`) — both forms are validated CLI conventions per the `unity-cli` skill's own example (`unity command eval --caller plugin --skill unity-cli 'code'`).

- [ ] **Step 3: Import TextMeshPro Essential Resources (idempotent)**

Write this to a scratch file (your scratchpad directory), e.g. `import_tmp_essentials.cs`:

```csharp
using TMPro;

var alreadyPresent = UnityEditor.AssetDatabase.IsValidFolder("Assets/TextMesh Pro");
if (!alreadyPresent)
{
    TMP_PackageResourceImporter.ImportResources(importEssentials: true, importExamples: false, importAudio: false);
}

UnityEngine.Debug.Log("TMP_ESSENTIALS_OK: alreadyPresent=" + alreadyPresent);
```

Run it:

```bash
unity command eval_file --caller plugin --skill subagent-driven-development --file "/absolute/path/to/import_tmp_essentials.cs" --format json
```

- [ ] **Step 4: Verify the import**

```bash
unity command console --caller plugin --skill subagent-driven-development --tail 20 --level log --format json
```

Expected: a `TMP_ESSENTIALS_OK: ...` line, no errors. If `alreadyPresent=false` was logged, a domain reload may follow — poll `unity command recompile_status --caller plugin --skill subagent-driven-development --format json` until it reports `completed` or `up_to_date` before moving to Task 1.

No commit for this task (no tracked files changed, unless the TMP import added `Assets/TextMesh Pro/...` — if it did, stage and note it in the Task 1 commit instead of committing it alone).

---

## Task 1: FSM — Weapon Baseline (new script + new scene)

**Files:**
- Create: `Unity/Assets/01_FSM/Demo/FsmWeaponBaselineDemoView.cs`
- Create (via live Editor): `Unity/Assets/01_FSM/01_FSM_WeaponBaseline.unity`

**Interfaces:**
- Consumes: `Clase07.FSM.Small.Baseline.WeaponBaseline` — `PressTrigger()`, `Tick(float)`, `State` (`WeaponState`), `AmmoInMagazine` (`int`). Already exists, unchanged.
- Produces: `Clase07.FSM.Demo.FsmWeaponBaselineDemoView : MonoBehaviour` with `public void OnFireClicked()` and `[SerializeField] private TMPro.TMP_Text _stateLabel`.

- [ ] **Step 1: Write the new view script**

```csharp
using UnityEngine;
using TMPro;
using Clase07.FSM.Small.Baseline;

namespace Clase07.FSM.Demo
{
    public class FsmWeaponBaselineDemoView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _stateLabel;

        private WeaponBaseline _weapon;

        private void Awake() => _weapon = new WeaponBaseline();

        public void OnFireClicked() => _weapon.PressTrigger();

        private void Update()
        {
            _weapon.Tick(Time.deltaTime);
            _stateLabel.text = $"[Baseline] {_weapon.State} (ammo: {_weapon.AmmoInMagazine})";
        }
    }
}
```

Save this as `Unity/Assets/01_FSM/Demo/FsmWeaponBaselineDemoView.cs` using your file-editing tool directly (this is a plain `.cs` text file, not a scene/prefab/asset — safe to write normally even with a live Editor connected).

- [ ] **Step 2: Add the TextMeshPro assembly reference to the FSM asmdef**

Edit `Unity/Assets/01_FSM/Clase07.FSM.asmdef`, adding `"Unity.TextMeshPro"` to `"references"`:

```json
{
    "name": "Clase07.FSM",
    "rootNamespace": "",
    "references": [
        "Clase07.Shared",
        "UnityEngine.UI",
        "Unity.TextMeshPro"
    ],
    "includePlatforms": [],
    "excludePlatforms": [],
    "allowUnsafeCode": false,
    "overrideReferences": false,
    "precompiledReferences": [],
    "autoReferenced": true,
    "defineConstraints": [],
    "versionDefines": [],
    "noEngineReferences": false
}
```

- [ ] **Step 3: Recompile and confirm success**

```bash
unity command recompile --caller plugin --skill subagent-driven-development --format json
```

Poll until done:

```bash
unity command recompile_status --caller plugin --skill subagent-driven-development --format json
```

Expected: eventually `"status": "completed"` (or `"up_to_date"`), and:

```bash
unity command console_status --caller plugin --skill subagent-driven-development --format json
```

Expected: no compile-failure flag set. If it is set, run `unity command console --caller plugin --skill subagent-driven-development --tail 50 --level error --format json`, fix the reported error in the file from Step 1, and repeat Step 3.

- [ ] **Step 4: Build the scene**

Write this to your scratchpad as `build_fsm_weapon_baseline.cs`:

```csharp
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using Clase07.FSM.Demo;

var canvasGo = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
var canvas = canvasGo.GetComponent<Canvas>();
canvas.renderMode = RenderMode.ScreenSpaceOverlay;
var scaler = canvasGo.GetComponent<CanvasScaler>();
scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
scaler.referenceResolution = new Vector2(1280, 720);
scaler.matchWidthOrHeight = 0.5f;

new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));

var panelGo = new GameObject("Panel", typeof(Image));
panelGo.transform.SetParent(canvasGo.transform, false);
var panelRect = panelGo.GetComponent<RectTransform>();
panelRect.anchorMin = Vector2.zero;
panelRect.anchorMax = Vector2.one;
panelRect.offsetMin = Vector2.zero;
panelRect.offsetMax = Vector2.zero;
panelGo.GetComponent<Image>().color = new Color32(0x1C, 0x1E, 0x26, 0xFF);

var contentGo = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
contentGo.transform.SetParent(panelGo.transform, false);
var contentRect = contentGo.GetComponent<RectTransform>();
contentRect.anchorMin = new Vector2(0.5f, 0.5f);
contentRect.anchorMax = new Vector2(0.5f, 0.5f);
contentRect.pivot = new Vector2(0.5f, 0.5f);
contentRect.sizeDelta = new Vector2(420, 0);
var vlg = contentGo.GetComponent<VerticalLayoutGroup>();
vlg.childAlignment = TextAnchor.MiddleCenter;
vlg.spacing = 20f;
vlg.childControlWidth = true;
vlg.childControlHeight = true;
vlg.childForceExpandWidth = true;
vlg.childForceExpandHeight = false;
var fitter = contentGo.GetComponent<ContentSizeFitter>();
fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

var titleGo = new GameObject("Title", typeof(TextMeshProUGUI), typeof(LayoutElement));
titleGo.transform.SetParent(contentGo.transform, false);
var title = titleGo.GetComponent<TextMeshProUGUI>();
title.text = "FSM — Weapon Baseline";
title.fontSize = 32;
title.alignment = TextAlignmentOptions.Center;
title.color = new Color32(0xF2, 0xF3, 0xF5, 0xFF);
titleGo.GetComponent<LayoutElement>().preferredHeight = 44;

var buttonGo = new GameObject("FireButton", typeof(Image), typeof(Button), typeof(LayoutElement));
buttonGo.transform.SetParent(contentGo.transform, false);
buttonGo.GetComponent<Image>().color = new Color32(0x33, 0x88, 0xE6, 0xFF);
var buttonLE = buttonGo.GetComponent<LayoutElement>();
buttonLE.preferredHeight = 48;
buttonLE.preferredWidth = 320;
var buttonTextGo = new GameObject("Label", typeof(TextMeshProUGUI));
buttonTextGo.transform.SetParent(buttonGo.transform, false);
var buttonText = buttonTextGo.GetComponent<TextMeshProUGUI>();
buttonText.text = "Fire (Baseline)";
buttonText.alignment = TextAlignmentOptions.Center;
buttonText.color = Color.white;
buttonText.fontSize = 22;
var btRect = buttonTextGo.GetComponent<RectTransform>();
btRect.anchorMin = Vector2.zero;
btRect.anchorMax = Vector2.one;
btRect.offsetMin = Vector2.zero;
btRect.offsetMax = Vector2.zero;

var stateGo = new GameObject("StateLabel", typeof(TextMeshProUGUI), typeof(LayoutElement));
stateGo.transform.SetParent(contentGo.transform, false);
var stateLabel = stateGo.GetComponent<TextMeshProUGUI>();
stateLabel.alignment = TextAlignmentOptions.Center;
stateLabel.color = new Color32(0xF2, 0xF3, 0xF5, 0xFF);
stateLabel.fontSize = 24;
stateGo.GetComponent<LayoutElement>().preferredHeight = 40;

var viewGo = new GameObject("WeaponBaselineDemo", typeof(FsmWeaponBaselineDemoView));
var view = viewGo.GetComponent<FsmWeaponBaselineDemoView>();

var so = new SerializedObject(view);
so.FindProperty("_stateLabel").objectReferenceValue = stateLabel;
so.ApplyModifiedPropertiesWithoutUndo();

UnityEventTools.AddPersistentListener(buttonGo.GetComponent<Button>().onClick, new UnityEngine.Events.UnityAction(view.OnFireClicked));

var scene = SceneManager.GetActiveScene();
EditorSceneManager.MarkSceneDirty(scene);
EditorSceneManager.SaveScene(scene);

Debug.Log("SCENE_BUILD_OK: 01_FSM_WeaponBaseline");
```

First create the scene file, then run the build script against it:

```bash
unity command create_scene --caller plugin --skill subagent-driven-development --path "Assets/01_FSM/01_FSM_WeaponBaseline.unity" --format json
unity command eval_file --caller plugin --skill subagent-driven-development --file "/absolute/path/to/build_fsm_weapon_baseline.cs" --format json
```

- [ ] **Step 5: Verify it compiled and ran cleanly**

```bash
unity command console --caller plugin --skill subagent-driven-development --tail 20 --level log --format json
```

Expected: a `SCENE_BUILD_OK: 01_FSM_WeaponBaseline` line and no errors since Step 4 started.

- [ ] **Step 6: Functional check in Play mode**

Write to scratchpad as `verify_fsm_weapon_baseline.cs`:

```csharp
using UnityEngine;
using UnityEngine.UI;
using TMPro;

var button = GameObject.Find("FireButton").GetComponent<Button>();
button.onClick.Invoke();
var label = GameObject.Find("StateLabel").GetComponent<TextMeshProUGUI>();
Debug.Log("VERIFY_LABEL: " + label.text);
```

```bash
unity command editor_play --caller plugin --skill subagent-driven-development --format json
unity command editor_status --caller plugin --skill subagent-driven-development --format json   # poll until isPlaying = true
unity command eval_file --caller plugin --skill subagent-driven-development --file "/absolute/path/to/verify_fsm_weapon_baseline.cs" --format json
unity command console --caller plugin --skill subagent-driven-development --tail 5 --level log --format json
unity command editor_stop --caller plugin --skill subagent-driven-development --format json
```

Expected: `VERIFY_LABEL: [Baseline] Firing (ammo: 5)` (or `Idle`/other ammo count depending on frame timing — any non-empty `[Baseline] ...` string with `ammo: 5` after one fire confirms wiring works; the exact state string is timing-dependent and not the point of this check).

- [ ] **Step 7: Register the scene in Build Settings**

```bash
unity command add_scene_to_build --caller plugin --skill subagent-driven-development --path "Assets/01_FSM/01_FSM_WeaponBaseline.unity" --enabled true --format json
```

- [ ] **Step 8: Commit**

```bash
cd clases/clase07/Unity
git add Assets/01_FSM/Demo/FsmWeaponBaselineDemoView.cs Assets/01_FSM/Demo/FsmWeaponBaselineDemoView.cs.meta \
        Assets/01_FSM/01_FSM_WeaponBaseline.unity Assets/01_FSM/01_FSM_WeaponBaseline.unity.meta \
        Assets/01_FSM/Clase07.FSM.asmdef ProjectSettings/EditorBuildSettings.asset
git commit -m "clase07/FSM: escena propia para el arma Baseline, UI armada en el Inspector"
```

---

## Task 2: FSM — Weapon State Pattern (new script + new scene)

**Files:**
- Create: `Unity/Assets/01_FSM/Demo/FsmWeaponStatePatternDemoView.cs`
- Create (via live Editor): `Unity/Assets/01_FSM/02_FSM_WeaponStatePattern.unity`

**Interfaces:**
- Consumes: `Clase07.FSM.Small.StatePattern.WeaponStatePatternController` — `PressTrigger()`, `Tick(float)`, `Context.AmmoInMagazine` (`int`). Already exists, unchanged.
- Produces: `Clase07.FSM.Demo.FsmWeaponStatePatternDemoView : MonoBehaviour` with `public void OnFireClicked()` and `[SerializeField] private TMPro.TMP_Text _stateLabel`.

- [ ] **Step 1: Write the new view script**

```csharp
using UnityEngine;
using TMPro;
using Clase07.FSM.Small.StatePattern;

namespace Clase07.FSM.Demo
{
    public class FsmWeaponStatePatternDemoView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _stateLabel;

        private WeaponStatePatternController _weapon;

        private void Awake() => _weapon = new WeaponStatePatternController();

        public void OnFireClicked() => _weapon.PressTrigger();

        private void Update()
        {
            _weapon.Tick(Time.deltaTime);
            _stateLabel.text = $"[StatePattern] ammo: {_weapon.Context.AmmoInMagazine}";
        }
    }
}
```

Save this as `Unity/Assets/01_FSM/Demo/FsmWeaponStatePatternDemoView.cs`.

- [ ] **Step 2: Recompile and confirm success**

The FSM asmdef already gained `Unity.TextMeshPro` in Task 1. Just recompile:

```bash
unity command recompile --caller plugin --skill subagent-driven-development --format json
unity command recompile_status --caller plugin --skill subagent-driven-development --format json   # poll until completed/up_to_date
unity command console_status --caller plugin --skill subagent-driven-development --format json      # confirm no compile-failure flag
```

If a compile error is reported, fix it in Step 1's file and repeat.

- [ ] **Step 3: Build the scene**

Write to scratchpad as `build_fsm_weapon_statepattern.cs`:

```csharp
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using Clase07.FSM.Demo;

var canvasGo = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
var canvas = canvasGo.GetComponent<Canvas>();
canvas.renderMode = RenderMode.ScreenSpaceOverlay;
var scaler = canvasGo.GetComponent<CanvasScaler>();
scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
scaler.referenceResolution = new Vector2(1280, 720);
scaler.matchWidthOrHeight = 0.5f;

new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));

var panelGo = new GameObject("Panel", typeof(Image));
panelGo.transform.SetParent(canvasGo.transform, false);
var panelRect = panelGo.GetComponent<RectTransform>();
panelRect.anchorMin = Vector2.zero;
panelRect.anchorMax = Vector2.one;
panelRect.offsetMin = Vector2.zero;
panelRect.offsetMax = Vector2.zero;
panelGo.GetComponent<Image>().color = new Color32(0x1C, 0x1E, 0x26, 0xFF);

var contentGo = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
contentGo.transform.SetParent(panelGo.transform, false);
var contentRect = contentGo.GetComponent<RectTransform>();
contentRect.anchorMin = new Vector2(0.5f, 0.5f);
contentRect.anchorMax = new Vector2(0.5f, 0.5f);
contentRect.pivot = new Vector2(0.5f, 0.5f);
contentRect.sizeDelta = new Vector2(420, 0);
var vlg = contentGo.GetComponent<VerticalLayoutGroup>();
vlg.childAlignment = TextAnchor.MiddleCenter;
vlg.spacing = 20f;
vlg.childControlWidth = true;
vlg.childControlHeight = true;
vlg.childForceExpandWidth = true;
vlg.childForceExpandHeight = false;
var fitter = contentGo.GetComponent<ContentSizeFitter>();
fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

var titleGo = new GameObject("Title", typeof(TextMeshProUGUI), typeof(LayoutElement));
titleGo.transform.SetParent(contentGo.transform, false);
var title = titleGo.GetComponent<TextMeshProUGUI>();
title.text = "FSM — Weapon State Pattern";
title.fontSize = 32;
title.alignment = TextAlignmentOptions.Center;
title.color = new Color32(0xF2, 0xF3, 0xF5, 0xFF);
titleGo.GetComponent<LayoutElement>().preferredHeight = 44;

var buttonGo = new GameObject("FireButton", typeof(Image), typeof(Button), typeof(LayoutElement));
buttonGo.transform.SetParent(contentGo.transform, false);
buttonGo.GetComponent<Image>().color = new Color32(0x33, 0x88, 0xE6, 0xFF);
var buttonLE = buttonGo.GetComponent<LayoutElement>();
buttonLE.preferredHeight = 48;
buttonLE.preferredWidth = 320;
var buttonTextGo = new GameObject("Label", typeof(TextMeshProUGUI));
buttonTextGo.transform.SetParent(buttonGo.transform, false);
var buttonText = buttonTextGo.GetComponent<TextMeshProUGUI>();
buttonText.text = "Fire (State Pattern)";
buttonText.alignment = TextAlignmentOptions.Center;
buttonText.color = Color.white;
buttonText.fontSize = 22;
var btRect = buttonTextGo.GetComponent<RectTransform>();
btRect.anchorMin = Vector2.zero;
btRect.anchorMax = Vector2.one;
btRect.offsetMin = Vector2.zero;
btRect.offsetMax = Vector2.zero;

var stateGo = new GameObject("StateLabel", typeof(TextMeshProUGUI), typeof(LayoutElement));
stateGo.transform.SetParent(contentGo.transform, false);
var stateLabel = stateGo.GetComponent<TextMeshProUGUI>();
stateLabel.alignment = TextAlignmentOptions.Center;
stateLabel.color = new Color32(0xF2, 0xF3, 0xF5, 0xFF);
stateLabel.fontSize = 24;
stateGo.GetComponent<LayoutElement>().preferredHeight = 40;

var viewGo = new GameObject("WeaponStatePatternDemo", typeof(FsmWeaponStatePatternDemoView));
var view = viewGo.GetComponent<FsmWeaponStatePatternDemoView>();

var so = new SerializedObject(view);
so.FindProperty("_stateLabel").objectReferenceValue = stateLabel;
so.ApplyModifiedPropertiesWithoutUndo();

UnityEventTools.AddPersistentListener(buttonGo.GetComponent<Button>().onClick, new UnityEngine.Events.UnityAction(view.OnFireClicked));

var scene = SceneManager.GetActiveScene();
EditorSceneManager.MarkSceneDirty(scene);
EditorSceneManager.SaveScene(scene);

Debug.Log("SCENE_BUILD_OK: 02_FSM_WeaponStatePattern");
```

```bash
unity command create_scene --caller plugin --skill subagent-driven-development --path "Assets/01_FSM/02_FSM_WeaponStatePattern.unity" --format json
unity command eval_file --caller plugin --skill subagent-driven-development --file "/absolute/path/to/build_fsm_weapon_statepattern.cs" --format json
```

- [ ] **Step 4: Verify it compiled and ran cleanly**

```bash
unity command console --caller plugin --skill subagent-driven-development --tail 20 --level log --format json
```

Expected: `SCENE_BUILD_OK: 02_FSM_WeaponStatePattern`, no errors.

- [ ] **Step 5: Functional check in Play mode**

Write to scratchpad as `verify_fsm_weapon_statepattern.cs`:

```csharp
using UnityEngine;
using UnityEngine.UI;
using TMPro;

var button = GameObject.Find("FireButton").GetComponent<Button>();
button.onClick.Invoke();
var label = GameObject.Find("StateLabel").GetComponent<TextMeshProUGUI>();
Debug.Log("VERIFY_LABEL: " + label.text);
```

```bash
unity command editor_play --caller plugin --skill subagent-driven-development --format json
unity command editor_status --caller plugin --skill subagent-driven-development --format json   # poll until isPlaying = true
unity command eval_file --caller plugin --skill subagent-driven-development --file "/absolute/path/to/verify_fsm_weapon_statepattern.cs" --format json
unity command console --caller plugin --skill subagent-driven-development --tail 5 --level log --format json
unity command editor_stop --caller plugin --skill subagent-driven-development --format json
```

Expected: `VERIFY_LABEL: [StatePattern] ammo: 5`.

- [ ] **Step 6: Register the scene in Build Settings**

```bash
unity command add_scene_to_build --caller plugin --skill subagent-driven-development --path "Assets/01_FSM/02_FSM_WeaponStatePattern.unity" --enabled true --format json
```

- [ ] **Step 7: Commit**

```bash
cd clases/clase07/Unity
git add Assets/01_FSM/Demo/FsmWeaponStatePatternDemoView.cs Assets/01_FSM/Demo/FsmWeaponStatePatternDemoView.cs.meta \
        Assets/01_FSM/02_FSM_WeaponStatePattern.unity Assets/01_FSM/02_FSM_WeaponStatePattern.unity.meta \
        ProjectSettings/EditorBuildSettings.asset
git commit -m "clase07/FSM: escena propia para el arma State Pattern, UI armada en el Inspector"
```

---

## Task 3: FSM — Game Flow (refactor existing script + new scene)

**Files:**
- Modify: `Unity/Assets/01_FSM/Demo/FsmGameFlowDemoView.cs` (full rewrite of the class body)
- Create (via live Editor): `Unity/Assets/01_FSM/03_FSM_GameFlow.unity`

**Interfaces:**
- Consumes: `Clase07.FSM.Large.StatePattern.GameFlowController` — `FinishLoading()`, `Play()`, `Pause()`, `OpenSettings()`, `CloseSettings()`, `Resume()`, `CurrentState.Name` (`string`), `CurrentSubstateName` (`string`). Already exists, unchanged.
- Produces: `Clase07.FSM.Demo.FsmGameFlowDemoView : MonoBehaviour` with six public no-arg methods (`OnFinishLoadingClicked`, `OnPlayClicked`, `OnPauseClicked`, `OnOpenSettingsClicked`, `OnCloseSettingsClicked`, `OnResumeClicked`) and `[SerializeField] private TMPro.TMP_Text _stateLabel`.

- [ ] **Step 1: Rewrite the view script**

Replace the full contents of `Unity/Assets/01_FSM/Demo/FsmGameFlowDemoView.cs` with:

```csharp
using UnityEngine;
using TMPro;
using Clase07.FSM.Large.StatePattern;

namespace Clase07.FSM.Demo
{
    public class FsmGameFlowDemoView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _stateLabel;

        private GameFlowController _flow;

        private void Awake()
        {
            _flow = new GameFlowController();
            Refresh();
        }

        public void OnFinishLoadingClicked() { _flow.FinishLoading(); Refresh(); }
        public void OnPlayClicked() { _flow.Play(); Refresh(); }
        public void OnPauseClicked() { _flow.Pause(); Refresh(); }
        public void OnOpenSettingsClicked() { _flow.OpenSettings(); Refresh(); }
        public void OnCloseSettingsClicked() { _flow.CloseSettings(); Refresh(); }
        public void OnResumeClicked() { _flow.Resume(); Refresh(); }

        private void Refresh()
        {
            _stateLabel.text = $"State: {_flow.CurrentState.Name} | Substate: {_flow.CurrentSubstateName}";
        }
    }
}
```

- [ ] **Step 2: Recompile and confirm success**

```bash
unity command recompile --caller plugin --skill subagent-driven-development --format json
unity command recompile_status --caller plugin --skill subagent-driven-development --format json   # poll until completed/up_to_date
unity command console_status --caller plugin --skill subagent-driven-development --format json      # confirm no compile-failure flag
```

Note: the old `01_FSM_Demo.unity` scene still references this same class (its `GameFlowDemo` GameObject) — the new fields/methods are a superset-compatible rewrite of the same public surface used the same way, so that old scene keeps working until Task 4 deletes it. No stale references are introduced by this step.

- [ ] **Step 3: Build the scene**

Write to scratchpad as `build_fsm_gameflow.cs`:

```csharp
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using Clase07.FSM.Demo;

var canvasGo = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
var canvas = canvasGo.GetComponent<Canvas>();
canvas.renderMode = RenderMode.ScreenSpaceOverlay;
var scaler = canvasGo.GetComponent<CanvasScaler>();
scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
scaler.referenceResolution = new Vector2(1280, 720);
scaler.matchWidthOrHeight = 0.5f;

new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));

var panelGo = new GameObject("Panel", typeof(Image));
panelGo.transform.SetParent(canvasGo.transform, false);
var panelRect = panelGo.GetComponent<RectTransform>();
panelRect.anchorMin = Vector2.zero;
panelRect.anchorMax = Vector2.one;
panelRect.offsetMin = Vector2.zero;
panelRect.offsetMax = Vector2.zero;
panelGo.GetComponent<Image>().color = new Color32(0x1C, 0x1E, 0x26, 0xFF);

var contentGo = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
contentGo.transform.SetParent(panelGo.transform, false);
var contentRect = contentGo.GetComponent<RectTransform>();
contentRect.anchorMin = new Vector2(0.5f, 0.5f);
contentRect.anchorMax = new Vector2(0.5f, 0.5f);
contentRect.pivot = new Vector2(0.5f, 0.5f);
contentRect.sizeDelta = new Vector2(380, 0);
var vlg = contentGo.GetComponent<VerticalLayoutGroup>();
vlg.childAlignment = TextAnchor.MiddleCenter;
vlg.spacing = 12f;
vlg.childControlWidth = true;
vlg.childControlHeight = true;
vlg.childForceExpandWidth = true;
vlg.childForceExpandHeight = false;
var fitter = contentGo.GetComponent<ContentSizeFitter>();
fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

var titleGo = new GameObject("Title", typeof(TextMeshProUGUI), typeof(LayoutElement));
titleGo.transform.SetParent(contentGo.transform, false);
var title = titleGo.GetComponent<TextMeshProUGUI>();
title.text = "FSM — Game Flow";
title.fontSize = 32;
title.alignment = TextAlignmentOptions.Center;
title.color = new Color32(0xF2, 0xF3, 0xF5, 0xFF);
titleGo.GetComponent<LayoutElement>().preferredHeight = 44;

Button MakeButton(string label)
{
    var go = new GameObject(label.Replace(" ", ""), typeof(Image), typeof(Button), typeof(LayoutElement));
    go.transform.SetParent(contentGo.transform, false);
    go.GetComponent<Image>().color = new Color32(0x33, 0x88, 0xE6, 0xFF);
    var le = go.GetComponent<LayoutElement>();
    le.preferredHeight = 40;
    le.preferredWidth = 320;
    var textGo = new GameObject("Label", typeof(TextMeshProUGUI));
    textGo.transform.SetParent(go.transform, false);
    var text = textGo.GetComponent<TextMeshProUGUI>();
    text.text = label;
    text.alignment = TextAlignmentOptions.Center;
    text.color = Color.white;
    text.fontSize = 20;
    var textRect = textGo.GetComponent<RectTransform>();
    textRect.anchorMin = Vector2.zero;
    textRect.anchorMax = Vector2.one;
    textRect.offsetMin = Vector2.zero;
    textRect.offsetMax = Vector2.zero;
    return go.GetComponent<Button>();
}

var finishLoadingButton = MakeButton("Finish Loading");
var playButton = MakeButton("Play");
var pauseButton = MakeButton("Pause");
var openSettingsButton = MakeButton("Open Settings");
var closeSettingsButton = MakeButton("Close Settings");
var resumeButton = MakeButton("Resume");

var stateGo = new GameObject("StateLabel", typeof(TextMeshProUGUI), typeof(LayoutElement));
stateGo.transform.SetParent(contentGo.transform, false);
var stateLabel = stateGo.GetComponent<TextMeshProUGUI>();
stateLabel.alignment = TextAlignmentOptions.Center;
stateLabel.color = new Color32(0xF2, 0xF3, 0xF5, 0xFF);
stateLabel.fontSize = 22;
stateGo.GetComponent<LayoutElement>().preferredHeight = 40;

var viewGo = new GameObject("GameFlowDemo", typeof(FsmGameFlowDemoView));
var view = viewGo.GetComponent<FsmGameFlowDemoView>();

var so = new SerializedObject(view);
so.FindProperty("_stateLabel").objectReferenceValue = stateLabel;
so.ApplyModifiedPropertiesWithoutUndo();

UnityEventTools.AddPersistentListener(finishLoadingButton.onClick, new UnityEngine.Events.UnityAction(view.OnFinishLoadingClicked));
UnityEventTools.AddPersistentListener(playButton.onClick, new UnityEngine.Events.UnityAction(view.OnPlayClicked));
UnityEventTools.AddPersistentListener(pauseButton.onClick, new UnityEngine.Events.UnityAction(view.OnPauseClicked));
UnityEventTools.AddPersistentListener(openSettingsButton.onClick, new UnityEngine.Events.UnityAction(view.OnOpenSettingsClicked));
UnityEventTools.AddPersistentListener(closeSettingsButton.onClick, new UnityEngine.Events.UnityAction(view.OnCloseSettingsClicked));
UnityEventTools.AddPersistentListener(resumeButton.onClick, new UnityEngine.Events.UnityAction(view.OnResumeClicked));

var scene = SceneManager.GetActiveScene();
EditorSceneManager.MarkSceneDirty(scene);
EditorSceneManager.SaveScene(scene);

Debug.Log("SCENE_BUILD_OK: 03_FSM_GameFlow");
```

```bash
unity command create_scene --caller plugin --skill subagent-driven-development --path "Assets/01_FSM/03_FSM_GameFlow.unity" --format json
unity command eval_file --caller plugin --skill subagent-driven-development --file "/absolute/path/to/build_fsm_gameflow.cs" --format json
```

- [ ] **Step 4: Verify it compiled and ran cleanly**

```bash
unity command console --caller plugin --skill subagent-driven-development --tail 20 --level log --format json
```

Expected: `SCENE_BUILD_OK: 03_FSM_GameFlow`, no errors.

- [ ] **Step 5: Functional check in Play mode**

Write to scratchpad as `verify_fsm_gameflow.cs`:

```csharp
using UnityEngine;
using UnityEngine.UI;
using TMPro;

GameObject.Find("Play").GetComponent<Button>().onClick.Invoke();
GameObject.Find("OpenSettings").GetComponent<Button>().onClick.Invoke();
var label = GameObject.Find("StateLabel").GetComponent<TextMeshProUGUI>();
Debug.Log("VERIFY_LABEL: " + label.text);
```

```bash
unity command editor_play --caller plugin --skill subagent-driven-development --format json
unity command editor_status --caller plugin --skill subagent-driven-development --format json   # poll until isPlaying = true
unity command eval_file --caller plugin --skill subagent-driven-development --file "/absolute/path/to/verify_fsm_gameflow.cs" --format json
unity command console --caller plugin --skill subagent-driven-development --tail 5 --level log --format json
unity command editor_stop --caller plugin --skill subagent-driven-development --format json
```

Expected: `VERIFY_LABEL: State: Playing | Substate: Settings` (exact substate name depends on `IGameFlowState.Name`/`CurrentSubstateName` implementation — any non-empty `State: ... | Substate: ...` reflecting the two clicks confirms wiring works).

- [ ] **Step 6: Register the scene in Build Settings**

```bash
unity command add_scene_to_build --caller plugin --skill subagent-driven-development --path "Assets/01_FSM/03_FSM_GameFlow.unity" --enabled true --format json
```

- [ ] **Step 7: Commit**

```bash
cd clases/clase07/Unity
git add Assets/01_FSM/Demo/FsmGameFlowDemoView.cs \
        Assets/01_FSM/03_FSM_GameFlow.unity Assets/01_FSM/03_FSM_GameFlow.unity.meta \
        ProjectSettings/EditorBuildSettings.asset
git commit -m "clase07/FSM: escena propia para el game flow, UI armada en el Inspector"
```

---

## Task 4: FSM cleanup — delete old script/scene/scaffolder, update the PlayMode test

**Files:**
- Delete: `Unity/Assets/01_FSM/Demo/FsmWeaponDemoView.cs`
- Delete: `Unity/Assets/01_FSM/01_FSM_Demo.unity`
- Delete: `Unity/Assets/01_FSM/Editor/FsmSceneScaffolding.cs`, `Unity/Assets/01_FSM/Editor/Clase07.FSM.Editor.asmdef`, and the now-empty `Unity/Assets/01_FSM/Editor/` folder
- Modify: `Unity/Assets/01_FSM/Tests/PlayMode/FsmDemoViewsPlayModeTests.cs`
- Modify: `Unity/Assets/01_FSM/Tests/PlayMode/Clase07.FSM.Tests.PlayMode.asmdef`

**Interfaces:**
- Consumes: the three scenes built in Tasks 1-3 (`01_FSM_WeaponBaseline.unity`, `02_FSM_WeaponStatePattern.unity`, `03_FSM_GameFlow.unity`).
- Produces: nothing consumed by later tasks.

- [ ] **Step 1: Remove the old scene and script through the live Editor**

```bash
unity command delete_asset --caller plugin --skill subagent-driven-development --asset "Assets/01_FSM/01_FSM_Demo.unity" --confirm true --format json
unity command delete_asset --caller plugin --skill subagent-driven-development --asset "Assets/01_FSM/Demo/FsmWeaponDemoView.cs" --confirm true --format json
```

- [ ] **Step 2: Remove the FSM scene scaffolder**

```bash
unity command delete_asset --caller plugin --skill subagent-driven-development --asset "Assets/01_FSM/Editor/FsmSceneScaffolding.cs" --confirm true --format json
unity command delete_asset --caller plugin --skill subagent-driven-development --asset "Assets/01_FSM/Editor/Clase07.FSM.Editor.asmdef" --confirm true --format json
```

Check whether `Unity/Assets/01_FSM/Editor/` is now empty on disk; if so, delete the empty folder (a plain filesystem `rmdir`/`Remove` is fine for an empty directory with no remaining assets):

```bash
rmdir clases/clase07/Unity/Assets/01_FSM/Editor 2>/dev/null || true
```

- [ ] **Step 3: Rewrite the PlayMode test to cover the 3 new scenes**

Replace the full contents of `Unity/Assets/01_FSM/Tests/PlayMode/FsmDemoViewsPlayModeTests.cs` with:

```csharp
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
using TMPro;

namespace Clase07.FSM.Tests
{
    public class FsmDemoViewsPlayModeTests
    {
        [UnityTest]
        public IEnumerator WeaponBaselineScene_LoadsAndRespondsToButtons()
        {
            SceneManager.LoadScene("Assets/01_FSM/01_FSM_WeaponBaseline.unity", LoadSceneMode.Single);
            yield return null;

            yield return ClickAllButtonsAndAssertLabelsUpdated();
        }

        [UnityTest]
        public IEnumerator WeaponStatePatternScene_LoadsAndRespondsToButtons()
        {
            SceneManager.LoadScene("Assets/01_FSM/02_FSM_WeaponStatePattern.unity", LoadSceneMode.Single);
            yield return null;

            yield return ClickAllButtonsAndAssertLabelsUpdated();
        }

        [UnityTest]
        public IEnumerator GameFlowScene_LoadsAndRespondsToButtons()
        {
            SceneManager.LoadScene("Assets/01_FSM/03_FSM_GameFlow.unity", LoadSceneMode.Single);
            yield return null;

            yield return ClickAllButtonsAndAssertLabelsUpdated();
        }

        private static IEnumerator ClickAllButtonsAndAssertLabelsUpdated()
        {
            var buttons = Object.FindObjectsOfType<Button>();
            Assert.Greater(buttons.Length, 0);

            foreach (var button in buttons)
            {
                button.onClick.Invoke();
            }
            yield return null;

            var labels = Object.FindObjectsOfType<TMP_Text>();
            Assert.Greater(labels.Length, 0);
        }
    }
}
```

- [ ] **Step 4: Add the TextMeshPro reference to the PlayMode test asmdef**

Edit `Unity/Assets/01_FSM/Tests/PlayMode/Clase07.FSM.Tests.PlayMode.asmdef`, adding `"Unity.TextMeshPro"` to `"references"`:

```json
{
    "name": "Clase07.FSM.Tests.PlayMode",
    "rootNamespace": "",
    "references": [
        "Clase07.FSM",
        "UnityEngine.UI",
        "Unity.TextMeshPro"
    ],
    "includePlatforms": [],
    "excludePlatforms": [],
    "allowUnsafeCode": false,
    "overrideReferences": false,
    "precompiledReferences": [],
    "autoReferenced": true,
    "defineConstraints": [],
    "versionDefines": [],
    "noEngineReferences": false,
    "optionalUnityReferences": [
        "TestAssemblies"
    ]
}
```

- [ ] **Step 5: Recompile and run the FSM test suites**

```bash
unity command recompile --caller plugin --skill subagent-driven-development --format json
unity command recompile_status --caller plugin --skill subagent-driven-development --format json   # poll until completed/up_to_date
unity command run_tests --caller plugin --skill subagent-driven-development --mode EditMode --filter "Clase07.FSM" --filter_type namespace --format json
unity command run_tests --caller plugin --skill subagent-driven-development --mode PlayMode --filter "Clase07.FSM.Tests.FsmDemoViewsPlayModeTests" --filter_type testName --format json
```

Expected: all tests pass (0 failed). If `run_tests` rejects `--filter_type namespace`, retry with `--filter_type testName --filter "Clase07.FSM"` or drop `--filter`/`--filter_type` entirely and run `--mode EditMode`/`--mode PlayMode` for the whole project, checking the FSM tests specifically in the result.

- [ ] **Step 6: Commit**

```bash
cd clases/clase07/Unity
git add -A Assets/01_FSM/
git status   # confirm the old scene/script/scaffolder show as deleted, nothing unexpected staged
git commit -m "clase07/FSM: borrar la escena/vista combinada y el scaffolder, actualizar el test de PlayMode"
```

---

## Task 5: DI — Singleton (refactor script + rebuild scene)

**Files:**
- Modify: `Unity/Assets/02_DependencyInjection/01_Singleton/DiSingletonDemoBootstrapper.cs`
- Modify: `Unity/Assets/02_DependencyInjection/Clase07.DI.asmdef`
- Delete + recreate (via live Editor): `Unity/Assets/02_DependencyInjection/01_Singleton/01_DI_Singleton.unity`

**Interfaces:**
- Consumes: `ScoreServiceSingleton.Instance` / `.Service.AddScore(int)` / `.Service.CurrentScore`, `AudioServiceSingleton.Instance` / `.Service.PlayCoinSound()`. Already exist, unchanged.
- Produces: `Clase07.DI.Singleton.DiSingletonDemoBootstrapper : MonoBehaviour` with `public void OnCoinClicked()` and `[SerializeField] private TMPro.TMP_Text _scoreLabel`.

- [ ] **Step 1: Rewrite the bootstrapper script**

Replace the full contents of `Unity/Assets/02_DependencyInjection/01_Singleton/DiSingletonDemoBootstrapper.cs` with:

```csharp
using UnityEngine;
using TMPro;

namespace Clase07.DI.Singleton
{
    public class DiSingletonDemoBootstrapper : MonoBehaviour
    {
        [SerializeField] private TMP_Text _scoreLabel;

        private void Awake()
        {
            if (ScoreServiceSingleton.Instance == null)
            {
                new GameObject("ScoreServiceSingleton").AddComponent<ScoreServiceSingleton>().Initialize();
            }
            if (AudioServiceSingleton.Instance == null)
            {
                new GameObject("AudioServiceSingleton").AddComponent<AudioServiceSingleton>().Initialize();
            }
            _scoreLabel.text = "Score: 0";
        }

        public void OnCoinClicked()
        {
            ScoreServiceSingleton.Instance.Service.AddScore(10);
            AudioServiceSingleton.Instance.Service.PlayCoinSound();
            _scoreLabel.text = $"Score: {ScoreServiceSingleton.Instance.Service.CurrentScore}";
        }
    }
}
```

- [ ] **Step 2: Add the TextMeshPro assembly reference to the DI asmdef**

Edit `Unity/Assets/02_DependencyInjection/Clase07.DI.asmdef`, adding `"Unity.TextMeshPro"` to `"references"`:

```json
{
    "name": "Clase07.DI",
    "rootNamespace": "",
    "references": [
        "Clase07.Shared",
        "VContainer",
        "UnityEngine.UI",
        "Unity.TextMeshPro"
    ],
    "includePlatforms": [],
    "excludePlatforms": [],
    "allowUnsafeCode": false,
    "overrideReferences": false,
    "precompiledReferences": [],
    "autoReferenced": true,
    "defineConstraints": [],
    "versionDefines": [],
    "noEngineReferences": false
}
```

- [ ] **Step 3: Recompile and confirm success**

```bash
unity command recompile --caller plugin --skill subagent-driven-development --format json
unity command recompile_status --caller plugin --skill subagent-driven-development --format json   # poll until completed/up_to_date
unity command console_status --caller plugin --skill subagent-driven-development --format json      # confirm no compile-failure flag
```

- [ ] **Step 4: Delete the old scaffolded scene and rebuild it**

```bash
unity command delete_asset --caller plugin --skill subagent-driven-development --asset "Assets/02_DependencyInjection/01_Singleton/01_DI_Singleton.unity" --confirm true --format json
unity command create_scene --caller plugin --skill subagent-driven-development --path "Assets/02_DependencyInjection/01_Singleton/01_DI_Singleton.unity" --format json
```

Write to scratchpad as `build_di_singleton.cs`:

```csharp
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using Clase07.DI.Singleton;

var canvasGo = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
var canvas = canvasGo.GetComponent<Canvas>();
canvas.renderMode = RenderMode.ScreenSpaceOverlay;
var scaler = canvasGo.GetComponent<CanvasScaler>();
scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
scaler.referenceResolution = new Vector2(1280, 720);
scaler.matchWidthOrHeight = 0.5f;

new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));

var panelGo = new GameObject("Panel", typeof(Image));
panelGo.transform.SetParent(canvasGo.transform, false);
var panelRect = panelGo.GetComponent<RectTransform>();
panelRect.anchorMin = Vector2.zero;
panelRect.anchorMax = Vector2.one;
panelRect.offsetMin = Vector2.zero;
panelRect.offsetMax = Vector2.zero;
panelGo.GetComponent<Image>().color = new Color32(0x1C, 0x1E, 0x26, 0xFF);

var contentGo = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
contentGo.transform.SetParent(panelGo.transform, false);
var contentRect = contentGo.GetComponent<RectTransform>();
contentRect.anchorMin = new Vector2(0.5f, 0.5f);
contentRect.anchorMax = new Vector2(0.5f, 0.5f);
contentRect.pivot = new Vector2(0.5f, 0.5f);
contentRect.sizeDelta = new Vector2(420, 0);
var vlg = contentGo.GetComponent<VerticalLayoutGroup>();
vlg.childAlignment = TextAnchor.MiddleCenter;
vlg.spacing = 20f;
vlg.childControlWidth = true;
vlg.childControlHeight = true;
vlg.childForceExpandWidth = true;
vlg.childForceExpandHeight = false;
var fitter = contentGo.GetComponent<ContentSizeFitter>();
fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

var titleGo = new GameObject("Title", typeof(TextMeshProUGUI), typeof(LayoutElement));
titleGo.transform.SetParent(contentGo.transform, false);
var title = titleGo.GetComponent<TextMeshProUGUI>();
title.text = "DI — Singleton";
title.fontSize = 32;
title.alignment = TextAlignmentOptions.Center;
title.color = new Color32(0xF2, 0xF3, 0xF5, 0xFF);
titleGo.GetComponent<LayoutElement>().preferredHeight = 44;

var buttonGo = new GameObject("CoinButton", typeof(Image), typeof(Button), typeof(LayoutElement));
buttonGo.transform.SetParent(contentGo.transform, false);
buttonGo.GetComponent<Image>().color = new Color32(0x33, 0x88, 0xE6, 0xFF);
var buttonLE = buttonGo.GetComponent<LayoutElement>();
buttonLE.preferredHeight = 48;
buttonLE.preferredWidth = 320;
var buttonTextGo = new GameObject("Label", typeof(TextMeshProUGUI));
buttonTextGo.transform.SetParent(buttonGo.transform, false);
var buttonText = buttonTextGo.GetComponent<TextMeshProUGUI>();
buttonText.text = "Coin (Singleton)";
buttonText.alignment = TextAlignmentOptions.Center;
buttonText.color = Color.white;
buttonText.fontSize = 22;
var btRect = buttonTextGo.GetComponent<RectTransform>();
btRect.anchorMin = Vector2.zero;
btRect.anchorMax = Vector2.one;
btRect.offsetMin = Vector2.zero;
btRect.offsetMax = Vector2.zero;

var scoreGo = new GameObject("ScoreLabel", typeof(TextMeshProUGUI), typeof(LayoutElement));
scoreGo.transform.SetParent(contentGo.transform, false);
var scoreLabel = scoreGo.GetComponent<TextMeshProUGUI>();
scoreLabel.alignment = TextAlignmentOptions.Center;
scoreLabel.color = new Color32(0xF2, 0xF3, 0xF5, 0xFF);
scoreLabel.fontSize = 26;
scoreGo.GetComponent<LayoutElement>().preferredHeight = 40;

var bootstrapperGo = new GameObject("Bootstrapper", typeof(DiSingletonDemoBootstrapper));
var bootstrapper = bootstrapperGo.GetComponent<DiSingletonDemoBootstrapper>();

var so = new SerializedObject(bootstrapper);
so.FindProperty("_scoreLabel").objectReferenceValue = scoreLabel;
so.ApplyModifiedPropertiesWithoutUndo();

UnityEventTools.AddPersistentListener(buttonGo.GetComponent<Button>().onClick, new UnityEngine.Events.UnityAction(bootstrapper.OnCoinClicked));

var scene = SceneManager.GetActiveScene();
EditorSceneManager.MarkSceneDirty(scene);
EditorSceneManager.SaveScene(scene);

Debug.Log("SCENE_BUILD_OK: 01_DI_Singleton");
```

```bash
unity command eval_file --caller plugin --skill subagent-driven-development --file "/absolute/path/to/build_di_singleton.cs" --format json
```

- [ ] **Step 5: Verify it compiled and ran cleanly**

```bash
unity command console --caller plugin --skill subagent-driven-development --tail 20 --level log --format json
```

Expected: `SCENE_BUILD_OK: 01_DI_Singleton`, no errors.

- [ ] **Step 6: Functional check in Play mode**

Write to scratchpad as `verify_di_singleton.cs`:

```csharp
using UnityEngine;
using UnityEngine.UI;
using TMPro;

GameObject.Find("CoinButton").GetComponent<Button>().onClick.Invoke();
var label = GameObject.Find("ScoreLabel").GetComponent<TextMeshProUGUI>();
Debug.Log("VERIFY_LABEL: " + label.text);
```

```bash
unity command editor_play --caller plugin --skill subagent-driven-development --format json
unity command editor_status --caller plugin --skill subagent-driven-development --format json   # poll until isPlaying = true
unity command eval_file --caller plugin --skill subagent-driven-development --file "/absolute/path/to/verify_di_singleton.cs" --format json
unity command console --caller plugin --skill subagent-driven-development --tail 5 --level log --format json
unity command editor_stop --caller plugin --skill subagent-driven-development --format json
```

Expected: `VERIFY_LABEL: Score: 10`.

- [ ] **Step 7: Register the scene in Build Settings**

```bash
unity command add_scene_to_build --caller plugin --skill subagent-driven-development --path "Assets/02_DependencyInjection/01_Singleton/01_DI_Singleton.unity" --enabled true --format json
```

- [ ] **Step 8: Commit**

```bash
cd clases/clase07/Unity
git add Assets/02_DependencyInjection/01_Singleton/DiSingletonDemoBootstrapper.cs \
        Assets/02_DependencyInjection/01_Singleton/01_DI_Singleton.unity \
        Assets/02_DependencyInjection/Clase07.DI.asmdef ProjectSettings/EditorBuildSettings.asset
git commit -m "clase07/DI: reconstruir la escena de Singleton con UI armada en el Inspector"
```

---

## Task 6: DI — ServiceLocator (refactor script + rebuild scene)

**Files:**
- Modify: `Unity/Assets/02_DependencyInjection/02_ServiceLocator/DiServiceLocatorDemoBootstrapper.cs`
- Delete + recreate (via live Editor): `Unity/Assets/02_DependencyInjection/02_ServiceLocator/02_DI_ServiceLocator.unity`

**Interfaces:**
- Consumes: `Clase07.DI.Shared.ServiceLocatorCompositionRoot.Bootstrap()`, `Clase07.DI.Shared.ServiceLocator.Resolve<T>()`, `IScoreService`/`IAudioService`. Already exist, unchanged.
- Produces: `Clase07.DI.ServiceLocatorPattern.DiServiceLocatorDemoBootstrapper : MonoBehaviour` with `public void OnCoinClicked()` and `[SerializeField] private TMPro.TMP_Text _scoreLabel`.

- [ ] **Step 1: Rewrite the bootstrapper script**

Replace the full contents of `Unity/Assets/02_DependencyInjection/02_ServiceLocator/DiServiceLocatorDemoBootstrapper.cs` with:

```csharp
using UnityEngine;
using TMPro;
using Clase07.DI.Shared;

namespace Clase07.DI.ServiceLocatorPattern
{
    public class DiServiceLocatorDemoBootstrapper : MonoBehaviour
    {
        [SerializeField] private TMP_Text _scoreLabel;

        private void Awake()
        {
            ServiceLocatorCompositionRoot.Bootstrap();
            _scoreLabel.text = "Score: 0";
        }

        public void OnCoinClicked()
        {
            var scoreService = ServiceLocator.Resolve<IScoreService>();
            ServiceLocator.Resolve<IAudioService>().PlayCoinSound();
            scoreService.AddScore(10);
            _scoreLabel.text = $"Score: {scoreService.CurrentScore}";
        }
    }
}
```

- [ ] **Step 2: Recompile and confirm success**

The DI asmdef already gained `Unity.TextMeshPro` in Task 5. Just recompile:

```bash
unity command recompile --caller plugin --skill subagent-driven-development --format json
unity command recompile_status --caller plugin --skill subagent-driven-development --format json   # poll until completed/up_to_date
unity command console_status --caller plugin --skill subagent-driven-development --format json      # confirm no compile-failure flag
```

- [ ] **Step 3: Delete the old scaffolded scene and rebuild it**

```bash
unity command delete_asset --caller plugin --skill subagent-driven-development --asset "Assets/02_DependencyInjection/02_ServiceLocator/02_DI_ServiceLocator.unity" --confirm true --format json
unity command create_scene --caller plugin --skill subagent-driven-development --path "Assets/02_DependencyInjection/02_ServiceLocator/02_DI_ServiceLocator.unity" --format json
```

Write to scratchpad as `build_di_servicelocator.cs` (identical layout to Task 5, swapped namespace/title/button text):

```csharp
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using Clase07.DI.ServiceLocatorPattern;

var canvasGo = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
var canvas = canvasGo.GetComponent<Canvas>();
canvas.renderMode = RenderMode.ScreenSpaceOverlay;
var scaler = canvasGo.GetComponent<CanvasScaler>();
scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
scaler.referenceResolution = new Vector2(1280, 720);
scaler.matchWidthOrHeight = 0.5f;

new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));

var panelGo = new GameObject("Panel", typeof(Image));
panelGo.transform.SetParent(canvasGo.transform, false);
var panelRect = panelGo.GetComponent<RectTransform>();
panelRect.anchorMin = Vector2.zero;
panelRect.anchorMax = Vector2.one;
panelRect.offsetMin = Vector2.zero;
panelRect.offsetMax = Vector2.zero;
panelGo.GetComponent<Image>().color = new Color32(0x1C, 0x1E, 0x26, 0xFF);

var contentGo = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
contentGo.transform.SetParent(panelGo.transform, false);
var contentRect = contentGo.GetComponent<RectTransform>();
contentRect.anchorMin = new Vector2(0.5f, 0.5f);
contentRect.anchorMax = new Vector2(0.5f, 0.5f);
contentRect.pivot = new Vector2(0.5f, 0.5f);
contentRect.sizeDelta = new Vector2(420, 0);
var vlg = contentGo.GetComponent<VerticalLayoutGroup>();
vlg.childAlignment = TextAnchor.MiddleCenter;
vlg.spacing = 20f;
vlg.childControlWidth = true;
vlg.childControlHeight = true;
vlg.childForceExpandWidth = true;
vlg.childForceExpandHeight = false;
var fitter = contentGo.GetComponent<ContentSizeFitter>();
fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

var titleGo = new GameObject("Title", typeof(TextMeshProUGUI), typeof(LayoutElement));
titleGo.transform.SetParent(contentGo.transform, false);
var title = titleGo.GetComponent<TextMeshProUGUI>();
title.text = "DI — Service Locator";
title.fontSize = 32;
title.alignment = TextAlignmentOptions.Center;
title.color = new Color32(0xF2, 0xF3, 0xF5, 0xFF);
titleGo.GetComponent<LayoutElement>().preferredHeight = 44;

var buttonGo = new GameObject("CoinButton", typeof(Image), typeof(Button), typeof(LayoutElement));
buttonGo.transform.SetParent(contentGo.transform, false);
buttonGo.GetComponent<Image>().color = new Color32(0x33, 0x88, 0xE6, 0xFF);
var buttonLE = buttonGo.GetComponent<LayoutElement>();
buttonLE.preferredHeight = 48;
buttonLE.preferredWidth = 320;
var buttonTextGo = new GameObject("Label", typeof(TextMeshProUGUI));
buttonTextGo.transform.SetParent(buttonGo.transform, false);
var buttonText = buttonTextGo.GetComponent<TextMeshProUGUI>();
buttonText.text = "Coin (ServiceLocator)";
buttonText.alignment = TextAlignmentOptions.Center;
buttonText.color = Color.white;
buttonText.fontSize = 22;
var btRect = buttonTextGo.GetComponent<RectTransform>();
btRect.anchorMin = Vector2.zero;
btRect.anchorMax = Vector2.one;
btRect.offsetMin = Vector2.zero;
btRect.offsetMax = Vector2.zero;

var scoreGo = new GameObject("ScoreLabel", typeof(TextMeshProUGUI), typeof(LayoutElement));
scoreGo.transform.SetParent(contentGo.transform, false);
var scoreLabel = scoreGo.GetComponent<TextMeshProUGUI>();
scoreLabel.alignment = TextAlignmentOptions.Center;
scoreLabel.color = new Color32(0xF2, 0xF3, 0xF5, 0xFF);
scoreLabel.fontSize = 26;
scoreGo.GetComponent<LayoutElement>().preferredHeight = 40;

var bootstrapperGo = new GameObject("Bootstrapper", typeof(DiServiceLocatorDemoBootstrapper));
var bootstrapper = bootstrapperGo.GetComponent<DiServiceLocatorDemoBootstrapper>();

var so = new SerializedObject(bootstrapper);
so.FindProperty("_scoreLabel").objectReferenceValue = scoreLabel;
so.ApplyModifiedPropertiesWithoutUndo();

UnityEventTools.AddPersistentListener(buttonGo.GetComponent<Button>().onClick, new UnityEngine.Events.UnityAction(bootstrapper.OnCoinClicked));

var scene = SceneManager.GetActiveScene();
EditorSceneManager.MarkSceneDirty(scene);
EditorSceneManager.SaveScene(scene);

Debug.Log("SCENE_BUILD_OK: 02_DI_ServiceLocator");
```

```bash
unity command eval_file --caller plugin --skill subagent-driven-development --file "/absolute/path/to/build_di_servicelocator.cs" --format json
```

- [ ] **Step 4: Verify it compiled and ran cleanly**

```bash
unity command console --caller plugin --skill subagent-driven-development --tail 20 --level log --format json
```

Expected: `SCENE_BUILD_OK: 02_DI_ServiceLocator`, no errors.

- [ ] **Step 5: Functional check in Play mode**

Write to scratchpad as `verify_di_servicelocator.cs`:

```csharp
using UnityEngine;
using UnityEngine.UI;
using TMPro;

GameObject.Find("CoinButton").GetComponent<Button>().onClick.Invoke();
var label = GameObject.Find("ScoreLabel").GetComponent<TextMeshProUGUI>();
Debug.Log("VERIFY_LABEL: " + label.text);
```

```bash
unity command editor_play --caller plugin --skill subagent-driven-development --format json
unity command editor_status --caller plugin --skill subagent-driven-development --format json   # poll until isPlaying = true
unity command eval_file --caller plugin --skill subagent-driven-development --file "/absolute/path/to/verify_di_servicelocator.cs" --format json
unity command console --caller plugin --skill subagent-driven-development --tail 5 --level log --format json
unity command editor_stop --caller plugin --skill subagent-driven-development --format json
```

Expected: `VERIFY_LABEL: Score: 10`.

- [ ] **Step 6: Register the scene in Build Settings**

```bash
unity command add_scene_to_build --caller plugin --skill subagent-driven-development --path "Assets/02_DependencyInjection/02_ServiceLocator/02_DI_ServiceLocator.unity" --enabled true --format json
```

- [ ] **Step 7: Commit**

```bash
cd clases/clase07/Unity
git add Assets/02_DependencyInjection/02_ServiceLocator/DiServiceLocatorDemoBootstrapper.cs \
        Assets/02_DependencyInjection/02_ServiceLocator/02_DI_ServiceLocator.unity \
        ProjectSettings/EditorBuildSettings.asset
git commit -m "clase07/DI: reconstruir la escena de Service Locator con UI armada en el Inspector"
```

---

## Task 7: DI — VContainer (refactor script + rebuild scene) + DI cleanup

**Files:**
- Modify: `Unity/Assets/02_DependencyInjection/03_VContainer/CoinPickupVContainer.cs`
- Delete + recreate (via live Editor): `Unity/Assets/02_DependencyInjection/03_VContainer/03_DI_VContainer.unity`
- Delete: `Unity/Assets/02_DependencyInjection/Editor/DiSceneScaffolding.cs`, `Unity/Assets/02_DependencyInjection/Editor/Clase07.DI.Editor.asmdef`, and the now-empty `Unity/Assets/02_DependencyInjection/Editor/` folder

**Interfaces:**
- Consumes: `Clase07.DI.VContainerExample.DiVContainerLifetimeScope` (unchanged — registers `IScoreService`/`IAudioService` and `RegisterComponentInHierarchy<CoinPickupVContainer>()`), `IScoreService`/`IAudioService` via `[Inject]`.
- Produces: `Clase07.DI.VContainerExample.CoinPickupVContainer : MonoBehaviour` with `public void OnCoinClicked()` and `[SerializeField] private TMPro.TMP_Text _scoreLabel`.

- [ ] **Step 1: Rewrite the consumer script**

Replace the full contents of `Unity/Assets/02_DependencyInjection/03_VContainer/CoinPickupVContainer.cs` with:

```csharp
using UnityEngine;
using TMPro;
using VContainer;
using Clase07.DI.Shared;

namespace Clase07.DI.VContainerExample
{
    public class CoinPickupVContainer : MonoBehaviour
    {
        [SerializeField] private TMP_Text _scoreLabel;

        private IScoreService _scoreService;
        private IAudioService _audioService;

        [Inject]
        public void Construct(IScoreService scoreService, IAudioService audioService)
        {
            _scoreService = scoreService;
            _audioService = audioService;
        }

        private void Start() => _scoreLabel.text = "Score: 0";

        public void OnCoinClicked()
        {
            _scoreService.AddScore(10);
            _audioService.PlayCoinSound();
            _scoreLabel.text = $"Score: {_scoreService.CurrentScore}";
        }
    }
}
```

- [ ] **Step 2: Recompile and confirm success**

```bash
unity command recompile --caller plugin --skill subagent-driven-development --format json
unity command recompile_status --caller plugin --skill subagent-driven-development --format json   # poll until completed/up_to_date
unity command console_status --caller plugin --skill subagent-driven-development --format json      # confirm no compile-failure flag
```

- [ ] **Step 3: Delete the old scaffolded scene and rebuild it**

This scene needs **two** GameObjects: `LifetimeScope` (composition root only) and `CoinPickup` (the consumer, holding the UI reference) — preserving the existing asymmetry versus Singleton/ServiceLocator documented in the module's `README.md`.

```bash
unity command delete_asset --caller plugin --skill subagent-driven-development --asset "Assets/02_DependencyInjection/03_VContainer/03_DI_VContainer.unity" --confirm true --format json
unity command create_scene --caller plugin --skill subagent-driven-development --path "Assets/02_DependencyInjection/03_VContainer/03_DI_VContainer.unity" --format json
```

Write to scratchpad as `build_di_vcontainer.cs`:

```csharp
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using Clase07.DI.VContainerExample;

var canvasGo = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
var canvas = canvasGo.GetComponent<Canvas>();
canvas.renderMode = RenderMode.ScreenSpaceOverlay;
var scaler = canvasGo.GetComponent<CanvasScaler>();
scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
scaler.referenceResolution = new Vector2(1280, 720);
scaler.matchWidthOrHeight = 0.5f;

new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));

var panelGo = new GameObject("Panel", typeof(Image));
panelGo.transform.SetParent(canvasGo.transform, false);
var panelRect = panelGo.GetComponent<RectTransform>();
panelRect.anchorMin = Vector2.zero;
panelRect.anchorMax = Vector2.one;
panelRect.offsetMin = Vector2.zero;
panelRect.offsetMax = Vector2.zero;
panelGo.GetComponent<Image>().color = new Color32(0x1C, 0x1E, 0x26, 0xFF);

var contentGo = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
contentGo.transform.SetParent(panelGo.transform, false);
var contentRect = contentGo.GetComponent<RectTransform>();
contentRect.anchorMin = new Vector2(0.5f, 0.5f);
contentRect.anchorMax = new Vector2(0.5f, 0.5f);
contentRect.pivot = new Vector2(0.5f, 0.5f);
contentRect.sizeDelta = new Vector2(420, 0);
var vlg = contentGo.GetComponent<VerticalLayoutGroup>();
vlg.childAlignment = TextAnchor.MiddleCenter;
vlg.spacing = 20f;
vlg.childControlWidth = true;
vlg.childControlHeight = true;
vlg.childForceExpandWidth = true;
vlg.childForceExpandHeight = false;
var fitter = contentGo.GetComponent<ContentSizeFitter>();
fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

var titleGo = new GameObject("Title", typeof(TextMeshProUGUI), typeof(LayoutElement));
titleGo.transform.SetParent(contentGo.transform, false);
var title = titleGo.GetComponent<TextMeshProUGUI>();
title.text = "DI — VContainer";
title.fontSize = 32;
title.alignment = TextAlignmentOptions.Center;
title.color = new Color32(0xF2, 0xF3, 0xF5, 0xFF);
titleGo.GetComponent<LayoutElement>().preferredHeight = 44;

var buttonGo = new GameObject("CoinButton", typeof(Image), typeof(Button), typeof(LayoutElement));
buttonGo.transform.SetParent(contentGo.transform, false);
buttonGo.GetComponent<Image>().color = new Color32(0x33, 0x88, 0xE6, 0xFF);
var buttonLE = buttonGo.GetComponent<LayoutElement>();
buttonLE.preferredHeight = 48;
buttonLE.preferredWidth = 320;
var buttonTextGo = new GameObject("Label", typeof(TextMeshProUGUI));
buttonTextGo.transform.SetParent(buttonGo.transform, false);
var buttonText = buttonTextGo.GetComponent<TextMeshProUGUI>();
buttonText.text = "Coin (VContainer)";
buttonText.alignment = TextAlignmentOptions.Center;
buttonText.color = Color.white;
buttonText.fontSize = 22;
var btRect = buttonTextGo.GetComponent<RectTransform>();
btRect.anchorMin = Vector2.zero;
btRect.anchorMax = Vector2.one;
btRect.offsetMin = Vector2.zero;
btRect.offsetMax = Vector2.zero;

var scoreGo = new GameObject("ScoreLabel", typeof(TextMeshProUGUI), typeof(LayoutElement));
scoreGo.transform.SetParent(contentGo.transform, false);
var scoreLabel = scoreGo.GetComponent<TextMeshProUGUI>();
scoreLabel.alignment = TextAlignmentOptions.Center;
scoreLabel.color = new Color32(0xF2, 0xF3, 0xF5, 0xFF);
scoreLabel.fontSize = 26;
scoreGo.GetComponent<LayoutElement>().preferredHeight = 40;

var lifetimeScopeGo = new GameObject("LifetimeScope", typeof(DiVContainerLifetimeScope));

var coinPickupGo = new GameObject("CoinPickup", typeof(CoinPickupVContainer));
var coinPickup = coinPickupGo.GetComponent<CoinPickupVContainer>();

var so = new SerializedObject(coinPickup);
so.FindProperty("_scoreLabel").objectReferenceValue = scoreLabel;
so.ApplyModifiedPropertiesWithoutUndo();

UnityEventTools.AddPersistentListener(buttonGo.GetComponent<Button>().onClick, new UnityEngine.Events.UnityAction(coinPickup.OnCoinClicked));

var scene = SceneManager.GetActiveScene();
EditorSceneManager.MarkSceneDirty(scene);
EditorSceneManager.SaveScene(scene);

Debug.Log("SCENE_BUILD_OK: 03_DI_VContainer");
```

```bash
unity command eval_file --caller plugin --skill subagent-driven-development --file "/absolute/path/to/build_di_vcontainer.cs" --format json
```

- [ ] **Step 4: Verify it compiled and ran cleanly**

```bash
unity command console --caller plugin --skill subagent-driven-development --tail 20 --level log --format json
```

Expected: `SCENE_BUILD_OK: 03_DI_VContainer`, no errors.

- [ ] **Step 5: Functional check in Play mode**

VContainer resolves `[Inject]` on `LifetimeScope.Awake()`, so the button must be clicked only after entering Play mode (same as the other DI scenes).

Write to scratchpad as `verify_di_vcontainer.cs`:

```csharp
using UnityEngine;
using UnityEngine.UI;
using TMPro;

GameObject.Find("CoinButton").GetComponent<Button>().onClick.Invoke();
var label = GameObject.Find("ScoreLabel").GetComponent<TextMeshProUGUI>();
Debug.Log("VERIFY_LABEL: " + label.text);
```

```bash
unity command editor_play --caller plugin --skill subagent-driven-development --format json
unity command editor_status --caller plugin --skill subagent-driven-development --format json   # poll until isPlaying = true
unity command eval_file --caller plugin --skill subagent-driven-development --file "/absolute/path/to/verify_di_vcontainer.cs" --format json
unity command console --caller plugin --skill subagent-driven-development --tail 5 --level log --format json
unity command editor_stop --caller plugin --skill subagent-driven-development --format json
```

Expected: `VERIFY_LABEL: Score: 10`.

- [ ] **Step 6: Register the scene in Build Settings**

```bash
unity command add_scene_to_build --caller plugin --skill subagent-driven-development --path "Assets/02_DependencyInjection/03_VContainer/03_DI_VContainer.unity" --enabled true --format json
```

- [ ] **Step 7: Remove the DI scene scaffolder**

```bash
unity command delete_asset --caller plugin --skill subagent-driven-development --asset "Assets/02_DependencyInjection/Editor/DiSceneScaffolding.cs" --confirm true --format json
unity command delete_asset --caller plugin --skill subagent-driven-development --asset "Assets/02_DependencyInjection/Editor/Clase07.DI.Editor.asmdef" --confirm true --format json
rmdir clases/clase07/Unity/Assets/02_DependencyInjection/Editor 2>/dev/null || true
```

- [ ] **Step 8: Run the DI EditMode tests**

```bash
unity command recompile --caller plugin --skill subagent-driven-development --format json
unity command recompile_status --caller plugin --skill subagent-driven-development --format json   # poll until completed/up_to_date
unity command run_tests --caller plugin --skill subagent-driven-development --mode EditMode --filter "Clase07.DI" --filter_type namespace --format json
```

Expected: all pass (0 failed). These tests exercise `DiVContainerLifetimeScope.RegisterServices` directly and are unaffected by the UI change — they should already pass; this step is a regression check, not new coverage.

- [ ] **Step 9: Commit**

```bash
cd clases/clase07/Unity
git add -A Assets/02_DependencyInjection/
git status   # confirm the old scaffolder shows as deleted, nothing unexpected staged
git commit -m "clase07/DI: reconstruir la escena de VContainer con UI armada en el Inspector, borrar el scaffolder"
```

---

## Task 8: MessageBroker — DIBroker (refactor script + rebuild scene)

**Files:**
- Modify: `Unity/Assets/03_MessageBroker/01_DIBroker/DiBrokerDemoView.cs`
- Modify: `Unity/Assets/03_MessageBroker/Clase07.MessageBroker.asmdef`
- Delete + recreate (via live Editor): `Unity/Assets/03_MessageBroker/01_DIBroker/01_MessageBroker_DIBroker.unity`

**Interfaces:**
- Consumes: `Clase07.MessageBroker.DIBroker.DiBrokerLifetimeScope` (unchanged — registers `IMessageBroker` and `RegisterComponentInHierarchy<DiBrokerDemoView>()`), `IMessageBroker.Subscribe<T>/Publish<T>`, `Clase07.MessageBroker.Shared.ScorePickedUpEvent`.
- Produces: `Clase07.MessageBroker.DIBroker.DiBrokerDemoView : MonoBehaviour` with `public void OnPublishClicked()` and `[SerializeField] private TMPro.TMP_Text _scoreLabel`.

- [ ] **Step 1: Rewrite the view script**

Replace the full contents of `Unity/Assets/03_MessageBroker/01_DIBroker/DiBrokerDemoView.cs` with:

```csharp
using UnityEngine;
using TMPro;
using VContainer;
using Clase07.MessageBroker.Shared;

namespace Clase07.MessageBroker.DIBroker
{
    public class DiBrokerDemoView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _scoreLabel;

        private IMessageBroker _broker;
        private int _total;

        [Inject]
        public void Construct(IMessageBroker broker)
        {
            _broker = broker;
            _broker.Subscribe<ScorePickedUpEvent>(OnScorePickedUp);
        }

        private void Start() => _scoreLabel.text = "Score: 0";

        public void OnPublishClicked() => _broker.Publish(new ScorePickedUpEvent(10));

        private void OnScorePickedUp(ScorePickedUpEvent evt)
        {
            _total += evt.Amount;
            if (_scoreLabel != null) _scoreLabel.text = $"Score: {_total}";
        }
    }
}
```

- [ ] **Step 2: Add the TextMeshPro assembly reference to the MessageBroker asmdef**

Edit `Unity/Assets/03_MessageBroker/Clase07.MessageBroker.asmdef`, adding `"Unity.TextMeshPro"` to `"references"`:

```json
{
    "name": "Clase07.MessageBroker",
    "rootNamespace": "",
    "references": [
        "Clase07.Shared",
        "VContainer",
        "MessagePipe",
        "MessagePipe.VContainer",
        "UniTask",
        "UnityEngine.UI",
        "Unity.TextMeshPro"
    ],
    "includePlatforms": [],
    "excludePlatforms": [],
    "allowUnsafeCode": false,
    "overrideReferences": false,
    "precompiledReferences": [],
    "autoReferenced": true,
    "defineConstraints": [],
    "versionDefines": [],
    "noEngineReferences": false
}
```

- [ ] **Step 3: Recompile and confirm success**

```bash
unity command recompile --caller plugin --skill subagent-driven-development --format json
unity command recompile_status --caller plugin --skill subagent-driven-development --format json   # poll until completed/up_to_date
unity command console_status --caller plugin --skill subagent-driven-development --format json      # confirm no compile-failure flag
```

- [ ] **Step 4: Delete the old scaffolded scene and rebuild it**

This scene keeps **one** GameObject holding both `DiBrokerLifetimeScope` and `DiBrokerDemoView`, matching the existing scaffolder's layout.

```bash
unity command delete_asset --caller plugin --skill subagent-driven-development --asset "Assets/03_MessageBroker/01_DIBroker/01_MessageBroker_DIBroker.unity" --confirm true --format json
unity command create_scene --caller plugin --skill subagent-driven-development --path "Assets/03_MessageBroker/01_DIBroker/01_MessageBroker_DIBroker.unity" --format json
```

Write to scratchpad as `build_messagebroker_dibroker.cs`:

```csharp
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using Clase07.MessageBroker.DIBroker;

var canvasGo = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
var canvas = canvasGo.GetComponent<Canvas>();
canvas.renderMode = RenderMode.ScreenSpaceOverlay;
var scaler = canvasGo.GetComponent<CanvasScaler>();
scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
scaler.referenceResolution = new Vector2(1280, 720);
scaler.matchWidthOrHeight = 0.5f;

new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));

var panelGo = new GameObject("Panel", typeof(Image));
panelGo.transform.SetParent(canvasGo.transform, false);
var panelRect = panelGo.GetComponent<RectTransform>();
panelRect.anchorMin = Vector2.zero;
panelRect.anchorMax = Vector2.one;
panelRect.offsetMin = Vector2.zero;
panelRect.offsetMax = Vector2.zero;
panelGo.GetComponent<Image>().color = new Color32(0x1C, 0x1E, 0x26, 0xFF);

var contentGo = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
contentGo.transform.SetParent(panelGo.transform, false);
var contentRect = contentGo.GetComponent<RectTransform>();
contentRect.anchorMin = new Vector2(0.5f, 0.5f);
contentRect.anchorMax = new Vector2(0.5f, 0.5f);
contentRect.pivot = new Vector2(0.5f, 0.5f);
contentRect.sizeDelta = new Vector2(460, 0);
var vlg = contentGo.GetComponent<VerticalLayoutGroup>();
vlg.childAlignment = TextAnchor.MiddleCenter;
vlg.spacing = 20f;
vlg.childControlWidth = true;
vlg.childControlHeight = true;
vlg.childForceExpandWidth = true;
vlg.childForceExpandHeight = false;
var fitter = contentGo.GetComponent<ContentSizeFitter>();
fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

var titleGo = new GameObject("Title", typeof(TextMeshProUGUI), typeof(LayoutElement));
titleGo.transform.SetParent(contentGo.transform, false);
var title = titleGo.GetComponent<TextMeshProUGUI>();
title.text = "Message Broker — DI Broker";
title.fontSize = 30;
title.alignment = TextAlignmentOptions.Center;
title.color = new Color32(0xF2, 0xF3, 0xF5, 0xFF);
titleGo.GetComponent<LayoutElement>().preferredHeight = 44;

var buttonGo = new GameObject("PublishButton", typeof(Image), typeof(Button), typeof(LayoutElement));
buttonGo.transform.SetParent(contentGo.transform, false);
buttonGo.GetComponent<Image>().color = new Color32(0x33, 0x88, 0xE6, 0xFF);
var buttonLE = buttonGo.GetComponent<LayoutElement>();
buttonLE.preferredHeight = 48;
buttonLE.preferredWidth = 360;
var buttonTextGo = new GameObject("Label", typeof(TextMeshProUGUI));
buttonTextGo.transform.SetParent(buttonGo.transform, false);
var buttonText = buttonTextGo.GetComponent<TextMeshProUGUI>();
buttonText.text = "Publish Score+10 (DIBroker)";
buttonText.alignment = TextAlignmentOptions.Center;
buttonText.color = Color.white;
buttonText.fontSize = 20;
var btRect = buttonTextGo.GetComponent<RectTransform>();
btRect.anchorMin = Vector2.zero;
btRect.anchorMax = Vector2.one;
btRect.offsetMin = Vector2.zero;
btRect.offsetMax = Vector2.zero;

var scoreGo = new GameObject("ScoreLabel", typeof(TextMeshProUGUI), typeof(LayoutElement));
scoreGo.transform.SetParent(contentGo.transform, false);
var scoreLabel = scoreGo.GetComponent<TextMeshProUGUI>();
scoreLabel.alignment = TextAlignmentOptions.Center;
scoreLabel.color = new Color32(0xF2, 0xF3, 0xF5, 0xFF);
scoreLabel.fontSize = 26;
scoreGo.GetComponent<LayoutElement>().preferredHeight = 40;

var lifetimeScopeGo = new GameObject("LifetimeScope", typeof(DiBrokerLifetimeScope), typeof(DiBrokerDemoView));
var view = lifetimeScopeGo.GetComponent<DiBrokerDemoView>();

var so = new SerializedObject(view);
so.FindProperty("_scoreLabel").objectReferenceValue = scoreLabel;
so.ApplyModifiedPropertiesWithoutUndo();

UnityEventTools.AddPersistentListener(buttonGo.GetComponent<Button>().onClick, new UnityEngine.Events.UnityAction(view.OnPublishClicked));

var scene = SceneManager.GetActiveScene();
EditorSceneManager.MarkSceneDirty(scene);
EditorSceneManager.SaveScene(scene);

Debug.Log("SCENE_BUILD_OK: 01_MessageBroker_DIBroker");
```

```bash
unity command eval_file --caller plugin --skill subagent-driven-development --file "/absolute/path/to/build_messagebroker_dibroker.cs" --format json
```

- [ ] **Step 5: Verify it compiled and ran cleanly**

```bash
unity command console --caller plugin --skill subagent-driven-development --tail 20 --level log --format json
```

Expected: `SCENE_BUILD_OK: 01_MessageBroker_DIBroker`, no errors.

- [ ] **Step 6: Functional check in Play mode**

Write to scratchpad as `verify_messagebroker_dibroker.cs`:

```csharp
using UnityEngine;
using UnityEngine.UI;
using TMPro;

GameObject.Find("PublishButton").GetComponent<Button>().onClick.Invoke();
var label = GameObject.Find("ScoreLabel").GetComponent<TextMeshProUGUI>();
Debug.Log("VERIFY_LABEL: " + label.text);
```

```bash
unity command editor_play --caller plugin --skill subagent-driven-development --format json
unity command editor_status --caller plugin --skill subagent-driven-development --format json   # poll until isPlaying = true
unity command eval_file --caller plugin --skill subagent-driven-development --file "/absolute/path/to/verify_messagebroker_dibroker.cs" --format json
unity command console --caller plugin --skill subagent-driven-development --tail 5 --level log --format json
unity command editor_stop --caller plugin --skill subagent-driven-development --format json
```

Expected: `VERIFY_LABEL: Score: 10`.

- [ ] **Step 7: Register the scene in Build Settings**

```bash
unity command add_scene_to_build --caller plugin --skill subagent-driven-development --path "Assets/03_MessageBroker/01_DIBroker/01_MessageBroker_DIBroker.unity" --enabled true --format json
```

- [ ] **Step 8: Commit**

```bash
cd clases/clase07/Unity
git add Assets/03_MessageBroker/01_DIBroker/DiBrokerDemoView.cs \
        Assets/03_MessageBroker/01_DIBroker/01_MessageBroker_DIBroker.unity \
        Assets/03_MessageBroker/Clase07.MessageBroker.asmdef ProjectSettings/EditorBuildSettings.asset
git commit -m "clase07/MessageBroker: reconstruir la escena de DI Broker con UI armada en el Inspector"
```

---

## Task 9: MessageBroker — ScriptableObject Channels (refactor script + rebuild scene)

**Files:**
- Modify: `Unity/Assets/03_MessageBroker/02_ScriptableObjectChannels/ScoreEventChannelsDemoView.cs`
- Delete + recreate (via live Editor): `Unity/Assets/03_MessageBroker/02_ScriptableObjectChannels/02_MessageBroker_SOChannels.unity`

**Interfaces:**
- Consumes: the existing `Assets/03_MessageBroker/02_ScriptableObjectChannels/ScoreEventChannel.asset` (a `ScoreEventChannelSO`, unchanged) and `Clase07.MessageBroker.Shared.ScorePickedUpEvent`.
- Produces: `Clase07.MessageBroker.ScriptableObjectChannels.ScoreEventChannelsDemoView : MonoBehaviour` with `public void OnPublishClicked()`, `[SerializeField] private ScoreEventChannelSO _channel`, `[SerializeField] private TMPro.TMP_Text _scoreLabel`.

- [ ] **Step 1: Rewrite the view script**

Replace the full contents of `Unity/Assets/03_MessageBroker/02_ScriptableObjectChannels/ScoreEventChannelsDemoView.cs` with:

```csharp
using UnityEngine;
using TMPro;
using Clase07.MessageBroker.Shared;

namespace Clase07.MessageBroker.ScriptableObjectChannels
{
    public class ScoreEventChannelsDemoView : MonoBehaviour
    {
        [SerializeField] private ScoreEventChannelSO _channel;
        [SerializeField] private TMP_Text _scoreLabel;

        private int _total;

        private void Awake()
        {
            _channel.OnRaised += OnRaised;
            _scoreLabel.text = "Score: 0";
        }

        public void OnPublishClicked() => _channel.Raise(new ScorePickedUpEvent(10));

        private void OnRaised(ScorePickedUpEvent evt)
        {
            _total += evt.Amount;
            if (_scoreLabel != null) _scoreLabel.text = $"Score: {_total}";
        }

        private void OnDestroy()
        {
            if (_channel != null) _channel.OnRaised -= OnRaised;
        }
    }
}
```

Note: `SetChannel(...)` is removed — it existed only so the old scaffolder could inject the channel asset; the scene now wires `_channel` directly to the existing `ScoreEventChannel.asset`.

- [ ] **Step 2: Recompile and confirm success**

```bash
unity command recompile --caller plugin --skill subagent-driven-development --format json
unity command recompile_status --caller plugin --skill subagent-driven-development --format json   # poll until completed/up_to_date
unity command console_status --caller plugin --skill subagent-driven-development --format json      # confirm no compile-failure flag
```

- [ ] **Step 3: Delete the old scaffolded scene and rebuild it**

```bash
unity command delete_asset --caller plugin --skill subagent-driven-development --asset "Assets/03_MessageBroker/02_ScriptableObjectChannels/02_MessageBroker_SOChannels.unity" --confirm true --format json
unity command create_scene --caller plugin --skill subagent-driven-development --path "Assets/03_MessageBroker/02_ScriptableObjectChannels/02_MessageBroker_SOChannels.unity" --format json
```

Write to scratchpad as `build_messagebroker_sochannels.cs`:

```csharp
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using Clase07.MessageBroker.ScriptableObjectChannels;

var canvasGo = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
var canvas = canvasGo.GetComponent<Canvas>();
canvas.renderMode = RenderMode.ScreenSpaceOverlay;
var scaler = canvasGo.GetComponent<CanvasScaler>();
scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
scaler.referenceResolution = new Vector2(1280, 720);
scaler.matchWidthOrHeight = 0.5f;

new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));

var panelGo = new GameObject("Panel", typeof(Image));
panelGo.transform.SetParent(canvasGo.transform, false);
var panelRect = panelGo.GetComponent<RectTransform>();
panelRect.anchorMin = Vector2.zero;
panelRect.anchorMax = Vector2.one;
panelRect.offsetMin = Vector2.zero;
panelRect.offsetMax = Vector2.zero;
panelGo.GetComponent<Image>().color = new Color32(0x1C, 0x1E, 0x26, 0xFF);

var contentGo = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
contentGo.transform.SetParent(panelGo.transform, false);
var contentRect = contentGo.GetComponent<RectTransform>();
contentRect.anchorMin = new Vector2(0.5f, 0.5f);
contentRect.anchorMax = new Vector2(0.5f, 0.5f);
contentRect.pivot = new Vector2(0.5f, 0.5f);
contentRect.sizeDelta = new Vector2(460, 0);
var vlg = contentGo.GetComponent<VerticalLayoutGroup>();
vlg.childAlignment = TextAnchor.MiddleCenter;
vlg.spacing = 20f;
vlg.childControlWidth = true;
vlg.childControlHeight = true;
vlg.childForceExpandWidth = true;
vlg.childForceExpandHeight = false;
var fitter = contentGo.GetComponent<ContentSizeFitter>();
fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

var titleGo = new GameObject("Title", typeof(TextMeshProUGUI), typeof(LayoutElement));
titleGo.transform.SetParent(contentGo.transform, false);
var title = titleGo.GetComponent<TextMeshProUGUI>();
title.text = "Message Broker — SO Channels";
title.fontSize = 30;
title.alignment = TextAlignmentOptions.Center;
title.color = new Color32(0xF2, 0xF3, 0xF5, 0xFF);
titleGo.GetComponent<LayoutElement>().preferredHeight = 44;

var buttonGo = new GameObject("PublishButton", typeof(Image), typeof(Button), typeof(LayoutElement));
buttonGo.transform.SetParent(contentGo.transform, false);
buttonGo.GetComponent<Image>().color = new Color32(0x33, 0x88, 0xE6, 0xFF);
var buttonLE = buttonGo.GetComponent<LayoutElement>();
buttonLE.preferredHeight = 48;
buttonLE.preferredWidth = 360;
var buttonTextGo = new GameObject("Label", typeof(TextMeshProUGUI));
buttonTextGo.transform.SetParent(buttonGo.transform, false);
var buttonText = buttonTextGo.GetComponent<TextMeshProUGUI>();
buttonText.text = "Publish Score+10 (SO Channel)";
buttonText.alignment = TextAlignmentOptions.Center;
buttonText.color = Color.white;
buttonText.fontSize = 20;
var btRect = buttonTextGo.GetComponent<RectTransform>();
btRect.anchorMin = Vector2.zero;
btRect.anchorMax = Vector2.one;
btRect.offsetMin = Vector2.zero;
btRect.offsetMax = Vector2.zero;

var scoreGo = new GameObject("ScoreLabel", typeof(TextMeshProUGUI), typeof(LayoutElement));
scoreGo.transform.SetParent(contentGo.transform, false);
var scoreLabel = scoreGo.GetComponent<TextMeshProUGUI>();
scoreLabel.alignment = TextAlignmentOptions.Center;
scoreLabel.color = new Color32(0xF2, 0xF3, 0xF5, 0xFF);
scoreLabel.fontSize = 26;
scoreGo.GetComponent<LayoutElement>().preferredHeight = 40;

var viewGo = new GameObject("DemoView", typeof(ScoreEventChannelsDemoView));
var view = viewGo.GetComponent<ScoreEventChannelsDemoView>();

var channel = AssetDatabase.LoadAssetAtPath<ScoreEventChannelSO>("Assets/03_MessageBroker/02_ScriptableObjectChannels/ScoreEventChannel.asset");

var so = new SerializedObject(view);
so.FindProperty("_channel").objectReferenceValue = channel;
so.FindProperty("_scoreLabel").objectReferenceValue = scoreLabel;
so.ApplyModifiedPropertiesWithoutUndo();

UnityEventTools.AddPersistentListener(buttonGo.GetComponent<Button>().onClick, new UnityEngine.Events.UnityAction(view.OnPublishClicked));

var scene = SceneManager.GetActiveScene();
EditorSceneManager.MarkSceneDirty(scene);
EditorSceneManager.SaveScene(scene);

Debug.Log("SCENE_BUILD_OK: 02_MessageBroker_SOChannels, channelLoaded=" + (channel != null));
```

```bash
unity command eval_file --caller plugin --skill subagent-driven-development --file "/absolute/path/to/build_messagebroker_sochannels.cs" --format json
```

- [ ] **Step 4: Verify it compiled and ran cleanly**

```bash
unity command console --caller plugin --skill subagent-driven-development --tail 20 --level log --format json
```

Expected: `SCENE_BUILD_OK: 02_MessageBroker_SOChannels, channelLoaded=True`, no errors. If `channelLoaded=False`, confirm `Assets/03_MessageBroker/02_ScriptableObjectChannels/ScoreEventChannel.asset` still exists (it predates this migration and should not have been touched) before proceeding.

- [ ] **Step 5: Functional check in Play mode**

Write to scratchpad as `verify_messagebroker_sochannels.cs`:

```csharp
using UnityEngine;
using UnityEngine.UI;
using TMPro;

GameObject.Find("PublishButton").GetComponent<Button>().onClick.Invoke();
var label = GameObject.Find("ScoreLabel").GetComponent<TextMeshProUGUI>();
Debug.Log("VERIFY_LABEL: " + label.text);
```

```bash
unity command editor_play --caller plugin --skill subagent-driven-development --format json
unity command editor_status --caller plugin --skill subagent-driven-development --format json   # poll until isPlaying = true
unity command eval_file --caller plugin --skill subagent-driven-development --file "/absolute/path/to/verify_messagebroker_sochannels.cs" --format json
unity command console --caller plugin --skill subagent-driven-development --tail 5 --level log --format json
unity command editor_stop --caller plugin --skill subagent-driven-development --format json
```

Expected: `VERIFY_LABEL: Score: 10`.

- [ ] **Step 6: Register the scene in Build Settings**

```bash
unity command add_scene_to_build --caller plugin --skill subagent-driven-development --path "Assets/03_MessageBroker/02_ScriptableObjectChannels/02_MessageBroker_SOChannels.unity" --enabled true --format json
```

- [ ] **Step 7: Commit**

```bash
cd clases/clase07/Unity
git add Assets/03_MessageBroker/02_ScriptableObjectChannels/ScoreEventChannelsDemoView.cs \
        Assets/03_MessageBroker/02_ScriptableObjectChannels/02_MessageBroker_SOChannels.unity \
        ProjectSettings/EditorBuildSettings.asset
git commit -m "clase07/MessageBroker: reconstruir la escena de SO Channels con UI armada en el Inspector"
```

---

## Task 10: MessageBroker — MessagePipe (refactor script + rebuild scene) + MessageBroker cleanup

**Files:**
- Modify: `Unity/Assets/03_MessageBroker/03_MessagePipe/MessagePipeDemoView.cs`
- Delete + recreate (via live Editor): `Unity/Assets/03_MessageBroker/03_MessagePipe/03_MessageBroker_MessagePipe.unity`
- Delete: `Unity/Assets/03_MessageBroker/Editor/MessageBrokerSceneScaffolding.cs`, `Unity/Assets/03_MessageBroker/Editor/Clase07.MessageBroker.Editor.asmdef`, and the now-empty `Unity/Assets/03_MessageBroker/Editor/` folder

**Interfaces:**
- Consumes: `Clase07.MessageBroker.MessagePipeExample.MessagePipeLifetimeScope` (unchanged), `IPublisher<ScorePickedUpEvent>`/`ISubscriber<ScorePickedUpEvent>` via `[Inject]`.
- Produces: `Clase07.MessageBroker.MessagePipeExample.MessagePipeDemoView : MonoBehaviour` with `public void OnPublishClicked()` and `[SerializeField] private TMPro.TMP_Text _scoreLabel`.

- [ ] **Step 1: Rewrite the view script**

Replace the full contents of `Unity/Assets/03_MessageBroker/03_MessagePipe/MessagePipeDemoView.cs` with:

```csharp
using System;
using UnityEngine;
using TMPro;
using VContainer;
using MessagePipe;
using Clase07.MessageBroker.Shared;

namespace Clase07.MessageBroker.MessagePipeExample
{
    public class MessagePipeDemoView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _scoreLabel;

        private IPublisher<ScorePickedUpEvent> _publisher;
        private IDisposable _subscription;
        private int _total;

        [Inject]
        public void Construct(IPublisher<ScorePickedUpEvent> publisher, ISubscriber<ScorePickedUpEvent> subscriber)
        {
            _publisher = publisher;
            _subscription = subscriber.Subscribe(OnScorePickedUp);
        }

        private void Start() => _scoreLabel.text = "Score: 0";

        public void OnPublishClicked() => _publisher.Publish(new ScorePickedUpEvent(10));

        private void OnScorePickedUp(ScorePickedUpEvent evt)
        {
            _total += evt.Amount;
            if (_scoreLabel != null) _scoreLabel.text = $"Score: {_total}";
        }

        private void OnDestroy() => _subscription?.Dispose();
    }
}
```

- [ ] **Step 2: Recompile and confirm success**

The MessageBroker asmdef already gained `Unity.TextMeshPro` in Task 8. Just recompile:

```bash
unity command recompile --caller plugin --skill subagent-driven-development --format json
unity command recompile_status --caller plugin --skill subagent-driven-development --format json   # poll until completed/up_to_date
unity command console_status --caller plugin --skill subagent-driven-development --format json      # confirm no compile-failure flag
```

- [ ] **Step 3: Delete the old scaffolded scene and rebuild it**

One GameObject holding both `MessagePipeLifetimeScope` and `MessagePipeDemoView`, matching the existing scaffolder's layout.

```bash
unity command delete_asset --caller plugin --skill subagent-driven-development --asset "Assets/03_MessageBroker/03_MessagePipe/03_MessageBroker_MessagePipe.unity" --confirm true --format json
unity command create_scene --caller plugin --skill subagent-driven-development --path "Assets/03_MessageBroker/03_MessagePipe/03_MessageBroker_MessagePipe.unity" --format json
```

Write to scratchpad as `build_messagebroker_messagepipe.cs`:

```csharp
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using Clase07.MessageBroker.MessagePipeExample;

var canvasGo = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
var canvas = canvasGo.GetComponent<Canvas>();
canvas.renderMode = RenderMode.ScreenSpaceOverlay;
var scaler = canvasGo.GetComponent<CanvasScaler>();
scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
scaler.referenceResolution = new Vector2(1280, 720);
scaler.matchWidthOrHeight = 0.5f;

new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));

var panelGo = new GameObject("Panel", typeof(Image));
panelGo.transform.SetParent(canvasGo.transform, false);
var panelRect = panelGo.GetComponent<RectTransform>();
panelRect.anchorMin = Vector2.zero;
panelRect.anchorMax = Vector2.one;
panelRect.offsetMin = Vector2.zero;
panelRect.offsetMax = Vector2.zero;
panelGo.GetComponent<Image>().color = new Color32(0x1C, 0x1E, 0x26, 0xFF);

var contentGo = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
contentGo.transform.SetParent(panelGo.transform, false);
var contentRect = contentGo.GetComponent<RectTransform>();
contentRect.anchorMin = new Vector2(0.5f, 0.5f);
contentRect.anchorMax = new Vector2(0.5f, 0.5f);
contentRect.pivot = new Vector2(0.5f, 0.5f);
contentRect.sizeDelta = new Vector2(460, 0);
var vlg = contentGo.GetComponent<VerticalLayoutGroup>();
vlg.childAlignment = TextAnchor.MiddleCenter;
vlg.spacing = 20f;
vlg.childControlWidth = true;
vlg.childControlHeight = true;
vlg.childForceExpandWidth = true;
vlg.childForceExpandHeight = false;
var fitter = contentGo.GetComponent<ContentSizeFitter>();
fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

var titleGo = new GameObject("Title", typeof(TextMeshProUGUI), typeof(LayoutElement));
titleGo.transform.SetParent(contentGo.transform, false);
var title = titleGo.GetComponent<TextMeshProUGUI>();
title.text = "Message Broker — MessagePipe";
title.fontSize = 30;
title.alignment = TextAlignmentOptions.Center;
title.color = new Color32(0xF2, 0xF3, 0xF5, 0xFF);
titleGo.GetComponent<LayoutElement>().preferredHeight = 44;

var buttonGo = new GameObject("PublishButton", typeof(Image), typeof(Button), typeof(LayoutElement));
buttonGo.transform.SetParent(contentGo.transform, false);
buttonGo.GetComponent<Image>().color = new Color32(0x33, 0x88, 0xE6, 0xFF);
var buttonLE = buttonGo.GetComponent<LayoutElement>();
buttonLE.preferredHeight = 48;
buttonLE.preferredWidth = 360;
var buttonTextGo = new GameObject("Label", typeof(TextMeshProUGUI));
buttonTextGo.transform.SetParent(buttonGo.transform, false);
var buttonText = buttonTextGo.GetComponent<TextMeshProUGUI>();
buttonText.text = "Publish Score+10 (MessagePipe)";
buttonText.alignment = TextAlignmentOptions.Center;
buttonText.color = Color.white;
buttonText.fontSize = 20;
var btRect = buttonTextGo.GetComponent<RectTransform>();
btRect.anchorMin = Vector2.zero;
btRect.anchorMax = Vector2.one;
btRect.offsetMin = Vector2.zero;
btRect.offsetMax = Vector2.zero;

var scoreGo = new GameObject("ScoreLabel", typeof(TextMeshProUGUI), typeof(LayoutElement));
scoreGo.transform.SetParent(contentGo.transform, false);
var scoreLabel = scoreGo.GetComponent<TextMeshProUGUI>();
scoreLabel.alignment = TextAlignmentOptions.Center;
scoreLabel.color = new Color32(0xF2, 0xF3, 0xF5, 0xFF);
scoreLabel.fontSize = 26;
scoreGo.GetComponent<LayoutElement>().preferredHeight = 40;

var lifetimeScopeGo = new GameObject("LifetimeScope", typeof(MessagePipeLifetimeScope), typeof(MessagePipeDemoView));
var view = lifetimeScopeGo.GetComponent<MessagePipeDemoView>();

var so = new SerializedObject(view);
so.FindProperty("_scoreLabel").objectReferenceValue = scoreLabel;
so.ApplyModifiedPropertiesWithoutUndo();

UnityEventTools.AddPersistentListener(buttonGo.GetComponent<Button>().onClick, new UnityEngine.Events.UnityAction(view.OnPublishClicked));

var scene = SceneManager.GetActiveScene();
EditorSceneManager.MarkSceneDirty(scene);
EditorSceneManager.SaveScene(scene);

Debug.Log("SCENE_BUILD_OK: 03_MessageBroker_MessagePipe");
```

```bash
unity command eval_file --caller plugin --skill subagent-driven-development --file "/absolute/path/to/build_messagebroker_messagepipe.cs" --format json
```

- [ ] **Step 4: Verify it compiled and ran cleanly**

```bash
unity command console --caller plugin --skill subagent-driven-development --tail 20 --level log --format json
```

Expected: `SCENE_BUILD_OK: 03_MessageBroker_MessagePipe`, no errors.

- [ ] **Step 5: Functional check in Play mode**

Write to scratchpad as `verify_messagebroker_messagepipe.cs`:

```csharp
using UnityEngine;
using UnityEngine.UI;
using TMPro;

GameObject.Find("PublishButton").GetComponent<Button>().onClick.Invoke();
var label = GameObject.Find("ScoreLabel").GetComponent<TextMeshProUGUI>();
Debug.Log("VERIFY_LABEL: " + label.text);
```

```bash
unity command editor_play --caller plugin --skill subagent-driven-development --format json
unity command editor_status --caller plugin --skill subagent-driven-development --format json   # poll until isPlaying = true
unity command eval_file --caller plugin --skill subagent-driven-development --file "/absolute/path/to/verify_messagebroker_messagepipe.cs" --format json
unity command console --caller plugin --skill subagent-driven-development --tail 5 --level log --format json
unity command editor_stop --caller plugin --skill subagent-driven-development --format json
```

Expected: `VERIFY_LABEL: Score: 10`.

- [ ] **Step 6: Register the scene in Build Settings**

```bash
unity command add_scene_to_build --caller plugin --skill subagent-driven-development --path "Assets/03_MessageBroker/03_MessagePipe/03_MessageBroker_MessagePipe.unity" --enabled true --format json
```

- [ ] **Step 7: Remove the MessageBroker scene scaffolder**

```bash
unity command delete_asset --caller plugin --skill subagent-driven-development --asset "Assets/03_MessageBroker/Editor/MessageBrokerSceneScaffolding.cs" --confirm true --format json
unity command delete_asset --caller plugin --skill subagent-driven-development --asset "Assets/03_MessageBroker/Editor/Clase07.MessageBroker.Editor.asmdef" --confirm true --format json
rmdir clases/clase07/Unity/Assets/03_MessageBroker/Editor 2>/dev/null || true
```

- [ ] **Step 8: Run the MessageBroker EditMode tests**

```bash
unity command recompile --caller plugin --skill subagent-driven-development --format json
unity command recompile_status --caller plugin --skill subagent-driven-development --format json   # poll until completed/up_to_date
unity command run_tests --caller plugin --skill subagent-driven-development --mode EditMode --filter "Clase07.MessageBroker" --filter_type namespace --format json
```

Expected: all pass (0 failed).

- [ ] **Step 9: Commit**

```bash
cd clases/clase07/Unity
git add -A Assets/03_MessageBroker/
git status   # confirm the old scaffolder shows as deleted, nothing unexpected staged
git commit -m "clase07/MessageBroker: reconstruir la escena de MessagePipe con UI armada en el Inspector, borrar el scaffolder"
```

---

## Task 11: MVx — shared `InventoryItemRowView` prefab

**Files:**
- Create: `Unity/Assets/04_MVC_MVP_MVVM/Shared/InventoryItemRowView.cs`
- Modify: `Unity/Assets/04_MVC_MVP_MVVM/Clase07.Mvx.asmdef`
- Create (via live Editor): `Unity/Assets/04_MVC_MVP_MVVM/Shared/InventoryItemRow.prefab`

**Interfaces:**
- Produces: `Clase07.Mvx.Shared.InventoryItemRowView : MonoBehaviour` with `public void SetLabel(string text)`, `public void SetRemoveAction(UnityEngine.Events.UnityAction onRemove)`, `[SerializeField] private TMPro.TMP_Text _label`, `[SerializeField] private UnityEngine.UI.Button _removeButton`. This is consumed by Tasks 12-14 (`InventoryController`, `InventoryMvpView`, `InventoryMvvmView`), which `Instantiate()` the prefab per list item and call these two methods.

- [ ] **Step 1: Write the row component script**

```csharp
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Clase07.Mvx.Shared
{
    public class InventoryItemRowView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _label;
        [SerializeField] private Button _removeButton;

        public void SetLabel(string text) => _label.text = text;

        public void SetRemoveAction(UnityEngine.Events.UnityAction onRemove)
        {
            _removeButton.onClick.RemoveAllListeners();
            _removeButton.onClick.AddListener(onRemove);
        }
    }
}
```

Save this as `Unity/Assets/04_MVC_MVP_MVVM/Shared/InventoryItemRowView.cs`. Note: `SetRemoveAction` is called from code at runtime with a closure over each item's identity — this is a normal, unavoidable runtime listener (not persistent), exactly like the existing `_nameInput.onValueChanged` wiring in the MVVM view; it is not part of the static, Inspector-authored hierarchy this migration targets.

- [ ] **Step 2: Add the TextMeshPro assembly reference to the Mvx asmdef**

Edit `Unity/Assets/04_MVC_MVP_MVVM/Clase07.Mvx.asmdef`, adding `"Unity.TextMeshPro"` to `"references"`:

```json
{
    "name": "Clase07.Mvx",
    "rootNamespace": "",
    "references": [
        "Clase07.Shared",
        "UnityEngine.UI",
        "Unity.TextMeshPro"
    ],
    "includePlatforms": [],
    "excludePlatforms": [],
    "allowUnsafeCode": false,
    "overrideReferences": false,
    "precompiledReferences": [],
    "autoReferenced": true,
    "defineConstraints": [],
    "versionDefines": [],
    "noEngineReferences": false
}
```

- [ ] **Step 3: Recompile and confirm success**

```bash
unity command recompile --caller plugin --skill subagent-driven-development --format json
unity command recompile_status --caller plugin --skill subagent-driven-development --format json   # poll until completed/up_to_date
unity command console_status --caller plugin --skill subagent-driven-development --format json      # confirm no compile-failure flag
```

- [ ] **Step 4: Build and save the prefab**

This runs against whatever scene is currently open — it creates a temporary GameObject, saves it as a prefab asset, then destroys the temporary instance. Do **not** save the scene afterward; leaving it dirty and unsaved is fine since nothing in it is kept.

Write to scratchpad as `build_inventory_item_row_prefab.cs`:

```csharp
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor;
using Clase07.Mvx.Shared;

var rowGo = new GameObject("InventoryItemRow", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(InventoryItemRowView));
var rowRect = rowGo.GetComponent<RectTransform>();
rowRect.sizeDelta = new Vector2(480, 36);
var hlg = rowGo.GetComponent<HorizontalLayoutGroup>();
hlg.childAlignment = TextAnchor.MiddleLeft;
hlg.spacing = 12f;
hlg.childControlWidth = true;
hlg.childControlHeight = true;
hlg.childForceExpandWidth = false;
hlg.childForceExpandHeight = true;

var labelGo = new GameObject("Label", typeof(TextMeshProUGUI), typeof(LayoutElement));
labelGo.transform.SetParent(rowGo.transform, false);
var label = labelGo.GetComponent<TextMeshProUGUI>();
label.alignment = TextAlignmentOptions.MidlineLeft;
label.color = new Color32(0xF2, 0xF3, 0xF5, 0xFF);
label.fontSize = 22;
var labelLE = labelGo.GetComponent<LayoutElement>();
labelLE.flexibleWidth = 1f;
labelLE.preferredHeight = 36f;

var removeGo = new GameObject("RemoveButton", typeof(Image), typeof(Button), typeof(LayoutElement));
removeGo.transform.SetParent(rowGo.transform, false);
removeGo.GetComponent<Image>().color = new Color32(0xC0, 0x39, 0x2B, 0xFF);
var removeLE = removeGo.GetComponent<LayoutElement>();
removeLE.preferredWidth = 100f;
removeLE.preferredHeight = 36f;
var removeTextGo = new GameObject("Label", typeof(TextMeshProUGUI));
removeTextGo.transform.SetParent(removeGo.transform, false);
var removeText = removeTextGo.GetComponent<TextMeshProUGUI>();
removeText.text = "Remove";
removeText.alignment = TextAlignmentOptions.Center;
removeText.color = Color.white;
removeText.fontSize = 18;
var removeTextRect = removeTextGo.GetComponent<RectTransform>();
removeTextRect.anchorMin = Vector2.zero;
removeTextRect.anchorMax = Vector2.one;
removeTextRect.offsetMin = Vector2.zero;
removeTextRect.offsetMax = Vector2.zero;

var rowView = rowGo.GetComponent<InventoryItemRowView>();
var so = new SerializedObject(rowView);
so.FindProperty("_label").objectReferenceValue = label;
so.FindProperty("_removeButton").objectReferenceValue = removeGo.GetComponent<Button>();
so.ApplyModifiedPropertiesWithoutUndo();

System.IO.Directory.CreateDirectory(Application.dataPath + "/04_MVC_MVP_MVVM/Shared");
var prefab = PrefabUtility.SaveAsPrefabAsset(rowGo, "Assets/04_MVC_MVP_MVVM/Shared/InventoryItemRow.prefab");
Object.DestroyImmediate(rowGo);
AssetDatabase.SaveAssets();

Debug.Log("PREFAB_BUILD_OK: InventoryItemRow, saved=" + (prefab != null));
```

```bash
unity command eval_file --caller plugin --skill subagent-driven-development --file "/absolute/path/to/build_inventory_item_row_prefab.cs" --format json
```

- [ ] **Step 5: Verify**

```bash
unity command console --caller plugin --skill subagent-driven-development --tail 20 --level log --format json
```

Expected: `PREFAB_BUILD_OK: InventoryItemRow, saved=True`, no errors.

```bash
unity command find_assets --caller plugin --skill subagent-driven-development --type InventoryItemRowView --format json
```

Expected: one result at `Assets/04_MVC_MVP_MVVM/Shared/InventoryItemRow.prefab`.

- [ ] **Step 6: Commit**

```bash
cd clases/clase07/Unity
git add Assets/04_MVC_MVP_MVVM/Shared/InventoryItemRowView.cs \
        Assets/04_MVC_MVP_MVVM/Shared/InventoryItemRow.prefab \
        Assets/04_MVC_MVP_MVVM/Clase07.Mvx.asmdef
git commit -m "clase07/MVx: agregar el prefab compartido InventoryItemRow para las filas de la lista"
```

---

## Task 12: MVx — MVC (refactor script + rebuild scene)

**Files:**
- Modify: `Unity/Assets/04_MVC_MVP_MVVM/01_MVC/InventoryController.cs`
- Delete + recreate (via live Editor): `Unity/Assets/04_MVC_MVP_MVVM/01_MVC/01_Mvx_MVC.unity`

**Interfaces:**
- Consumes: `Clase07.Mvx.Shared.InventoryModel` (unchanged), `Clase07.Mvx.Shared.InventoryItemRowView` (Task 11) — `SetLabel(string)`, `SetRemoveAction(UnityAction)`.
- Produces: `Clase07.Mvx.Mvc.InventoryController : MonoBehaviour` with `public void OnAddClicked()`, `[SerializeField] private TMPro.TMP_InputField _nameInput`, `[SerializeField] private Transform _listContent`, `[SerializeField] private InventoryItemRowView _rowPrefab`.

- [ ] **Step 1: Rewrite the controller script**

Replace the full contents of `Unity/Assets/04_MVC_MVP_MVVM/01_MVC/InventoryController.cs` with:

```csharp
using UnityEngine;
using TMPro;
using Clase07.Mvx.Shared;

namespace Clase07.Mvx.Mvc
{
    public class InventoryController : MonoBehaviour
    {
        [SerializeField] private TMP_InputField _nameInput;
        [SerializeField] private Transform _listContent;
        [SerializeField] private InventoryItemRowView _rowPrefab;

        private readonly InventoryModel _model = new InventoryModel();

        private void Awake()
        {
            _model.Changed += RenderList;
            RenderList();
        }

        public void OnAddClicked()
        {
            if (string.IsNullOrWhiteSpace(_nameInput.text)) return;
            _model.AddItem(_nameInput.text);
            _nameInput.text = string.Empty;
        }

        private void RenderList()
        {
            for (var i = _listContent.childCount - 1; i >= 0; i--)
            {
                Destroy(_listContent.GetChild(i).gameObject);
            }

            foreach (var item in _model.Items)
            {
                var row = Instantiate(_rowPrefab, _listContent);
                row.SetLabel($"{item.Name} x{item.Quantity}");
                var itemName = item.Name;
                row.SetRemoveAction(() => _model.RemoveItem(itemName));
            }
        }
    }
}
```

- [ ] **Step 2: Recompile and confirm success**

```bash
unity command recompile --caller plugin --skill subagent-driven-development --format json
unity command recompile_status --caller plugin --skill subagent-driven-development --format json   # poll until completed/up_to_date
unity command console_status --caller plugin --skill subagent-driven-development --format json      # confirm no compile-failure flag
```

- [ ] **Step 3: Delete the old scaffolded scene and rebuild it**

```bash
unity command delete_asset --caller plugin --skill subagent-driven-development --asset "Assets/04_MVC_MVP_MVVM/01_MVC/01_Mvx_MVC.unity" --confirm true --format json
unity command create_scene --caller plugin --skill subagent-driven-development --path "Assets/04_MVC_MVP_MVVM/01_MVC/01_Mvx_MVC.unity" --format json
```

Write to scratchpad as `build_mvx_mvc.cs`:

```csharp
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using Clase07.Mvx.Mvc;
using Clase07.Mvx.Shared;

var canvasGo = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
var canvas = canvasGo.GetComponent<Canvas>();
canvas.renderMode = RenderMode.ScreenSpaceOverlay;
var scaler = canvasGo.GetComponent<CanvasScaler>();
scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
scaler.referenceResolution = new Vector2(1280, 720);
scaler.matchWidthOrHeight = 0.5f;

new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));

var panelGo = new GameObject("Panel", typeof(Image));
panelGo.transform.SetParent(canvasGo.transform, false);
var panelRect = panelGo.GetComponent<RectTransform>();
panelRect.anchorMin = Vector2.zero;
panelRect.anchorMax = Vector2.one;
panelRect.offsetMin = Vector2.zero;
panelRect.offsetMax = Vector2.zero;
panelGo.GetComponent<Image>().color = new Color32(0x1C, 0x1E, 0x26, 0xFF);

var contentGo = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
contentGo.transform.SetParent(panelGo.transform, false);
var contentRect = contentGo.GetComponent<RectTransform>();
contentRect.anchorMin = new Vector2(0.5f, 1f);
contentRect.anchorMax = new Vector2(0.5f, 1f);
contentRect.pivot = new Vector2(0.5f, 1f);
contentRect.anchoredPosition = new Vector2(0f, -40f);
contentRect.sizeDelta = new Vector2(560, 0);
var vlg = contentGo.GetComponent<VerticalLayoutGroup>();
vlg.childAlignment = TextAnchor.UpperCenter;
vlg.spacing = 16f;
vlg.childControlWidth = true;
vlg.childControlHeight = true;
vlg.childForceExpandWidth = true;
vlg.childForceExpandHeight = false;
var fitter = contentGo.GetComponent<ContentSizeFitter>();
fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

var titleGo = new GameObject("Title", typeof(TextMeshProUGUI), typeof(LayoutElement));
titleGo.transform.SetParent(contentGo.transform, false);
var title = titleGo.GetComponent<TextMeshProUGUI>();
title.text = "MVx — MVC";
title.fontSize = 32;
title.alignment = TextAlignmentOptions.Center;
title.color = new Color32(0xF2, 0xF3, 0xF5, 0xFF);
titleGo.GetComponent<LayoutElement>().preferredHeight = 44;

var inputRowGo = new GameObject("InputRow", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
inputRowGo.transform.SetParent(contentGo.transform, false);
inputRowGo.GetComponent<LayoutElement>().preferredHeight = 44;
var hlg = inputRowGo.GetComponent<HorizontalLayoutGroup>();
hlg.spacing = 12f;
hlg.childAlignment = TextAnchor.MiddleLeft;
hlg.childControlWidth = true;
hlg.childControlHeight = true;
hlg.childForceExpandWidth = false;
hlg.childForceExpandHeight = true;

var inputGo = new GameObject("NameInput", typeof(Image), typeof(TMP_InputField), typeof(LayoutElement));
inputGo.transform.SetParent(inputRowGo.transform, false);
inputGo.GetComponent<Image>().color = new Color32(0x2A, 0x2D, 0x37, 0xFF);
var inputLE = inputGo.GetComponent<LayoutElement>();
inputLE.flexibleWidth = 1f;
inputLE.preferredHeight = 44f;
var inputField = inputGo.GetComponent<TMP_InputField>();

var textAreaGo = new GameObject("TextArea", typeof(RectMask2D));
textAreaGo.transform.SetParent(inputGo.transform, false);
var textAreaRect = textAreaGo.GetComponent<RectTransform>();
textAreaRect.anchorMin = Vector2.zero;
textAreaRect.anchorMax = Vector2.one;
textAreaRect.offsetMin = new Vector2(8, 4);
textAreaRect.offsetMax = new Vector2(-8, -4);

var inputTextGo = new GameObject("Text", typeof(TextMeshProUGUI));
inputTextGo.transform.SetParent(textAreaGo.transform, false);
var inputText = inputTextGo.GetComponent<TextMeshProUGUI>();
inputText.color = Color.white;
inputText.fontSize = 22;
inputText.alignment = TextAlignmentOptions.MidlineLeft;
var inputTextRect = inputTextGo.GetComponent<RectTransform>();
inputTextRect.anchorMin = Vector2.zero;
inputTextRect.anchorMax = Vector2.one;
inputTextRect.offsetMin = Vector2.zero;
inputTextRect.offsetMax = Vector2.zero;

var placeholderGo = new GameObject("Placeholder", typeof(TextMeshProUGUI));
placeholderGo.transform.SetParent(textAreaGo.transform, false);
var placeholder = placeholderGo.GetComponent<TextMeshProUGUI>();
placeholder.text = "Item name...";
placeholder.color = new Color(1f, 1f, 1f, 0.4f);
placeholder.fontSize = 22;
placeholder.fontStyle = FontStyles.Italic;
placeholder.alignment = TextAlignmentOptions.MidlineLeft;
var placeholderRect = placeholderGo.GetComponent<RectTransform>();
placeholderRect.anchorMin = Vector2.zero;
placeholderRect.anchorMax = Vector2.one;
placeholderRect.offsetMin = Vector2.zero;
placeholderRect.offsetMax = Vector2.zero;

inputField.textViewport = textAreaRect;
inputField.textComponent = inputText;
inputField.placeholder = placeholder;

var addGo = new GameObject("AddButton", typeof(Image), typeof(Button), typeof(LayoutElement));
addGo.transform.SetParent(inputRowGo.transform, false);
addGo.GetComponent<Image>().color = new Color32(0x33, 0x88, 0xE6, 0xFF);
var addLE = addGo.GetComponent<LayoutElement>();
addLE.preferredWidth = 120f;
addLE.preferredHeight = 44f;
var addTextGo = new GameObject("Label", typeof(TextMeshProUGUI));
addTextGo.transform.SetParent(addGo.transform, false);
var addText = addTextGo.GetComponent<TextMeshProUGUI>();
addText.text = "Add";
addText.alignment = TextAlignmentOptions.Center;
addText.color = Color.white;
addText.fontSize = 22;
var addTextRect = addTextGo.GetComponent<RectTransform>();
addTextRect.anchorMin = Vector2.zero;
addTextRect.anchorMax = Vector2.one;
addTextRect.offsetMin = Vector2.zero;
addTextRect.offsetMax = Vector2.zero;

var listGo = new GameObject("ListContent", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
listGo.transform.SetParent(contentGo.transform, false);
var listVlg = listGo.GetComponent<VerticalLayoutGroup>();
listVlg.spacing = 8f;
listVlg.childAlignment = TextAnchor.UpperCenter;
listVlg.childControlWidth = true;
listVlg.childControlHeight = true;
listVlg.childForceExpandWidth = true;
listVlg.childForceExpandHeight = false;
var listFitter = listGo.GetComponent<ContentSizeFitter>();
listFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

var controllerGo = new GameObject("InventoryController", typeof(InventoryController));
var controller = controllerGo.GetComponent<InventoryController>();

var rowPrefab = AssetDatabase.LoadAssetAtPath<InventoryItemRowView>("Assets/04_MVC_MVP_MVVM/Shared/InventoryItemRow.prefab");

var so = new SerializedObject(controller);
so.FindProperty("_nameInput").objectReferenceValue = inputField;
so.FindProperty("_listContent").objectReferenceValue = listGo.transform;
so.FindProperty("_rowPrefab").objectReferenceValue = rowPrefab;
so.ApplyModifiedPropertiesWithoutUndo();

UnityEventTools.AddPersistentListener(addGo.GetComponent<Button>().onClick, new UnityEngine.Events.UnityAction(controller.OnAddClicked));

var scene = SceneManager.GetActiveScene();
EditorSceneManager.MarkSceneDirty(scene);
EditorSceneManager.SaveScene(scene);

Debug.Log("SCENE_BUILD_OK: 01_Mvx_MVC, rowPrefabLoaded=" + (rowPrefab != null));
```

```bash
unity command eval_file --caller plugin --skill subagent-driven-development --file "/absolute/path/to/build_mvx_mvc.cs" --format json
```

- [ ] **Step 4: Verify it compiled and ran cleanly**

```bash
unity command console --caller plugin --skill subagent-driven-development --tail 20 --level log --format json
```

Expected: `SCENE_BUILD_OK: 01_Mvx_MVC, rowPrefabLoaded=True`, no errors.

- [ ] **Step 5: Functional check in Play mode**

Write to scratchpad as `verify_mvx_mvc.cs`:

```csharp
using UnityEngine;
using UnityEngine.UI;
using TMPro;

var input = GameObject.Find("NameInput").GetComponent<TMP_InputField>();
input.text = "Sword";
GameObject.Find("AddButton").GetComponent<Button>().onClick.Invoke();
var listContent = GameObject.Find("ListContent").transform;
Debug.Log("VERIFY_ROW_COUNT: " + listContent.childCount);
var rowLabel = listContent.GetChild(0).GetComponentInChildren<TextMeshProUGUI>();
Debug.Log("VERIFY_ROW_LABEL: " + rowLabel.text);
```

```bash
unity command editor_play --caller plugin --skill subagent-driven-development --format json
unity command editor_status --caller plugin --skill subagent-driven-development --format json   # poll until isPlaying = true
unity command eval_file --caller plugin --skill subagent-driven-development --file "/absolute/path/to/verify_mvx_mvc.cs" --format json
unity command console --caller plugin --skill subagent-driven-development --tail 5 --level log --format json
unity command editor_stop --caller plugin --skill subagent-driven-development --format json
```

Expected: `VERIFY_ROW_COUNT: 1` and `VERIFY_ROW_LABEL: Sword x1`.

- [ ] **Step 6: Register the scene in Build Settings**

```bash
unity command add_scene_to_build --caller plugin --skill subagent-driven-development --path "Assets/04_MVC_MVP_MVVM/01_MVC/01_Mvx_MVC.unity" --enabled true --format json
```

- [ ] **Step 7: Commit**

```bash
cd clases/clase07/Unity
git add Assets/04_MVC_MVP_MVVM/01_MVC/InventoryController.cs \
        Assets/04_MVC_MVP_MVVM/01_MVC/01_Mvx_MVC.unity \
        ProjectSettings/EditorBuildSettings.asset
git commit -m "clase07/MVx: reconstruir la escena de MVC con UI armada en el Inspector"
```

---

## Task 13: MVx — MVP (refactor script + rebuild scene)

**Files:**
- Modify: `Unity/Assets/04_MVC_MVP_MVVM/02_MVP/InventoryMvpView.cs`
- Delete + recreate (via live Editor): `Unity/Assets/04_MVC_MVP_MVVM/02_MVP/02_Mvx_MVP.unity`

**Interfaces:**
- Consumes: `Clase07.Mvx.Shared.InventoryModel`, `Clase07.Mvx.Mvp.InventoryPresenter`, `Clase07.Mvx.Mvp.IInventoryView` (all unchanged), `Clase07.Mvx.Shared.InventoryItemRowView` (Task 11).
- Produces: `Clase07.Mvx.Mvp.InventoryMvpView : MonoBehaviour, IInventoryView` with `public void OnAddClicked()`, `[SerializeField] private TMPro.TMP_InputField _nameInput`, `[SerializeField] private Transform _listContent`, `[SerializeField] private InventoryItemRowView _rowPrefab`. Keeps its existing `AddRequested`/`RemoveRequested` events and `ShowItems(IReadOnlyList<InventoryItem>)` method.

- [ ] **Step 1: Rewrite the view script**

Replace the full contents of `Unity/Assets/04_MVC_MVP_MVVM/02_MVP/InventoryMvpView.cs` with:

```csharp
using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Clase07.Mvx.Shared;

namespace Clase07.Mvx.Mvp
{
    public class InventoryMvpView : MonoBehaviour, IInventoryView
    {
        [SerializeField] private TMP_InputField _nameInput;
        [SerializeField] private Transform _listContent;
        [SerializeField] private InventoryItemRowView _rowPrefab;

        public event Action<string> AddRequested;
        public event Action<string> RemoveRequested;

        private void Awake()
        {
            new InventoryPresenter(new InventoryModel(), this);
        }

        public void OnAddClicked()
        {
            AddRequested?.Invoke(_nameInput.text);
            _nameInput.text = string.Empty;
        }

        public void ShowItems(IReadOnlyList<InventoryItem> items)
        {
            for (var i = _listContent.childCount - 1; i >= 0; i--)
            {
                Destroy(_listContent.GetChild(i).gameObject);
            }

            foreach (var item in items)
            {
                var row = Instantiate(_rowPrefab, _listContent);
                row.SetLabel($"{item.Name} x{item.Quantity}");
                var itemName = item.Name;
                row.SetRemoveAction(() => RemoveRequested?.Invoke(itemName));
            }
        }
    }
}
```

- [ ] **Step 2: Recompile and confirm success**

```bash
unity command recompile --caller plugin --skill subagent-driven-development --format json
unity command recompile_status --caller plugin --skill subagent-driven-development --format json   # poll until completed/up_to_date
unity command console_status --caller plugin --skill subagent-driven-development --format json      # confirm no compile-failure flag
```

- [ ] **Step 3: Delete the old scaffolded scene and rebuild it**

```bash
unity command delete_asset --caller plugin --skill subagent-driven-development --asset "Assets/04_MVC_MVP_MVVM/02_MVP/02_Mvx_MVP.unity" --confirm true --format json
unity command create_scene --caller plugin --skill subagent-driven-development --path "Assets/04_MVC_MVP_MVVM/02_MVP/02_Mvx_MVP.unity" --format json
```

Write to scratchpad as `build_mvx_mvp.cs` — identical layout to Task 12, swapped namespace/component/title:

```csharp
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using Clase07.Mvx.Mvp;
using Clase07.Mvx.Shared;

var canvasGo = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
var canvas = canvasGo.GetComponent<Canvas>();
canvas.renderMode = RenderMode.ScreenSpaceOverlay;
var scaler = canvasGo.GetComponent<CanvasScaler>();
scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
scaler.referenceResolution = new Vector2(1280, 720);
scaler.matchWidthOrHeight = 0.5f;

new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));

var panelGo = new GameObject("Panel", typeof(Image));
panelGo.transform.SetParent(canvasGo.transform, false);
var panelRect = panelGo.GetComponent<RectTransform>();
panelRect.anchorMin = Vector2.zero;
panelRect.anchorMax = Vector2.one;
panelRect.offsetMin = Vector2.zero;
panelRect.offsetMax = Vector2.zero;
panelGo.GetComponent<Image>().color = new Color32(0x1C, 0x1E, 0x26, 0xFF);

var contentGo = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
contentGo.transform.SetParent(panelGo.transform, false);
var contentRect = contentGo.GetComponent<RectTransform>();
contentRect.anchorMin = new Vector2(0.5f, 1f);
contentRect.anchorMax = new Vector2(0.5f, 1f);
contentRect.pivot = new Vector2(0.5f, 1f);
contentRect.anchoredPosition = new Vector2(0f, -40f);
contentRect.sizeDelta = new Vector2(560, 0);
var vlg = contentGo.GetComponent<VerticalLayoutGroup>();
vlg.childAlignment = TextAnchor.UpperCenter;
vlg.spacing = 16f;
vlg.childControlWidth = true;
vlg.childControlHeight = true;
vlg.childForceExpandWidth = true;
vlg.childForceExpandHeight = false;
var fitter = contentGo.GetComponent<ContentSizeFitter>();
fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

var titleGo = new GameObject("Title", typeof(TextMeshProUGUI), typeof(LayoutElement));
titleGo.transform.SetParent(contentGo.transform, false);
var title = titleGo.GetComponent<TextMeshProUGUI>();
title.text = "MVx — MVP";
title.fontSize = 32;
title.alignment = TextAlignmentOptions.Center;
title.color = new Color32(0xF2, 0xF3, 0xF5, 0xFF);
titleGo.GetComponent<LayoutElement>().preferredHeight = 44;

var inputRowGo = new GameObject("InputRow", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
inputRowGo.transform.SetParent(contentGo.transform, false);
inputRowGo.GetComponent<LayoutElement>().preferredHeight = 44;
var hlg = inputRowGo.GetComponent<HorizontalLayoutGroup>();
hlg.spacing = 12f;
hlg.childAlignment = TextAnchor.MiddleLeft;
hlg.childControlWidth = true;
hlg.childControlHeight = true;
hlg.childForceExpandWidth = false;
hlg.childForceExpandHeight = true;

var inputGo = new GameObject("NameInput", typeof(Image), typeof(TMP_InputField), typeof(LayoutElement));
inputGo.transform.SetParent(inputRowGo.transform, false);
inputGo.GetComponent<Image>().color = new Color32(0x2A, 0x2D, 0x37, 0xFF);
var inputLE = inputGo.GetComponent<LayoutElement>();
inputLE.flexibleWidth = 1f;
inputLE.preferredHeight = 44f;
var inputField = inputGo.GetComponent<TMP_InputField>();

var textAreaGo = new GameObject("TextArea", typeof(RectMask2D));
textAreaGo.transform.SetParent(inputGo.transform, false);
var textAreaRect = textAreaGo.GetComponent<RectTransform>();
textAreaRect.anchorMin = Vector2.zero;
textAreaRect.anchorMax = Vector2.one;
textAreaRect.offsetMin = new Vector2(8, 4);
textAreaRect.offsetMax = new Vector2(-8, -4);

var inputTextGo = new GameObject("Text", typeof(TextMeshProUGUI));
inputTextGo.transform.SetParent(textAreaGo.transform, false);
var inputText = inputTextGo.GetComponent<TextMeshProUGUI>();
inputText.color = Color.white;
inputText.fontSize = 22;
inputText.alignment = TextAlignmentOptions.MidlineLeft;
var inputTextRect = inputTextGo.GetComponent<RectTransform>();
inputTextRect.anchorMin = Vector2.zero;
inputTextRect.anchorMax = Vector2.one;
inputTextRect.offsetMin = Vector2.zero;
inputTextRect.offsetMax = Vector2.zero;

var placeholderGo = new GameObject("Placeholder", typeof(TextMeshProUGUI));
placeholderGo.transform.SetParent(textAreaGo.transform, false);
var placeholder = placeholderGo.GetComponent<TextMeshProUGUI>();
placeholder.text = "Item name...";
placeholder.color = new Color(1f, 1f, 1f, 0.4f);
placeholder.fontSize = 22;
placeholder.fontStyle = FontStyles.Italic;
placeholder.alignment = TextAlignmentOptions.MidlineLeft;
var placeholderRect = placeholderGo.GetComponent<RectTransform>();
placeholderRect.anchorMin = Vector2.zero;
placeholderRect.anchorMax = Vector2.one;
placeholderRect.offsetMin = Vector2.zero;
placeholderRect.offsetMax = Vector2.zero;

inputField.textViewport = textAreaRect;
inputField.textComponent = inputText;
inputField.placeholder = placeholder;

var addGo = new GameObject("AddButton", typeof(Image), typeof(Button), typeof(LayoutElement));
addGo.transform.SetParent(inputRowGo.transform, false);
addGo.GetComponent<Image>().color = new Color32(0x33, 0x88, 0xE6, 0xFF);
var addLE = addGo.GetComponent<LayoutElement>();
addLE.preferredWidth = 120f;
addLE.preferredHeight = 44f;
var addTextGo = new GameObject("Label", typeof(TextMeshProUGUI));
addTextGo.transform.SetParent(addGo.transform, false);
var addText = addTextGo.GetComponent<TextMeshProUGUI>();
addText.text = "Add";
addText.alignment = TextAlignmentOptions.Center;
addText.color = Color.white;
addText.fontSize = 22;
var addTextRect = addTextGo.GetComponent<RectTransform>();
addTextRect.anchorMin = Vector2.zero;
addTextRect.anchorMax = Vector2.one;
addTextRect.offsetMin = Vector2.zero;
addTextRect.offsetMax = Vector2.zero;

var listGo = new GameObject("ListContent", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
listGo.transform.SetParent(contentGo.transform, false);
var listVlg = listGo.GetComponent<VerticalLayoutGroup>();
listVlg.spacing = 8f;
listVlg.childAlignment = TextAnchor.UpperCenter;
listVlg.childControlWidth = true;
listVlg.childControlHeight = true;
listVlg.childForceExpandWidth = true;
listVlg.childForceExpandHeight = false;
var listFitter = listGo.GetComponent<ContentSizeFitter>();
listFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

var viewGo = new GameObject("InventoryMvpView", typeof(InventoryMvpView));
var view = viewGo.GetComponent<InventoryMvpView>();

var rowPrefab = AssetDatabase.LoadAssetAtPath<InventoryItemRowView>("Assets/04_MVC_MVP_MVVM/Shared/InventoryItemRow.prefab");

var so = new SerializedObject(view);
so.FindProperty("_nameInput").objectReferenceValue = inputField;
so.FindProperty("_listContent").objectReferenceValue = listGo.transform;
so.FindProperty("_rowPrefab").objectReferenceValue = rowPrefab;
so.ApplyModifiedPropertiesWithoutUndo();

UnityEventTools.AddPersistentListener(addGo.GetComponent<Button>().onClick, new UnityEngine.Events.UnityAction(view.OnAddClicked));

var scene = SceneManager.GetActiveScene();
EditorSceneManager.MarkSceneDirty(scene);
EditorSceneManager.SaveScene(scene);

Debug.Log("SCENE_BUILD_OK: 02_Mvx_MVP, rowPrefabLoaded=" + (rowPrefab != null));
```

```bash
unity command eval_file --caller plugin --skill subagent-driven-development --file "/absolute/path/to/build_mvx_mvp.cs" --format json
```

- [ ] **Step 4: Verify it compiled and ran cleanly**

```bash
unity command console --caller plugin --skill subagent-driven-development --tail 20 --level log --format json
```

Expected: `SCENE_BUILD_OK: 02_Mvx_MVP, rowPrefabLoaded=True`, no errors.

- [ ] **Step 5: Functional check in Play mode**

Write to scratchpad as `verify_mvx_mvp.cs`:

```csharp
using UnityEngine;
using UnityEngine.UI;
using TMPro;

var input = GameObject.Find("NameInput").GetComponent<TMP_InputField>();
input.text = "Shield";
GameObject.Find("AddButton").GetComponent<Button>().onClick.Invoke();
var listContent = GameObject.Find("ListContent").transform;
Debug.Log("VERIFY_ROW_COUNT: " + listContent.childCount);
var rowLabel = listContent.GetChild(0).GetComponentInChildren<TextMeshProUGUI>();
Debug.Log("VERIFY_ROW_LABEL: " + rowLabel.text);
```

```bash
unity command editor_play --caller plugin --skill subagent-driven-development --format json
unity command editor_status --caller plugin --skill subagent-driven-development --format json   # poll until isPlaying = true
unity command eval_file --caller plugin --skill subagent-driven-development --file "/absolute/path/to/verify_mvx_mvp.cs" --format json
unity command console --caller plugin --skill subagent-driven-development --tail 5 --level log --format json
unity command editor_stop --caller plugin --skill subagent-driven-development --format json
```

Expected: `VERIFY_ROW_COUNT: 1` and `VERIFY_ROW_LABEL: Shield x1`.

- [ ] **Step 6: Register the scene in Build Settings**

```bash
unity command add_scene_to_build --caller plugin --skill subagent-driven-development --path "Assets/04_MVC_MVP_MVVM/02_MVP/02_Mvx_MVP.unity" --enabled true --format json
```

- [ ] **Step 7: Commit**

```bash
cd clases/clase07/Unity
git add Assets/04_MVC_MVP_MVVM/02_MVP/InventoryMvpView.cs \
        Assets/04_MVC_MVP_MVVM/02_MVP/02_Mvx_MVP.unity \
        ProjectSettings/EditorBuildSettings.asset
git commit -m "clase07/MVx: reconstruir la escena de MVP con UI armada en el Inspector"
```

---

## Task 14: MVx — MVVM (refactor script + rebuild scene) + MVx cleanup

**Files:**
- Modify: `Unity/Assets/04_MVC_MVP_MVVM/03_MVVM/InventoryMvvmView.cs`
- Delete + recreate (via live Editor): `Unity/Assets/04_MVC_MVP_MVVM/03_MVVM/03_Mvx_MVVM.unity`
- Delete: `Unity/Assets/04_MVC_MVP_MVVM/Editor/MvxSceneScaffolding.cs`, `Unity/Assets/04_MVC_MVP_MVVM/Editor/Clase07.Mvx.Editor.asmdef`, and the now-empty `Unity/Assets/04_MVC_MVP_MVVM/Editor/` folder

**Interfaces:**
- Consumes: `Clase07.Mvx.Shared.InventoryModel`, `Clase07.Mvx.Mvvm.InventoryViewModel` (unchanged), `Clase07.Mvx.Shared.InventoryItemRowView` (Task 11).
- Produces: `Clase07.Mvx.Mvvm.InventoryMvvmView : MonoBehaviour` with `public void OnAddClicked()`, `[SerializeField] private TMPro.TMP_InputField _nameInput`, `[SerializeField] private Transform _listContent`, `[SerializeField] private InventoryItemRowView _rowPrefab`.

- [ ] **Step 1: Rewrite the view script**

Replace the full contents of `Unity/Assets/04_MVC_MVP_MVVM/03_MVVM/InventoryMvvmView.cs` with:

```csharp
using UnityEngine;
using TMPro;
using Clase07.Mvx.Shared;

namespace Clase07.Mvx.Mvvm
{
    public class InventoryMvvmView : MonoBehaviour
    {
        [SerializeField] private TMP_InputField _nameInput;
        [SerializeField] private Transform _listContent;
        [SerializeField] private InventoryItemRowView _rowPrefab;

        private InventoryViewModel _viewModel;

        private void Awake()
        {
            _viewModel = new InventoryViewModel(new InventoryModel());
            _nameInput.onValueChanged.AddListener(value => _viewModel.PendingName = value);
            _viewModel.Items.CollectionChanged += (_, __) => Render();
            Render();
        }

        public void OnAddClicked()
        {
            _viewModel.AddCommand.Execute();
            _nameInput.text = string.Empty;
        }

        private void Render()
        {
            for (var i = _listContent.childCount - 1; i >= 0; i--)
            {
                Destroy(_listContent.GetChild(i).gameObject);
            }

            foreach (var itemVm in _viewModel.Items)
            {
                var row = Instantiate(_rowPrefab, _listContent);
                row.SetLabel($"{itemVm.Name} x{itemVm.Quantity}");
                row.SetRemoveAction(itemVm.RemoveCommand.Execute);
            }
        }
    }
}
```

- [ ] **Step 2: Recompile and confirm success**

```bash
unity command recompile --caller plugin --skill subagent-driven-development --format json
unity command recompile_status --caller plugin --skill subagent-driven-development --format json   # poll until completed/up_to_date
unity command console_status --caller plugin --skill subagent-driven-development --format json      # confirm no compile-failure flag
```

- [ ] **Step 3: Delete the old scaffolded scene and rebuild it**

```bash
unity command delete_asset --caller plugin --skill subagent-driven-development --asset "Assets/04_MVC_MVP_MVVM/03_MVVM/03_Mvx_MVVM.unity" --confirm true --format json
unity command create_scene --caller plugin --skill subagent-driven-development --path "Assets/04_MVC_MVP_MVVM/03_MVVM/03_Mvx_MVVM.unity" --format json
```

Write to scratchpad as `build_mvx_mvvm.cs` — identical layout to Tasks 12-13, swapped namespace/component/title:

```csharp
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using Clase07.Mvx.Mvvm;
using Clase07.Mvx.Shared;

var canvasGo = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
var canvas = canvasGo.GetComponent<Canvas>();
canvas.renderMode = RenderMode.ScreenSpaceOverlay;
var scaler = canvasGo.GetComponent<CanvasScaler>();
scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
scaler.referenceResolution = new Vector2(1280, 720);
scaler.matchWidthOrHeight = 0.5f;

new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));

var panelGo = new GameObject("Panel", typeof(Image));
panelGo.transform.SetParent(canvasGo.transform, false);
var panelRect = panelGo.GetComponent<RectTransform>();
panelRect.anchorMin = Vector2.zero;
panelRect.anchorMax = Vector2.one;
panelRect.offsetMin = Vector2.zero;
panelRect.offsetMax = Vector2.zero;
panelGo.GetComponent<Image>().color = new Color32(0x1C, 0x1E, 0x26, 0xFF);

var contentGo = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
contentGo.transform.SetParent(panelGo.transform, false);
var contentRect = contentGo.GetComponent<RectTransform>();
contentRect.anchorMin = new Vector2(0.5f, 1f);
contentRect.anchorMax = new Vector2(0.5f, 1f);
contentRect.pivot = new Vector2(0.5f, 1f);
contentRect.anchoredPosition = new Vector2(0f, -40f);
contentRect.sizeDelta = new Vector2(560, 0);
var vlg = contentGo.GetComponent<VerticalLayoutGroup>();
vlg.childAlignment = TextAnchor.UpperCenter;
vlg.spacing = 16f;
vlg.childControlWidth = true;
vlg.childControlHeight = true;
vlg.childForceExpandWidth = true;
vlg.childForceExpandHeight = false;
var fitter = contentGo.GetComponent<ContentSizeFitter>();
fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

var titleGo = new GameObject("Title", typeof(TextMeshProUGUI), typeof(LayoutElement));
titleGo.transform.SetParent(contentGo.transform, false);
var title = titleGo.GetComponent<TextMeshProUGUI>();
title.text = "MVx — MVVM";
title.fontSize = 32;
title.alignment = TextAlignmentOptions.Center;
title.color = new Color32(0xF2, 0xF3, 0xF5, 0xFF);
titleGo.GetComponent<LayoutElement>().preferredHeight = 44;

var inputRowGo = new GameObject("InputRow", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
inputRowGo.transform.SetParent(contentGo.transform, false);
inputRowGo.GetComponent<LayoutElement>().preferredHeight = 44;
var hlg = inputRowGo.GetComponent<HorizontalLayoutGroup>();
hlg.spacing = 12f;
hlg.childAlignment = TextAnchor.MiddleLeft;
hlg.childControlWidth = true;
hlg.childControlHeight = true;
hlg.childForceExpandWidth = false;
hlg.childForceExpandHeight = true;

var inputGo = new GameObject("NameInput", typeof(Image), typeof(TMP_InputField), typeof(LayoutElement));
inputGo.transform.SetParent(inputRowGo.transform, false);
inputGo.GetComponent<Image>().color = new Color32(0x2A, 0x2D, 0x37, 0xFF);
var inputLE = inputGo.GetComponent<LayoutElement>();
inputLE.flexibleWidth = 1f;
inputLE.preferredHeight = 44f;
var inputField = inputGo.GetComponent<TMP_InputField>();

var textAreaGo = new GameObject("TextArea", typeof(RectMask2D));
textAreaGo.transform.SetParent(inputGo.transform, false);
var textAreaRect = textAreaGo.GetComponent<RectTransform>();
textAreaRect.anchorMin = Vector2.zero;
textAreaRect.anchorMax = Vector2.one;
textAreaRect.offsetMin = new Vector2(8, 4);
textAreaRect.offsetMax = new Vector2(-8, -4);

var inputTextGo = new GameObject("Text", typeof(TextMeshProUGUI));
inputTextGo.transform.SetParent(textAreaGo.transform, false);
var inputText = inputTextGo.GetComponent<TextMeshProUGUI>();
inputText.color = Color.white;
inputText.fontSize = 22;
inputText.alignment = TextAlignmentOptions.MidlineLeft;
var inputTextRect = inputTextGo.GetComponent<RectTransform>();
inputTextRect.anchorMin = Vector2.zero;
inputTextRect.anchorMax = Vector2.one;
inputTextRect.offsetMin = Vector2.zero;
inputTextRect.offsetMax = Vector2.zero;

var placeholderGo = new GameObject("Placeholder", typeof(TextMeshProUGUI));
placeholderGo.transform.SetParent(textAreaGo.transform, false);
var placeholder = placeholderGo.GetComponent<TextMeshProUGUI>();
placeholder.text = "Item name...";
placeholder.color = new Color(1f, 1f, 1f, 0.4f);
placeholder.fontSize = 22;
placeholder.fontStyle = FontStyles.Italic;
placeholder.alignment = TextAlignmentOptions.MidlineLeft;
var placeholderRect = placeholderGo.GetComponent<RectTransform>();
placeholderRect.anchorMin = Vector2.zero;
placeholderRect.anchorMax = Vector2.one;
placeholderRect.offsetMin = Vector2.zero;
placeholderRect.offsetMax = Vector2.zero;

inputField.textViewport = textAreaRect;
inputField.textComponent = inputText;
inputField.placeholder = placeholder;

var addGo = new GameObject("AddButton", typeof(Image), typeof(Button), typeof(LayoutElement));
addGo.transform.SetParent(inputRowGo.transform, false);
addGo.GetComponent<Image>().color = new Color32(0x33, 0x88, 0xE6, 0xFF);
var addLE = addGo.GetComponent<LayoutElement>();
addLE.preferredWidth = 120f;
addLE.preferredHeight = 44f;
var addTextGo = new GameObject("Label", typeof(TextMeshProUGUI));
addTextGo.transform.SetParent(addGo.transform, false);
var addText = addTextGo.GetComponent<TextMeshProUGUI>();
addText.text = "Add";
addText.alignment = TextAlignmentOptions.Center;
addText.color = Color.white;
addText.fontSize = 22;
var addTextRect = addTextGo.GetComponent<RectTransform>();
addTextRect.anchorMin = Vector2.zero;
addTextRect.anchorMax = Vector2.one;
addTextRect.offsetMin = Vector2.zero;
addTextRect.offsetMax = Vector2.zero;

var listGo = new GameObject("ListContent", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
listGo.transform.SetParent(contentGo.transform, false);
var listVlg = listGo.GetComponent<VerticalLayoutGroup>();
listVlg.spacing = 8f;
listVlg.childAlignment = TextAnchor.UpperCenter;
listVlg.childControlWidth = true;
listVlg.childControlHeight = true;
listVlg.childForceExpandWidth = true;
listVlg.childForceExpandHeight = false;
var listFitter = listGo.GetComponent<ContentSizeFitter>();
listFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

var viewGo = new GameObject("InventoryMvvmView", typeof(InventoryMvvmView));
var view = viewGo.GetComponent<InventoryMvvmView>();

var rowPrefab = AssetDatabase.LoadAssetAtPath<InventoryItemRowView>("Assets/04_MVC_MVP_MVVM/Shared/InventoryItemRow.prefab");

var so = new SerializedObject(view);
so.FindProperty("_nameInput").objectReferenceValue = inputField;
so.FindProperty("_listContent").objectReferenceValue = listGo.transform;
so.FindProperty("_rowPrefab").objectReferenceValue = rowPrefab;
so.ApplyModifiedPropertiesWithoutUndo();

UnityEventTools.AddPersistentListener(addGo.GetComponent<Button>().onClick, new UnityEngine.Events.UnityAction(view.OnAddClicked));

var scene = SceneManager.GetActiveScene();
EditorSceneManager.MarkSceneDirty(scene);
EditorSceneManager.SaveScene(scene);

Debug.Log("SCENE_BUILD_OK: 03_Mvx_MVVM, rowPrefabLoaded=" + (rowPrefab != null));
```

```bash
unity command eval_file --caller plugin --skill subagent-driven-development --file "/absolute/path/to/build_mvx_mvvm.cs" --format json
```

- [ ] **Step 4: Verify it compiled and ran cleanly**

```bash
unity command console --caller plugin --skill subagent-driven-development --tail 20 --level log --format json
```

Expected: `SCENE_BUILD_OK: 03_Mvx_MVVM, rowPrefabLoaded=True`, no errors.

- [ ] **Step 5: Functional check in Play mode**

Write to scratchpad as `verify_mvx_mvvm.cs`:

```csharp
using UnityEngine;
using UnityEngine.UI;
using TMPro;

var input = GameObject.Find("NameInput").GetComponent<TMP_InputField>();
input.text = "Potion";
GameObject.Find("AddButton").GetComponent<Button>().onClick.Invoke();
var listContent = GameObject.Find("ListContent").transform;
Debug.Log("VERIFY_ROW_COUNT: " + listContent.childCount);
var rowLabel = listContent.GetChild(0).GetComponentInChildren<TextMeshProUGUI>();
Debug.Log("VERIFY_ROW_LABEL: " + rowLabel.text);
```

```bash
unity command editor_play --caller plugin --skill subagent-driven-development --format json
unity command editor_status --caller plugin --skill subagent-driven-development --format json   # poll until isPlaying = true
unity command eval_file --caller plugin --skill subagent-driven-development --file "/absolute/path/to/verify_mvx_mvvm.cs" --format json
unity command console --caller plugin --skill subagent-driven-development --tail 5 --level log --format json
unity command editor_stop --caller plugin --skill subagent-driven-development --format json
```

Expected: `VERIFY_ROW_COUNT: 1` and `VERIFY_ROW_LABEL: Potion x1`. Note the `NameInput`'s `onValueChanged` code-side listener (needed to feed `_viewModel.PendingName`) does not fire from a direct `.text = "Potion"` assignment via `eval` the way a real keystroke would in a UI test — if `VERIFY_ROW_COUNT` comes back `0`, add `input.onValueChanged.Invoke("Potion");` before the click in the verify script and retry.

- [ ] **Step 6: Register the scene in Build Settings**

```bash
unity command add_scene_to_build --caller plugin --skill subagent-driven-development --path "Assets/04_MVC_MVP_MVVM/03_MVVM/03_Mvx_MVVM.unity" --enabled true --format json
```

- [ ] **Step 7: Remove the MVx scene scaffolder**

```bash
unity command delete_asset --caller plugin --skill subagent-driven-development --asset "Assets/04_MVC_MVP_MVVM/Editor/MvxSceneScaffolding.cs" --confirm true --format json
unity command delete_asset --caller plugin --skill subagent-driven-development --asset "Assets/04_MVC_MVP_MVVM/Editor/Clase07.Mvx.Editor.asmdef" --confirm true --format json
rmdir clases/clase07/Unity/Assets/04_MVC_MVP_MVVM/Editor 2>/dev/null || true
```

- [ ] **Step 8: Run the MVx EditMode tests**

```bash
unity command recompile --caller plugin --skill subagent-driven-development --format json
unity command recompile_status --caller plugin --skill subagent-driven-development --format json   # poll until completed/up_to_date
unity command run_tests --caller plugin --skill subagent-driven-development --mode EditMode --filter "Clase07.Mvx" --filter_type namespace --format json
```

Expected: all pass (0 failed).

- [ ] **Step 9: Commit**

```bash
cd clases/clase07/Unity
git add -A Assets/04_MVC_MVP_MVVM/
git status   # confirm the old scaffolder shows as deleted, nothing unexpected staged
git commit -m "clase07/MVx: reconstruir la escena de MVVM con UI armada en el Inspector, borrar el scaffolder"
```

---

## Task 15: Global cleanup — delete `DemoUiFactory`, trim unused asmdef references

**Files:**
- Delete: `Unity/Assets/Shared/UI/DemoUiFactory.cs`
- Delete: `Unity/Assets/Shared/Tests/DemoUiFactoryTests.cs`
- Delete: `Unity/Assets/Shared/Editor/SceneScaffolding.cs`, `Unity/Assets/Shared/Editor/Clase07.Shared.Editor.asmdef`, and the now-empty `Unity/Assets/Shared/Editor/` folder
- Modify: `Unity/Assets/Shared/Clase07.Shared.asmdef`
- Modify: `Unity/Assets/Shared/Tests/Clase07.Shared.Tests.asmdef`

**Interfaces:** none — this task only removes now-dead code. By this point (Tasks 1-14 done) nothing references `DemoUiFactory` or any `*SceneScaffolding` class.

- [ ] **Step 1: Confirm nothing still references `DemoUiFactory` or the scaffolders**

```bash
cd clases/clase07/Unity
grep -rln "DemoUiFactory\|SceneScaffolding" Assets --include="*.cs"
```

Expected: only `Assets/Shared/UI/DemoUiFactory.cs`, `Assets/Shared/Tests/DemoUiFactoryTests.cs`, and `Assets/Shared/Editor/SceneScaffolding.cs` itself (the four per-module scaffolders were already deleted in Tasks 4/7/10/14). If anything else appears, stop and investigate before deleting.

- [ ] **Step 2: Delete `DemoUiFactory` and its test through the live Editor**

```bash
unity command delete_asset --caller plugin --skill subagent-driven-development --asset "Assets/Shared/UI/DemoUiFactory.cs" --confirm true --format json
unity command delete_asset --caller plugin --skill subagent-driven-development --asset "Assets/Shared/Tests/DemoUiFactoryTests.cs" --confirm true --format json
```

If `Unity/Assets/Shared/UI/` is now empty, remove it:

```bash
rmdir clases/clase07/Unity/Assets/Shared/UI 2>/dev/null || true
```

- [ ] **Step 3: Delete the shared scene scaffolder**

```bash
unity command delete_asset --caller plugin --skill subagent-driven-development --asset "Assets/Shared/Editor/SceneScaffolding.cs" --confirm true --format json
unity command delete_asset --caller plugin --skill subagent-driven-development --asset "Assets/Shared/Editor/Clase07.Shared.Editor.asmdef" --confirm true --format json
rmdir clases/clase07/Unity/Assets/Shared/Editor 2>/dev/null || true
```

- [ ] **Step 4: Remove the now-unused `UnityEngine.UI` reference from the Shared asmdefs**

`DemoUiFactory` was the only thing in `Clase07.Shared`/`Clase07.Shared.Tests` that used `UnityEngine.UI`. Edit `Unity/Assets/Shared/Clase07.Shared.asmdef`:

```json
{
    "name": "Clase07.Shared",
    "rootNamespace": "",
    "references": [],
    "includePlatforms": [],
    "excludePlatforms": [],
    "allowUnsafeCode": false,
    "overrideReferences": false,
    "precompiledReferences": [],
    "autoReferenced": true,
    "defineConstraints": [],
    "versionDefines": [],
    "noEngineReferences": false
}
```

Edit `Unity/Assets/Shared/Tests/Clase07.Shared.Tests.asmdef`:

```json
{
    "name": "Clase07.Shared.Tests",
    "rootNamespace": "",
    "references": [
        "Clase07.Shared",
        "UnityEngine.TestRunner",
        "UnityEditor.TestRunner"
    ],
    "includePlatforms": [
        "Editor"
    ],
    "excludePlatforms": [],
    "allowUnsafeCode": false,
    "overrideReferences": true,
    "precompiledReferences": [
        "nunit.framework.dll"
    ],
    "autoReferenced": true,
    "defineConstraints": [
        "UNITY_INCLUDE_TESTS"
    ],
    "versionDefines": [],
    "noEngineReferences": false
}
```

- [ ] **Step 5: Recompile and confirm success**

```bash
unity command recompile --caller plugin --skill subagent-driven-development --format json
unity command recompile_status --caller plugin --skill subagent-driven-development --format json   # poll until completed/up_to_date
unity command console_status --caller plugin --skill subagent-driven-development --format json      # confirm no compile-failure flag
```

If a compile error appears because something in `Clase07.Shared`/`Clase07.Shared.Tests` still needs `UnityEngine.UI` (i.e. Step 1's grep missed a real usage), re-add the reference to the relevant asmdef and re-check with `grep` before retrying.

- [ ] **Step 6: Verify the final Build Settings list**

```bash
unity command get_build_settings --caller plugin --skill subagent-driven-development --format json
```

Expected: exactly these 12 scene paths, in this order, all enabled:

```
Assets/01_FSM/01_FSM_WeaponBaseline.unity
Assets/01_FSM/02_FSM_WeaponStatePattern.unity
Assets/01_FSM/03_FSM_GameFlow.unity
Assets/02_DependencyInjection/01_Singleton/01_DI_Singleton.unity
Assets/02_DependencyInjection/02_ServiceLocator/02_DI_ServiceLocator.unity
Assets/02_DependencyInjection/03_VContainer/03_DI_VContainer.unity
Assets/03_MessageBroker/01_DIBroker/01_MessageBroker_DIBroker.unity
Assets/03_MessageBroker/02_ScriptableObjectChannels/02_MessageBroker_SOChannels.unity
Assets/03_MessageBroker/03_MessagePipe/03_MessageBroker_MessagePipe.unity
Assets/04_MVC_MVP_MVVM/01_MVC/01_Mvx_MVC.unity
Assets/04_MVC_MVP_MVVM/02_MVP/02_Mvx_MVP.unity
Assets/04_MVC_MVP_MVVM/03_MVVM/03_Mvx_MVVM.unity
```

If the order differs (each `add_scene_to_build` call across Tasks 1-14 appended in call order, which should already match this), it's cosmetic and not worth fixing; if any path is missing or an old path (e.g. `Assets/01_FSM/01_FSM_Demo.unity`) is still present, fix it with `unity command remove_scene_from_build` / `add_scene_to_build` as needed.

- [ ] **Step 7: Revert the `com.unity.pipeline` authoring dependency**

This package was added in Task 0 only so this migration could drive the Editor live — it is not part of what the demos teach and should not linger in the student-facing project's dependency list (every other package in `manifest.json` is explained by the module's `README.md`).

Edit `Unity/Packages/manifest.json`, removing the `"com.unity.pipeline": "0.7.0-exp.1"` line added in Task 0 (restore it to exactly its pre-Task-0 state — diff against `git show HEAD~14:clases/clase07/Unity/Packages/manifest.json` or equivalent if unsure which line to remove).

- [ ] **Step 8: Commit**

```bash
cd clases/clase07/Unity
git add -A Assets/Shared/ Packages/manifest.json Packages/packages-lock.json
git status   # confirm DemoUiFactory/.Tests/Editor scaffolder show as deleted, manifest.json shows the pipeline line removed
git commit -m "clase07: borrar DemoUiFactory y el scaffolder compartido, revertir la dependencia de autoría com.unity.pipeline"
```

Note: after this commit, closing and reopening the Unity Editor (or a package resolve) is expected to remove the Pipeline connection — that's fine, no more live-Editor tasks remain in this plan.

---

## Task 16: Update `README.md` and `SPEC.md`

**Files:**
- Modify: `clases/clase07/README.md`
- Modify: `clases/clase07/SPEC.md`

**Interfaces:** none — documentation only.

- [ ] **Step 1: Update `README.md`'s "Cómo correr cada módulo" section**

In `clases/clase07/README.md`, find the **01 — FSM** bullet (currently describing a single `01_FSM_Demo.unity` with the weapon comparison and game flow together) and replace it with:

```markdown
- **01 — FSM**, tres escenas independientes:
  - `Assets/01_FSM/01_FSM_WeaponBaseline.unity` — botón "Fire" para la versión del
    arma con `enum` + `switch`, con texto de debug mostrando estado y munición.
  - `Assets/01_FSM/02_FSM_WeaponStatePattern.unity` — el mismo arma sobre el motor
    genérico de estados (`IState` + `StateMachine<TState>`), en su propia escena.
  - `Assets/01_FSM/03_FSM_GameFlow.unity` — botones para avanzar el game flow
    (`Finish Loading`, `Play`, `Pause`, `Open Settings`, `Close Settings`, `Resume`),
    con texto de debug mostrando el estado y el subestado activos. La escena muestra
    **sólo** la variante OOP del game flow (`FsmGameFlowDemoView`); la variante con
    estados como `ScriptableObject` se verifica por tests de EditMode
    (`GameFlowSORunnerTests`) en vez de montarse también en una escena — sigue siendo
    una decisión de alcance deliberada, ver [`SPEC.md`](SPEC.md).
```

Then find the sentence right before the module list that currently reads something like "Todas las escenas son mínimas (botones y texto de debug, sin arte): abrir la escena indicada y entrar en Play mode." and replace it with:

```markdown
Todas las escenas están armadas en el Inspector (Canvas + TextMeshPro + `Layout Group`,
sin posiciones absolutas por pixel), con la UI mínima pero prolija: abrir la escena
indicada y entrar en Play mode.
```

- [ ] **Step 2: Update `SPEC.md`'s FSM scene description and separate-scenes rule**

In `clases/clase07/SPEC.md`, find the line describing `01_FSM_Demo.unity` (around "Una escena `01_FSM_Demo.unity`: botón 'Fire' para el arma (ambas versiones montadas...") and replace it with:

```markdown
- Tres escenas independientes: `01_FSM_WeaponBaseline.unity` y
  `02_FSM_WeaponStatePattern.unity` (una por versión del arma, cada una con su botón
  "Fire" y su texto de debug) y `03_FSM_GameFlow.unity` (botones para el game flow,
  variante OOP únicamente — ver más abajo).
```

Find the line describing when implementations go in separate scenes (around "Las escenas de demo son mínimas: UI de uGUI (botones, texto), sin arte. Cuando dos implementaciones necesitan estado separado...") and replace it with:

```markdown
- Las escenas de demo son mínimas: UI de uGUI + TextMeshPro armada en el Inspector
  (Canvas, `Layout Group`, botones/texto reales, sin posiciones absolutas por pixel ni
  UI generada por código), sin arte. Cada implementación vive en su propia escena —
  tanto por necesitar estado separado (ej: `static` de un Singleton) como para poder
  compartimentalizar cada demo de forma independiente.
```

- [ ] **Step 3: Verify the docs reference only paths that exist**

```bash
cd clases/clase07
grep -o "Assets/[A-Za-z0-9_/]*\.unity" README.md SPEC.md | sort -u
```

For each path printed, confirm the file exists:

```bash
for p in $(grep -ho "Assets/[A-Za-z0-9_/]*\.unity" README.md SPEC.md | sort -u); do
  test -f "Unity/$p" && echo "OK: $p" || echo "MISSING: $p"
done
```

Expected: every line printed as `OK: ...`, none as `MISSING: ...`.

- [ ] **Step 4: Commit**

```bash
cd clases/clase07/Unity
git add ../README.md ../SPEC.md
git commit -m "clase07: actualizar README y SPEC con las escenas de FSM separadas y la UI armada en el Inspector"
```

---

## Task 17: Final verification — full test suite, scene sanity pass

**Files:** none — verification only.

**Interfaces:** none.

- [ ] **Step 1: Run the full EditMode suite**

```bash
cd clases/clase07/Unity
unity command run_tests --caller plugin --skill subagent-driven-development --mode EditMode --format json
```

Expected: 0 failed. If anything fails, use `superpowers:systematic-debugging` to investigate before proceeding — do not skip or silence a failing test.

- [ ] **Step 2: Run the full PlayMode suite**

```bash
unity command run_tests --caller plugin --skill subagent-driven-development --mode PlayMode --format json
```

Expected: 0 failed (this includes the three FSM PlayMode tests from Task 4).

- [ ] **Step 3: Cross-check against the documented batchmode invocation**

The module's `README.md` documents running tests headlessly from outside the live Editor (useful for CI or a fresh clone with no Editor open). From the monorepo root, with the Unity Editor from this session **closed** first (batchmode cannot share the project with an open Editor instance):

```bash
/Applications/Unity/Hub/Editor/6000.3.21f1/Unity.app/Contents/MacOS/Unity \
  -batchmode -nographic \
  -projectPath clases/clase07/Unity \
  -runTests -testPlatform EditMode \
  -testResults "$(pwd)/clases/clase07/Unity_editmode_results.xml" \
  -logFile "$(pwd)/clases/clase07/Unity_editmode.log"
echo "Exit code: $?"

/Applications/Unity/Hub/Editor/6000.3.21f1/Unity.app/Contents/MacOS/Unity \
  -batchmode -nographic \
  -projectPath clases/clase07/Unity \
  -runTests -testPlatform PlayMode \
  -testResults "$(pwd)/clases/clase07/Unity_playmode_results.xml" \
  -logFile "$(pwd)/clases/clase07/Unity_playmode.log"
echo "Exit code: $?"
```

Expected: both exit code `0`. Grep each results XML for `result="Failed"` — expect no matches.

```bash
grep -c 'result="Failed"' clases/clase07/Unity_editmode_results.xml clases/clase07/Unity_playmode_results.xml
```

Expected: `0` for both files. Delete the two `.xml`/`.log` files afterward (they are scratch output, not meant to be committed):

```bash
rm -f clases/clase07/Unity_editmode_results.xml clases/clase07/Unity_editmode.log \
      clases/clase07/Unity_playmode_results.xml clases/clase07/Unity_playmode.log
```

- [ ] **Step 4: Confirm no leftover references to deleted types**

```bash
cd clases/clase07/Unity
grep -rn "DemoUiFactory\|SceneScaffolding\|FsmWeaponDemoView\b" Assets --include="*.cs" --include="*.unity"
```

Expected: no output at all.

- [ ] **Step 5: Confirm the working tree is clean**

```bash
cd clases/clase07/Unity
git status
```

Expected: nothing to commit (every task committed its own changes; Steps 1-4 here produced no file changes other than the deleted scratch XML/log files, which were never tracked).

This is the final task — the migration is complete once Steps 1, 2, 3 and 5 all report clean.
