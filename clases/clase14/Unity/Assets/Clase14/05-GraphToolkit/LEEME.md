# 05 · Graph Toolkit: diálogos por nodos

Un editor de nodos para que un guionista arme diálogos, y un juego que los usa.

1. `Editor/Dialogue/DialogueGraph.cs` y `DialogueNodes.cs`: el grafo y sus nodos (Inicio,
   Línea, Elección, Fin). La ventana, los cables y el guardado los pone Unity.
2. `Editor/Dialogue/DialogueValidation.cs`: los errores y avisos que se ven en el grafo.
3. `Editor/Dialogue/DialogueImporter.cs`: al guardar el grafo lo convierte en un `DialogueAsset`.
   Graph Toolkit **no ejecuta** el grafo: el juego solo ve ese asset.
4. `Runtime/DialogueAsset.cs` (el asset y un cursor para recorrerlo) y `Runtime/DialogueNpc.cs`
   (el juego: hablar con el Guardia y mostrar la caja con `DialogueBox`).

Para verlo: doble clic en `Data/Dialogos/Guardia.dialogue`, cambiá un texto, guardá y dale Play a
la escena **Dialogo** (menú **Clase14 > Escenas**).
