# 07 · Tests

- `EditMode/`: reglas puras (`CombatRulesTests`), validaciones de los ScriptableObjects, el grafo
  de diálogo y los assets reales del proyecto. Corren sin escena y en segundos.
- `PlayMode/`: arman una escena mínima por código y dejan pasar frames reales
  (`WaveGameplayTests`, `DialogueNpcTests`).

Desde **Window > General > Test Runner**, o sin abrir Unity: `./scripts/Test.ps1` y
`./scripts/Test.ps1 -Platform PlayMode` (desde `clases/clase14`).
