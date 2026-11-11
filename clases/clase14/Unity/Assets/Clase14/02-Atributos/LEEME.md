# 02 · Atributos y PropertyDrawers

Un atributo propio cambia cómo se dibuja un campo en **todos** los Inspectors.

1. `Runtime/FloatRange.cs`: el dato (`FloatRange`) y el atributo (`[MinMaxRange]`). Es runtime:
   el juego lo usa, pero no sabe nada de cómo se dibuja.
2. `Editor/MinMaxRangeDrawer.cs`: el dibujo, un slider de dos puntas. Tiene la versión UI Toolkit
   (`CreatePropertyGUI`) y la IMGUI (`OnGUI`) una al lado de la otra para compararlas.

Para verlo: el campo *Spawn Interval* de cualquier oleada en `Data/Oleadas`.
