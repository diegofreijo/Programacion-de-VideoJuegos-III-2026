# Clase07: implementaciones de patrones autocontenidas y portables — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Convertir cada una de las 13 implementaciones de patrón en `clases/clase07/Unity/Assets` (FSM, Dependency Injection, Message Broker, MVC/MVP/MVVM) en una carpeta autocontenida — su propio asmdef, su propia copia del dominio que hoy es código compartido del módulo, su propio prefijo de letra de orden de lectura — de forma que se pueda copiar cualquiera de esas carpetas a otro proyecto Unity y compile sola, salvo por los paquetes de terceros (VContainer, MessagePipe, MessagePipe.VContainer, UniTask, uGUI).

**Architecture:** Por cada módulo, se trabaja implementación por implementación: mover su carpeta y escena a un nombre con prefijo de letra (`a_`, `b_`, `c_`, ...), copiar (no referenciar) las clases que hoy viven en `Shared/`/`Core/` del módulo con el namespace fusionado al de esa implementación, crear un asmdef runtime + uno de test propios, mover o duplicar sus tests existentes (duplicando con valores hardcodeados los pocos casos que hoy comparan dos implementaciones en el mismo archivo), y agregar comentarios pedagógicos en el punto exacto que es la razón de mostrar esa variante. Recién cuando las implementaciones de un módulo ya no referencian su `Shared/`/`Core/` ni su asmdef de módulo, se borran esos remanentes. Al final se actualiza la documentación del proyecto (README.md, SPEC.md) con las rutas y la convención nuevas.

**Tech Stack:** Unity 6000.3.21f1, VContainer 1.19.0, MessagePipe 1.8.2 + MessagePipe.VContainer 1.8.2, UniTask 2.5.11, com.unity.ugui 2.0.0, Unity Test Framework (NUnit) vía `-runTests` batchmode. Sin cambios de versión de ningún paquete — este plan solo reorganiza código y assets existentes.

**Spec:** `docs/superpowers/specs/2026-09-22-clase07-self-contained-pattern-implementations-design.md`

## Global Constraints

- Todos los comandos de batchmode se corren desde la raíz del monorepo (`/Users/giga/code/Programacion-de-VideoJuegos-III-2026`), con `-projectPath clases/clase07/Unity`, usando el binario `/Applications/Unity/Hub/Editor/6000.3.21f1/Unity.app/Contents/MacOS/Unity`.
- **Nunca combinar `-quit` con `-runTests`** en la misma invocación de batchmode — compiten por cerrar el Editor y el run puede no llegar a ejecutarse (regla ya documentada en el README del proyecto).
- Las rutas pasadas a `-testResults`/`-logFile` van siempre como `"$(pwd)/clases/clase07/Unity_<algo>.xml"` — Unity las resuelve contra su propio `-projectPath` interno, no contra el directorio desde el que se invoca.
- Cada implementación termina con exactamente un asmdef runtime propio (más uno de test, y uno de test PlayMode donde la implementación tenga escena y ya lo tuviera hoy) — nunca comparte asmdef con otra implementación del mismo módulo.
- El namespace de cualquier clase copiada desde `Shared/`/`Core/` de un módulo se funde con el namespace propio de la implementación que la recibe — nunca queda un sub-namespace `.Shared`/`.Core` dentro de una carpeta de implementación.
- Convención de nombres: número de carpeta de módulo (`01_FSM`, `02_DependencyInjection`, ...) ordena entre patrones distintos y no cambia; letra de carpeta de implementación (`a_`, `b_`, `c_`, ...) ordena de más simple/ingenua a más sofisticada dentro del mismo patrón, y es nueva en este plan. El archivo `.unity` de cada implementación lleva el mismo prefijo de letra que su carpeta.
- Cada commit sigue la convención ya usada en el repo (español, imperativo, cuerpo corto si hace falta) y termina con:
  ```
  Co-Authored-By: Claude Sonnet 5 <noreply@anthropic.com>
  Claude-Session: https://claude.ai/code/session_01YGZHSkcGRaTFS7xCnNWa5E
  ```
- No se cambia el comportamiento observable de ninguna implementación ni se agregan escenas nuevas — `Large/b_ScriptableObjectStates` de FSM sigue sin escena (decisión de alcance ya existente).
- Solo se borra `Shared/`/`Core/` y el asmdef de módulo de un módulo cuando un `grep` confirma que ninguna implementación restante los referencia — nunca antes.

---
### Task 1: `01_FSM/Small/a_Baseline/` autocontenida

**Files:**
- Move: `clases/clase07/Unity/Assets/01_FSM/Small/Baseline/WeaponBaseline.cs` → `clases/clase07/Unity/Assets/01_FSM/Small/a_Baseline/WeaponBaseline.cs`
- Move: `clases/clase07/Unity/Assets/01_FSM/Demo/FsmWeaponBaselineDemoView.cs` → `clases/clase07/Unity/Assets/01_FSM/Small/a_Baseline/FsmWeaponBaselineDemoView.cs`
- Move: `clases/clase07/Unity/Assets/01_FSM/01_FSM_WeaponBaseline.unity` (+ `.meta`) → `clases/clase07/Unity/Assets/01_FSM/Small/a_Baseline/a_FSM_WeaponBaseline.unity`
- Create: `clases/clase07/Unity/Assets/01_FSM/Small/a_Baseline/Clase07.FSM.Small.Baseline.asmdef`
- Move: `clases/clase07/Unity/Assets/01_FSM/Tests/WeaponBaselineTests.cs` → `clases/clase07/Unity/Assets/01_FSM/Small/a_Baseline/Tests/WeaponBaselineTests.cs`
- Create: `clases/clase07/Unity/Assets/01_FSM/Small/a_Baseline/Tests/WeaponBaselineExpectedSequenceTests.cs`
- Create: `clases/clase07/Unity/Assets/01_FSM/Small/a_Baseline/Tests/Clase07.FSM.Small.Baseline.Tests.asmdef`
- Create: `clases/clase07/Unity/Assets/01_FSM/Small/a_Baseline/Tests/PlayMode/WeaponBaselineDemoViewPlayModeTests.cs`
- Create: `clases/clase07/Unity/Assets/01_FSM/Small/a_Baseline/Tests/PlayMode/Clase07.FSM.Small.Baseline.Tests.PlayMode.asmdef`

**Interfaces:**
- Produces: namespace `Clase07.FSM.Small.Baseline` — `WeaponState` (enum: `Idle`, `Firing`, `Reloading`), `WeaponBaseline` (`State`, `AmmoInMagazine`, `ShotsFired`, `PressTrigger()`, `Tick(float deltaTime)`, consts `FireDuration`/`ReloadDuration`/`MagazineSize`), `FsmWeaponBaselineDemoView : MonoBehaviour` (`OnFireClicked()`).
- No consume — esta implementación no depende de `Core/` ni de ninguna otra carpeta del módulo.

- [ ] **Step 1: Mover la carpeta de implementación con git**

```bash
cd /Users/giga/code/Programacion-de-VideoJuegos-III-2026
git mv clases/clase07/Unity/Assets/01_FSM/Small/Baseline clases/clase07/Unity/Assets/01_FSM/Small/a_Baseline
git mv clases/clase07/Unity/Assets/01_FSM/Demo/FsmWeaponBaselineDemoView.cs clases/clase07/Unity/Assets/01_FSM/Small/a_Baseline/FsmWeaponBaselineDemoView.cs
git mv clases/clase07/Unity/Assets/01_FSM/Demo/FsmWeaponBaselineDemoView.cs.meta clases/clase07/Unity/Assets/01_FSM/Small/a_Baseline/FsmWeaponBaselineDemoView.cs.meta
git mv clases/clase07/Unity/Assets/01_FSM/01_FSM_WeaponBaseline.unity clases/clase07/Unity/Assets/01_FSM/Small/a_Baseline/a_FSM_WeaponBaseline.unity
git mv clases/clase07/Unity/Assets/01_FSM/01_FSM_WeaponBaseline.unity.meta clases/clase07/Unity/Assets/01_FSM/Small/a_Baseline/a_FSM_WeaponBaseline.unity.meta
```

- [ ] **Step 2: Crear el asmdef runtime**

`clases/clase07/Unity/Assets/01_FSM/Small/a_Baseline/Clase07.FSM.Small.Baseline.asmdef`:

```json
{
    "name": "Clase07.FSM.Small.Baseline",
    "rootNamespace": "",
    "references": [
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

- [ ] **Step 3: Agregar el comentario pedagógico en el switch**

En `WeaponBaseline.cs`, el método `Tick` es el punto exacto que hay que mirar (el switch que crece con cada estado nuevo). El comentario de clase ya explica la idea general; agregale una línea puntual arriba del switch señalando el caso concreto que hay que imaginar creciendo:

```csharp
        public void Tick(float deltaTime)
        {
            // Cada estado nuevo (ej. "Aiming", "Overheated") agrega un case acá
            // y potencialmente en PressTrigger — nada obliga a cubrir todos los
            // casos, a diferencia de un objeto de estado que implementa su propia
            // interfaz.
            switch (State)
            {
```

- [ ] **Step 4: Mover los tests existentes**

```bash
git mv clases/clase07/Unity/Assets/01_FSM/Tests/WeaponBaselineTests.cs clases/clase07/Unity/Assets/01_FSM/Small/a_Baseline/Tests/WeaponBaselineTests.cs
git mv clases/clase07/Unity/Assets/01_FSM/Tests/WeaponBaselineTests.cs.meta clases/clase07/Unity/Assets/01_FSM/Small/a_Baseline/Tests/WeaponBaselineTests.cs.meta
```

El namespace del archivo movido (`Clase07.FSM.Tests`) y su contenido no cambian.

- [ ] **Step 5: Extraer la mitad de `WeaponEquivalenceTests` que ejercita `WeaponBaseline`**

Crear `clases/clase07/Unity/Assets/01_FSM/Small/a_Baseline/Tests/WeaponBaselineExpectedSequenceTests.cs`. Corre la misma secuencia de 7 disparos que hoy corre `WeaponEquivalenceTests.BothImplementations_MatchAfterFiringUntilEmptyAndReloading`, pero contra valores esperados hardcodeados (derivados de la lógica real: `MagazineSize=6`, cada disparo consume una bala hasta llegar a 0, el 7º intento sin balas pasa a `Reloading`, y tras suficiente tiempo total vuelve a `Idle` con el cargador lleno) en vez de comparar con `WeaponStatePatternController`:

```csharp
using NUnit.Framework;

namespace Clase07.FSM.Tests
{
    // Corre la misma secuencia de inputs que WeaponStatePatternExpectedSequenceTests
    // en Small/b_StatePattern/Tests/ — a propósito NO referencia ese tipo: cada
    // implementación se verifica contra los mismos valores esperados hardcodeados
    // en vez de compararse en runtime, para que esta carpeta compile sola.
    public class WeaponBaselineExpectedSequenceTests
    {
        [Test]
        public void FiringUntilEmptyThenReloading_MatchesExpectedAmmoAndStateSequence()
        {
            var weapon = new WeaponBaseline();
            var expectedAmmoAfterShot = new[] { 5, 4, 3, 2, 1, 0 };

            for (var shot = 0; shot < WeaponBaseline.MagazineSize; shot++)
            {
                weapon.PressTrigger();

                Assert.AreEqual(WeaponState.Firing, weapon.State);
                Assert.AreEqual(expectedAmmoAfterShot[shot], weapon.AmmoInMagazine);

                weapon.Tick(WeaponBaseline.FireDuration + 0.01f);
                Assert.AreEqual(WeaponState.Idle, weapon.State);
            }

            weapon.PressTrigger();
            Assert.AreEqual(WeaponState.Reloading, weapon.State);
            Assert.AreEqual(0, weapon.AmmoInMagazine);

            weapon.Tick(WeaponBaseline.ReloadDuration + 0.01f);
            Assert.AreEqual(WeaponState.Idle, weapon.State);
            Assert.AreEqual(WeaponBaseline.MagazineSize, weapon.AmmoInMagazine);
        }
    }
}
```

- [ ] **Step 6: Crear el asmdef de tests EditMode**

`clases/clase07/Unity/Assets/01_FSM/Small/a_Baseline/Tests/Clase07.FSM.Small.Baseline.Tests.asmdef`:

```json
{
    "name": "Clase07.FSM.Small.Baseline.Tests",
    "rootNamespace": "",
    "references": [
        "Clase07.FSM.Small.Baseline",
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

- [ ] **Step 7: Extraer el test PlayMode de esta escena**

Crear `clases/clase07/Unity/Assets/01_FSM/Small/a_Baseline/Tests/PlayMode/WeaponBaselineDemoViewPlayModeTests.cs` con solo el caso de esta escena (hoy vive junto a los otros dos en `FsmDemoViewsPlayModeTests.cs`):

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
    public class WeaponBaselineDemoViewPlayModeTests
    {
        [UnityTest]
        public IEnumerator WeaponBaselineScene_LoadsAndRespondsToButtons()
        {
            SceneManager.LoadScene("Assets/01_FSM/Small/a_Baseline/a_FSM_WeaponBaseline.unity", LoadSceneMode.Single);
            yield return null;

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

- [ ] **Step 8: Crear el asmdef de tests PlayMode**

`clases/clase07/Unity/Assets/01_FSM/Small/a_Baseline/Tests/PlayMode/Clase07.FSM.Small.Baseline.Tests.PlayMode.asmdef`:

```json
{
    "name": "Clase07.FSM.Small.Baseline.Tests.PlayMode",
    "rootNamespace": "",
    "references": [
        "Clase07.FSM.Small.Baseline",
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

- [ ] **Step 9: Verificar EditMode**

Correr desde la raíz del monorepo (`/Users/giga/code/Programacion-de-VideoJuegos-III-2026`):

```bash
/Applications/Unity/Hub/Editor/6000.3.21f1/Unity.app/Contents/MacOS/Unity \
  -batchmode -nographic \
  -projectPath clases/clase07/Unity \
  -runTests -testPlatform EditMode \
  -testResults "$(pwd)/clases/clase07/Unity_fsm_a_baseline_editmode.xml" \
  -logFile "$(pwd)/clases/clase07/Unity_fsm_a_baseline_editmode.log"
```

Run: `grep -c 'result="Failed"' clases/clase07/Unity_fsm_a_baseline_editmode.xml`
Expected: `0`. También confirmar `grep -i "error CS" clases/clase07/Unity_fsm_a_baseline_editmode.log` sin resultados.

- [ ] **Step 10: Verificar PlayMode**

```bash
/Applications/Unity/Hub/Editor/6000.3.21f1/Unity.app/Contents/MacOS/Unity \
  -batchmode -nographic \
  -projectPath clases/clase07/Unity \
  -runTests -testPlatform PlayMode \
  -testResults "$(pwd)/clases/clase07/Unity_fsm_a_baseline_playmode.xml" \
  -logFile "$(pwd)/clases/clase07/Unity_fsm_a_baseline_playmode.log"
```

Run: `grep -c 'result="Failed"' clases/clase07/Unity_fsm_a_baseline_playmode.xml`
Expected: `0`.

- [ ] **Step 11: Limpiar logs y commitear**

```bash
rm clases/clase07/Unity_fsm_a_baseline_editmode.xml clases/clase07/Unity_fsm_a_baseline_editmode.log \
   clases/clase07/Unity_fsm_a_baseline_playmode.xml clases/clase07/Unity_fsm_a_baseline_playmode.log
git add clases/clase07/Unity/Assets/01_FSM
git commit -m "$(cat <<'EOF'
clase07/FSM: a_Baseline autocontenida (asmdef propio, sin depender de Core/)

Co-Authored-By: Claude Sonnet 5 <noreply@anthropic.com>
Claude-Session: https://claude.ai/code/session_01YGZHSkcGRaTFS7xCnNWa5E
EOF
)"
```

---

### Task 2: `01_FSM/Small/b_StatePattern/` autocontenida

**Files:**
- Move: `clases/clase07/Unity/Assets/01_FSM/Small/StatePattern/{WeaponContext.cs, WeaponStatePatternController.cs, WeaponIdleState.cs, WeaponFiringState.cs, WeaponReloadingState.cs}` → `clases/clase07/Unity/Assets/01_FSM/Small/b_StatePattern/` (mismos nombres)
- Move: `clases/clase07/Unity/Assets/01_FSM/Demo/FsmWeaponStatePatternDemoView.cs` → `clases/clase07/Unity/Assets/01_FSM/Small/b_StatePattern/FsmWeaponStatePatternDemoView.cs`
- Move: `clases/clase07/Unity/Assets/01_FSM/02_FSM_WeaponStatePattern.unity` (+ `.meta`) → `clases/clase07/Unity/Assets/01_FSM/Small/b_StatePattern/b_FSM_WeaponStatePattern.unity`
- Create: `clases/clase07/Unity/Assets/01_FSM/Small/b_StatePattern/IState.cs` (copia de `Core/IState.cs`, namespace fusionado)
- Create: `clases/clase07/Unity/Assets/01_FSM/Small/b_StatePattern/StateMachine.cs` (copia de `Core/StateMachine.cs`, namespace fusionado)
- Create: `clases/clase07/Unity/Assets/01_FSM/Small/b_StatePattern/Clase07.FSM.Small.StatePattern.asmdef`
- Create: `clases/clase07/Unity/Assets/01_FSM/Small/b_StatePattern/Tests/WeaponStatePatternExpectedSequenceTests.cs`
- Create: `clases/clase07/Unity/Assets/01_FSM/Small/b_StatePattern/Tests/StateMachineTests.cs` (copia de `01_FSM/Tests/StateMachineTests.cs`, contra esta copia local del motor)
- Create: `clases/clase07/Unity/Assets/01_FSM/Small/b_StatePattern/Tests/Clase07.FSM.Small.StatePattern.Tests.asmdef`
- Create: `clases/clase07/Unity/Assets/01_FSM/Small/b_StatePattern/Tests/PlayMode/WeaponStatePatternDemoViewPlayModeTests.cs`
- Create: `clases/clase07/Unity/Assets/01_FSM/Small/b_StatePattern/Tests/PlayMode/Clase07.FSM.Small.StatePattern.Tests.PlayMode.asmdef`

**Interfaces:**
- Produces: namespace `Clase07.FSM.Small.StatePattern` — `IState` (`OnEnter()`, `OnUpdate(float)`, `OnExit()`), `StateMachine<TState>` (`CurrentState`, `ChangeState(TState)`, `Tick(float)`, evento `StateChanged`), `WeaponContext`, `WeaponStatePatternController` (`Context`, `CurrentState`, `IdleState`/`FiringState`/`ReloadingState`, `PressTrigger()`, `Tick(float)`), `FsmWeaponStatePatternDemoView : MonoBehaviour` (`OnFireClicked()`).
- No consume — copia propia de `IState`/`StateMachine<TState>`, no referencia `Clase07.FSM.Core`.

- [ ] **Step 1: Mover archivos con git**

```bash
cd /Users/giga/code/Programacion-de-VideoJuegos-III-2026
git mv clases/clase07/Unity/Assets/01_FSM/Small/StatePattern clases/clase07/Unity/Assets/01_FSM/Small/b_StatePattern
git mv clases/clase07/Unity/Assets/01_FSM/Demo/FsmWeaponStatePatternDemoView.cs clases/clase07/Unity/Assets/01_FSM/Small/b_StatePattern/FsmWeaponStatePatternDemoView.cs
git mv clases/clase07/Unity/Assets/01_FSM/Demo/FsmWeaponStatePatternDemoView.cs.meta clases/clase07/Unity/Assets/01_FSM/Small/b_StatePattern/FsmWeaponStatePatternDemoView.cs.meta
git mv clases/clase07/Unity/Assets/01_FSM/02_FSM_WeaponStatePattern.unity clases/clase07/Unity/Assets/01_FSM/Small/b_StatePattern/b_FSM_WeaponStatePattern.unity
git mv clases/clase07/Unity/Assets/01_FSM/02_FSM_WeaponStatePattern.unity.meta clases/clase07/Unity/Assets/01_FSM/Small/b_StatePattern/b_FSM_WeaponStatePattern.unity.meta
```

- [ ] **Step 2: Copiar `IState`/`StateMachine` con namespace fusionado**

`clases/clase07/Unity/Assets/01_FSM/Small/b_StatePattern/IState.cs`:

```csharp
namespace Clase07.FSM.Small.StatePattern
{
    public interface IState
    {
        void OnEnter();
        void OnUpdate(float deltaTime);
        void OnExit();
    }
}
```

`clases/clase07/Unity/Assets/01_FSM/Small/b_StatePattern/StateMachine.cs`:

```csharp
using System;

namespace Clase07.FSM.Small.StatePattern
{
    // Copia local del motor genérico — esta carpeta es autocontenida a propósito,
    // ver Large/a_StatePattern/StateMachine.cs para la otra copia independiente.
    public class StateMachine<TState> where TState : class, IState
    {
        public TState CurrentState { get; private set; }
        public event Action<TState, TState> StateChanged;

        public StateMachine(TState initialState)
        {
            CurrentState = initialState ?? throw new ArgumentNullException(nameof(initialState));
            CurrentState.OnEnter();
        }

        public void ChangeState(TState next)
        {
            if (next == null) throw new ArgumentNullException(nameof(next));
            if (ReferenceEquals(next, CurrentState)) return;

            var previous = CurrentState;
            previous.OnExit();
            CurrentState = next;
            CurrentState.OnEnter();
            StateChanged?.Invoke(previous, CurrentState);
        }

        public void Tick(float deltaTime) => CurrentState.OnUpdate(deltaTime);
    }
}
```

- [ ] **Step 3: Quitar los `using Clase07.FSM.Core;` que ya no hacen falta**

En `WeaponStatePatternController.cs`, `WeaponIdleState.cs`, `WeaponFiringState.cs`, `WeaponReloadingState.cs`: borrar la línea `using Clase07.FSM.Core;` de cada uno (queda vacía esa línea, `IState`/`StateMachine<TState>` ya están en el mismo namespace `Clase07.FSM.Small.StatePattern`).

- [ ] **Step 4: Agregar el comentario pedagógico en `ChangeState(...)`**

En `WeaponIdleState.cs`, arriba de la llamada a `_controller.ChangeState(...)` dentro de `OnUpdate`:

```csharp
        public void OnUpdate(float deltaTime)
        {
            if (!_context.TriggerPressedThisFrame) return;
            _context.TriggerPressedThisFrame = false;

            // La transición vive acá, en el estado que la dispara — no hay un
            // switch central que decida "si estoy en Idle y aprietan, ¿a dónde
            // voy?": cada IState sabe a qué otro estado puede pasar.
            _controller.ChangeState(_context.AmmoInMagazine > 0
                ? _controller.FiringState
                : _controller.ReloadingState);
        }
```

En `WeaponFiringState.cs` y `WeaponReloadingState.cs`, agregar una línea corta arriba de su propio `_controller.ChangeState(_controller.IdleState);`:

```csharp
            // Mismo mecanismo: este estado decide sólo su propia salida.
            _controller.ChangeState(_controller.IdleState);
```

- [ ] **Step 5: Crear el asmdef runtime**

`clases/clase07/Unity/Assets/01_FSM/Small/b_StatePattern/Clase07.FSM.Small.StatePattern.asmdef`:

```json
{
    "name": "Clase07.FSM.Small.StatePattern",
    "rootNamespace": "",
    "references": [
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

- [ ] **Step 6: Test de secuencia esperada (mitad de `WeaponEquivalenceTests` para esta implementación)**

Crear `clases/clase07/Unity/Assets/01_FSM/Small/b_StatePattern/Tests/WeaponStatePatternExpectedSequenceTests.cs`. Mismos valores esperados que `WeaponBaselineExpectedSequenceTests` (Task 1, Step 5), pero conducido a través de `WeaponStatePatternController` — a propósito no importa `Clase07.FSM.Small.Baseline`:

```csharp
using NUnit.Framework;

namespace Clase07.FSM.Tests
{
    public class WeaponStatePatternExpectedSequenceTests
    {
        private static string StateName(WeaponStatePatternController c)
        {
            if (c.CurrentState == c.IdleState) return "Idle";
            if (c.CurrentState == c.FiringState) return "Firing";
            return "Reloading";
        }

        [Test]
        public void FiringUntilEmptyThenReloading_MatchesExpectedAmmoAndStateSequence()
        {
            var weapon = new WeaponStatePatternController();
            var expectedAmmoAfterShot = new[] { 5, 4, 3, 2, 1, 0 };

            for (var shot = 0; shot < WeaponContext.MagazineSize; shot++)
            {
                weapon.PressTrigger();
                weapon.Tick(0f);

                Assert.AreEqual("Firing", StateName(weapon));
                Assert.AreEqual(expectedAmmoAfterShot[shot], weapon.Context.AmmoInMagazine);

                weapon.Tick(WeaponContext.FireDuration + 0.01f);
                Assert.AreEqual("Idle", StateName(weapon));
            }

            weapon.PressTrigger();
            weapon.Tick(0f);
            Assert.AreEqual("Reloading", StateName(weapon));
            Assert.AreEqual(0, weapon.Context.AmmoInMagazine);

            weapon.Tick(WeaponContext.ReloadDuration + 0.01f);
            Assert.AreEqual("Idle", StateName(weapon));
            Assert.AreEqual(WeaponContext.MagazineSize, weapon.Context.AmmoInMagazine);
        }
    }
}
```

- [ ] **Step 7: Duplicar `StateMachineTests.cs` contra la copia local del motor genérico**

`StateMachineTests.cs` (hoy en `01_FSM/Tests/`) testea `Clase07.FSM.Core.StateMachine<TState>` con un `RecordingState` de prueba — ya es autocontenido (no referencia ninguna otra implementación), pero como el motor ahora tiene una copia independiente en esta carpeta (Step 2) y otra en `Large/a_StatePattern/` (Task 3), hace falta una copia del test por cada copia del motor, o esta cobertura desaparece cuando `01_FSM/Tests/` se borre en la Task 5.

Crear `clases/clase07/Unity/Assets/01_FSM/Small/b_StatePattern/Tests/StateMachineTests.cs` (mismo contenido que el original, namespace ajustado a `Clase07.FSM.Small.StatePattern.Tests`, sin el `using Clase07.FSM.Core;` ya que `IState`/`StateMachine<TState>` viven en `Clase07.FSM.Small.StatePattern`):

```csharp
using System.Collections.Generic;
using NUnit.Framework;
using Clase07.FSM.Small.StatePattern;

namespace Clase07.FSM.Small.StatePattern.Tests
{
    public class StateMachineTests
    {
        private class RecordingState : IState
        {
            public readonly List<string> Calls = new List<string>();
            private readonly string _name;
            public RecordingState(string name) => _name = name;
            public void OnEnter() => Calls.Add($"{_name}:Enter");
            public void OnUpdate(float deltaTime) => Calls.Add($"{_name}:Update");
            public void OnExit() => Calls.Add($"{_name}:Exit");
        }

        [Test]
        public void Constructor_CallsOnEnterOnInitialState()
        {
            var a = new RecordingState("A");
            new StateMachine<IState>(a);
            CollectionAssert.AreEqual(new[] { "A:Enter" }, a.Calls);
        }

        [Test]
        public void ChangeState_ExitsPreviousThenEntersNext()
        {
            var a = new RecordingState("A");
            var b = new RecordingState("B");
            var machine = new StateMachine<IState>(a);

            machine.ChangeState(b);

            CollectionAssert.AreEqual(new[] { "A:Enter", "A:Exit" }, a.Calls);
            CollectionAssert.AreEqual(new[] { "B:Enter" }, b.Calls);
            Assert.AreSame(b, machine.CurrentState);
        }

        [Test]
        public void ChangeState_ToSameState_IsNoOp()
        {
            var a = new RecordingState("A");
            var machine = new StateMachine<IState>(a);

            machine.ChangeState(a);

            CollectionAssert.AreEqual(new[] { "A:Enter" }, a.Calls);
        }

        [Test]
        public void Tick_CallsOnUpdateOnCurrentState()
        {
            var a = new RecordingState("A");
            var machine = new StateMachine<IState>(a);

            machine.Tick(0.016f);

            CollectionAssert.AreEqual(new[] { "A:Enter", "A:Update" }, a.Calls);
        }

        [Test]
        public void StateChanged_FiresWithPreviousAndNext()
        {
            var a = new RecordingState("A");
            var b = new RecordingState("B");
            var machine = new StateMachine<IState>(a);
            IState firedPrevious = null, firedNext = null;
            machine.StateChanged += (prev, next) => { firedPrevious = prev; firedNext = next; };

            machine.ChangeState(b);

            Assert.AreSame(a, firedPrevious);
            Assert.AreSame(b, firedNext);
        }
    }
}
```

- [ ] **Step 8: Crear el asmdef de tests EditMode**

`clases/clase07/Unity/Assets/01_FSM/Small/b_StatePattern/Tests/Clase07.FSM.Small.StatePattern.Tests.asmdef`:

```json
{
    "name": "Clase07.FSM.Small.StatePattern.Tests",
    "rootNamespace": "",
    "references": [
        "Clase07.FSM.Small.StatePattern",
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

- [ ] **Step 9: Extraer el test PlayMode de esta escena**

Crear `clases/clase07/Unity/Assets/01_FSM/Small/b_StatePattern/Tests/PlayMode/WeaponStatePatternDemoViewPlayModeTests.cs`:

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
    public class WeaponStatePatternDemoViewPlayModeTests
    {
        [UnityTest]
        public IEnumerator WeaponStatePatternScene_LoadsAndRespondsToButtons()
        {
            SceneManager.LoadScene("Assets/01_FSM/Small/b_StatePattern/b_FSM_WeaponStatePattern.unity", LoadSceneMode.Single);
            yield return null;

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

- [ ] **Step 10: Crear el asmdef de tests PlayMode**

`clases/clase07/Unity/Assets/01_FSM/Small/b_StatePattern/Tests/PlayMode/Clase07.FSM.Small.StatePattern.Tests.PlayMode.asmdef`:

```json
{
    "name": "Clase07.FSM.Small.StatePattern.Tests.PlayMode",
    "rootNamespace": "",
    "references": [
        "Clase07.FSM.Small.StatePattern",
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

- [ ] **Step 11: Verificar EditMode**

```bash
/Applications/Unity/Hub/Editor/6000.3.21f1/Unity.app/Contents/MacOS/Unity \
  -batchmode -nographic \
  -projectPath clases/clase07/Unity \
  -runTests -testPlatform EditMode \
  -testResults "$(pwd)/clases/clase07/Unity_fsm_b_statepattern_editmode.xml" \
  -logFile "$(pwd)/clases/clase07/Unity_fsm_b_statepattern_editmode.log"
```

Run: `grep -c 'result="Failed"' clases/clase07/Unity_fsm_b_statepattern_editmode.xml`
Expected: `0`.

- [ ] **Step 12: Verificar PlayMode**

```bash
/Applications/Unity/Hub/Editor/6000.3.21f1/Unity.app/Contents/MacOS/Unity \
  -batchmode -nographic \
  -projectPath clases/clase07/Unity \
  -runTests -testPlatform PlayMode \
  -testResults "$(pwd)/clases/clase07/Unity_fsm_b_statepattern_playmode.xml" \
  -logFile "$(pwd)/clases/clase07/Unity_fsm_b_statepattern_playmode.log"
```

Run: `grep -c 'result="Failed"' clases/clase07/Unity_fsm_b_statepattern_playmode.xml`
Expected: `0`.

- [ ] **Step 13: Limpiar logs y commitear**

```bash
rm clases/clase07/Unity_fsm_b_statepattern_editmode.xml clases/clase07/Unity_fsm_b_statepattern_editmode.log \
   clases/clase07/Unity_fsm_b_statepattern_playmode.xml clases/clase07/Unity_fsm_b_statepattern_playmode.log
git add clases/clase07/Unity/Assets/01_FSM
git commit -m "$(cat <<'EOF'
clase07/FSM: b_StatePattern autocontenida (copia propia de IState/StateMachine)

Co-Authored-By: Claude Sonnet 5 <noreply@anthropic.com>
Claude-Session: https://claude.ai/code/session_01YGZHSkcGRaTFS7xCnNWa5E
EOF
)"
```

---

### Task 3: `01_FSM/Large/a_StatePattern/` autocontenida

**Files:**
- Move: `clases/clase07/Unity/Assets/01_FSM/Large/StatePattern/{GameFlowController.cs, IGameFlowState.cs, LoadingState.cs, MainMenuState.cs, PlayingState.cs, UserPlayingState.cs, PauseMenuState.cs, SettingsMenuState.cs}` → `clases/clase07/Unity/Assets/01_FSM/Large/a_StatePattern/` (mismos nombres)
- Move: `clases/clase07/Unity/Assets/01_FSM/Demo/FsmGameFlowDemoView.cs` → `clases/clase07/Unity/Assets/01_FSM/Large/a_StatePattern/FsmGameFlowDemoView.cs`
- Move: `clases/clase07/Unity/Assets/01_FSM/03_FSM_GameFlow.unity` (+ `.meta`) → `clases/clase07/Unity/Assets/01_FSM/Large/a_StatePattern/a_FSM_GameFlow.unity`
- Create: `clases/clase07/Unity/Assets/01_FSM/Large/a_StatePattern/IState.cs` (copia de `Core/IState.cs`, namespace fusionado)
- Create: `clases/clase07/Unity/Assets/01_FSM/Large/a_StatePattern/StateMachine.cs` (copia de `Core/StateMachine.cs`, namespace fusionado)
- Create: `clases/clase07/Unity/Assets/01_FSM/Large/a_StatePattern/Clase07.FSM.Large.StatePattern.asmdef`
- Move: `clases/clase07/Unity/Assets/01_FSM/Tests/GameFlowControllerTests.cs` → `clases/clase07/Unity/Assets/01_FSM/Large/a_StatePattern/Tests/GameFlowControllerTests.cs`
- Create: `clases/clase07/Unity/Assets/01_FSM/Large/a_StatePattern/Tests/StateMachineTests.cs` (copia de `01_FSM/Tests/StateMachineTests.cs`, contra esta copia local del motor)
- Create: `clases/clase07/Unity/Assets/01_FSM/Large/a_StatePattern/Tests/Clase07.FSM.Large.StatePattern.Tests.asmdef`
- Create: `clases/clase07/Unity/Assets/01_FSM/Large/a_StatePattern/Tests/PlayMode/GameFlowDemoViewPlayModeTests.cs`
- Create: `clases/clase07/Unity/Assets/01_FSM/Large/a_StatePattern/Tests/PlayMode/Clase07.FSM.Large.StatePattern.Tests.PlayMode.asmdef`

**Interfaces:**
- Produces: namespace `Clase07.FSM.Large.StatePattern` — `IState`, `StateMachine<TState>`, `IGameFlowState : IState` (`Name`), `GameFlowController` (`CurrentState`, `CurrentSubstateName`, `Load()`, `FinishLoading()`, `Play()`, `Pause()`, `Resume()`, `OpenSettings()`, `CloseSettings()`), `FsmGameFlowDemoView : MonoBehaviour`.
- No consume — copia propia de `IState`/`StateMachine<TState>`, independiente de la de `Small/b_StatePattern/`.

- [ ] **Step 1: Mover archivos con git**

```bash
cd /Users/giga/code/Programacion-de-VideoJuegos-III-2026
git mv clases/clase07/Unity/Assets/01_FSM/Large/StatePattern clases/clase07/Unity/Assets/01_FSM/Large/a_StatePattern
git mv clases/clase07/Unity/Assets/01_FSM/Demo/FsmGameFlowDemoView.cs clases/clase07/Unity/Assets/01_FSM/Large/a_StatePattern/FsmGameFlowDemoView.cs
git mv clases/clase07/Unity/Assets/01_FSM/Demo/FsmGameFlowDemoView.cs.meta clases/clase07/Unity/Assets/01_FSM/Large/a_StatePattern/FsmGameFlowDemoView.cs.meta
git mv clases/clase07/Unity/Assets/01_FSM/03_FSM_GameFlow.unity clases/clase07/Unity/Assets/01_FSM/Large/a_StatePattern/a_FSM_GameFlow.unity
git mv clases/clase07/Unity/Assets/01_FSM/03_FSM_GameFlow.unity.meta clases/clase07/Unity/Assets/01_FSM/Large/a_StatePattern/a_FSM_GameFlow.unity.meta
```

- [ ] **Step 2: Copiar `IState`/`StateMachine` con namespace fusionado**

`clases/clase07/Unity/Assets/01_FSM/Large/a_StatePattern/IState.cs`:

```csharp
namespace Clase07.FSM.Large.StatePattern
{
    public interface IState
    {
        void OnEnter();
        void OnUpdate(float deltaTime);
        void OnExit();
    }
}
```

`clases/clase07/Unity/Assets/01_FSM/Large/a_StatePattern/StateMachine.cs`:

```csharp
using System;

namespace Clase07.FSM.Large.StatePattern
{
    // Copia local del motor genérico — independiente de la copia en
    // Small/b_StatePattern/StateMachine.cs, a propósito.
    public class StateMachine<TState> where TState : class, IState
    {
        public TState CurrentState { get; private set; }
        public event Action<TState, TState> StateChanged;

        public StateMachine(TState initialState)
        {
            CurrentState = initialState ?? throw new ArgumentNullException(nameof(initialState));
            CurrentState.OnEnter();
        }

        public void ChangeState(TState next)
        {
            if (next == null) throw new ArgumentNullException(nameof(next));
            if (ReferenceEquals(next, CurrentState)) return;

            var previous = CurrentState;
            previous.OnExit();
            CurrentState = next;
            CurrentState.OnEnter();
            StateChanged?.Invoke(previous, CurrentState);
        }

        public void Tick(float deltaTime) => CurrentState.OnUpdate(deltaTime);
    }
}
```

- [ ] **Step 3: Quitar los `using Clase07.FSM.Core;` que ya no hacen falta**

En `GameFlowController.cs`, `IGameFlowState.cs` y `PlayingState.cs`: borrar la línea `using Clase07.FSM.Core;`.

- [ ] **Step 4: Agregar el comentario pedagógico en `_substateMachine`**

En `PlayingState.cs`, arriba del campo:

```csharp
        // Una StateMachine adentro de un estado: Playing es, para la máquina de
        // arriba, un único estado — pero puertas adentro tiene su propia
        // sub-máquina (UserPlaying → PauseMenu → SettingsMenu) que arranca en
        // OnEnter() y se tickea desde OnUpdate(). Los guards "!= null" de Pause/
        // Resume/OpenSettings/CloseSettings existen porque GameFlowController
        // expone esos métodos incondicionalmente y una escena con botones
        // independientes los puede disparar antes de que Play() haya creado esta
        // sub-máquina.
        private StateMachine<IGameFlowState> _substateMachine;
```

- [ ] **Step 5: Crear el asmdef runtime**

`clases/clase07/Unity/Assets/01_FSM/Large/a_StatePattern/Clase07.FSM.Large.StatePattern.asmdef`:

```json
{
    "name": "Clase07.FSM.Large.StatePattern",
    "rootNamespace": "",
    "references": [
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

- [ ] **Step 6: Mover `GameFlowControllerTests.cs`**

```bash
git mv clases/clase07/Unity/Assets/01_FSM/Tests/GameFlowControllerTests.cs clases/clase07/Unity/Assets/01_FSM/Large/a_StatePattern/Tests/GameFlowControllerTests.cs
git mv clases/clase07/Unity/Assets/01_FSM/Tests/GameFlowControllerTests.cs.meta clases/clase07/Unity/Assets/01_FSM/Large/a_StatePattern/Tests/GameFlowControllerTests.cs.meta
```

Ya es autocontenido (solo referencia `Clase07.FSM.Large.StatePattern`) — no requiere cambios de contenido.

- [ ] **Step 7: Duplicar `StateMachineTests.cs` contra la copia local del motor genérico**

Mismo motivo que en Task 2, Step 7: el motor genérico ahora tiene una copia independiente en esta carpeta (Step 2), así que necesita su propia copia del test o la cobertura de `StateMachine<TState>` desaparece cuando `01_FSM/Tests/` se borre en la Task 5.

Crear `clases/clase07/Unity/Assets/01_FSM/Large/a_StatePattern/Tests/StateMachineTests.cs` (mismo contenido que el original, namespace ajustado a `Clase07.FSM.Large.StatePattern.Tests`, sin el `using Clase07.FSM.Core;`):

```csharp
using System.Collections.Generic;
using NUnit.Framework;
using Clase07.FSM.Large.StatePattern;

namespace Clase07.FSM.Large.StatePattern.Tests
{
    public class StateMachineTests
    {
        private class RecordingState : IState
        {
            public readonly List<string> Calls = new List<string>();
            private readonly string _name;
            public RecordingState(string name) => _name = name;
            public void OnEnter() => Calls.Add($"{_name}:Enter");
            public void OnUpdate(float deltaTime) => Calls.Add($"{_name}:Update");
            public void OnExit() => Calls.Add($"{_name}:Exit");
        }

        [Test]
        public void Constructor_CallsOnEnterOnInitialState()
        {
            var a = new RecordingState("A");
            new StateMachine<IState>(a);
            CollectionAssert.AreEqual(new[] { "A:Enter" }, a.Calls);
        }

        [Test]
        public void ChangeState_ExitsPreviousThenEntersNext()
        {
            var a = new RecordingState("A");
            var b = new RecordingState("B");
            var machine = new StateMachine<IState>(a);

            machine.ChangeState(b);

            CollectionAssert.AreEqual(new[] { "A:Enter", "A:Exit" }, a.Calls);
            CollectionAssert.AreEqual(new[] { "B:Enter" }, b.Calls);
            Assert.AreSame(b, machine.CurrentState);
        }

        [Test]
        public void ChangeState_ToSameState_IsNoOp()
        {
            var a = new RecordingState("A");
            var machine = new StateMachine<IState>(a);

            machine.ChangeState(a);

            CollectionAssert.AreEqual(new[] { "A:Enter" }, a.Calls);
        }

        [Test]
        public void Tick_CallsOnUpdateOnCurrentState()
        {
            var a = new RecordingState("A");
            var machine = new StateMachine<IState>(a);

            machine.Tick(0.016f);

            CollectionAssert.AreEqual(new[] { "A:Enter", "A:Update" }, a.Calls);
        }

        [Test]
        public void StateChanged_FiresWithPreviousAndNext()
        {
            var a = new RecordingState("A");
            var b = new RecordingState("B");
            var machine = new StateMachine<IState>(a);
            IState firedPrevious = null, firedNext = null;
            machine.StateChanged += (prev, next) => { firedPrevious = prev; firedNext = next; };

            machine.ChangeState(b);

            Assert.AreSame(a, firedPrevious);
            Assert.AreSame(b, firedNext);
        }
    }
}
```

- [ ] **Step 8: Crear el asmdef de tests EditMode**

`clases/clase07/Unity/Assets/01_FSM/Large/a_StatePattern/Tests/Clase07.FSM.Large.StatePattern.Tests.asmdef`:

```json
{
    "name": "Clase07.FSM.Large.StatePattern.Tests",
    "rootNamespace": "",
    "references": [
        "Clase07.FSM.Large.StatePattern",
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

- [ ] **Step 9: Extraer el test PlayMode de esta escena**

Crear `clases/clase07/Unity/Assets/01_FSM/Large/a_StatePattern/Tests/PlayMode/GameFlowDemoViewPlayModeTests.cs`:

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
    public class GameFlowDemoViewPlayModeTests
    {
        [UnityTest]
        public IEnumerator GameFlowScene_LoadsAndRespondsToButtons()
        {
            SceneManager.LoadScene("Assets/01_FSM/Large/a_StatePattern/a_FSM_GameFlow.unity", LoadSceneMode.Single);
            yield return null;

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

- [ ] **Step 10: Crear el asmdef de tests PlayMode**

`clases/clase07/Unity/Assets/01_FSM/Large/a_StatePattern/Tests/PlayMode/Clase07.FSM.Large.StatePattern.Tests.PlayMode.asmdef`:

```json
{
    "name": "Clase07.FSM.Large.StatePattern.Tests.PlayMode",
    "rootNamespace": "",
    "references": [
        "Clase07.FSM.Large.StatePattern",
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

- [ ] **Step 11: Verificar EditMode**

```bash
/Applications/Unity/Hub/Editor/6000.3.21f1/Unity.app/Contents/MacOS/Unity \
  -batchmode -nographic \
  -projectPath clases/clase07/Unity \
  -runTests -testPlatform EditMode \
  -testResults "$(pwd)/clases/clase07/Unity_fsm_a_gameflow_statepattern_editmode.xml" \
  -logFile "$(pwd)/clases/clase07/Unity_fsm_a_gameflow_statepattern_editmode.log"
```

Run: `grep -c 'result="Failed"' clases/clase07/Unity_fsm_a_gameflow_statepattern_editmode.xml`
Expected: `0`.

- [ ] **Step 12: Verificar PlayMode**

```bash
/Applications/Unity/Hub/Editor/6000.3.21f1/Unity.app/Contents/MacOS/Unity \
  -batchmode -nographic \
  -projectPath clases/clase07/Unity \
  -runTests -testPlatform PlayMode \
  -testResults "$(pwd)/clases/clase07/Unity_fsm_a_gameflow_statepattern_playmode.xml" \
  -logFile "$(pwd)/clases/clase07/Unity_fsm_a_gameflow_statepattern_playmode.log"
```

Run: `grep -c 'result="Failed"' clases/clase07/Unity_fsm_a_gameflow_statepattern_playmode.xml`
Expected: `0`.

- [ ] **Step 13: Limpiar logs y commitear**

```bash
rm clases/clase07/Unity_fsm_a_gameflow_statepattern_editmode.xml clases/clase07/Unity_fsm_a_gameflow_statepattern_editmode.log \
   clases/clase07/Unity_fsm_a_gameflow_statepattern_playmode.xml clases/clase07/Unity_fsm_a_gameflow_statepattern_playmode.log
git add clases/clase07/Unity/Assets/01_FSM
git commit -m "$(cat <<'EOF'
clase07/FSM: Large/a_StatePattern autocontenida (copia propia de IState/StateMachine)

Co-Authored-By: Claude Sonnet 5 <noreply@anthropic.com>
Claude-Session: https://claude.ai/code/session_01YGZHSkcGRaTFS7xCnNWa5E
EOF
)"
```

---

### Task 4: `01_FSM/Large/b_ScriptableObjectStates/` autocontenida

**Files:**
- Move: `clases/clase07/Unity/Assets/01_FSM/Large/ScriptableObjectStates/{GameFlowSORunner.cs, GameFlowStateSO.cs, LoadingStateSO.cs, MainMenuStateSO.cs, PauseMenuStateSO.cs, PlayingStateSO.cs, SettingsMenuStateSO.cs, UserPlayingStateSO.cs}` → `clases/clase07/Unity/Assets/01_FSM/Large/b_ScriptableObjectStates/` (mismos nombres)
- Create: `clases/clase07/Unity/Assets/01_FSM/Large/b_ScriptableObjectStates/Clase07.FSM.Large.ScriptableObjectStates.asmdef`
- Move: `clases/clase07/Unity/Assets/01_FSM/Tests/GameFlowSORunnerTests.cs` → `clases/clase07/Unity/Assets/01_FSM/Large/b_ScriptableObjectStates/Tests/GameFlowSORunnerTests.cs`
- Create: `clases/clase07/Unity/Assets/01_FSM/Large/b_ScriptableObjectStates/Tests/Clase07.FSM.Large.ScriptableObjectStates.Tests.asmdef`

**Interfaces:**
- Produces: namespace `Clase07.FSM.Large.ScriptableObjectStates` — `GameFlowStateSO : ScriptableObject` (`Name`, `Enter()`, `Tick(float)`, `Exit()`), `PlayingStateSO` (`Configure(...)`, `CurrentSubstateName`), `GameFlowSORunner` (`Initialize(...)`, `CurrentState`, `CurrentSubstateName`, `FinishLoading()`, `Play()`, `Pause()`, `Resume()`, `OpenSettings()`, `CloseSettings()`).
- No consume — no depende de `Core/` ni de `IState`/`StateMachine<TState>`. Sin escena (decisión de alcance existente: esta variante solo se verifica por tests).

- [ ] **Step 1: Mover la carpeta con git**

```bash
cd /Users/giga/code/Programacion-de-VideoJuegos-III-2026
git mv clases/clase07/Unity/Assets/01_FSM/Large/ScriptableObjectStates clases/clase07/Unity/Assets/01_FSM/Large/b_ScriptableObjectStates
```

- [ ] **Step 2: Agregar el comentario pedagógico en los campos de transición de `PlayingStateSO`**

```csharp
        // A diferencia de Large/a_StatePattern, acá "a qué estado puedo ir" no es
        // código — son tres referencias serializadas, arrastrables desde el
        // Inspector sin tocar PlayingStateSO.cs.
        [SerializeField] private UserPlayingStateSO _userPlaying;
        [SerializeField] private PauseMenuStateSO _pauseMenu;
        [SerializeField] private SettingsMenuStateSO _settingsMenu;
```

- [ ] **Step 3: Crear el asmdef runtime**

`clases/clase07/Unity/Assets/01_FSM/Large/b_ScriptableObjectStates/Clase07.FSM.Large.ScriptableObjectStates.asmdef`:

```json
{
    "name": "Clase07.FSM.Large.ScriptableObjectStates",
    "rootNamespace": "",
    "references": [
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

- [ ] **Step 4: Mover `GameFlowSORunnerTests.cs`**

```bash
git mv clases/clase07/Unity/Assets/01_FSM/Tests/GameFlowSORunnerTests.cs clases/clase07/Unity/Assets/01_FSM/Large/b_ScriptableObjectStates/Tests/GameFlowSORunnerTests.cs
git mv clases/clase07/Unity/Assets/01_FSM/Tests/GameFlowSORunnerTests.cs.meta clases/clase07/Unity/Assets/01_FSM/Large/b_ScriptableObjectStates/Tests/GameFlowSORunnerTests.cs.meta
```

Ya es autocontenido — no requiere cambios de contenido.

- [ ] **Step 5: Crear el asmdef de tests**

`clases/clase07/Unity/Assets/01_FSM/Large/b_ScriptableObjectStates/Tests/Clase07.FSM.Large.ScriptableObjectStates.Tests.asmdef`:

```json
{
    "name": "Clase07.FSM.Large.ScriptableObjectStates.Tests",
    "rootNamespace": "",
    "references": [
        "Clase07.FSM.Large.ScriptableObjectStates",
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

- [ ] **Step 6: Verificar EditMode**

```bash
/Applications/Unity/Hub/Editor/6000.3.21f1/Unity.app/Contents/MacOS/Unity \
  -batchmode -nographic \
  -projectPath clases/clase07/Unity \
  -runTests -testPlatform EditMode \
  -testResults "$(pwd)/clases/clase07/Unity_fsm_b_so_states_editmode.xml" \
  -logFile "$(pwd)/clases/clase07/Unity_fsm_b_so_states_editmode.log"
```

Run: `grep -c 'result="Failed"' clases/clase07/Unity_fsm_b_so_states_editmode.xml`
Expected: `0`.

- [ ] **Step 7: Limpiar logs y commitear**

```bash
rm clases/clase07/Unity_fsm_b_so_states_editmode.xml clases/clase07/Unity_fsm_b_so_states_editmode.log
git add clases/clase07/Unity/Assets/01_FSM
git commit -m "$(cat <<'EOF'
clase07/FSM: Large/b_ScriptableObjectStates autocontenida (asmdef propio)

Co-Authored-By: Claude Sonnet 5 <noreply@anthropic.com>
Claude-Session: https://claude.ai/code/session_01YGZHSkcGRaTFS7xCnNWa5E
EOF
)"
```

---

### Task 5: limpiar `01_FSM/Core/`, `Demo/` y los asmdefs de módulo

**Files:**
- Delete: `clases/clase07/Unity/Assets/01_FSM/Core/` (carpeta completa)
- Delete: `clases/clase07/Unity/Assets/01_FSM/Demo/` (carpeta completa, si queda vacía)
- Delete: `clases/clase07/Unity/Assets/01_FSM/Clase07.FSM.asmdef`
- Delete: `clases/clase07/Unity/Assets/01_FSM/Tests/` (carpeta completa: `Clase07.FSM.Tests.asmdef`, `StateMachineTests.cs`, `WeaponEquivalenceTests.cs`, `Tests/PlayMode/Clase07.FSM.Tests.PlayMode.asmdef`, `Tests/PlayMode/FsmDemoViewsPlayModeTests.cs`)

**Interfaces:** Ninguna — solo limpieza, ningún archivo nuevo expone tipos.

- [ ] **Step 1: Confirmar que nada sigue referenciando `Clase07.FSM.Core`**

```bash
grep -rl "Clase07.FSM.Core" clases/clase07/Unity/Assets/01_FSM || echo "sin referencias"
```

Expected: `sin referencias` (cada implementación ya tiene su propia copia de `IState`/`StateMachine` desde las Tasks 1 a 4).

- [ ] **Step 2: Confirmar que nada sigue referenciando el asmdef de módulo `Clase07.FSM`**

```bash
grep -rl '"Clase07\.FSM"' clases/clase07/Unity/Assets/01_FSM --include="*.asmdef" || echo "sin referencias"
```

Expected: `sin referencias`.

- [ ] **Step 3: Borrar `Core/`, `Demo/`, y los asmdefs/tests de nivel de módulo**

```bash
cd /Users/giga/code/Programacion-de-VideoJuegos-III-2026
git rm -r clases/clase07/Unity/Assets/01_FSM/Core
git rm -r clases/clase07/Unity/Assets/01_FSM/Demo
git rm clases/clase07/Unity/Assets/01_FSM/Clase07.FSM.asmdef clases/clase07/Unity/Assets/01_FSM/Clase07.FSM.asmdef.meta
git rm -r clases/clase07/Unity/Assets/01_FSM/Tests
```

Nota: `WeaponBaselineTests.cs`, `GameFlowControllerTests.cs` y `GameFlowSORunnerTests.cs` ya fueron movidos (no borrados) en las Tasks 1, 3 y 4 respectivamente — `git rm -r` sobre `Tests/` en este punto solo borra lo que quedó ahí: `StateMachineTests.cs` (ya duplicado en `Small/b_StatePattern/Tests/` y `Large/a_StatePattern/Tests/` en las Tasks 2 y 3 — este borrado es del original, no de una cobertura sin reemplazo), `WeaponEquivalenceTests.cs`, los dos asmdefs de módulo y `PlayMode/FsmDemoViewsPlayModeTests.cs`.

- [ ] **Step 4: Verificación final — compilar y correr el módulo completo**

```bash
/Applications/Unity/Hub/Editor/6000.3.21f1/Unity.app/Contents/MacOS/Unity \
  -batchmode -nographic -quit \
  -projectPath clases/clase07/Unity \
  -logFile "$(pwd)/clases/clase07/Unity_fsm_final_compile.log"
grep -i "error CS" clases/clase07/Unity_fsm_final_compile.log || echo "sin errores de compilación"

/Applications/Unity/Hub/Editor/6000.3.21f1/Unity.app/Contents/MacOS/Unity \
  -batchmode -nographic \
  -projectPath clases/clase07/Unity \
  -runTests -testPlatform EditMode \
  -testResults "$(pwd)/clases/clase07/Unity_fsm_final_editmode.xml" \
  -logFile "$(pwd)/clases/clase07/Unity_fsm_final_editmode.log"

/Applications/Unity/Hub/Editor/6000.3.21f1/Unity.app/Contents/MacOS/Unity \
  -batchmode -nographic \
  -projectPath clases/clase07/Unity \
  -runTests -testPlatform PlayMode \
  -testResults "$(pwd)/clases/clase07/Unity_fsm_final_playmode.xml" \
  -logFile "$(pwd)/clases/clase07/Unity_fsm_final_playmode.log"
```

Run: `grep -c 'result="Failed"' clases/clase07/Unity_fsm_final_editmode.xml clases/clase07/Unity_fsm_final_playmode.xml`
Expected: `0` en ambos.

- [ ] **Step 5: Limpiar logs y commitear**

```bash
rm clases/clase07/Unity_fsm_final_compile.log clases/clase07/Unity_fsm_final_editmode.xml \
   clases/clase07/Unity_fsm_final_editmode.log clases/clase07/Unity_fsm_final_playmode.xml \
   clases/clase07/Unity_fsm_final_playmode.log
git add clases/clase07/Unity/Assets/01_FSM
git commit -m "$(cat <<'EOF'
clase07/FSM: borrar Core/, Demo/ y los asmdefs de módulo — cada implementación ya es autocontenida

Co-Authored-By: Claude Sonnet 5 <noreply@anthropic.com>
Claude-Session: https://claude.ai/code/session_01YGZHSkcGRaTFS7xCnNWa5E
EOF
)"
```
### Task 6: `a_Singleton/` autocontenida

**Files:**
- Move: `Unity/Assets/02_DependencyInjection/01_Singleton/` → `Unity/Assets/02_DependencyInjection/a_Singleton/` (git mv, carpeta completa)
- Move: `a_Singleton/01_DI_Singleton.unity` → `a_Singleton/a_DI_Singleton.unity` (+ `.meta`, git mv)
- Create: `Unity/Assets/02_DependencyInjection/a_Singleton/IScoreService.cs`
- Create: `Unity/Assets/02_DependencyInjection/a_Singleton/IAudioService.cs`
- Create: `Unity/Assets/02_DependencyInjection/a_Singleton/ScoreService.cs`
- Create: `Unity/Assets/02_DependencyInjection/a_Singleton/AudioService.cs`
- Modify: `a_Singleton/ScoreServiceSingleton.cs`
- Modify: `a_Singleton/AudioServiceSingleton.cs`
- Create: `Unity/Assets/02_DependencyInjection/a_Singleton/Clase07.DI.Singleton.asmdef`
- Create: `Unity/Assets/02_DependencyInjection/a_Singleton/Tests/Clase07.DI.Singleton.Tests.asmdef`
- Create: `Unity/Assets/02_DependencyInjection/a_Singleton/Tests/ScoreServiceTests.cs`

**Interfaces:**
- Produces: namespace `Clase07.DI.Singleton` (asmdef `Clase07.DI.Singleton`) exponiendo `IScoreService`, `IAudioService`, `ScoreService`, `AudioService`, `ScoreServiceSingleton` (`Instance`, `Service`, `Initialize()`), `AudioServiceSingleton` (`Instance`, `Service`, `Initialize()`), `DiSingletonDemoBootstrapper` (sin cambios de firma).

- [ ] **Step 1: Mover la carpeta y la escena**

```bash
cd /Users/giga/code/Programacion-de-VideoJuegos-III-2026
git mv clases/clase07/Unity/Assets/02_DependencyInjection/01_Singleton clases/clase07/Unity/Assets/02_DependencyInjection/a_Singleton
git mv clases/clase07/Unity/Assets/02_DependencyInjection/a_Singleton/01_DI_Singleton.unity clases/clase07/Unity/Assets/02_DependencyInjection/a_Singleton/a_DI_Singleton.unity
git mv clases/clase07/Unity/Assets/02_DependencyInjection/a_Singleton/01_DI_Singleton.unity.meta clases/clase07/Unity/Assets/02_DependencyInjection/a_Singleton/a_DI_Singleton.unity.meta
```

- [ ] **Step 2: Copiar el dominio compartido con namespace propio**

Crear `Unity/Assets/02_DependencyInjection/a_Singleton/IScoreService.cs`:

```csharp
using System;

namespace Clase07.DI.Singleton
{
    public interface IScoreService
    {
        int CurrentScore { get; }
        event Action<int> OnScoreChanged;
        void AddScore(int amount);
    }
}
```

Crear `Unity/Assets/02_DependencyInjection/a_Singleton/IAudioService.cs`:

```csharp
namespace Clase07.DI.Singleton
{
    public interface IAudioService
    {
        void PlayCoinSound();
    }
}
```

Crear `Unity/Assets/02_DependencyInjection/a_Singleton/ScoreService.cs`:

```csharp
using System;

namespace Clase07.DI.Singleton
{
    public class ScoreService : IScoreService
    {
        public int CurrentScore { get; private set; }
        public event Action<int> OnScoreChanged;

        public void AddScore(int amount)
        {
            if (amount <= 0) throw new ArgumentOutOfRangeException(nameof(amount));
            CurrentScore += amount;
            OnScoreChanged?.Invoke(CurrentScore);
        }
    }
}
```

Crear `Unity/Assets/02_DependencyInjection/a_Singleton/AudioService.cs`:

```csharp
using UnityEngine;

namespace Clase07.DI.Singleton
{
    // Un juego real usaría AudioSource.PlayOneShot; un log alcanza para no
    // depender de assets de audio en un ejemplo didáctico.
    public class AudioService : IAudioService
    {
        public void PlayCoinSound() => Debug.Log("[AudioService] coin!");
    }
}
```

- [ ] **Step 3: Sacar la dependencia a `Shared` y agregar el comentario pedagógico sobre `Instance`**

Reemplazar el contenido completo de `a_Singleton/ScoreServiceSingleton.cs`:

```csharp
using UnityEngine;

namespace Clase07.DI.Singleton
{
    public class ScoreServiceSingleton : MonoBehaviour
    {
        // Acceso global mutable: cualquier clase del proyecto puede leer
        // Instance sin que la dependencia quede declarada en su constructor —
        // es justo lo que Service Locator (b_ServiceLocator/) y la inyección
        // por constructor (c_VContainer/) buscan dejar de esconder.
        public static ScoreServiceSingleton Instance { get; private set; }
        public IScoreService Service { get; private set; }

        private void Awake() => Initialize();

        public void Initialize()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            Service ??= new ScoreService();
        }
    }
}
```

Reemplazar el contenido completo de `a_Singleton/AudioServiceSingleton.cs`:

```csharp
using UnityEngine;

namespace Clase07.DI.Singleton
{
    public class AudioServiceSingleton : MonoBehaviour
    {
        // Mismo problema que ScoreServiceSingleton.Instance: acceso global
        // mutable, dependencia oculta del consumidor.
        public static AudioServiceSingleton Instance { get; private set; }
        public IAudioService Service { get; private set; }

        private void Awake() => Initialize();

        public void Initialize()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            Service ??= new AudioService();
        }
    }
}
```

`DiSingletonDemoBootstrapper.cs` no cambia: ya está en el namespace `Clase07.DI.Singleton` y nunca importó `Clase07.DI.Shared`.

- [ ] **Step 4: Asmdef runtime**

Crear `Unity/Assets/02_DependencyInjection/a_Singleton/Clase07.DI.Singleton.asmdef`:

```json
{
    "name": "Clase07.DI.Singleton",
    "rootNamespace": "",
    "references": [
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

- [ ] **Step 5: Asmdef y test de dominio**

Crear `Unity/Assets/02_DependencyInjection/a_Singleton/Tests/Clase07.DI.Singleton.Tests.asmdef`:

```json
{
    "name": "Clase07.DI.Singleton.Tests",
    "rootNamespace": "",
    "references": [
        "Clase07.DI.Singleton",
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

Crear `Unity/Assets/02_DependencyInjection/a_Singleton/Tests/ScoreServiceTests.cs` (copia de `02_DependencyInjection/Tests/ScoreServiceTests.cs`, ahora contra la copia local de `ScoreService`):

```csharp
using NUnit.Framework;
using Clase07.DI.Singleton;

namespace Clase07.DI.Singleton.Tests
{
    public class ScoreServiceTests
    {
        [Test]
        public void AddScore_AccumulatesAcrossCalls()
        {
            var service = new ScoreService();
            service.AddScore(10);
            service.AddScore(5);
            Assert.AreEqual(15, service.CurrentScore);
        }

        [Test]
        public void AddScore_FiresOnScoreChangedWithNewTotal()
        {
            var service = new ScoreService();
            int? received = null;
            service.OnScoreChanged += value => received = value;

            service.AddScore(10);

            Assert.AreEqual(10, received);
        }

        [Test]
        public void AddScore_WithNonPositiveAmount_Throws()
        {
            var service = new ScoreService();
            Assert.Throws<System.ArgumentOutOfRangeException>(() => service.AddScore(0));
        }
    }
}
```

No hay ningún test de wiring específico de Singleton en `02_DependencyInjection/Tests/` hoy (solo existen `ScoreServiceTests.cs`, `ServiceLocatorTests.cs` y `DiVContainerWiringTests.cs`) — no se inventa uno nuevo.

- [ ] **Step 6: Verificar**

Desde la raíz del repo:

```bash
/Applications/Unity/Hub/Editor/6000.3.21f1/Unity.app/Contents/MacOS/Unity \
  -batchmode -nographic \
  -projectPath clases/clase07/Unity \
  -runTests -testPlatform EditMode \
  -testResults "$(pwd)/clases/clase07/Unity_di_a_singleton_editmode.xml" \
  -logFile "$(pwd)/clases/clase07/Unity_di_a_singleton_editmode.log"
```

Run: `grep -c 'result="Failed"' clases/clase07/Unity_di_a_singleton_editmode.xml; grep -i "error CS" clases/clase07/Unity_di_a_singleton_editmode.log || echo "sin errores"`
Expected: el conteo de `Failed` en `0` y `sin errores`. (En este punto `02_DependencyInjection/Shared/` y `Clase07.DI.asmdef` todavía existen y siguen compilando — Task 9 los borra recién cuando las tres implementaciones ya no los necesitan.)

- [ ] **Step 7: Limpiar logs y commitear**

```bash
rm clases/clase07/Unity_di_a_singleton_editmode.xml clases/clase07/Unity_di_a_singleton_editmode.log
git add clases/clase07/Unity/Assets/02_DependencyInjection/a_Singleton
git commit -m "$(cat <<'EOF'
clase07/DI: a_Singleton autocontenida (asmdef propio, copia local del dominio)

Co-Authored-By: Claude Sonnet 5 <noreply@anthropic.com>
Claude-Session: https://claude.ai/code/session_01YGZHSkcGRaTFS7xCnNWa5E
EOF
)"
```

---

### Task 7: `b_ServiceLocator/` autocontenida

**Files:**
- Move: `Unity/Assets/02_DependencyInjection/02_ServiceLocator/` → `Unity/Assets/02_DependencyInjection/b_ServiceLocator/`
- Move: `b_ServiceLocator/02_DI_ServiceLocator.unity` → `b_ServiceLocator/b_DI_ServiceLocator.unity` (+ `.meta`)
- Create: `Unity/Assets/02_DependencyInjection/b_ServiceLocator/IScoreService.cs`
- Create: `Unity/Assets/02_DependencyInjection/b_ServiceLocator/IAudioService.cs`
- Create: `Unity/Assets/02_DependencyInjection/b_ServiceLocator/ScoreService.cs`
- Create: `Unity/Assets/02_DependencyInjection/b_ServiceLocator/AudioService.cs`
- Modify: `b_ServiceLocator/ServiceLocatorCompositionRoot.cs`
- Modify: `b_ServiceLocator/DiServiceLocatorDemoBootstrapper.cs`
- Create: `Unity/Assets/02_DependencyInjection/b_ServiceLocator/Clase07.DI.ServiceLocatorPattern.asmdef`
- Create: `Unity/Assets/02_DependencyInjection/b_ServiceLocator/Tests/Clase07.DI.ServiceLocatorPattern.Tests.asmdef`
- Create: `Unity/Assets/02_DependencyInjection/b_ServiceLocator/Tests/ScoreServiceTests.cs`
- Move: `02_DependencyInjection/Tests/ServiceLocatorTests.cs` → `b_ServiceLocator/Tests/ServiceLocatorTests.cs`

**Interfaces:**
- Consumes: nada de otras carpetas de implementación.
- Produces: namespace `Clase07.DI.ServiceLocatorPattern` (asmdef `Clase07.DI.ServiceLocatorPattern`) exponiendo `IScoreService`, `IAudioService`, `ScoreService`, `AudioService`, `ServiceLocator` (`Register<T>`, `Resolve<T>`, `Clear`), `ServiceLocatorCompositionRoot.Bootstrap()`, `DiServiceLocatorDemoBootstrapper`.

- [ ] **Step 1: Mover la carpeta y la escena**

```bash
cd /Users/giga/code/Programacion-de-VideoJuegos-III-2026
git mv clases/clase07/Unity/Assets/02_DependencyInjection/02_ServiceLocator clases/clase07/Unity/Assets/02_DependencyInjection/b_ServiceLocator
git mv clases/clase07/Unity/Assets/02_DependencyInjection/b_ServiceLocator/02_DI_ServiceLocator.unity clases/clase07/Unity/Assets/02_DependencyInjection/b_ServiceLocator/b_DI_ServiceLocator.unity
git mv clases/clase07/Unity/Assets/02_DependencyInjection/b_ServiceLocator/02_DI_ServiceLocator.unity.meta clases/clase07/Unity/Assets/02_DependencyInjection/b_ServiceLocator/b_DI_ServiceLocator.unity.meta
```

- [ ] **Step 2: Copiar el dominio compartido con namespace propio**

Crear `Unity/Assets/02_DependencyInjection/b_ServiceLocator/IScoreService.cs`:

```csharp
using System;

namespace Clase07.DI.ServiceLocatorPattern
{
    public interface IScoreService
    {
        int CurrentScore { get; }
        event Action<int> OnScoreChanged;
        void AddScore(int amount);
    }
}
```

Crear `Unity/Assets/02_DependencyInjection/b_ServiceLocator/IAudioService.cs`:

```csharp
namespace Clase07.DI.ServiceLocatorPattern
{
    public interface IAudioService
    {
        void PlayCoinSound();
    }
}
```

Crear `Unity/Assets/02_DependencyInjection/b_ServiceLocator/ScoreService.cs`:

```csharp
using System;

namespace Clase07.DI.ServiceLocatorPattern
{
    public class ScoreService : IScoreService
    {
        public int CurrentScore { get; private set; }
        public event Action<int> OnScoreChanged;

        public void AddScore(int amount)
        {
            if (amount <= 0) throw new ArgumentOutOfRangeException(nameof(amount));
            CurrentScore += amount;
            OnScoreChanged?.Invoke(CurrentScore);
        }
    }
}
```

Crear `Unity/Assets/02_DependencyInjection/b_ServiceLocator/AudioService.cs`:

```csharp
using UnityEngine;

namespace Clase07.DI.ServiceLocatorPattern
{
    // Un juego real usaría AudioSource.PlayOneShot; un log alcanza para no
    // depender de assets de audio en un ejemplo didáctico.
    public class AudioService : IAudioService
    {
        public void PlayCoinSound() => Debug.Log("[AudioService] coin!");
    }
}
```

`ServiceLocator.cs` no cambia: ya vive en `Clase07.DI.ServiceLocatorPattern` y nunca importó `Clase07.DI.Shared` (el registro es genérico, no conoce `IScoreService`).

- [ ] **Step 3: Sacar la dependencia a `Shared` y agregar el comentario pedagógico sobre `Resolve<T>()`**

Reemplazar el contenido completo de `b_ServiceLocator/ServiceLocatorCompositionRoot.cs`:

```csharp
namespace Clase07.DI.ServiceLocatorPattern
{
    public static class ServiceLocatorCompositionRoot
    {
        public static void Bootstrap()
        {
            ServiceLocator.Register<IScoreService>(new ScoreService());
            ServiceLocator.Register<IAudioService>(new AudioService());
        }
    }
}
```

Reemplazar el contenido completo de `b_ServiceLocator/DiServiceLocatorDemoBootstrapper.cs`:

```csharp
using UnityEngine;
using TMPro;

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
            // El consumidor pide el servicio por tipo en el momento en que lo
            // necesita — a diferencia de VContainer (c_VContainer/), esta
            // dependencia no aparece en ningún constructor ni firma pública.
            var scoreService = ServiceLocator.Resolve<IScoreService>();
            ServiceLocator.Resolve<IAudioService>().PlayCoinSound();
            scoreService.AddScore(10);
            _scoreLabel.text = $"Score: {scoreService.CurrentScore}";
        }
    }
}
```

- [ ] **Step 4: Asmdef runtime**

Crear `Unity/Assets/02_DependencyInjection/b_ServiceLocator/Clase07.DI.ServiceLocatorPattern.asmdef`:

```json
{
    "name": "Clase07.DI.ServiceLocatorPattern",
    "rootNamespace": "",
    "references": [
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

- [ ] **Step 5: Asmdef y tests**

Crear `Unity/Assets/02_DependencyInjection/b_ServiceLocator/Tests/Clase07.DI.ServiceLocatorPattern.Tests.asmdef`:

```json
{
    "name": "Clase07.DI.ServiceLocatorPattern.Tests",
    "rootNamespace": "",
    "references": [
        "Clase07.DI.ServiceLocatorPattern",
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

Crear `Unity/Assets/02_DependencyInjection/b_ServiceLocator/Tests/ScoreServiceTests.cs`:

```csharp
using NUnit.Framework;
using Clase07.DI.ServiceLocatorPattern;

namespace Clase07.DI.ServiceLocatorPattern.Tests
{
    public class ScoreServiceTests
    {
        [Test]
        public void AddScore_AccumulatesAcrossCalls()
        {
            var service = new ScoreService();
            service.AddScore(10);
            service.AddScore(5);
            Assert.AreEqual(15, service.CurrentScore);
        }

        [Test]
        public void AddScore_FiresOnScoreChangedWithNewTotal()
        {
            var service = new ScoreService();
            int? received = null;
            service.OnScoreChanged += value => received = value;

            service.AddScore(10);

            Assert.AreEqual(10, received);
        }

        [Test]
        public void AddScore_WithNonPositiveAmount_Throws()
        {
            var service = new ScoreService();
            Assert.Throws<System.ArgumentOutOfRangeException>(() => service.AddScore(0));
        }
    }
}
```

Mover `02_DependencyInjection/Tests/ServiceLocatorTests.cs` a
`b_ServiceLocator/Tests/ServiceLocatorTests.cs` con `git mv`, y reemplazar su namespace
(era `Clase07.DI.Tests`) para que quede junto al resto de los tests de esta carpeta:

```csharp
using NUnit.Framework;
using Clase07.DI.ServiceLocatorPattern;

namespace Clase07.DI.ServiceLocatorPattern.Tests
{
    public class ServiceLocatorTests
    {
        [TearDown]
        public void TearDown() => ServiceLocator.Clear();

        [Test]
        public void Register_ThenResolve_ReturnsSameInstance()
        {
            var instance = new object();
            ServiceLocator.Register(instance);

            Assert.AreSame(instance, ServiceLocator.Resolve<object>());
        }

        [Test]
        public void Resolve_WithoutRegistering_Throws()
        {
            Assert.Throws<System.InvalidOperationException>(() => ServiceLocator.Resolve<string>());
        }

        [Test]
        public void Clear_RemovesAllRegistrations()
        {
            ServiceLocator.Register("hello");
            ServiceLocator.Clear();

            Assert.Throws<System.InvalidOperationException>(() => ServiceLocator.Resolve<string>());
        }
    }
}
```

- [ ] **Step 6: Verificar**

```bash
/Applications/Unity/Hub/Editor/6000.3.21f1/Unity.app/Contents/MacOS/Unity \
  -batchmode -nographic \
  -projectPath clases/clase07/Unity \
  -runTests -testPlatform EditMode \
  -testResults "$(pwd)/clases/clase07/Unity_di_b_servicelocator_editmode.xml" \
  -logFile "$(pwd)/clases/clase07/Unity_di_b_servicelocator_editmode.log"
```

Run: `grep -c 'result="Failed"' clases/clase07/Unity_di_b_servicelocator_editmode.xml; grep -i "error CS" clases/clase07/Unity_di_b_servicelocator_editmode.log || echo "sin errores"`
Expected: `0` y `sin errores`.

- [ ] **Step 7: Limpiar logs y commitear**

```bash
rm clases/clase07/Unity_di_b_servicelocator_editmode.xml clases/clase07/Unity_di_b_servicelocator_editmode.log
git add clases/clase07/Unity/Assets/02_DependencyInjection/b_ServiceLocator clases/clase07/Unity/Assets/02_DependencyInjection/Tests
git commit -m "$(cat <<'EOF'
clase07/DI: b_ServiceLocator autocontenida (asmdef propio, copia local del dominio)

Co-Authored-By: Claude Sonnet 5 <noreply@anthropic.com>
Claude-Session: https://claude.ai/code/session_01YGZHSkcGRaTFS7xCnNWa5E
EOF
)"
```

---

### Task 8: `c_VContainer/` autocontenida

**Files:**
- Move: `Unity/Assets/02_DependencyInjection/03_VContainer/` → `Unity/Assets/02_DependencyInjection/c_VContainer/`
- Move: `c_VContainer/03_DI_VContainer.unity` → `c_VContainer/c_DI_VContainer.unity` (+ `.meta`)
- Create: `Unity/Assets/02_DependencyInjection/c_VContainer/IScoreService.cs`
- Create: `Unity/Assets/02_DependencyInjection/c_VContainer/IAudioService.cs`
- Create: `Unity/Assets/02_DependencyInjection/c_VContainer/ScoreService.cs`
- Create: `Unity/Assets/02_DependencyInjection/c_VContainer/AudioService.cs`
- Modify: `c_VContainer/CoinPickupVContainer.cs`
- Modify: `c_VContainer/DiVContainerLifetimeScope.cs`
- Create: `Unity/Assets/02_DependencyInjection/c_VContainer/Clase07.DI.VContainerExample.asmdef`
- Create: `Unity/Assets/02_DependencyInjection/c_VContainer/Tests/Clase07.DI.VContainerExample.Tests.asmdef`
- Create: `Unity/Assets/02_DependencyInjection/c_VContainer/Tests/ScoreServiceTests.cs`
- Move: `02_DependencyInjection/Tests/DiVContainerWiringTests.cs` → `c_VContainer/Tests/DiVContainerWiringTests.cs`

**Interfaces:**
- Consumes: `VContainer.LifetimeScope`, `VContainer.IContainerBuilder`, `VContainer.Lifetime` (paquete de terceros, sin cambios).
- Produces: namespace `Clase07.DI.VContainerExample` (asmdef `Clase07.DI.VContainerExample`) exponiendo `IScoreService`, `IAudioService`, `ScoreService`, `AudioService`, `CoinPickupVContainer`, `DiVContainerLifetimeScope.RegisterServices(IContainerBuilder)`.

- [ ] **Step 1: Mover la carpeta y la escena**

```bash
cd /Users/giga/code/Programacion-de-VideoJuegos-III-2026
git mv clases/clase07/Unity/Assets/02_DependencyInjection/03_VContainer clases/clase07/Unity/Assets/02_DependencyInjection/c_VContainer
git mv clases/clase07/Unity/Assets/02_DependencyInjection/c_VContainer/03_DI_VContainer.unity clases/clase07/Unity/Assets/02_DependencyInjection/c_VContainer/c_DI_VContainer.unity
git mv clases/clase07/Unity/Assets/02_DependencyInjection/c_VContainer/03_DI_VContainer.unity.meta clases/clase07/Unity/Assets/02_DependencyInjection/c_VContainer/c_DI_VContainer.unity.meta
```

- [ ] **Step 2: Copiar el dominio compartido con namespace propio**

Crear `Unity/Assets/02_DependencyInjection/c_VContainer/IScoreService.cs`:

```csharp
using System;

namespace Clase07.DI.VContainerExample
{
    public interface IScoreService
    {
        int CurrentScore { get; }
        event Action<int> OnScoreChanged;
        void AddScore(int amount);
    }
}
```

Crear `Unity/Assets/02_DependencyInjection/c_VContainer/IAudioService.cs`:

```csharp
namespace Clase07.DI.VContainerExample
{
    public interface IAudioService
    {
        void PlayCoinSound();
    }
}
```

Crear `Unity/Assets/02_DependencyInjection/c_VContainer/ScoreService.cs`:

```csharp
using System;

namespace Clase07.DI.VContainerExample
{
    public class ScoreService : IScoreService
    {
        public int CurrentScore { get; private set; }
        public event Action<int> OnScoreChanged;

        public void AddScore(int amount)
        {
            if (amount <= 0) throw new ArgumentOutOfRangeException(nameof(amount));
            CurrentScore += amount;
            OnScoreChanged?.Invoke(CurrentScore);
        }
    }
}
```

Crear `Unity/Assets/02_DependencyInjection/c_VContainer/AudioService.cs`:

```csharp
using UnityEngine;

namespace Clase07.DI.VContainerExample
{
    // Un juego real usaría AudioSource.PlayOneShot; un log alcanza para no
    // depender de assets de audio en un ejemplo didáctico.
    public class AudioService : IAudioService
    {
        public void PlayCoinSound() => Debug.Log("[AudioService] coin!");
    }
}
```

- [ ] **Step 3: Sacar la dependencia a `Shared` y agregar el comentario pedagógico sobre `[Inject]`**

Reemplazar el contenido completo de `c_VContainer/CoinPickupVContainer.cs`:

```csharp
using UnityEngine;
using TMPro;
using VContainer;

namespace Clase07.DI.VContainerExample
{
    public class CoinPickupVContainer : MonoBehaviour
    {
        [SerializeField] private TMP_Text _scoreLabel;

        private IScoreService _scoreService;
        private IAudioService _audioService;

        // La dependencia se DECLARA acá, no se busca (Instance estático) ni se
        // pide bajo demanda (ServiceLocator.Resolve<T>()) — VContainer la
        // resuelve y la pasa antes de que el objeto se use, así que además
        // queda trivial de testear con un fake, sin tocar el contenedor.
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

Reemplazar el contenido completo de `c_VContainer/DiVContainerLifetimeScope.cs`:

```csharp
using VContainer;
using VContainer.Unity;

namespace Clase07.DI.VContainerExample
{
    public class DiVContainerLifetimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            RegisterServices(builder);

            // RegisterComponentInHierarchy necesita la escena del LifetimeScope, así que
            // queda afuera de RegisterServices: los tests de EditMode no tienen jerarquía
            // que recorrer, pero sí pueden verificar los registros de servicios.
            builder.RegisterComponentInHierarchy<CoinPickupVContainer>();
        }

        // Los registros que no dependen de la escena viven acá para que los tests
        // ejerciten exactamente el mismo cableado que usa la escena, en vez de duplicarlo.
        public static void RegisterServices(IContainerBuilder builder)
        {
            builder.Register<IScoreService, ScoreService>(Lifetime.Singleton);
            builder.Register<IAudioService, AudioService>(Lifetime.Singleton);
        }
    }
}
```

- [ ] **Step 4: Asmdef runtime**

Crear `Unity/Assets/02_DependencyInjection/c_VContainer/Clase07.DI.VContainerExample.asmdef`:

```json
{
    "name": "Clase07.DI.VContainerExample",
    "rootNamespace": "",
    "references": [
        "VContainer",
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

- [ ] **Step 5: Asmdef y tests**

Crear `Unity/Assets/02_DependencyInjection/c_VContainer/Tests/Clase07.DI.VContainerExample.Tests.asmdef`:

```json
{
    "name": "Clase07.DI.VContainerExample.Tests",
    "rootNamespace": "",
    "references": [
        "Clase07.DI.VContainerExample",
        "VContainer",
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

Crear `Unity/Assets/02_DependencyInjection/c_VContainer/Tests/ScoreServiceTests.cs`:

```csharp
using NUnit.Framework;
using Clase07.DI.VContainerExample;

namespace Clase07.DI.VContainerExample.Tests
{
    public class ScoreServiceTests
    {
        [Test]
        public void AddScore_AccumulatesAcrossCalls()
        {
            var service = new ScoreService();
            service.AddScore(10);
            service.AddScore(5);
            Assert.AreEqual(15, service.CurrentScore);
        }

        [Test]
        public void AddScore_FiresOnScoreChangedWithNewTotal()
        {
            var service = new ScoreService();
            int? received = null;
            service.OnScoreChanged += value => received = value;

            service.AddScore(10);

            Assert.AreEqual(10, received);
        }

        [Test]
        public void AddScore_WithNonPositiveAmount_Throws()
        {
            var service = new ScoreService();
            Assert.Throws<System.ArgumentOutOfRangeException>(() => service.AddScore(0));
        }
    }
}
```

Mover `02_DependencyInjection/Tests/DiVContainerWiringTests.cs` a
`c_VContainer/Tests/DiVContainerWiringTests.cs` con `git mv`, sacar la dependencia a
`Clase07.DI.Shared` y ajustar el namespace:

```csharp
using NUnit.Framework;
using VContainer;
using Clase07.DI.VContainerExample;

namespace Clase07.DI.VContainerExample.Tests
{
    // Los tests llaman al mismo DiVContainerLifetimeScope.RegisterServices que usa la
    // escena: si alguien rompe ese cableado, estos tests lo detectan.
    public class DiVContainerWiringTests
    {
        [Test]
        public void LifetimeScopeRegistrations_ResolveScoreAndAudioServices()
        {
            var builder = new ContainerBuilder();
            DiVContainerLifetimeScope.RegisterServices(builder);

            using var container = builder.Build();

            Assert.IsInstanceOf<ScoreService>(container.Resolve<IScoreService>());
            Assert.IsInstanceOf<AudioService>(container.Resolve<IAudioService>());
        }

        [Test]
        public void LifetimeScopeRegistrations_SingletonLifetime_ReturnsSameInstance()
        {
            var builder = new ContainerBuilder();
            DiVContainerLifetimeScope.RegisterServices(builder);

            using var container = builder.Build();

            Assert.AreSame(container.Resolve<IScoreService>(), container.Resolve<IScoreService>());
            Assert.AreSame(container.Resolve<IAudioService>(), container.Resolve<IAudioService>());
        }
    }
}
```

- [ ] **Step 6: Verificar**

```bash
/Applications/Unity/Hub/Editor/6000.3.21f1/Unity.app/Contents/MacOS/Unity \
  -batchmode -nographic \
  -projectPath clases/clase07/Unity \
  -runTests -testPlatform EditMode \
  -testResults "$(pwd)/clases/clase07/Unity_di_c_vcontainer_editmode.xml" \
  -logFile "$(pwd)/clases/clase07/Unity_di_c_vcontainer_editmode.log"
```

Run: `grep -c 'result="Failed"' clases/clase07/Unity_di_c_vcontainer_editmode.xml; grep -i "error CS" clases/clase07/Unity_di_c_vcontainer_editmode.log || echo "sin errores"`
Expected: `0` y `sin errores`.

- [ ] **Step 7: Limpiar logs y commitear**

```bash
rm clases/clase07/Unity_di_c_vcontainer_editmode.xml clases/clase07/Unity_di_c_vcontainer_editmode.log
git add clases/clase07/Unity/Assets/02_DependencyInjection/c_VContainer clases/clase07/Unity/Assets/02_DependencyInjection/Tests
git commit -m "$(cat <<'EOF'
clase07/DI: c_VContainer autocontenida (asmdef propio, copia local del dominio)

Co-Authored-By: Claude Sonnet 5 <noreply@anthropic.com>
Claude-Session: https://claude.ai/code/session_01YGZHSkcGRaTFS7xCnNWa5E
EOF
)"
```

---

### Task 9: limpieza de `02_DependencyInjection` a nivel de módulo

**Files:**
- Delete: `Unity/Assets/02_DependencyInjection/Shared/` (4 `.cs` + `.meta`, carpeta completa)
- Delete: `Unity/Assets/02_DependencyInjection/Clase07.DI.asmdef` (+ `.meta`)
- Delete: `Unity/Assets/02_DependencyInjection/Tests/Clase07.DI.Tests.asmdef` (+ `.meta`)
- Delete: `Unity/Assets/02_DependencyInjection/Tests/ScoreServiceTests.cs` (+ `.meta`) — el original: Tasks 6/7/8 crearon cada una su propia copia nueva, nunca lo movieron de acá
- Delete: `Unity/Assets/02_DependencyInjection/Tests/` (carpeta, si queda vacía tras borrar el asmdef y el test original)

**Interfaces:** Ninguna — task de limpieza pura, no agrega ni cambia comportamiento.

- [ ] **Step 1: Confirmar que nada referencia ya `Clase07.DI.Shared`**

```bash
cd /Users/giga/code/Programacion-de-VideoJuegos-III-2026
grep -rl "Clase07.DI.Shared" clases/clase07/Unity/Assets/02_DependencyInjection || echo "sin referencias"
```

Expected: `sin referencias`. Si aparece algún archivo, es que a alguna de las tres
implementaciones (Tasks 6/7/8) le faltó copiar una clase o actualizar un
`using` — corregir esa carpeta antes de continuar con este task.

- [ ] **Step 2: Borrar `Shared/`, el `ScoreServiceTests.cs` original y los asmdef de módulo**

`Tests/ScoreServiceTests.cs` a nivel de módulo es el original: las Tasks 6, 7 y 8
crearon cada una su propia copia dentro de su propia carpeta (mismo patrón de
duplicación que el código de producción), pero ninguna lo sacó de acá con `git mv` —
sigue estando, y hay que borrarlo explícitamente o `Tests/` nunca queda vacía:

```bash
git rm -r clases/clase07/Unity/Assets/02_DependencyInjection/Shared
git rm clases/clase07/Unity/Assets/02_DependencyInjection/Clase07.DI.asmdef clases/clase07/Unity/Assets/02_DependencyInjection/Clase07.DI.asmdef.meta
git rm clases/clase07/Unity/Assets/02_DependencyInjection/Tests/Clase07.DI.Tests.asmdef clases/clase07/Unity/Assets/02_DependencyInjection/Tests/Clase07.DI.Tests.asmdef.meta
git rm clases/clase07/Unity/Assets/02_DependencyInjection/Tests/ScoreServiceTests.cs clases/clase07/Unity/Assets/02_DependencyInjection/Tests/ScoreServiceTests.cs.meta
```

Ahora sí, `clases/clase07/Unity/Assets/02_DependencyInjection/Tests/` debería quedar
vacía (sin archivos `.cs` — `ServiceLocatorTests.cs` y `DiVContainerWiringTests.cs` ya
se movieron en Tasks 7 y 8, y `ScoreServiceTests.cs` se acaba de borrar arriba) —
borrarla:

```bash
rmdir clases/clase07/Unity/Assets/02_DependencyInjection/Tests 2>/dev/null || true
git rm clases/clase07/Unity/Assets/02_DependencyInjection/Tests.meta 2>/dev/null || true
```

- [ ] **Step 3: Verificación final del módulo completo**

```bash
/Applications/Unity/Hub/Editor/6000.3.21f1/Unity.app/Contents/MacOS/Unity \
  -batchmode -nographic \
  -projectPath clases/clase07/Unity \
  -runTests -testPlatform EditMode \
  -testResults "$(pwd)/clases/clase07/Unity_di_final_editmode.xml" \
  -logFile "$(pwd)/clases/clase07/Unity_di_final_editmode.log"
```

Run: `grep -c 'result="Failed"' clases/clase07/Unity_di_final_editmode.xml; grep -i "error CS" clases/clase07/Unity_di_final_editmode.log || echo "sin errores"`
Expected: `0` y `sin errores` (corre los tests de las tres implementaciones nuevas
más cualquier otro test del proyecto, ya que no se filtra por carpeta).

- [ ] **Step 4: Limpiar logs y commitear**

```bash
rm clases/clase07/Unity_di_final_editmode.xml clases/clase07/Unity_di_final_editmode.log
git add -A clases/clase07/Unity/Assets/02_DependencyInjection
git commit -m "$(cat <<'EOF'
clase07/DI: borrar Shared/ y el asmdef de módulo, ya sin referencias

Co-Authored-By: Claude Sonnet 5 <noreply@anthropic.com>
Claude-Session: https://claude.ai/code/session_01YGZHSkcGRaTFS7xCnNWa5E
EOF
)"
```
### Task 10: `03_MessageBroker/a_DIBroker/` autocontenida

**Files:**
- Move: `clases/clase07/Unity/Assets/03_MessageBroker/01_DIBroker/` → `clases/clase07/Unity/Assets/03_MessageBroker/a_DIBroker/`
- Move: `.../a_DIBroker/01_MessageBroker_DIBroker.unity` (+ `.meta`) → `.../a_DIBroker/a_MessageBroker_DIBroker.unity`
- Create: `clases/clase07/Unity/Assets/03_MessageBroker/a_DIBroker/ScorePickedUpEvent.cs`
- Create: `clases/clase07/Unity/Assets/03_MessageBroker/a_DIBroker/PlayerDamagedEvent.cs`
- Create: `clases/clase07/Unity/Assets/03_MessageBroker/a_DIBroker/Clase07.MessageBroker.DIBroker.asmdef`
- Modify: `.../a_DIBroker/IMessageBroker.cs`, `.../a_DIBroker/MessageBroker.cs` (comentarios pedagógicos)
- Move: `clases/clase07/Unity/Assets/03_MessageBroker/Tests/MessageBrokerTests.cs` → `clases/clase07/Unity/Assets/03_MessageBroker/a_DIBroker/Tests/MessageBrokerTests.cs`
- Create: `clases/clase07/Unity/Assets/03_MessageBroker/a_DIBroker/Tests/Clase07.MessageBroker.DIBroker.Tests.asmdef`

**Interfaces:**
- Consumes: nada (autocontenida) salvo `VContainer`/`VContainer.Unity` (paquete de terceros) y `TMPro` (uGUI/TextMeshPro, paquete de terceros).
- Produces: `Clase07.MessageBroker.DIBroker.ScorePickedUpEvent` (`int Amount`), `Clase07.MessageBroker.DIBroker.PlayerDamagedEvent` (`int Amount`), `Clase07.MessageBroker.DIBroker.IMessageBroker`/`MessageBroker` sin cambios de firma. No lo consume ninguna otra carpeta.

- [ ] **Step 1: Mover la carpeta y renombrar la escena con `git mv`**

Correr desde la raíz del repo:

```bash
git -C clases/clase07 mv Unity/Assets/03_MessageBroker/01_DIBroker Unity/Assets/03_MessageBroker/a_DIBroker
git -C clases/clase07 mv Unity/Assets/03_MessageBroker/a_DIBroker/01_MessageBroker_DIBroker.unity Unity/Assets/03_MessageBroker/a_DIBroker/a_MessageBroker_DIBroker.unity
git -C clases/clase07 mv Unity/Assets/03_MessageBroker/a_DIBroker/01_MessageBroker_DIBroker.unity.meta Unity/Assets/03_MessageBroker/a_DIBroker/a_MessageBroker_DIBroker.unity.meta
```

- [ ] **Step 2: Copiar los dos eventos con namespace propio**

`03_MessageBroker/Tests/MessageBrokerTests.cs` usa `PlayerDamagedEvent` en un test
(`Publish_WithNoSubscribers_DoesNotThrow`), así que esta carpeta necesita copiar los
**dos** eventos, no solo el de Score.

Crear `clases/clase07/Unity/Assets/03_MessageBroker/a_DIBroker/ScorePickedUpEvent.cs`:

```csharp
namespace Clase07.MessageBroker.DIBroker
{
    public readonly struct ScorePickedUpEvent
    {
        public readonly int Amount;
        public ScorePickedUpEvent(int amount) => Amount = amount;
    }
}
```

Crear `clases/clase07/Unity/Assets/03_MessageBroker/a_DIBroker/PlayerDamagedEvent.cs`:

```csharp
namespace Clase07.MessageBroker.DIBroker
{
    public readonly struct PlayerDamagedEvent
    {
        public readonly int Amount;
        public PlayerDamagedEvent(int amount) => Amount = amount;
    }
}
```

- [ ] **Step 3: Quitar el `using Clase07.MessageBroker.Shared;` de `DiBrokerDemoView.cs`**

En `clases/clase07/Unity/Assets/03_MessageBroker/a_DIBroker/DiBrokerDemoView.cs`, el
tipo `ScorePickedUpEvent` ya vive en el mismo namespace (`Clase07.MessageBroker.DIBroker`)
que la vista, así que el using a `Shared` sobra. Cambiar:

```csharp
using UnityEngine;
using TMPro;
using VContainer;
using Clase07.MessageBroker.Shared;

namespace Clase07.MessageBroker.DIBroker
```

por:

```csharp
using UnityEngine;
using TMPro;
using VContainer;

namespace Clase07.MessageBroker.DIBroker
```

El resto del archivo no cambia.

- [ ] **Step 4: Comentarios pedagógicos en `IMessageBroker.cs` y `MessageBroker.cs`**

En `clases/clase07/Unity/Assets/03_MessageBroker/a_DIBroker/IMessageBroker.cs`, agregar
un comentario sobre el contrato mínimo de un broker hecho a mano:

```csharp
using System;

namespace Clase07.MessageBroker.DIBroker
{
    // El contrato completo de un broker pub/sub cabe en dos métodos: Subscribe
    // devuelve un IDisposable que hay que guardar y disponer para cancelar la
    // suscripción (si no, la suscripción queda viva para siempre).
    public interface IMessageBroker
    {
        IDisposable Subscribe<T>(Action<T> handler);
        void Publish<T>(T message);
    }
}
```

En `clases/clase07/Unity/Assets/03_MessageBroker/a_DIBroker/MessageBroker.cs`, agregar
un comentario en `Publish<T>` señalando el mecanismo interno:

```csharp
        public void Publish<T>(T message)
        {
            if (!_handlers.TryGetValue(typeof(T), out var list)) return;
            // ToArray() copia la lista antes de iterar: si un handler se desuscribe
            // (Dispose) durante su propio callback, no rompe la iteración en curso.
            foreach (var handler in list.ToArray())
            {
                ((Action<T>)handler).Invoke(message);
            }
        }
```

(El resto de `MessageBroker.cs` no cambia.)

- [ ] **Step 5: Crear el asmdef runtime**

Crear `clases/clase07/Unity/Assets/03_MessageBroker/a_DIBroker/Clase07.MessageBroker.DIBroker.asmdef`:

```json
{
    "name": "Clase07.MessageBroker.DIBroker",
    "rootNamespace": "",
    "references": [
        "VContainer",
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

- [ ] **Step 6: Mover y adaptar los tests**

```bash
mkdir -p clases/clase07/Unity/Assets/03_MessageBroker/a_DIBroker/Tests
git -C clases/clase07 mv Unity/Assets/03_MessageBroker/Tests/MessageBrokerTests.cs Unity/Assets/03_MessageBroker/a_DIBroker/Tests/MessageBrokerTests.cs
git -C clases/clase07 mv Unity/Assets/03_MessageBroker/Tests/MessageBrokerTests.cs.meta Unity/Assets/03_MessageBroker/a_DIBroker/Tests/MessageBrokerTests.cs.meta
```

Editar `clases/clase07/Unity/Assets/03_MessageBroker/a_DIBroker/Tests/MessageBrokerTests.cs`
para usar el namespace propio y simplificar las referencias ya no calificadas:

```csharp
using NUnit.Framework;
using Clase07.MessageBroker.DIBroker;

namespace Clase07.MessageBroker.DIBroker.Tests
{
    public class MessageBrokerTests
    {
        [Test]
        public void Publish_DeliversToSubscriber()
        {
            var broker = new MessageBroker();
            ScorePickedUpEvent? received = null;
            broker.Subscribe<ScorePickedUpEvent>(e => received = e);

            broker.Publish(new ScorePickedUpEvent(10));

            Assert.AreEqual(10, received.Value.Amount);
        }

        [Test]
        public void Publish_DeliversToMultipleSubscribers()
        {
            var broker = new MessageBroker();
            var count = 0;
            broker.Subscribe<ScorePickedUpEvent>(_ => count++);
            broker.Subscribe<ScorePickedUpEvent>(_ => count++);

            broker.Publish(new ScorePickedUpEvent(1));

            Assert.AreEqual(2, count);
        }

        [Test]
        public void Dispose_CancelsSubscription()
        {
            var broker = new MessageBroker();
            var count = 0;
            var subscription = broker.Subscribe<ScorePickedUpEvent>(_ => count++);

            subscription.Dispose();
            broker.Publish(new ScorePickedUpEvent(1));

            Assert.AreEqual(0, count);
        }

        [Test]
        public void Publish_WithNoSubscribers_DoesNotThrow()
        {
            var broker = new MessageBroker();
            Assert.DoesNotThrow(() => broker.Publish(new PlayerDamagedEvent(5)));
        }
    }
}
```

Crear `clases/clase07/Unity/Assets/03_MessageBroker/a_DIBroker/Tests/Clase07.MessageBroker.DIBroker.Tests.asmdef`:

```json
{
    "name": "Clase07.MessageBroker.DIBroker.Tests",
    "rootNamespace": "",
    "references": [
        "Clase07.MessageBroker.DIBroker",
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

- [ ] **Step 7: Verificar compilación y tests EditMode**

```bash
/Applications/Unity/Hub/Editor/6000.3.21f1/Unity.app/Contents/MacOS/Unity \
  -batchmode -nographic \
  -projectPath clases/clase07/Unity \
  -runTests -testPlatform EditMode \
  -testResults "$(pwd)/clases/clase07/Unity_mb_a_dibroker_editmode.xml" \
  -logFile "$(pwd)/clases/clase07/Unity_mb_a_dibroker_editmode.log"
```

(No combinar `-quit` con `-runTests`.)

Run: `grep -iE "error CS" clases/clase07/Unity_mb_a_dibroker_editmode.log || echo "sin errores de compilación"`
Expected: `sin errores de compilación`.

Run: `grep -oE 'result="Failed"' clases/clase07/Unity_mb_a_dibroker_editmode.xml | wc -l`
Expected: `0`.

- [ ] **Step 8: Limpiar logs y commitear**

```bash
rm clases/clase07/Unity_mb_a_dibroker_editmode.xml clases/clase07/Unity_mb_a_dibroker_editmode.log
git -C clases/clase07 add Unity/Assets/03_MessageBroker/a_DIBroker Unity/Assets/03_MessageBroker/Tests
git -C clases/clase07 commit -m "$(cat <<'EOF'
clase07/MessageBroker: a_DIBroker autocontenida

Co-Authored-By: Claude Sonnet 5 <noreply@anthropic.com>
Claude-Session: https://claude.ai/code/session_01YGZHSkcGRaTFS7xCnNWa5E
EOF
)"
```

---

### Task 11: `03_MessageBroker/b_ScriptableObjectChannels/` autocontenida

**Files:**
- Move: `clases/clase07/Unity/Assets/03_MessageBroker/02_ScriptableObjectChannels/` → `.../b_ScriptableObjectChannels/`
- Move: `.../b_ScriptableObjectChannels/02_MessageBroker_SOChannels.unity` (+ `.meta`) → `.../b_ScriptableObjectChannels/b_MessageBroker_SOChannels.unity`
- Create: `.../b_ScriptableObjectChannels/ScorePickedUpEvent.cs`
- Create: `.../b_ScriptableObjectChannels/Clase07.MessageBroker.ScriptableObjectChannels.asmdef`
- Modify: `.../b_ScriptableObjectChannels/ScoreEventChannelSO.cs`, `.../ScoreEventChannelsDemoView.cs` (namespace/using, comentario pedagógico)
- Move: `clases/clase07/Unity/Assets/03_MessageBroker/Tests/ScoreEventChannelSOTests.cs` → `.../b_ScriptableObjectChannels/Tests/ScoreEventChannelSOTests.cs`
- Create: `.../b_ScriptableObjectChannels/Tests/Clase07.MessageBroker.ScriptableObjectChannels.Tests.asmdef`

**Interfaces:**
- Consumes: nada propio (autocontenida) salvo `TMPro` (paquete de terceros).
- Produces: `Clase07.MessageBroker.ScriptableObjectChannels.ScorePickedUpEvent` (`int Amount`), `ScoreEventChannelSO` sin cambios de firma. Esta carpeta **no** usa `PlayerDamagedEvent` — es una asimetría de alcance ya existente (no se agrega un canal para ese evento acá).

- [ ] **Step 1: Mover la carpeta y renombrar la escena**

```bash
git -C clases/clase07 mv Unity/Assets/03_MessageBroker/02_ScriptableObjectChannels Unity/Assets/03_MessageBroker/b_ScriptableObjectChannels
git -C clases/clase07 mv Unity/Assets/03_MessageBroker/b_ScriptableObjectChannels/02_MessageBroker_SOChannels.unity Unity/Assets/03_MessageBroker/b_ScriptableObjectChannels/b_MessageBroker_SOChannels.unity
git -C clases/clase07 mv Unity/Assets/03_MessageBroker/b_ScriptableObjectChannels/02_MessageBroker_SOChannels.unity.meta Unity/Assets/03_MessageBroker/b_ScriptableObjectChannels/b_MessageBroker_SOChannels.unity.meta
```

- [ ] **Step 2: Copiar el evento que usa (solo Score)**

Crear `clases/clase07/Unity/Assets/03_MessageBroker/b_ScriptableObjectChannels/ScorePickedUpEvent.cs`:

```csharp
namespace Clase07.MessageBroker.ScriptableObjectChannels
{
    public readonly struct ScorePickedUpEvent
    {
        public readonly int Amount;
        public ScorePickedUpEvent(int amount) => Amount = amount;
    }
}
```

- [ ] **Step 3: Sacar el `using Clase07.MessageBroker.Shared;` y agregar el comentario pedagógico**

En `clases/clase07/Unity/Assets/03_MessageBroker/b_ScriptableObjectChannels/ScoreEventChannelSO.cs`,
sacar el using a `Shared` (ya no hace falta, `ScorePickedUpEvent` vive en el mismo
namespace) y comentar el punto clave — el asset en sí es el punto de conexión:

```csharp
using System;
using UnityEngine;

namespace Clase07.MessageBroker.ScriptableObjectChannels
{
    // Cada nuevo listener se conecta arrastrando este asset a un campo
    // [SerializeField] en el Inspector — cero código para agregar un suscriptor
    // nuevo, a diferencia de a_DIBroker/c_MessagePipe donde hace falta inyectar
    // el broker/publisher.
    [CreateAssetMenu(fileName = "ScoreEventChannel", menuName = "Clase07/MessageBroker/Score Event Channel")]
    public class ScoreEventChannelSO : ScriptableObject
    {
        public event Action<ScorePickedUpEvent> OnRaised;
        public void Raise(ScorePickedUpEvent evt) => OnRaised?.Invoke(evt);
    }
}
```

En `clases/clase07/Unity/Assets/03_MessageBroker/b_ScriptableObjectChannels/ScoreEventChannelsDemoView.cs`,
sacar el mismo using:

```csharp
using UnityEngine;
using TMPro;

namespace Clase07.MessageBroker.ScriptableObjectChannels
```

(el resto del archivo no cambia.)

- [ ] **Step 4: Crear el asmdef runtime**

Crear `clases/clase07/Unity/Assets/03_MessageBroker/b_ScriptableObjectChannels/Clase07.MessageBroker.ScriptableObjectChannels.asmdef`:

```json
{
    "name": "Clase07.MessageBroker.ScriptableObjectChannels",
    "rootNamespace": "",
    "references": [
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

- [ ] **Step 5: Mover y adaptar los tests**

```bash
mkdir -p clases/clase07/Unity/Assets/03_MessageBroker/b_ScriptableObjectChannels/Tests
git -C clases/clase07 mv Unity/Assets/03_MessageBroker/Tests/ScoreEventChannelSOTests.cs Unity/Assets/03_MessageBroker/b_ScriptableObjectChannels/Tests/ScoreEventChannelSOTests.cs
git -C clases/clase07 mv Unity/Assets/03_MessageBroker/Tests/ScoreEventChannelSOTests.cs.meta Unity/Assets/03_MessageBroker/b_ScriptableObjectChannels/Tests/ScoreEventChannelSOTests.cs.meta
```

Editar el archivo movido:

```csharp
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Clase07.MessageBroker.ScriptableObjectChannels.Tests
{
    public class ScoreEventChannelSOTests
    {
        [Test]
        public void Raise_NotifiesAllListeners()
        {
            var channel = ScriptableObject.CreateInstance<ScoreEventChannelSO>();
            var received = new List<int>();
            channel.OnRaised += evt => received.Add(evt.Amount);
            channel.OnRaised += evt => received.Add(evt.Amount * 10);

            channel.Raise(new ScorePickedUpEvent(5));

            CollectionAssert.AreEqual(new[] { 5, 50 }, received);
        }

        [Test]
        public void Raise_WithNoListeners_DoesNotThrow()
        {
            var channel = ScriptableObject.CreateInstance<ScoreEventChannelSO>();
            Assert.DoesNotThrow(() => channel.Raise(new ScorePickedUpEvent(5)));
        }
    }
}
```

Crear `clases/clase07/Unity/Assets/03_MessageBroker/b_ScriptableObjectChannels/Tests/Clase07.MessageBroker.ScriptableObjectChannels.Tests.asmdef`:

```json
{
    "name": "Clase07.MessageBroker.ScriptableObjectChannels.Tests",
    "rootNamespace": "",
    "references": [
        "Clase07.MessageBroker.ScriptableObjectChannels",
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

- [ ] **Step 6: Verificar compilación y tests EditMode**

```bash
/Applications/Unity/Hub/Editor/6000.3.21f1/Unity.app/Contents/MacOS/Unity \
  -batchmode -nographic \
  -projectPath clases/clase07/Unity \
  -runTests -testPlatform EditMode \
  -testResults "$(pwd)/clases/clase07/Unity_mb_b_sochannels_editmode.xml" \
  -logFile "$(pwd)/clases/clase07/Unity_mb_b_sochannels_editmode.log"
```

Run: `grep -iE "error CS" clases/clase07/Unity_mb_b_sochannels_editmode.log || echo "sin errores de compilación"`
Expected: `sin errores de compilación`.

Run: `grep -oE 'result="Failed"' clases/clase07/Unity_mb_b_sochannels_editmode.xml | wc -l`
Expected: `0`.

- [ ] **Step 7: Limpiar logs y commitear**

```bash
rm clases/clase07/Unity_mb_b_sochannels_editmode.xml clases/clase07/Unity_mb_b_sochannels_editmode.log
git -C clases/clase07 add Unity/Assets/03_MessageBroker/b_ScriptableObjectChannels Unity/Assets/03_MessageBroker/Tests
git -C clases/clase07 commit -m "$(cat <<'EOF'
clase07/MessageBroker: b_ScriptableObjectChannels autocontenida

Co-Authored-By: Claude Sonnet 5 <noreply@anthropic.com>
Claude-Session: https://claude.ai/code/session_01YGZHSkcGRaTFS7xCnNWa5E
EOF
)"
```

---

### Task 12: `03_MessageBroker/c_MessagePipe/` autocontenida

**Files:**
- Move: `clases/clase07/Unity/Assets/03_MessageBroker/03_MessagePipe/` → `.../c_MessagePipe/`
- Move: `.../c_MessagePipe/03_MessageBroker_MessagePipe.unity` (+ `.meta`) → `.../c_MessagePipe/c_MessageBroker_MessagePipe.unity`
- Create: `.../c_MessagePipe/ScorePickedUpEvent.cs`
- Create: `.../c_MessagePipe/Clase07.MessageBroker.MessagePipeExample.asmdef`
- Modify: `.../c_MessagePipe/MessagePipeLifetimeScope.cs`, `.../MessagePipeDemoView.cs` (namespace/using, comentario pedagógico)
- Move: `clases/clase07/Unity/Assets/03_MessageBroker/Tests/MessagePipeWiringTests.cs` → `.../c_MessagePipe/Tests/MessagePipeWiringTests.cs`
- Create: `.../c_MessagePipe/Tests/Clase07.MessageBroker.MessagePipeExample.Tests.asmdef`

**Interfaces:**
- Consumes: `VContainer`, `MessagePipe`, `MessagePipe.VContainer`, `UniTask` (paquetes de terceros — `UniTask` hace falta como reference directa porque `ISubscriber<T>.Subscribe(...)` toca tipos de UniTask, mismo motivo documentado para el asmdef combinado original).
- Produces: `Clase07.MessageBroker.MessagePipeExample.ScorePickedUpEvent` (`int Amount`), `MessagePipeLifetimeScope.RegisterMessaging(IContainerBuilder)` sin cambios de firma. Esta carpeta tampoco usa `PlayerDamagedEvent` — el código real (`MessagePipeLifetimeScope`/`MessagePipeDemoView`/`MessagePipeWiringTests`) solo publica/suscribe `ScorePickedUpEvent`.

- [ ] **Step 1: Mover la carpeta y renombrar la escena**

```bash
git -C clases/clase07 mv Unity/Assets/03_MessageBroker/03_MessagePipe Unity/Assets/03_MessageBroker/c_MessagePipe
git -C clases/clase07 mv Unity/Assets/03_MessageBroker/c_MessagePipe/03_MessageBroker_MessagePipe.unity Unity/Assets/03_MessageBroker/c_MessagePipe/c_MessageBroker_MessagePipe.unity
git -C clases/clase07 mv Unity/Assets/03_MessageBroker/c_MessagePipe/03_MessageBroker_MessagePipe.unity.meta Unity/Assets/03_MessageBroker/c_MessagePipe/c_MessageBroker_MessagePipe.unity.meta
```

- [ ] **Step 2: Copiar el evento que usa (solo Score)**

Crear `clases/clase07/Unity/Assets/03_MessageBroker/c_MessagePipe/ScorePickedUpEvent.cs`:

```csharp
namespace Clase07.MessageBroker.MessagePipeExample
{
    public readonly struct ScorePickedUpEvent
    {
        public readonly int Amount;
        public ScorePickedUpEvent(int amount) => Amount = amount;
    }
}
```

- [ ] **Step 3: Sacar el `using Clase07.MessageBroker.Shared;` y agregar el comentario pedagógico**

En `clases/clase07/Unity/Assets/03_MessageBroker/c_MessagePipe/MessagePipeLifetimeScope.cs`:

```csharp
using MessagePipe;
using VContainer;
using VContainer.Unity;

namespace Clase07.MessageBroker.MessagePipeExample
{
    public class MessagePipeLifetimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            RegisterMessaging(builder);

            // RegisterComponentInHierarchy necesita la escena del LifetimeScope, así que
            // queda afuera de RegisterMessaging: los tests de EditMode no tienen jerarquía
            // que recorrer, pero sí pueden verificar el registro del broker.
            builder.RegisterComponentInHierarchy<MessagePipeDemoView>();
        }

        // Los registros que no dependen de la escena viven acá para que los tests
        // ejerciten exactamente el mismo cableado que usa la escena, en vez de duplicarlo.
        // RegisterMessagePipe()/RegisterMessageBroker<T>() son lo único que cambia
        // respecto a a_DIBroker: en vez de escribir el broker a mano, se registra el
        // de una librería madura (con soporte async/keyed pub-sub, no usado acá pero
        // disponible sin reescribir nada).
        public static void RegisterMessaging(IContainerBuilder builder)
        {
            var options = builder.RegisterMessagePipe();
            builder.RegisterMessageBroker<ScorePickedUpEvent>(options);
        }
    }
}
```

En `clases/clase07/Unity/Assets/03_MessageBroker/c_MessagePipe/MessagePipeDemoView.cs`, sacar
el using a `Shared` y comentar la inyección de `IPublisher`/`ISubscriber`:

```csharp
using System;
using UnityEngine;
using TMPro;
using VContainer;
using MessagePipe;

namespace Clase07.MessageBroker.MessagePipeExample
{
    public class MessagePipeDemoView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _scoreLabel;

        private IPublisher<ScorePickedUpEvent> _publisher;
        private IDisposable _subscription;
        private int _total;

        // Mismo problema que a_DIBroker.IMessageBroker (Subscribe/Publish genéricos),
        // pero resuelto por MessagePipe: se inyectan interfaces específicas por tipo de
        // mensaje (IPublisher<T>/ISubscriber<T>) en vez de un broker genérico propio.
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

- [ ] **Step 4: Crear el asmdef runtime**

Crear `clases/clase07/Unity/Assets/03_MessageBroker/c_MessagePipe/Clase07.MessageBroker.MessagePipeExample.asmdef`:

```json
{
    "name": "Clase07.MessageBroker.MessagePipeExample",
    "rootNamespace": "",
    "references": [
        "VContainer",
        "MessagePipe",
        "MessagePipe.VContainer",
        "UniTask",
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

- [ ] **Step 5: Mover y adaptar los tests**

```bash
mkdir -p clases/clase07/Unity/Assets/03_MessageBroker/c_MessagePipe/Tests
git -C clases/clase07 mv Unity/Assets/03_MessageBroker/Tests/MessagePipeWiringTests.cs Unity/Assets/03_MessageBroker/c_MessagePipe/Tests/MessagePipeWiringTests.cs
git -C clases/clase07 mv Unity/Assets/03_MessageBroker/Tests/MessagePipeWiringTests.cs.meta Unity/Assets/03_MessageBroker/c_MessagePipe/Tests/MessagePipeWiringTests.cs.meta
```

Editar el archivo movido:

```csharp
using NUnit.Framework;
using MessagePipe;
using VContainer;

namespace Clase07.MessageBroker.MessagePipeExample.Tests
{
    // El test llama al mismo MessagePipeLifetimeScope.RegisterMessaging que usa la
    // escena: si alguien rompe ese cableado, este test lo detecta.
    public class MessagePipeWiringTests
    {
        [Test]
        public void LifetimeScopeRegistrations_DeliverPublishedMessageToSubscriber()
        {
            var builder = new ContainerBuilder();
            MessagePipeLifetimeScope.RegisterMessaging(builder);

            using var container = builder.Build();

            var publisher = container.Resolve<IPublisher<ScorePickedUpEvent>>();
            var subscriber = container.Resolve<ISubscriber<ScorePickedUpEvent>>();

            ScorePickedUpEvent? received = null;
            using var subscription = subscriber.Subscribe(e => received = e);

            publisher.Publish(new ScorePickedUpEvent(10));

            Assert.AreEqual(10, received.Value.Amount);
        }
    }
}
```

Crear `clases/clase07/Unity/Assets/03_MessageBroker/c_MessagePipe/Tests/Clase07.MessageBroker.MessagePipeExample.Tests.asmdef`:

```json
{
    "name": "Clase07.MessageBroker.MessagePipeExample.Tests",
    "rootNamespace": "",
    "references": [
        "Clase07.MessageBroker.MessagePipeExample",
        "VContainer",
        "MessagePipe",
        "MessagePipe.VContainer",
        "UniTask",
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

- [ ] **Step 6: Verificar compilación y tests EditMode**

```bash
/Applications/Unity/Hub/Editor/6000.3.21f1/Unity.app/Contents/MacOS/Unity \
  -batchmode -nographic \
  -projectPath clases/clase07/Unity \
  -runTests -testPlatform EditMode \
  -testResults "$(pwd)/clases/clase07/Unity_mb_c_messagepipe_editmode.xml" \
  -logFile "$(pwd)/clases/clase07/Unity_mb_c_messagepipe_editmode.log"
```

Run: `grep -iE "error CS" clases/clase07/Unity_mb_c_messagepipe_editmode.log || echo "sin errores de compilación"`
Expected: `sin errores de compilación`.

Run: `grep -oE 'result="Failed"' clases/clase07/Unity_mb_c_messagepipe_editmode.xml | wc -l`
Expected: `0`.

- [ ] **Step 7: Limpiar logs y commitear**

```bash
rm clases/clase07/Unity_mb_c_messagepipe_editmode.xml clases/clase07/Unity_mb_c_messagepipe_editmode.log
git -C clases/clase07 add Unity/Assets/03_MessageBroker/c_MessagePipe Unity/Assets/03_MessageBroker/Tests
git -C clases/clase07 commit -m "$(cat <<'EOF'
clase07/MessageBroker: c_MessagePipe autocontenida

Co-Authored-By: Claude Sonnet 5 <noreply@anthropic.com>
Claude-Session: https://claude.ai/code/session_01YGZHSkcGRaTFS7xCnNWa5E
EOF
)"
```

---

### Task 13: Limpieza del módulo `03_MessageBroker`

**Files:**
- Delete: `clases/clase07/Unity/Assets/03_MessageBroker/Shared/` (2 `.cs` + `.meta` + carpeta `.meta`)
- Delete: `clases/clase07/Unity/Assets/03_MessageBroker/Tests/` (si queda vacía tras Tasks 10..12)
- Delete: `clases/clase07/Unity/Assets/03_MessageBroker/Clase07.MessageBroker.asmdef`
- Delete: `clases/clase07/Unity/Assets/03_MessageBroker/Tests/Clase07.MessageBroker.Tests.asmdef` (borrado junto con `Tests/`)

**Interfaces:** Ninguna — solo borrado de lo que quedó sin referencias.

- [ ] **Step 1: Confirmar que nada referencia ya el namespace `Shared` ni los asmdefs de módulo**

```bash
grep -rl "Clase07.MessageBroker.Shared" clases/clase07/Unity/Assets/03_MessageBroker || echo "sin referencias a Shared"
```

Expected: `sin referencias a Shared`. Si aparece algún archivo, es señal de que alguna de
las Tasks 10/11/12 quedó incompleta — resolver antes de continuar.

- [ ] **Step 2: Borrar `Shared/` y la carpeta `Tests/` de módulo (si quedó vacía)**

```bash
git -C clases/clase07 rm -r Unity/Assets/03_MessageBroker/Shared
ls clases/clase07/Unity/Assets/03_MessageBroker/Tests
```

Si `Tests/` (nivel de módulo) quedó vacía (sin `.cs` ni `.asmdef`, ya que
`MessageBrokerTests.cs`, `ScoreEventChannelSOTests.cs` y `MessagePipeWiringTests.cs` se
movieron en Tasks 10/11/12):

```bash
git -C clases/clase07 rm -r Unity/Assets/03_MessageBroker/Tests
```

- [ ] **Step 3: Borrar los asmdefs de módulo**

```bash
git -C clases/clase07 rm Unity/Assets/03_MessageBroker/Clase07.MessageBroker.asmdef
```

(`Clase07.MessageBroker.Tests.asmdef` ya se borró junto con `Tests/` en el Step 2 si esa
carpeta estaba vacía; si por algún motivo sobrevivió algún archivo ahí, borrar el asmdef
de test explícitamente con `git rm Unity/Assets/03_MessageBroker/Tests/Clase07.MessageBroker.Tests.asmdef`.)

- [ ] **Step 4: Verificación final del módulo completo**

```bash
/Applications/Unity/Hub/Editor/6000.3.21f1/Unity.app/Contents/MacOS/Unity \
  -batchmode -nographic \
  -projectPath clases/clase07/Unity \
  -runTests -testPlatform EditMode \
  -testResults "$(pwd)/clases/clase07/Unity_mb_final_editmode.xml" \
  -logFile "$(pwd)/clases/clase07/Unity_mb_final_editmode.log"
```

Run: `grep -iE "error CS" clases/clase07/Unity_mb_final_editmode.log || echo "sin errores de compilación"`
Expected: `sin errores de compilación`.

Run: `grep -oE 'result="Failed"' clases/clase07/Unity_mb_final_editmode.xml | wc -l`
Expected: `0`.

- [ ] **Step 5: Limpiar logs y commitear**

```bash
rm clases/clase07/Unity_mb_final_editmode.xml clases/clase07/Unity_mb_final_editmode.log
git -C clases/clase07 status
git -C clases/clase07 commit -m "$(cat <<'EOF'
clase07/MessageBroker: borrar Shared/ y asmdefs de módulo tras autocontener las 3 implementaciones

Co-Authored-By: Claude Sonnet 5 <noreply@anthropic.com>
Claude-Session: https://claude.ai/code/session_01YGZHSkcGRaTFS7xCnNWa5E
EOF
)"
```
### Task 14: `a_MVC/` autocontenida

**Files:**
- Move: `clases/clase07/Unity/Assets/04_MVC_MVP_MVVM/01_MVC/` → `clases/clase07/Unity/Assets/04_MVC_MVP_MVVM/a_MVC/` (incluye `01_Mvx_MVC.unity`+`.meta` renombrada a `a_Mvx_MVC.unity`, `InventoryController.cs`+`.meta`)
- Create: `clases/clase07/Unity/Assets/04_MVC_MVP_MVVM/a_MVC/InventoryItem.cs`
- Create: `clases/clase07/Unity/Assets/04_MVC_MVP_MVVM/a_MVC/InventoryModel.cs`
- Create: `clases/clase07/Unity/Assets/04_MVC_MVP_MVVM/a_MVC/InventoryItemRowView.cs`
- Create: `clases/clase07/Unity/Assets/04_MVC_MVP_MVVM/a_MVC/InventoryItemRow.prefab`
- Create: `clases/clase07/Unity/Assets/04_MVC_MVP_MVVM/a_MVC/Clase07.Mvx.Mvc.asmdef`
- Modify: `clases/clase07/Unity/Assets/04_MVC_MVP_MVVM/a_MVC/InventoryController.cs`
- Create: `clases/clase07/Unity/Assets/04_MVC_MVP_MVVM/a_MVC/Tests/Clase07.Mvx.Mvc.Tests.asmdef`
- Create: `clases/clase07/Unity/Assets/04_MVC_MVP_MVVM/a_MVC/Tests/InventoryModelTests.cs`
- Create (herramienta temporal, se reusa en Tasks 15/16, se borra en Task 17): `clases/clase07/Unity/Assets/04_MVC_MVP_MVVM/_Migration/RepointRowPrefabReference.cs`
- Create: `clases/clase07/Unity/Assets/04_MVC_MVP_MVVM/_Migration/Clase07.Mvx.Migration.Editor.asmdef`

**Interfaces:**
- Consumes: nada de otro módulo/implementación.
- Produces: `Clase07.Mvx.Mvc.InventoryItem` (`Name`, `Quantity`), `Clase07.Mvx.Mvc.InventoryModel` (`AddItem(string)`, `RemoveItem(string)`, `Items: IReadOnlyList<InventoryItem>`, `event Action Changed`), `Clase07.Mvx.Mvc.InventoryItemRowView` (`SetLabel(string)`, `SetRemoveAction(UnityAction)`), `Clase07.Mvx.Mvc.InventoryController` (`OnAddClicked()`).

Nota sobre el prefab: `InventoryController._rowPrefab` es una referencia serializada por GUID en la escena — copiar el `.prefab` sin su `.meta` hace que Unity le asigne un GUID nuevo al importar, lo cual deja la referencia de la escena (que apunta al GUID viejo de `Shared/`) rota. El Step 6 usa una herramienta de Editor (`RepointRowPrefabReference`) para reapuntar la escena al prefab nuevo después de copiarlo. Es la misma herramienta que reusan 15 y 16 (mismo problema, mismo mecanismo) — se crea una sola vez acá.

- [ ] **Step 1: mover carpeta y escena**

```bash
cd /Users/giga/code/Programacion-de-VideoJuegos-III-2026
git mv clases/clase07/Unity/Assets/04_MVC_MVP_MVVM/01_MVC clases/clase07/Unity/Assets/04_MVC_MVP_MVVM/a_MVC
git mv clases/clase07/Unity/Assets/04_MVC_MVP_MVVM/a_MVC/01_Mvx_MVC.unity clases/clase07/Unity/Assets/04_MVC_MVP_MVVM/a_MVC/a_Mvx_MVC.unity
git mv clases/clase07/Unity/Assets/04_MVC_MVP_MVVM/a_MVC/01_Mvx_MVC.unity.meta clases/clase07/Unity/Assets/04_MVC_MVP_MVVM/a_MVC/a_Mvx_MVC.unity.meta
```

- [ ] **Step 2: copiar el dominio compartido, namespace fusionado a `Clase07.Mvx.Mvc`**

Crear `clases/clase07/Unity/Assets/04_MVC_MVP_MVVM/a_MVC/InventoryItem.cs`:

```csharp
namespace Clase07.Mvx.Mvc
{
    public class InventoryItem
    {
        public string Name { get; }
        public int Quantity { get; set; }

        public InventoryItem(string name, int quantity)
        {
            Name = name;
            Quantity = quantity;
        }
    }
}
```

Crear `clases/clase07/Unity/Assets/04_MVC_MVP_MVVM/a_MVC/InventoryModel.cs`:

```csharp
using System;
using System.Collections.Generic;
using System.Linq;

namespace Clase07.Mvx.Mvc
{
    public class InventoryModel
    {
        private readonly List<InventoryItem> _items = new List<InventoryItem>();
        public IReadOnlyList<InventoryItem> Items => _items;
        public event Action Changed;

        public void AddItem(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("El nombre no puede estar vacío", nameof(name));
            }

            var existing = _items.FirstOrDefault(i => i.Name == name);
            if (existing != null) existing.Quantity++;
            else _items.Add(new InventoryItem(name, 1));

            Changed?.Invoke();
        }

        public void RemoveItem(string name)
        {
            var existing = _items.FirstOrDefault(i => i.Name == name);
            if (existing == null) return;

            existing.Quantity--;
            if (existing.Quantity <= 0) _items.Remove(existing);

            Changed?.Invoke();
        }
    }
}
```

Crear `clases/clase07/Unity/Assets/04_MVC_MVP_MVVM/a_MVC/InventoryItemRowView.cs`:

```csharp
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Clase07.Mvx.Mvc
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

Copiar el prefab (sin su `.meta` — Unity le asigna uno nuevo al importar):

```bash
cp clases/clase07/Unity/Assets/04_MVC_MVP_MVVM/Shared/InventoryItemRow.prefab clases/clase07/Unity/Assets/04_MVC_MVP_MVVM/a_MVC/InventoryItemRow.prefab
```

- [ ] **Step 3: asmdef runtime**

Crear `clases/clase07/Unity/Assets/04_MVC_MVP_MVVM/a_MVC/Clase07.Mvx.Mvc.asmdef`:

```json
{
    "name": "Clase07.Mvx.Mvc",
    "rootNamespace": "",
    "references": [
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

- [ ] **Step 4: ajustar `InventoryController.cs` — quitar el `using` a `Shared` y agregar el comentario pedagógico**

Reemplazar el contenido completo de `clases/clase07/Unity/Assets/04_MVC_MVP_MVVM/a_MVC/InventoryController.cs`:

```csharp
using UnityEngine;
using TMPro;

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

        // A propósito el controller toca directamente los elementos concretos de la
        // vista (instancia/destruye filas acá adentro) — no hay una interfaz de vista
        // de por medio. Es lo que hace que este patrón sea difícil de testear sin
        // Unity: no existe una "lógica de presentación" separable de la manipulación
        // de GameObjects (comparar con b_MVP/IInventoryView y c_MVVM).
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

- [ ] **Step 5: herramienta temporal de Editor para reapuntar el prefab en la escena**

Crear `clases/clase07/Unity/Assets/04_MVC_MVP_MVVM/_Migration/Clase07.Mvx.Migration.Editor.asmdef`:

```json
{
    "name": "Clase07.Mvx.Migration.Editor",
    "rootNamespace": "",
    "references": [],
    "includePlatforms": [
        "Editor"
    ],
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

Crear `clases/clase07/Unity/Assets/04_MVC_MVP_MVVM/_Migration/RepointRowPrefabReference.cs`:

```csharp
using System;
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

            prop.objectReferenceValue = newPrefab;
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
```

- [ ] **Step 6: correr la herramienta para reapuntar `a_Mvx_MVC.unity` al prefab nuevo**

```bash
cd /Users/giga/code/Programacion-de-VideoJuegos-III-2026
/Applications/Unity/Hub/Editor/6000.3.21f1/Unity.app/Contents/MacOS/Unity \
  -batchmode -nographic \
  -projectPath clases/clase07/Unity \
  -executeMethod Clase07.EditorTools.RepointRowPrefabReference.Run \
  -scenePath "Assets/04_MVC_MVP_MVVM/a_MVC/a_Mvx_MVC.unity" \
  -componentType "Clase07.Mvx.Mvc.InventoryController, Clase07.Mvx.Mvc" \
  -fieldName "_rowPrefab" \
  -newPrefabPath "Assets/04_MVC_MVP_MVVM/a_MVC/InventoryItemRow.prefab" \
  -logFile "$(pwd)/clases/clase07/Unity_mvx_a_mvc_repoint.log"
```

Run: `grep -iE "error|No se (encontró|pudo)" clases/clase07/Unity_mvx_a_mvc_repoint.log || echo ok`
Expected: `ok`. Si falla, revisar el log — el motivo más probable es un typo en `componentType`/`fieldName`.

```bash
rm clases/clase07/Unity_mvx_a_mvc_repoint.log
```

- [ ] **Step 7: copiar el test de `InventoryModel` y crear el asmdef de test**

Crear `clases/clase07/Unity/Assets/04_MVC_MVP_MVVM/a_MVC/Tests/Clase07.Mvx.Mvc.Tests.asmdef`:

```json
{
    "name": "Clase07.Mvx.Mvc.Tests",
    "rootNamespace": "",
    "references": [
        "Clase07.Mvx.Mvc",
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

Crear `clases/clase07/Unity/Assets/04_MVC_MVP_MVVM/a_MVC/Tests/InventoryModelTests.cs`:

```csharp
using NUnit.Framework;
using System.Linq;
using Clase07.Mvx.Mvc;

namespace Clase07.Mvx.Mvc.Tests
{
    public class InventoryModelTests
    {
        [Test]
        public void AddItem_NewName_AddsWithQuantityOne()
        {
            var model = new InventoryModel();
            model.AddItem("Potion");
            Assert.AreEqual(1, model.Items.Single(i => i.Name == "Potion").Quantity);
        }

        [Test]
        public void AddItem_ExistingName_IncrementsQuantity()
        {
            var model = new InventoryModel();
            model.AddItem("Potion");
            model.AddItem("Potion");
            Assert.AreEqual(2, model.Items.Single(i => i.Name == "Potion").Quantity);
        }

        [Test]
        public void AddItem_EmptyName_Throws()
        {
            var model = new InventoryModel();
            Assert.Throws<System.ArgumentException>(() => model.AddItem(""));
        }

        [Test]
        public void RemoveItem_DecrementsQuantity_AndRemovesAtZero()
        {
            var model = new InventoryModel();
            model.AddItem("Potion");
            model.AddItem("Potion");

            model.RemoveItem("Potion");
            Assert.AreEqual(1, model.Items.Single(i => i.Name == "Potion").Quantity);

            model.RemoveItem("Potion");
            Assert.IsEmpty(model.Items);
        }

        [Test]
        public void RemoveItem_UnknownName_IsNoOp()
        {
            var model = new InventoryModel();
            Assert.DoesNotThrow(() => model.RemoveItem("Nothing"));
            Assert.IsEmpty(model.Items);
        }

        [Test]
        public void Changed_FiresOnAddAndRemove()
        {
            var model = new InventoryModel();
            var fireCount = 0;
            model.Changed += () => fireCount++;

            model.AddItem("Potion");
            model.RemoveItem("Potion");

            Assert.AreEqual(2, fireCount);
        }
    }
}
```

- [ ] **Step 8: verificar EditMode**

```bash
cd /Users/giga/code/Programacion-de-VideoJuegos-III-2026
/Applications/Unity/Hub/Editor/6000.3.21f1/Unity.app/Contents/MacOS/Unity \
  -batchmode -nographic \
  -projectPath clases/clase07/Unity \
  -runTests -testPlatform EditMode \
  -testResults "$(pwd)/clases/clase07/Unity_mvx_a_mvc_editmode.xml" \
  -logFile "$(pwd)/clases/clase07/Unity_mvx_a_mvc_editmode.log"
```

Run: `grep -c 'result="Failed"' clases/clase07/Unity_mvx_a_mvc_editmode.xml || true`
Expected: `0` (o el grep no encuentra matches, lo cual también es correcto).

```bash
rm clases/clase07/Unity_mvx_a_mvc_editmode.xml clases/clase07/Unity_mvx_a_mvc_editmode.log
```

- [ ] **Step 9: commit**

```bash
git add clases/clase07/Unity/Assets/04_MVC_MVP_MVVM/a_MVC clases/clase07/Unity/Assets/04_MVC_MVP_MVVM/_Migration
git commit -m "$(cat <<'EOF'
clase07/Mvx: a_MVC autocontenida

Co-Authored-By: Claude Sonnet 5 <noreply@anthropic.com>
Claude-Session: https://claude.ai/code/session_01YGZHSkcGRaTFS7xCnNWa5E
EOF
)"
```

---

### Task 15: `b_MVP/` autocontenida

**Files:**
- Move: `clases/clase07/Unity/Assets/04_MVC_MVP_MVVM/02_MVP/` → `clases/clase07/Unity/Assets/04_MVC_MVP_MVVM/b_MVP/` (incluye `02_Mvx_MVP.unity`+`.meta` renombrada a `b_Mvx_MVP.unity`, `IInventoryView.cs`, `InventoryMvpView.cs`, `InventoryPresenter.cs` +`.meta`)
- Create: `clases/clase07/Unity/Assets/04_MVC_MVP_MVVM/b_MVP/InventoryItem.cs`
- Create: `clases/clase07/Unity/Assets/04_MVC_MVP_MVVM/b_MVP/InventoryModel.cs`
- Create: `clases/clase07/Unity/Assets/04_MVC_MVP_MVVM/b_MVP/InventoryItemRowView.cs`
- Create: `clases/clase07/Unity/Assets/04_MVC_MVP_MVVM/b_MVP/InventoryItemRow.prefab`
- Create: `clases/clase07/Unity/Assets/04_MVC_MVP_MVVM/b_MVP/Clase07.Mvx.Mvp.asmdef`
- Modify: `clases/clase07/Unity/Assets/04_MVC_MVP_MVVM/b_MVP/IInventoryView.cs`
- Modify: `clases/clase07/Unity/Assets/04_MVC_MVP_MVVM/b_MVP/InventoryMvpView.cs`
- Modify: `clases/clase07/Unity/Assets/04_MVC_MVP_MVVM/b_MVP/InventoryPresenter.cs`
- Create: `clases/clase07/Unity/Assets/04_MVC_MVP_MVVM/b_MVP/Tests/Clase07.Mvx.Mvp.Tests.asmdef`
- Create: `clases/clase07/Unity/Assets/04_MVC_MVP_MVVM/b_MVP/Tests/InventoryModelTests.cs`
- Create: `clases/clase07/Unity/Assets/04_MVC_MVP_MVVM/b_MVP/Tests/InventoryPresenterTests.cs`

**Interfaces:**
- Consumes: `Clase07.EditorTools.RepointRowPrefabReference.Run` (de Task 14, `_Migration/`).
- Produces: `Clase07.Mvx.Mvp.IInventoryView` (`event Action<string> AddRequested`, `event Action<string> RemoveRequested`, `ShowItems(IReadOnlyList<InventoryItem>)`), `Clase07.Mvx.Mvp.InventoryPresenter`, `Clase07.Mvx.Mvp.InventoryMvpView` (`OnAddClicked()`).

- [ ] **Step 1: mover carpeta y escena**

```bash
cd /Users/giga/code/Programacion-de-VideoJuegos-III-2026
git mv clases/clase07/Unity/Assets/04_MVC_MVP_MVVM/02_MVP clases/clase07/Unity/Assets/04_MVC_MVP_MVVM/b_MVP
git mv clases/clase07/Unity/Assets/04_MVC_MVP_MVVM/b_MVP/02_Mvx_MVP.unity clases/clase07/Unity/Assets/04_MVC_MVP_MVVM/b_MVP/b_Mvx_MVP.unity
git mv clases/clase07/Unity/Assets/04_MVC_MVP_MVVM/b_MVP/02_Mvx_MVP.unity.meta clases/clase07/Unity/Assets/04_MVC_MVP_MVVM/b_MVP/b_Mvx_MVP.unity.meta
```

- [ ] **Step 2: copiar el dominio compartido, namespace fusionado a `Clase07.Mvx.Mvp`**

Crear `clases/clase07/Unity/Assets/04_MVC_MVP_MVVM/b_MVP/InventoryItem.cs` (mismo contenido que en `a_MVC`, namespace `Clase07.Mvx.Mvp`):

```csharp
namespace Clase07.Mvx.Mvp
{
    public class InventoryItem
    {
        public string Name { get; }
        public int Quantity { get; set; }

        public InventoryItem(string name, int quantity)
        {
            Name = name;
            Quantity = quantity;
        }
    }
}
```

Crear `clases/clase07/Unity/Assets/04_MVC_MVP_MVVM/b_MVP/InventoryModel.cs`:

```csharp
using System;
using System.Collections.Generic;
using System.Linq;

namespace Clase07.Mvx.Mvp
{
    public class InventoryModel
    {
        private readonly List<InventoryItem> _items = new List<InventoryItem>();
        public IReadOnlyList<InventoryItem> Items => _items;
        public event Action Changed;

        public void AddItem(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("El nombre no puede estar vacío", nameof(name));
            }

            var existing = _items.FirstOrDefault(i => i.Name == name);
            if (existing != null) existing.Quantity++;
            else _items.Add(new InventoryItem(name, 1));

            Changed?.Invoke();
        }

        public void RemoveItem(string name)
        {
            var existing = _items.FirstOrDefault(i => i.Name == name);
            if (existing == null) return;

            existing.Quantity--;
            if (existing.Quantity <= 0) _items.Remove(existing);

            Changed?.Invoke();
        }
    }
}
```

Crear `clases/clase07/Unity/Assets/04_MVC_MVP_MVVM/b_MVP/InventoryItemRowView.cs`:

```csharp
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Clase07.Mvx.Mvp
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

```bash
cp clases/clase07/Unity/Assets/04_MVC_MVP_MVVM/Shared/InventoryItemRow.prefab clases/clase07/Unity/Assets/04_MVC_MVP_MVVM/b_MVP/InventoryItemRow.prefab
```

- [ ] **Step 3: asmdef runtime**

Crear `clases/clase07/Unity/Assets/04_MVC_MVP_MVVM/b_MVP/Clase07.Mvx.Mvp.asmdef`:

```json
{
    "name": "Clase07.Mvx.Mvp",
    "rootNamespace": "",
    "references": [
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

- [ ] **Step 4: ajustar `IInventoryView.cs` — quitar el `using` a `Shared` y agregar el comentario pedagógico**

Reemplazar el contenido completo de `clases/clase07/Unity/Assets/04_MVC_MVP_MVVM/b_MVP/IInventoryView.cs`:

```csharp
using System;
using System.Collections.Generic;

namespace Clase07.Mvx.Mvp
{
    // El presenter (ver InventoryPresenter) habla únicamente con esta interfaz —
    // nunca con InventoryMvpView ni con ningún tipo de UnityEngine. Por eso se puede
    // testear con un fake en C# puro, sin abrir Unity (ver Tests/InventoryPresenterTests.cs),
    // a diferencia de a_MVC donde la vista y la lógica están mezcladas.
    public interface IInventoryView
    {
        event Action<string> AddRequested;
        event Action<string> RemoveRequested;
        void ShowItems(IReadOnlyList<InventoryItem> items);
    }
}
```

- [ ] **Step 5: ajustar `InventoryMvpView.cs` — quitar el `using` a `Shared`**

Reemplazar el contenido completo de `clases/clase07/Unity/Assets/04_MVC_MVP_MVVM/b_MVP/InventoryMvpView.cs`:

```csharp
using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

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

- [ ] **Step 6: ajustar `InventoryPresenter.cs` — quitar el `using` a `Shared` y agregar el comentario pedagógico**

Reemplazar el contenido completo de `clases/clase07/Unity/Assets/04_MVC_MVP_MVVM/b_MVP/InventoryPresenter.cs`:

```csharp
namespace Clase07.Mvx.Mvp
{
    // Ninguna línea de esta clase importa UnityEngine: solo conoce InventoryModel
    // (dominio) e IInventoryView (interfaz pasiva). Por eso es la primera de las tres
    // variantes testeable con un fake, sin levantar Unity.
    public class InventoryPresenter
    {
        private readonly InventoryModel _model;
        private readonly IInventoryView _view;

        public InventoryPresenter(InventoryModel model, IInventoryView view)
        {
            _model = model;
            _view = view;
            _view.AddRequested += OnAddRequested;
            _view.RemoveRequested += OnRemoveRequested;
            _model.Changed += Render;
            Render();
        }

        private void OnAddRequested(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return;
            _model.AddItem(name);
        }

        private void OnRemoveRequested(string name) => _model.RemoveItem(name);

        private void Render() => _view.ShowItems(_model.Items);
    }
}
```

- [ ] **Step 7: reapuntar `b_Mvx_MVP.unity` al prefab nuevo**

```bash
cd /Users/giga/code/Programacion-de-VideoJuegos-III-2026
/Applications/Unity/Hub/Editor/6000.3.21f1/Unity.app/Contents/MacOS/Unity \
  -batchmode -nographic \
  -projectPath clases/clase07/Unity \
  -executeMethod Clase07.EditorTools.RepointRowPrefabReference.Run \
  -scenePath "Assets/04_MVC_MVP_MVVM/b_MVP/b_Mvx_MVP.unity" \
  -componentType "Clase07.Mvx.Mvp.InventoryMvpView, Clase07.Mvx.Mvp" \
  -fieldName "_rowPrefab" \
  -newPrefabPath "Assets/04_MVC_MVP_MVVM/b_MVP/InventoryItemRow.prefab" \
  -logFile "$(pwd)/clases/clase07/Unity_mvx_b_mvp_repoint.log"
```

Run: `grep -iE "error|No se (encontró|pudo)" clases/clase07/Unity_mvx_b_mvp_repoint.log || echo ok`
Expected: `ok`.

```bash
rm clases/clase07/Unity_mvx_b_mvp_repoint.log
```

- [ ] **Step 8: copiar el test de `InventoryModel`, mover `InventoryPresenterTests.cs`, crear el asmdef de test**

Crear `clases/clase07/Unity/Assets/04_MVC_MVP_MVVM/b_MVP/Tests/Clase07.Mvx.Mvp.Tests.asmdef`:

```json
{
    "name": "Clase07.Mvx.Mvp.Tests",
    "rootNamespace": "",
    "references": [
        "Clase07.Mvx.Mvp",
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

Crear `clases/clase07/Unity/Assets/04_MVC_MVP_MVVM/b_MVP/Tests/InventoryModelTests.cs` (mismo contenido que en Task 14 Step 7, namespace `Clase07.Mvx.Mvp.Tests` importando `Clase07.Mvx.Mvp`):

```csharp
using NUnit.Framework;
using System.Linq;
using Clase07.Mvx.Mvp;

namespace Clase07.Mvx.Mvp.Tests
{
    public class InventoryModelTests
    {
        [Test]
        public void AddItem_NewName_AddsWithQuantityOne()
        {
            var model = new InventoryModel();
            model.AddItem("Potion");
            Assert.AreEqual(1, model.Items.Single(i => i.Name == "Potion").Quantity);
        }

        [Test]
        public void AddItem_ExistingName_IncrementsQuantity()
        {
            var model = new InventoryModel();
            model.AddItem("Potion");
            model.AddItem("Potion");
            Assert.AreEqual(2, model.Items.Single(i => i.Name == "Potion").Quantity);
        }

        [Test]
        public void AddItem_EmptyName_Throws()
        {
            var model = new InventoryModel();
            Assert.Throws<System.ArgumentException>(() => model.AddItem(""));
        }

        [Test]
        public void RemoveItem_DecrementsQuantity_AndRemovesAtZero()
        {
            var model = new InventoryModel();
            model.AddItem("Potion");
            model.AddItem("Potion");

            model.RemoveItem("Potion");
            Assert.AreEqual(1, model.Items.Single(i => i.Name == "Potion").Quantity);

            model.RemoveItem("Potion");
            Assert.IsEmpty(model.Items);
        }

        [Test]
        public void RemoveItem_UnknownName_IsNoOp()
        {
            var model = new InventoryModel();
            Assert.DoesNotThrow(() => model.RemoveItem("Nothing"));
            Assert.IsEmpty(model.Items);
        }

        [Test]
        public void Changed_FiresOnAddAndRemove()
        {
            var model = new InventoryModel();
            var fireCount = 0;
            model.Changed += () => fireCount++;

            model.AddItem("Potion");
            model.RemoveItem("Potion");

            Assert.AreEqual(2, fireCount);
        }
    }
}
```

Mover `InventoryPresenterTests.cs` (contenido sin cambios más allá del namespace de los `using`, ya que `Clase07.Mvx.Shared`/`Clase07.Mvx.Mvp` se funden en uno solo):

```bash
git mv clases/clase07/Unity/Assets/04_MVC_MVP_MVVM/Tests/InventoryPresenterTests.cs clases/clase07/Unity/Assets/04_MVC_MVP_MVVM/b_MVP/Tests/InventoryPresenterTests.cs
git mv clases/clase07/Unity/Assets/04_MVC_MVP_MVVM/Tests/InventoryPresenterTests.cs.meta clases/clase07/Unity/Assets/04_MVC_MVP_MVVM/b_MVP/Tests/InventoryPresenterTests.cs.meta
```

Editar el archivo movido para quitar el `using Clase07.Mvx.Shared;` (ya no hace falta, `InventoryItem`/`InventoryModel` están en el mismo namespace `Clase07.Mvx.Mvp` que el resto del archivo):

```csharp
using System;
using System.Collections.Generic;
using NUnit.Framework;
using Clase07.Mvx.Mvp;

namespace Clase07.Mvx.Mvp.Tests
{
    public class FakeInventoryView : IInventoryView
    {
        public event Action<string> AddRequested;
        public event Action<string> RemoveRequested;
        public IReadOnlyList<InventoryItem> LastShown { get; private set; }

        public void ShowItems(IReadOnlyList<InventoryItem> items) => LastShown = items;
        public void RaiseAddRequested(string name) => AddRequested?.Invoke(name);
        public void RaiseRemoveRequested(string name) => RemoveRequested?.Invoke(name);
    }

    public class InventoryPresenterTests
    {
        [Test]
        public void Construction_RendersEmptyInitialState()
        {
            var view = new FakeInventoryView();
            new InventoryPresenter(new InventoryModel(), view);

            Assert.IsEmpty(view.LastShown);
        }

        [Test]
        public void AddRequested_AddsItemAndRerenders()
        {
            var view = new FakeInventoryView();
            new InventoryPresenter(new InventoryModel(), view);

            view.RaiseAddRequested("Potion");

            Assert.AreEqual(1, view.LastShown.Count);
            Assert.AreEqual("Potion", view.LastShown[0].Name);
        }

        [Test]
        public void RemoveRequested_RemovesItemAndRerenders()
        {
            var view = new FakeInventoryView();
            new InventoryPresenter(new InventoryModel(), view);
            view.RaiseAddRequested("Potion");

            view.RaiseRemoveRequested("Potion");

            Assert.IsEmpty(view.LastShown);
        }

        [Test]
        public void AddRequested_WithEmptyName_DoesNotAddItem()
        {
            var view = new FakeInventoryView();
            new InventoryPresenter(new InventoryModel(), view);

            view.RaiseAddRequested("   ");

            Assert.IsEmpty(view.LastShown);
        }
    }
}
```

- [ ] **Step 9: verificar EditMode**

```bash
cd /Users/giga/code/Programacion-de-VideoJuegos-III-2026
/Applications/Unity/Hub/Editor/6000.3.21f1/Unity.app/Contents/MacOS/Unity \
  -batchmode -nographic \
  -projectPath clases/clase07/Unity \
  -runTests -testPlatform EditMode \
  -testResults "$(pwd)/clases/clase07/Unity_mvx_b_mvp_editmode.xml" \
  -logFile "$(pwd)/clases/clase07/Unity_mvx_b_mvp_editmode.log"
```

Run: `grep -c 'result="Failed"' clases/clase07/Unity_mvx_b_mvp_editmode.xml || true`
Expected: `0`.

```bash
rm clases/clase07/Unity_mvx_b_mvp_editmode.xml clases/clase07/Unity_mvx_b_mvp_editmode.log
```

- [ ] **Step 10: commit**

```bash
git add clases/clase07/Unity/Assets/04_MVC_MVP_MVVM/b_MVP clases/clase07/Unity/Assets/04_MVC_MVP_MVVM/Tests
git commit -m "$(cat <<'EOF'
clase07/Mvx: b_MVP autocontenida

Co-Authored-By: Claude Sonnet 5 <noreply@anthropic.com>
Claude-Session: https://claude.ai/code/session_01YGZHSkcGRaTFS7xCnNWa5E
EOF
)"
```

---

### Task 16: `c_MVVM/` autocontenida

**Files:**
- Move: `clases/clase07/Unity/Assets/04_MVC_MVP_MVVM/03_MVVM/` → `clases/clase07/Unity/Assets/04_MVC_MVP_MVVM/c_MVVM/` (incluye `03_Mvx_MVVM.unity`+`.meta` renombrada a `c_Mvx_MVVM.unity`, `InventoryItemViewModel.cs`, `InventoryMvvmView.cs`, `InventoryViewModel.cs`, `RelayCommand.cs` +`.meta`)
- Create: `clases/clase07/Unity/Assets/04_MVC_MVP_MVVM/c_MVVM/InventoryItem.cs`
- Create: `clases/clase07/Unity/Assets/04_MVC_MVP_MVVM/c_MVVM/InventoryModel.cs`
- Create: `clases/clase07/Unity/Assets/04_MVC_MVP_MVVM/c_MVVM/InventoryItemRowView.cs`
- Create: `clases/clase07/Unity/Assets/04_MVC_MVP_MVVM/c_MVVM/InventoryItemRow.prefab`
- Create: `clases/clase07/Unity/Assets/04_MVC_MVP_MVVM/c_MVVM/Clase07.Mvx.Mvvm.asmdef`
- Modify: `clases/clase07/Unity/Assets/04_MVC_MVP_MVVM/c_MVVM/InventoryItemViewModel.cs`
- Modify: `clases/clase07/Unity/Assets/04_MVC_MVP_MVVM/c_MVVM/InventoryMvvmView.cs`
- Modify: `clases/clase07/Unity/Assets/04_MVC_MVP_MVVM/c_MVVM/InventoryViewModel.cs`
- (RelayCommand.cs no cambia de namespace de contenido — ya está en `Clase07.Mvx.Mvvm` — solo se mueve de carpeta)
- Create: `clases/clase07/Unity/Assets/04_MVC_MVP_MVVM/c_MVVM/Tests/Clase07.Mvx.Mvvm.Tests.asmdef`
- Create: `clases/clase07/Unity/Assets/04_MVC_MVP_MVVM/c_MVVM/Tests/InventoryModelTests.cs`
- Create: `clases/clase07/Unity/Assets/04_MVC_MVP_MVVM/c_MVVM/Tests/InventoryViewModelTests.cs`

**Interfaces:**
- Consumes: `Clase07.EditorTools.RepointRowPrefabReference.Run` (de Task 14, `_Migration/`).
- Produces: `Clase07.Mvx.Mvvm.InventoryViewModel` (`Items: ObservableCollection<InventoryItemViewModel>`, `PendingName`, `AddCommand`), `Clase07.Mvx.Mvvm.InventoryItemViewModel` (`Name`, `Quantity`, `RemoveCommand`), `Clase07.Mvx.Mvvm.RelayCommand`, `Clase07.Mvx.Mvvm.InventoryMvvmView` (`OnAddClicked()`).

- [ ] **Step 1: mover carpeta y escena**

```bash
cd /Users/giga/code/Programacion-de-VideoJuegos-III-2026
git mv clases/clase07/Unity/Assets/04_MVC_MVP_MVVM/03_MVVM clases/clase07/Unity/Assets/04_MVC_MVP_MVVM/c_MVVM
git mv clases/clase07/Unity/Assets/04_MVC_MVP_MVVM/c_MVVM/03_Mvx_MVVM.unity clases/clase07/Unity/Assets/04_MVC_MVP_MVVM/c_MVVM/c_Mvx_MVVM.unity
git mv clases/clase07/Unity/Assets/04_MVC_MVP_MVVM/c_MVVM/03_Mvx_MVVM.unity.meta clases/clase07/Unity/Assets/04_MVC_MVP_MVVM/c_MVVM/c_Mvx_MVVM.unity.meta
```

- [ ] **Step 2: copiar el dominio compartido, namespace fusionado a `Clase07.Mvx.Mvvm`**

Crear `clases/clase07/Unity/Assets/04_MVC_MVP_MVVM/c_MVVM/InventoryItem.cs`:

```csharp
namespace Clase07.Mvx.Mvvm
{
    public class InventoryItem
    {
        public string Name { get; }
        public int Quantity { get; set; }

        public InventoryItem(string name, int quantity)
        {
            Name = name;
            Quantity = quantity;
        }
    }
}
```

Crear `clases/clase07/Unity/Assets/04_MVC_MVP_MVVM/c_MVVM/InventoryModel.cs`:

```csharp
using System;
using System.Collections.Generic;
using System.Linq;

namespace Clase07.Mvx.Mvvm
{
    public class InventoryModel
    {
        private readonly List<InventoryItem> _items = new List<InventoryItem>();
        public IReadOnlyList<InventoryItem> Items => _items;
        public event Action Changed;

        public void AddItem(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("El nombre no puede estar vacío", nameof(name));
            }

            var existing = _items.FirstOrDefault(i => i.Name == name);
            if (existing != null) existing.Quantity++;
            else _items.Add(new InventoryItem(name, 1));

            Changed?.Invoke();
        }

        public void RemoveItem(string name)
        {
            var existing = _items.FirstOrDefault(i => i.Name == name);
            if (existing == null) return;

            existing.Quantity--;
            if (existing.Quantity <= 0) _items.Remove(existing);

            Changed?.Invoke();
        }
    }
}
```

Crear `clases/clase07/Unity/Assets/04_MVC_MVP_MVVM/c_MVVM/InventoryItemRowView.cs`:

```csharp
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Clase07.Mvx.Mvvm
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

```bash
cp clases/clase07/Unity/Assets/04_MVC_MVP_MVVM/Shared/InventoryItemRow.prefab clases/clase07/Unity/Assets/04_MVC_MVP_MVVM/c_MVVM/InventoryItemRow.prefab
```

- [ ] **Step 3: asmdef runtime**

Crear `clases/clase07/Unity/Assets/04_MVC_MVP_MVVM/c_MVVM/Clase07.Mvx.Mvvm.asmdef`:

```json
{
    "name": "Clase07.Mvx.Mvvm",
    "rootNamespace": "",
    "references": [
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

- [ ] **Step 4: ajustar `InventoryItemViewModel.cs` y `InventoryViewModel.cs` — quitar el `using` a `Shared`**

`clases/clase07/Unity/Assets/04_MVC_MVP_MVVM/c_MVVM/InventoryItemViewModel.cs` no tenía `using Clase07.Mvx.Shared;` (no lo necesitaba), así que queda igual — solo se mueve de carpeta, sin editar contenido.

Reemplazar el contenido completo de `clases/clase07/Unity/Assets/04_MVC_MVP_MVVM/c_MVVM/InventoryViewModel.cs`:

```csharp
using System.Collections.ObjectModel;

namespace Clase07.Mvx.Mvvm
{
    public class InventoryViewModel
    {
        private readonly InventoryModel _model;
        public ObservableCollection<InventoryItemViewModel> Items { get; } = new ObservableCollection<InventoryItemViewModel>();
        public string PendingName { get; set; } = string.Empty;
        public RelayCommand AddCommand { get; }

        public InventoryViewModel(InventoryModel model)
        {
            _model = model;
            AddCommand = new RelayCommand(
                () => _model.AddItem(PendingName),
                () => !string.IsNullOrWhiteSpace(PendingName));
            _model.Changed += Refresh;
            Refresh();
        }

        private void Refresh()
        {
            Items.Clear();
            foreach (var item in _model.Items)
            {
                var name = item.Name;
                Items.Add(new InventoryItemViewModel(name, item.Quantity, () => _model.RemoveItem(name)));
            }
        }
    }
}
```

- [ ] **Step 5: ajustar `InventoryMvvmView.cs` — quitar el `using` a `Shared` y agregar el comentario pedagógico**

Reemplazar el contenido completo de `clases/clase07/Unity/Assets/04_MVC_MVP_MVVM/c_MVVM/InventoryMvvmView.cs`:

```csharp
using UnityEngine;
using TMPro;

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
            // La vista nunca llama a _model.AddItem/RemoveItem directamente: solo
            // escribe PendingName y ejecuta AddCommand/RemoveCommand (ver OnAddClicked
            // y SetRemoveAction más abajo), y solo redibuja cuando Items.CollectionChanged
            // avisa. Toda la lógica de cuándo se puede agregar o quitar un ítem vive en
            // InventoryViewModel/RelayCommand, no acá — a diferencia de a_MVC, que llama
            // al modelo directamente.
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

- [ ] **Step 6: reapuntar `c_Mvx_MVVM.unity` al prefab nuevo**

```bash
cd /Users/giga/code/Programacion-de-VideoJuegos-III-2026
/Applications/Unity/Hub/Editor/6000.3.21f1/Unity.app/Contents/MacOS/Unity \
  -batchmode -nographic \
  -projectPath clases/clase07/Unity \
  -executeMethod Clase07.EditorTools.RepointRowPrefabReference.Run \
  -scenePath "Assets/04_MVC_MVP_MVVM/c_MVVM/c_Mvx_MVVM.unity" \
  -componentType "Clase07.Mvx.Mvvm.InventoryMvvmView, Clase07.Mvx.Mvvm" \
  -fieldName "_rowPrefab" \
  -newPrefabPath "Assets/04_MVC_MVP_MVVM/c_MVVM/InventoryItemRow.prefab" \
  -logFile "$(pwd)/clases/clase07/Unity_mvx_c_mvvm_repoint.log"
```

Run: `grep -iE "error|No se (encontró|pudo)" clases/clase07/Unity_mvx_c_mvvm_repoint.log || echo ok`
Expected: `ok`.

```bash
rm clases/clase07/Unity_mvx_c_mvvm_repoint.log
```

- [ ] **Step 7: copiar el test de `InventoryModel`, mover `InventoryViewModelTests.cs`, crear el asmdef de test**

Crear `clases/clase07/Unity/Assets/04_MVC_MVP_MVVM/c_MVVM/Tests/Clase07.Mvx.Mvvm.Tests.asmdef`:

```json
{
    "name": "Clase07.Mvx.Mvvm.Tests",
    "rootNamespace": "",
    "references": [
        "Clase07.Mvx.Mvvm",
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

Crear `clases/clase07/Unity/Assets/04_MVC_MVP_MVVM/c_MVVM/Tests/InventoryModelTests.cs`:

```csharp
using NUnit.Framework;
using System.Linq;
using Clase07.Mvx.Mvvm;

namespace Clase07.Mvx.Mvvm.Tests
{
    public class InventoryModelTests
    {
        [Test]
        public void AddItem_NewName_AddsWithQuantityOne()
        {
            var model = new InventoryModel();
            model.AddItem("Potion");
            Assert.AreEqual(1, model.Items.Single(i => i.Name == "Potion").Quantity);
        }

        [Test]
        public void AddItem_ExistingName_IncrementsQuantity()
        {
            var model = new InventoryModel();
            model.AddItem("Potion");
            model.AddItem("Potion");
            Assert.AreEqual(2, model.Items.Single(i => i.Name == "Potion").Quantity);
        }

        [Test]
        public void AddItem_EmptyName_Throws()
        {
            var model = new InventoryModel();
            Assert.Throws<System.ArgumentException>(() => model.AddItem(""));
        }

        [Test]
        public void RemoveItem_DecrementsQuantity_AndRemovesAtZero()
        {
            var model = new InventoryModel();
            model.AddItem("Potion");
            model.AddItem("Potion");

            model.RemoveItem("Potion");
            Assert.AreEqual(1, model.Items.Single(i => i.Name == "Potion").Quantity);

            model.RemoveItem("Potion");
            Assert.IsEmpty(model.Items);
        }

        [Test]
        public void RemoveItem_UnknownName_IsNoOp()
        {
            var model = new InventoryModel();
            Assert.DoesNotThrow(() => model.RemoveItem("Nothing"));
            Assert.IsEmpty(model.Items);
        }

        [Test]
        public void Changed_FiresOnAddAndRemove()
        {
            var model = new InventoryModel();
            var fireCount = 0;
            model.Changed += () => fireCount++;

            model.AddItem("Potion");
            model.RemoveItem("Potion");

            Assert.AreEqual(2, fireCount);
        }
    }
}
```

Mover `InventoryViewModelTests.cs`:

```bash
git mv clases/clase07/Unity/Assets/04_MVC_MVP_MVVM/Tests/InventoryViewModelTests.cs clases/clase07/Unity/Assets/04_MVC_MVP_MVVM/c_MVVM/Tests/InventoryViewModelTests.cs
git mv clases/clase07/Unity/Assets/04_MVC_MVP_MVVM/Tests/InventoryViewModelTests.cs.meta clases/clase07/Unity/Assets/04_MVC_MVP_MVVM/c_MVVM/Tests/InventoryViewModelTests.cs.meta
```

Editar el archivo movido para quitar el `using Clase07.Mvx.Shared;`:

```csharp
using System.Linq;
using NUnit.Framework;
using Clase07.Mvx.Mvvm;

namespace Clase07.Mvx.Mvvm.Tests
{
    public class InventoryViewModelTests
    {
        [Test]
        public void AddCommand_CanExecute_FalseWhenPendingNameEmpty()
        {
            var vm = new InventoryViewModel(new InventoryModel());
            vm.PendingName = "";
            Assert.IsFalse(vm.AddCommand.CanExecute());
        }

        [Test]
        public void AddCommand_Execute_AddsItemToObservableCollection()
        {
            var vm = new InventoryViewModel(new InventoryModel());
            vm.PendingName = "Potion";

            vm.AddCommand.Execute();

            Assert.AreEqual(1, vm.Items.Count);
            Assert.AreEqual("Potion", vm.Items[0].Name);
        }

        [Test]
        public void RemoveCommand_Execute_RemovesItemFromCollection()
        {
            var vm = new InventoryViewModel(new InventoryModel());
            vm.PendingName = "Potion";
            vm.AddCommand.Execute();

            vm.Items.Single().RemoveCommand.Execute();

            Assert.IsEmpty(vm.Items);
        }
    }
}
```

- [ ] **Step 8: verificar EditMode**

```bash
cd /Users/giga/code/Programacion-de-VideoJuegos-III-2026
/Applications/Unity/Hub/Editor/6000.3.21f1/Unity.app/Contents/MacOS/Unity \
  -batchmode -nographic \
  -projectPath clases/clase07/Unity \
  -runTests -testPlatform EditMode \
  -testResults "$(pwd)/clases/clase07/Unity_mvx_c_mvvm_editmode.xml" \
  -logFile "$(pwd)/clases/clase07/Unity_mvx_c_mvvm_editmode.log"
```

Run: `grep -c 'result="Failed"' clases/clase07/Unity_mvx_c_mvvm_editmode.xml || true`
Expected: `0`.

```bash
rm clases/clase07/Unity_mvx_c_mvvm_editmode.xml clases/clase07/Unity_mvx_c_mvvm_editmode.log
```

- [ ] **Step 9: commit**

```bash
git add clases/clase07/Unity/Assets/04_MVC_MVP_MVVM/c_MVVM clases/clase07/Unity/Assets/04_MVC_MVP_MVVM/Tests
git commit -m "$(cat <<'EOF'
clase07/Mvx: c_MVVM autocontenida

Co-Authored-By: Claude Sonnet 5 <noreply@anthropic.com>
Claude-Session: https://claude.ai/code/session_01YGZHSkcGRaTFS7xCnNWa5E
EOF
)"
```

---

### Task 17: limpieza del módulo `04_MVC_MVP_MVVM`

**Files:**
- Delete: `clases/clase07/Unity/Assets/04_MVC_MVP_MVVM/Shared/` (completa, incluye `InventoryItem.cs`, `InventoryModel.cs`, `InventoryItemRowView.cs`, `InventoryItemRow.prefab` y sus `.meta`)
- Delete: `clases/clase07/Unity/Assets/04_MVC_MVP_MVVM/Tests/InventoryModelTests.cs` (+ `.meta`) — el original: Tasks 14/15/16 crearon cada una su propia copia nueva, nunca lo movieron de acá
- Delete: `clases/clase07/Unity/Assets/04_MVC_MVP_MVVM/Tests/` (queda vacía tras Tasks 15/16 y el borrado del `InventoryModelTests.cs` original — verificar antes de borrar)
- Delete: `clases/clase07/Unity/Assets/04_MVC_MVP_MVVM/Clase07.Mvx.asmdef`
- Delete: `clases/clase07/Unity/Assets/04_MVC_MVP_MVVM/Tests/Clase07.Mvx.Tests.asmdef` (si `Tests/` no se borró entera en el punto anterior)
- Delete: `clases/clase07/Unity/Assets/04_MVC_MVP_MVVM/_Migration/` (herramienta temporal, ya no hace falta)

**Interfaces:** Ninguna — esta task solo borra código que a esta altura no debería tener referencias vivas.

- [ ] **Step 1: confirmar que nada referencia ya `Clase07.Mvx.Shared`**

```bash
cd /Users/giga/code/Programacion-de-VideoJuegos-III-2026
grep -rl "Clase07.Mvx.Shared" clases/clase07/Unity/Assets --include="*.cs"
```

Expected: sin resultados (comando no imprime nada). Si aparece algún archivo, es porque a_MVC/b_MVP/c_MVVM no terminaron de migrarse — no seguir con el borrado hasta resolverlo.

- [ ] **Step 2: borrar `Shared/`, el `InventoryModelTests.cs` original, el asmdef de módulo y `_Migration/`**

`Tests/InventoryModelTests.cs` a nivel de módulo es el original: las Tasks 14, 15 y 16
crearon cada una su propia copia dentro de su propia carpeta (mismo criterio que el
resto del dominio duplicado), pero ninguna lo sacó de acá con `git mv` — hay que
borrarlo explícitamente o `Tests/` nunca queda vacía:

```bash
git rm -r clases/clase07/Unity/Assets/04_MVC_MVP_MVVM/Shared
git rm clases/clase07/Unity/Assets/04_MVC_MVP_MVVM/Clase07.Mvx.asmdef clases/clase07/Unity/Assets/04_MVC_MVP_MVVM/Clase07.Mvx.asmdef.meta
git rm clases/clase07/Unity/Assets/04_MVC_MVP_MVVM/Tests/InventoryModelTests.cs clases/clase07/Unity/Assets/04_MVC_MVP_MVVM/Tests/InventoryModelTests.cs.meta
git rm -r clases/clase07/Unity/Assets/04_MVC_MVP_MVVM/_Migration
ls clases/clase07/Unity/Assets/04_MVC_MVP_MVVM/Tests
```

Ahora el `ls` de `Tests/` no debería mostrar ningún `.cs` (solo puede quedar
`Clase07.Mvx.Tests.asmdef`/`.meta`) — borrar la carpeta entera:

```bash
git rm -r clases/clase07/Unity/Assets/04_MVC_MVP_MVVM/Tests
```

- [ ] **Step 3: verificación final EditMode del módulo completo**

```bash
/Applications/Unity/Hub/Editor/6000.3.21f1/Unity.app/Contents/MacOS/Unity \
  -batchmode -nographic \
  -projectPath clases/clase07/Unity \
  -runTests -testPlatform EditMode \
  -testResults "$(pwd)/clases/clase07/Unity_mvx_final_editmode.xml" \
  -logFile "$(pwd)/clases/clase07/Unity_mvx_final_editmode.log"
```

Run: `grep -iE "error CS" clases/clase07/Unity_mvx_final_editmode.log || echo "sin errores de compilación"`
Expected: `sin errores de compilación`.

Run: `grep -c 'result="Failed"' clases/clase07/Unity_mvx_final_editmode.xml || true`
Expected: `0`.

```bash
rm clases/clase07/Unity_mvx_final_editmode.xml clases/clase07/Unity_mvx_final_editmode.log
```

- [ ] **Step 4: commit**

```bash
git add -A clases/clase07/Unity/Assets/04_MVC_MVP_MVVM
git commit -m "$(cat <<'EOF'
clase07/Mvx: borrar Shared/, asmdef de módulo y herramienta de migración

Co-Authored-By: Claude Sonnet 5 <noreply@anthropic.com>
Claude-Session: https://claude.ai/code/session_01YGZHSkcGRaTFS7xCnNWa5E
EOF
)"
```
---

### Task 18: Actualizar `README.md` y `SPEC.md` con las rutas y la convención nuevas

**Files:**
- Modify: `clases/clase07/README.md`
- Modify: `clases/clase07/SPEC.md`

**Interfaces:** Ninguna — solo documentación, no toca código.

- [ ] **Step 1: Actualizar la sección "Estructura" de `README.md` con la convención de letras**

En `clases/clase07/README.md`, en la sección "Estructura" (después de la lista de módulos), agregar un párrafo nuevo antes de "Cada módulo tiene su propio Assembly Definition...":

```markdown
Dentro de cada módulo, cada implementación alternativa vive en su propia carpeta con
prefijo de **letra** (`a_`, `b_`, `c_`, ...), en orden de lectura sugerido de la más
simple/ingenua a la más sofisticada — a diferencia del prefijo **numérico** de las
carpetas de módulo (`01_FSM`, `02_DependencyInjection`, ...), que ordena entre
patrones distintos. Cada carpeta de implementación es autocontenida: tiene su propio
Assembly Definition, su propia copia de cualquier clase de dominio que antes era
compartida dentro del módulo, y su propia carpeta `Tests/` — se puede copiar cualquiera
de esas carpetas a otro proyecto Unity y compila sola, salvo por los paquetes de
terceros (VContainer, MessagePipe, UniTask, uGUI), que sí es correcto que se
compartan.
```

Y reemplazar la línea:

```markdown
- Cada módulo tiene su propio Assembly Definition (`Clase07.FSM`, `Clase07.DI`,
  `Clase07.MessageBroker`, `Clase07.Mvx`) y su propia carpeta `Tests/`
  (EditMode y, donde hace falta, PlayMode).
```

por:

```markdown
- Cada implementación tiene su propio Assembly Definition (ej. `Clase07.DI.Singleton`,
  `Clase07.MessageBroker.MessagePipeExample`, `Clase07.Mvx.Mvvm`) y su propia carpeta
  `Tests/` (EditMode y, donde hace falta, PlayMode) — ya no hay un asmdef ni una
  carpeta `Shared/`/`Core/` a nivel de módulo.
```

- [ ] **Step 2: Actualizar todas las rutas de escena en "Cómo correr cada módulo"**

En `clases/clase07/README.md`, reemplazar el bloque completo de la sección "Cómo correr
cada módulo" (las cuatro sub-listas con rutas de `.unity`) por:

```markdown
- **01 — FSM**, tres escenas independientes:
  - `Assets/01_FSM/Small/a_Baseline/a_FSM_WeaponBaseline.unity` — botón "Fire" para la
    versión del arma con `enum` + `switch`, con texto de debug mostrando estado y
    munición.
  - `Assets/01_FSM/Small/b_StatePattern/b_FSM_WeaponStatePattern.unity` — el mismo
    arma sobre el motor genérico de estados (`IState` + `StateMachine<TState>`, copia
    local de esta carpeta), en su propia escena.
  - `Assets/01_FSM/Large/a_StatePattern/a_FSM_GameFlow.unity` — botones para avanzar
    el game flow (`Finish Loading`, `Play`, `Pause`, `Open Settings`, `Close
    Settings`, `Resume`), con texto de debug mostrando el estado y el subestado
    activos. La escena muestra **sólo** la variante OOP del game flow; la variante con
    estados como `ScriptableObject`
    (`Assets/01_FSM/Large/b_ScriptableObjectStates/`) se verifica por tests de
    EditMode en vez de montarse también en una escena — sigue siendo una decisión de
    alcance deliberada, ver [`SPEC.md`](SPEC.md).
- **02 — Dependency Injection**, tres escenas casi idénticas (botón "Coin" + texto
  de score) que conviene correr una por vez, ya que alguna variante usa estado
  `static`:
  - `Assets/02_DependencyInjection/a_Singleton/a_DI_Singleton.unity`
  - `Assets/02_DependencyInjection/b_ServiceLocator/b_DI_ServiceLocator.unity`
  - `Assets/02_DependencyInjection/c_VContainer/c_DI_VContainer.unity`
- **03 — Message Broker**, tres escenas casi idénticas (botón que publica un evento
  + texto que se actualiza al recibirlo):
  - `Assets/03_MessageBroker/a_DIBroker/a_MessageBroker_DIBroker.unity`
  - `Assets/03_MessageBroker/b_ScriptableObjectChannels/b_MessageBroker_SOChannels.unity`
  - `Assets/03_MessageBroker/c_MessagePipe/c_MessageBroker_MessagePipe.unity`
- **04 — MVC / MVP / MVVM**, tres escenas casi idénticas (`TMP_InputField` + botón "Add"
  + lista con botón "Remove" por fila):
  - `Assets/04_MVC_MVP_MVVM/a_MVC/a_Mvx_MVC.unity`
  - `Assets/04_MVC_MVP_MVVM/b_MVP/b_Mvx_MVP.unity`
  - `Assets/04_MVC_MVP_MVVM/c_MVVM/c_Mvx_MVVM.unity`
```

- [ ] **Step 3: Actualizar la sección "Convenciones compartidas" de `SPEC.md`**

En `clases/clase07/SPEC.md`, en la sección "Convenciones compartidas", reemplazar el
primer bullet:

```markdown
- Cada implementación alternativa de un mismo patrón vive en su propia subcarpeta
  numerada (`01_...`, `02_...`, `03_...`) dentro del módulo, para que el orden de lectura
  sugerido sea obvio.
```

por:

```markdown
- Cada implementación alternativa de un mismo patrón vive en su propia subcarpeta con
  prefijo de **letra** (`a_...`, `b_...`, `c_...`) dentro del módulo, en orden de
  lectura sugerido de la más simple/ingenua a la más sofisticada — el prefijo
  **numérico** (`01_...`, `02_...`) queda reservado para las carpetas de módulo, que
  ordenan entre patrones distintos.
- Cada carpeta de implementación es **autocontenida**: tiene su propio Assembly
  Definition (runtime + test, y PlayMode donde aplica), y su propia copia de
  cualquier clase de dominio que en otra implementación hermana sea idéntica — no hay
  ninguna carpeta `Shared/`/`Core/` ni asmdef a nivel de módulo. Se puede copiar
  cualquiera de estas carpetas a otro proyecto Unity y compila sola, salvo por los
  paquetes de terceros (VContainer, MessagePipe, MessagePipe.VContainer, UniTask,
  uGUI), que sí es correcto que se compartan.
```

- [ ] **Step 4: Actualizar las rutas de carpeta en cada sección de módulo de `SPEC.md`**

En cada una de las cuatro secciones "Módulo N — ..." de `SPEC.md`, reemplazar las
rutas de carpeta antiguas por las nuevas (mismo mapeo que la tabla del spec de
diseño):

- Módulo 1 — FSM: `Small/Baseline/` → `Small/a_Baseline/`, `Small/StatePattern/` →
  `Small/b_StatePattern/`, `Large/StatePattern/` → `Large/a_StatePattern/`,
  `Large/ScriptableObjectStates/` → `Large/b_ScriptableObjectStates/`. Sacar toda
  mención a `Core/` compartido — reemplazar por "cada una de las dos implementaciones
  State Pattern (chica y grande) tiene su propia copia de `IState`/`StateMachine<TState>`".
- Módulo 2 — DI: `01_Singleton/` → `a_Singleton/`, `02_ServiceLocator/` →
  `b_ServiceLocator/`, `03_VContainer/` → `c_VContainer/`. Sacar la mención a
  `Shared/IScoreService.cs`/`Shared/IAudioService.cs`/etc. compartidos entre las tres
  — reemplazar por "cada implementación tiene su propia copia de
  `IScoreService`/`IAudioService`/`ScoreService`/`AudioService`".
- Módulo 3 — MessageBroker: `01_DIBroker/` → `a_DIBroker/`,
  `02_ScriptableObjectChannels/` → `b_ScriptableObjectChannels/`, `03_MessagePipe/` →
  `c_MessagePipe/`. Sacar la mención a `Shared/ScorePickedUpEvent.cs`/
  `Shared/PlayerDamagedEvent.cs` compartidos — reemplazar por "cada implementación
  tiene su propia copia de los eventos que usa (`b_ScriptableObjectChannels` y
  `c_MessagePipe` solo usan `ScorePickedUpEvent`; `a_DIBroker` usa ambos)".
- Módulo 4 — MVx: `01_MVC/` → `a_MVC/`, `02_MVP/` → `b_MVP/`, `03_MVVM/` → `c_MVVM/`.
  Sacar la mención a `Shared/InventoryItem.cs`/`Shared/InventoryModel.cs`/
  `Shared/InventoryItemRow.prefab` compartidos — reemplazar por "cada implementación
  tiene su propia copia del modelo y del prefab de fila".

- [ ] **Step 5: Commitear**

```bash
cd /Users/giga/code/Programacion-de-VideoJuegos-III-2026
git add clases/clase07/README.md clases/clase07/SPEC.md
git commit -m "$(cat <<'EOF'
clase07: actualizar README y SPEC con las carpetas autocontenidas y la convención de letras

Co-Authored-By: Claude Sonnet 5 <noreply@anthropic.com>
Claude-Session: https://claude.ai/code/session_01YGZHSkcGRaTFS7xCnNWa5E
EOF
)"
```
