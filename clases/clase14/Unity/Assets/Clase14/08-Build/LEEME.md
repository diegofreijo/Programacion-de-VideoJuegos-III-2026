# 08 · Build y contenido de ejemplo

- `Editor/BuildScript.cs`: el build de Windows por línea de comandos (`./scripts/Build.ps1`). En
  batch devuelve un código de salida, para que un script, CI o un agente sepa si salió bien.
- `Editor/SampleContent.cs`: genera por código los enemigos, oleadas, el diálogo y las escenas.
  Menú **Clase14 > Regenerar contenido de ejemplo** o **Regenerar escenas**.
- `Editor/SceneMenu.cs`: el menú **Clase14 > Escenas**.
