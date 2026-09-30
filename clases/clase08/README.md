# Clase 08 — Tests automatizados en Unity

Proyecto independiente para enseñar tests unitarios, de integración y funcionales.
Los tres ejemplos tienen pruebas de **Edit Mode** y **Play Mode** y se ejecutan en
el Test Runner real de Unity. No hace falta preparar GameObjects ni una escena:
los tests crean sus objetos y los eliminan al terminar, incluso si falla una aserción.

## Dos ejes diferentes

**Unitario / integración** describe el alcance de la prueba. **Funcional**
describe su propósito: comprobar un comportamiento requerido. **Edit Mode /
Play Mode** describe dónde se ejecuta. No son alternativas de una sola lista.

| Ejemplo | Edit Mode | Play Mode | Clasificación |
| --- | --- | --- | --- |
| Daño menos defensa | Tres combinaciones de ataque/defensa | La misma función en ejecución | Unitario |
| Proyectil y salud | Llama a `Impactar` con un componente `Salud` real | Produce un contacto físico que invoca `OnCollisionEnter` | Integración |
| Llave y puerta | Sin llave, llave incorrecta, llave correcta | Recoger la llave correcta y abrir | Funcional y también integración |

Una prueba unitaria también puede verificar una regla funcional. Las etiquetas
no son excluyentes. Aquí destacamos el propósito más útil para la explicación.

## Atributos y limpieza

- `[Test]`: prueba síncrona, válida tanto en Edit Mode como en Play Mode.
- `[TestCase]`: ejecuta el método una vez por conjunto de datos.
- `[UnityTest]`: devuelve `IEnumerator` y permite ceder frames con `yield`.
- `[UnityTearDown]`: limpieza con coroutine, incluso después de un fallo.
- `DestroyImmediate`: limpieza inmediata de los objetos creados en Edit Mode.
- `Destroy` y un frame de espera: limpieza de objetos en Play Mode.

El modo del test lo determina su **assembly**, no el atributo: usar `[UnityTest]`
no convierte automáticamente una prueba en Play Mode.

## Ejecutar desde terminal (opcional)

Con Unity CLI instalado, desde la raíz del repositorio y con este proyecto cerrado
en el Editor:

```powershell
unity test clases/clase08/Unity --mode EditMode --output clases/clase08/Unity/TestResults/editmode.xml --timeout 600
unity test clases/clase08/Unity --mode PlayMode --output clases/clase08/Unity/TestResults/playmode.xml --timeout 600
```

El Test Runner gráfico no requiere Unity CLI. Los resultados, cachés y archivos
generados están excluidos de Git mediante el `.gitignore` del proyecto.

Verificado con Unity 6000.3.9f1: **5/5 casos de Edit Mode y 3/3 de Play Mode
aprobados**, ejecutados con el Test Runner en batch mode. Esto incluye el contacto
físico real del proyectil. Los reportes locales quedan en `Unity/TestResults/`.

## Alcance de los ejemplos

Se asumen ataques, defensas y daños no negativos. El inventario no es nulo.
El collider del enemigo está en el mismo GameObject que `Salud`. Un proyectil
aplica daño una sola vez. Las llaves no se consumen y abrir la puerta modifica
un estado lógico, sin animación. Son decisiones para mantener la clase enfocada.

Como ejercicios posteriores: comprobar que un segundo impacto no duplique el
daño, que la salud no baje de cero y que una puerta ya abierta conserve su estado.

Referencia: [Edit Mode y Play Mode — Unity Test Framework](https://docs.unity3d.com/Packages/com.unity.test-framework@1.6/manual/edit-mode-vs-play-mode-tests.html).
