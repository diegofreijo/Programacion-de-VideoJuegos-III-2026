using System.Collections.Generic;
using System.IO;
using System.Linq;
using Clase14.Editor.Dialogue;
using Clase14.Editor.TurretCycle;
using Unity.GraphToolkit.Editor;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Clase14.Editor
{
    /// <summary>
    /// Genera los datos y escenas de ejemplo de la clase (enemigos, oleadas, diálogo y escenas).
    /// Se corre una vez desde el menú o en batch: los assets generados quedan versionados.
    /// </summary>
    public static class SampleContent
    {
        const string DataFolder = "Assets/Clase14/Data";
        const string WavesFolder = DataFolder + "/Oleadas";
        const string DialogueFolder = DataFolder + "/Dialogos";
        const string TurretCycleFolder = DataFolder + "/CiclosTorreta";
        const string ScenesFolder = "Assets/Clase14/Scenes";
        public const string MainScene = ScenesFolder + "/Main.unity";
        public const string ProfilerScene = ScenesFolder + "/Profiler.unity";
        public const string DialogueScene = ScenesFolder + "/Dialogo.unity";
        public const string TurretScene = ScenesFolder + "/Torreta.unity";
        public const string SampleTurretCycle = TurretCycleFolder + "/Sobrecalentamiento." + TurretCycleGraph.AssetExtension;
        public const string SampleDialogue = DialogueFolder + "/Guardia." + DialogueGraph.AssetExtension;

        [MenuItem("Clase14/Regenerar contenido de ejemplo", priority = 100)]
        public static void Generate()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Debug.LogWarning("Clase14: salir de Play Mode antes de regenerar el contenido.");
                return;
            }
            // Las escenas se recrean: antes, ofrecer guardar los cambios de la escena abierta.
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;
            foreach (var folder in new[] { BalanceWindow.EnemiesFolder, WavesFolder, DialogueFolder, TurretCycleFolder, ScenesFolder })
                Directory.CreateDirectory(folder);
            AssetDatabase.Refresh();

            var rata = Enemy("Rata", "Rata", new Color(0.6f, 0.5f, 0.4f), health: 4, speed: 4f, damage: 1, rate: 2f, reward: 1);
            var orco = Enemy("Orco", "Orco", new Color(0.3f, 0.7f, 0.3f), health: 16, speed: 2f, damage: 3, rate: 1f, reward: 3);
            var golem = Enemy("Golem", "Gólem", new Color(0.55f, 0.4f, 0.8f), health: 60, speed: 0.8f, damage: 8, rate: 0.5f, reward: 10);
            // Dos enemigos con datos inválidos a propósito, para ver las marcas en el Inspector y la ventana.
            var berserker = Enemy("Berserker", "Berserker", new Color(0.9f, 0.2f, 0.2f), health: 20, speed: 3f, damage: 6, rate: 4f, reward: 5);
            Enemy("Fantasma", "Fantasma", new Color(1f, 1f, 1f, 0.1f), health: 8, speed: 7f, damage: 2, rate: 1f, reward: 4);

            var wave1 = Wave("Oleada1", new FloatRange(0.6f, 1.2f), 8f, (rata, 8), (orco, 3));
            var wave2 = Wave("Oleada2", new FloatRange(0.4f, 1f), 9f, (rata, 10), (orco, 6), (golem, 2));
            Wave("Oleada3Jefe", new FloatRange(1f, 2f), 9f, (golem, 3), (berserker, 2));
            var giant = Wave("OleadaGigante", new FloatRange(0f, 0.01f), 12f, (rata, 400), (orco, 150), (golem, 50));
            // Larga y pareja, para que en la escena Torreta el ciclo dé varias vueltas.
            var longWave = Wave("OleadaLarga", new FloatRange(0.3f, 0.6f), 9f, (rata, 120), (orco, 20));

            CreateSampleDialogue();
            CreateSampleTurretCycle();
            AssetDatabase.SaveAssets();

            CreateScenes(wave1, wave2, giant, longWave);
            Debug.Log("Clase14: contenido de ejemplo generado.");
        }

        /// <summary>Recrea solo las escenas, con los datos que ya están en Data/ (no toca enemigos, oleadas ni diálogo).</summary>
        [MenuItem("Clase14/Regenerar escenas", priority = 101)]
        public static void GenerateScenes()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Debug.LogWarning("Clase14: salir de Play Mode antes de regenerar las escenas.");
                return;
            }
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;
            CreateScenes(
                AssetDatabase.LoadAssetAtPath<WaveDefinition>(WavesFolder + "/Oleada1.asset"),
                AssetDatabase.LoadAssetAtPath<WaveDefinition>(WavesFolder + "/Oleada2.asset"),
                AssetDatabase.LoadAssetAtPath<WaveDefinition>(WavesFolder + "/OleadaGigante.asset"),
                AssetDatabase.LoadAssetAtPath<WaveDefinition>(WavesFolder + "/OleadaLarga.asset"));
            Debug.Log("Clase14: escenas generadas.");
        }

        static void CreateScenes(WaveDefinition wave1, WaveDefinition wave2, WaveDefinition giant, WaveDefinition longWave)
        {
            CreateScene(MainScene, new List<WaveDefinition> { wave1, wave2 }, careless: false, withDialogue: true);
            CreateDialogueScene();
            CreateScene(ProfilerScene, new List<WaveDefinition> { giant }, careless: true, withDialogue: false);
            CreateTurretScene(longWave);
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(MainScene, true),
                new EditorBuildSettingsScene(DialogueScene, true),
                new EditorBuildSettingsScene(ProfilerScene, true),
                new EditorBuildSettingsScene(TurretScene, true),
            };
            AssetDatabase.SaveAssets();
            EditorSceneManager.OpenScene(MainScene);
        }

        static EnemyDefinition Enemy(string file, string displayName, Color color, float health, float speed, float damage, float rate, int reward)
        {
            var path = $"{BalanceWindow.EnemiesFolder}/{file}.asset";
            var enemy = AssetDatabase.LoadAssetAtPath<EnemyDefinition>(path);
            if (enemy == null)
            {
                enemy = ScriptableObject.CreateInstance<EnemyDefinition>();
                AssetDatabase.CreateAsset(enemy, path);
            }
            enemy.displayName = displayName;
            enemy.color = color;
            enemy.maxHealth = health;
            enemy.moveSpeed = speed;
            enemy.damage = damage;
            enemy.attacksPerSecond = rate;
            enemy.reward = reward;
            EditorUtility.SetDirty(enemy);
            return enemy;
        }

        static WaveDefinition Wave(string file, FloatRange interval, float radius, params (EnemyDefinition enemy, int count)[] entries)
        {
            var path = $"{WavesFolder}/{file}.asset";
            var wave = AssetDatabase.LoadAssetAtPath<WaveDefinition>(path);
            if (wave == null)
            {
                wave = ScriptableObject.CreateInstance<WaveDefinition>();
                AssetDatabase.CreateAsset(wave, path);
            }
            wave.entries = entries.Select(e => new WaveEntry { enemy = e.enemy, count = e.count }).ToList();
            wave.spawnInterval = interval;
            wave.spawnRadius = radius;
            EditorUtility.SetDirty(wave);
            return wave;
        }

        /// <summary>Arma el grafo de diálogo de ejemplo por código, como lo haría una herramienta.</summary>
        public static DialogueAsset CreateSampleDialogue()
        {
            AssetDatabase.DeleteAsset(SampleDialogue);
            var graph = GraphDatabase.CreateGraph<DialogueGraph>(SampleDialogue);
            BuildSampleGraph(graph);
            GraphDatabase.SaveGraph(graph);
            AssetDatabase.ImportAsset(SampleDialogue);
            return AssetDatabase.LoadAssetAtPath<DialogueAsset>(SampleDialogue);
        }

        /// <summary>Inicio, saludo, una pregunta con dos opciones, una respuesta por opción y Fin.</summary>
        public static void BuildSampleGraph(Graph graph)
        {
            graph.UndoBeginRecordGraph("Diálogo de ejemplo");

            var start = Add(graph, new StartNode(), 0, 120);
            var greet = Add(graph, new LineNode(), 260, 60);
            Set(greet, DialoguePorts.Speaker, "Guardia");
            Set(greet, DialoguePorts.Text, "¡Alto! ¿Quién anda ahí?");

            var ask = Add(graph, new ChoiceNode(), 600, 20);
            Set(ask, DialoguePorts.Speaker, "Guardia");
            Set(ask, DialoguePorts.Text, "¿Venís a defender la base?");
            Set(ask, DialoguePorts.ChoiceText(0), "Sí, traigo la torreta.");
            Set(ask, DialoguePorts.ChoiceText(1), "Solo estoy de paso.");

            var yes = Add(graph, new LineNode(), 980, -80);
            Set(yes, DialoguePorts.Speaker, "Guardia");
            Set(yes, DialoguePorts.Text, "Bienvenido. Llegan en dos oleadas.");
            var no = Add(graph, new LineNode(), 980, 220);
            Set(no, DialoguePorts.Speaker, "Guardia");
            Set(no, DialoguePorts.Text, "Entonces apurate: se vienen los orcos.");
            var end = Add(graph, new EndNode(), 1320, 100);

            Link(graph, start, DialoguePorts.Next, greet);
            Link(graph, greet, DialoguePorts.Next, ask);
            Link(graph, ask, DialoguePorts.ChoiceOut(0), yes);
            Link(graph, ask, DialoguePorts.ChoiceOut(1), no);
            Link(graph, yes, DialoguePorts.Next, end);
            Link(graph, no, DialoguePorts.Next, end);

            graph.UndoEndRecordGraph();
        }

        /// <summary>Arma el ciclo de torreta de ejemplo por código y lo importa.</summary>
        public static TurretCycleAsset CreateSampleTurretCycle()
        {
            AssetDatabase.DeleteAsset(SampleTurretCycle);
            var graph = GraphDatabase.CreateGraph<TurretCycleGraph>(SampleTurretCycle);
            BuildSampleTurretCycle(graph);
            GraphDatabase.SaveGraph(graph);
            AssetDatabase.ImportAsset(SampleTurretCycle);
            return AssetDatabase.LoadAssetAtPath<TurretCycleAsset>(SampleTurretCycle);
        }

        /// <summary>
        /// Activa → (Disparos ≥ 10) → Recalentada → (Pasaron 2 s) → Enfriando → (Pasaron 3 s o Sin
        /// enemigos) → Activa. Tiene un ciclo, y dos caminos que convergen en Activa.
        /// </summary>
        public static void BuildSampleTurretCycle(Graph graph)
        {
            graph.UndoBeginRecordGraph("Ciclo de torreta de ejemplo");

            var start = Add(graph, new CycleStartNode(), 0, 160);
            var active = State(graph, 260, 80, "Activa", canFire: true, rate: 1f, new Color(1f, 0.9f, 0.2f),
                (TurretCondition.ShotsFired, 10f));
            var hot = State(graph, 700, -120, "Recalentada", canFire: false, rate: 1f, new Color(0.9f, 0.15f, 0.1f),
                (TurretCondition.TimeElapsed, 2f));
            var cooling = State(graph, 700, 320, "Enfriando", canFire: true, rate: 0.5f, new Color(1f, 0.55f, 0.1f),
                (TurretCondition.TimeElapsed, 3f), (TurretCondition.NoEnemiesInRange, 0f));

            graph.Connect(start.GetOutputPortByName(TurretCyclePorts.Next), active.GetInputPortByName(TurretCyclePorts.In));
            LinkState(graph, active, 0, hot);
            LinkState(graph, hot, 0, cooling);
            LinkState(graph, cooling, 0, active);
            LinkState(graph, cooling, 1, active);

            graph.UndoEndRecordGraph();
        }

        static StateNode State(Graph graph, float x, float y, string name, bool canFire, float rate, Color color,
            params (TurretCondition condition, float value)[] transitions)
        {
            var node = Add(graph, new StateNode(), x, y);
            // Los puertos de transición dependen de esta opción: después de cambiarla se redefine el nodo.
            node.GetNodeOptionByName(TurretCyclePorts.TransitionCount).TrySetValue(transitions.Length);
            node.DefineNode();
            node.GetInputPortByName(TurretCyclePorts.Name).TrySetValue(name);
            node.GetInputPortByName(TurretCyclePorts.CanFire).TrySetValue(canFire);
            node.GetInputPortByName(TurretCyclePorts.FireRate).TrySetValue(rate);
            node.GetInputPortByName(TurretCyclePorts.Color).TrySetValue(color);
            for (var i = 0; i < transitions.Length; i++)
            {
                node.GetInputPortByName(TurretCyclePorts.Condition(i)).TrySetValue(transitions[i].condition);
                node.GetInputPortByName(TurretCyclePorts.Value(i)).TrySetValue(transitions[i].value);
            }
            return node;
        }

        static void LinkState(Graph graph, StateNode from, int transition, StateNode to) =>
            graph.Connect(from.GetOutputPortByName(TurretCyclePorts.TransitionOut(transition)), to.GetInputPortByName(TurretCyclePorts.In));

        static T Add<T>(Graph graph, T node, float x, float y) where T : Node
        {
            graph.AddNode(node);
            node.Position = new Vector2(x, y);
            return node;
        }

        static void Set(Node node, string port, string value) => node.GetInputPortByName(port).TrySetValue(value);

        static void Link(Graph graph, Node from, string output, Node to) =>
            graph.Connect(from.GetOutputPortByName(output), to.GetInputPortByName(DialoguePorts.In));

        static Camera CreateCamera(float size)
        {
            var camera = new GameObject("Main Camera").AddComponent<Camera>();
            camera.tag = "MainCamera";
            camera.orthographic = true;
            camera.orthographicSize = size;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.08f, 0.09f, 0.12f);
            camera.transform.position = new Vector3(0, 0, -10);
            return camera;
        }

        // El diálogo se carga después de NewScene: al cambiar de escena Unity descarga los sub-assets
        // importados que nadie usa, y una referencia tomada antes queda en null.
        static DialogueAsset LoadSampleDialogue() => AssetDatabase.LoadAssetAtPath<DialogueAsset>(SampleDialogue);

        static void CreateScene(string path, List<WaveDefinition> waves, bool careless, bool withDialogue)
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            CreateCamera(10f);

            var target = new GameObject("Objetivo").AddComponent<DefendedTarget>();
            var turret = new GameObject("Torreta").AddComponent<Turret>();
            turret.transform.position = Vector3.zero;

            var spawner = new GameObject("Oleadas").AddComponent<WaveSpawner>();
            spawner.target = target;
            spawner.waves = waves;
            spawner.carelessEnemies = careless;

            if (withDialogue)
                new GameObject("Diálogo").AddComponent<DialoguePlayer>().dialogue = LoadSampleDialogue();

            // Sin HUD en la escena del Profiler, para que su texto no se mezcle con la basura que se quiere mostrar.
            if (!careless)
            {
                var hud = new GameObject("HUD").AddComponent<Hud>();
                hud.spawner = spawner;
                hud.target = target;
            }

            EditorSceneManager.SaveScene(scene, path);
        }

        /// <summary>Una torreta manejada por el ciclo de sobrecalentamiento, contra una oleada larga.</summary>
        static void CreateTurretScene(WaveDefinition wave)
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            CreateCamera(8f);

            var target = new GameObject("Objetivo").AddComponent<DefendedTarget>();
            target.maxHealth = 300f;
            var turret = new GameObject("Torreta").AddComponent<Turret>();
            turret.transform.position = Vector3.zero;
            turret.transform.localScale = Vector3.one * 0.6f;
            // Con el namespace completo: dentro de Clase14.Editor, "TurretCycle" es el namespace del grafo.
            // El ciclo se carga después de NewScene, igual que el diálogo.
            turret.gameObject.AddComponent<Clase14.TurretCycle>().cycle = AssetDatabase.LoadAssetAtPath<TurretCycleAsset>(SampleTurretCycle);

            var spawner = new GameObject("Oleadas").AddComponent<WaveSpawner>();
            spawner.target = target;
            spawner.waves = new List<WaveDefinition> { wave };

            var hud = new GameObject("HUD").AddComponent<Hud>();
            hud.spawner = spawner;
            hud.target = target;

            EditorSceneManager.SaveScene(scene, TurretScene);
        }

        /// <summary>El jugador camina hasta el Guardia y conversa con él usando el diálogo del grafo.</summary>
        static void CreateDialogueScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var camera = CreateCamera(5f);

            var box = new GameObject("Caja de diálogo").AddComponent<DialogueBox>();
            box.transform.SetParent(camera.transform, false);
            box.transform.localPosition = new Vector3(0f, -3.1f, 10f);

            var player = new GameObject("Jugador").AddComponent<PlayerMover>();
            player.transform.position = new Vector3(-5f, 0.5f, 0f);

            var guard = new GameObject("Guardia").AddComponent<DialogueNpc>();
            guard.transform.position = new Vector3(3f, 1.5f, 0f);
            guard.dialogue = LoadSampleDialogue();
            guard.box = box;
            guard.player = player.transform;

            EditorSceneManager.SaveScene(scene, DialogueScene);
        }
    }
}
