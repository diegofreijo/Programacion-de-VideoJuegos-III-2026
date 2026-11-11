# 01 · ScriptableObjects

Los datos del juego viven en assets, no en el código: un diseñador crea y edita enemigos y oleadas
sin tocar C#.

Mirá primero `Runtime/EnemyDefinition.cs`:

- `[CreateAssetMenu]`: aparece en **Assets > Create**.
- `[Header]`, `[Tooltip]`, `[Range]`, `[Min]`, `[ContextMenu]`: el Inspector sin escribir UI.
- `OnValidate` **corrige** datos (clamp, valores por defecto); `Validate()` **avisa** problemas.
  El Inspector, la ventana de balanceo y los tests leen ese mismo `Validate()`.

Después `Runtime/WaveDefinition.cs` (una lista de enemigos y un `FloatRange` del tema 02).
Los assets están en `Data/Enemigos` y `Data/Oleadas`.
