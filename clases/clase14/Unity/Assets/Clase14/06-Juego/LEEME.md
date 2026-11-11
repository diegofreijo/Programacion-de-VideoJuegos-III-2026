# 06 · El juego

Lo que usa todos los datos de los temas anteriores: oleadas de enemigos que caminan hacia un
objetivo y una torreta que los defiende.

- `Runtime/CombatRules.cs`: reglas puras, sin escena; por eso se testean en Edit Mode.
- `Runtime/Enemy.cs`, `Turret.cs`, `WaveSpawner.cs`, `DefendedTarget.cs`: los componentes.
- `Runtime/Hud.cs`: el texto de arriba a la izquierda. Solo reescribe el texto cuando cambia algo.
- `Enemy.FindTargetCarelessly`: el código que genera basura a propósito para la escena **Profiler**.
- `SpriteFactory` y `WorldText`: sprites y textos hechos por código, para no depender de arte.
