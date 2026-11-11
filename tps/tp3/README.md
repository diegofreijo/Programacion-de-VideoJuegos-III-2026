# Trabajo Práctico 3: Herramientas de Editor y Agentes de LLM

- **Integrantes por equipo:** 1
- **Fecha límite de entrega:** 18/11, 23:59
- **Fecha límite de reentrega:** 25/11, 23:59
- **Exposición pública presencial:** 25/11
- **Se aprueba con:** 4

## 1. Objetivo

En el [TP2](../tp2/README.md) implementaron y testearon las soluciones de sus juegos. En este trabajo van a construir una **herramienta de Editor** para los datos de uno de esos prototipos, para que alguien del equipo que no programa pueda editarlos y detectar errores sin tocar código.

La van a construir trabajando con un **agente de LLM**, como vimos en la [clase 14](../../clases/clase14/README.md): un agente lee el repo, corre comandos, edita archivos, observa el resultado y repite. Para que trabaje bien, el proyecto tiene que darle contexto y una forma de verificar lo que hace.

No espero una suite de herramientas ni un editor de producción. Espero **una herramienta**, chica y bien hecha, sobre datos reales de su prototipo.

Recuerden que, por lo general, la solución más simple suele ser la mejor.

## 2. Condiciones de entrega

La entrega se hace sobre el **mismo repositorio de GitHub del TP2** e incluye:

* El código de la herramienta y sus tests automatizados.
* Un `AGENTS.md` en el repo, según lo indicado en la sección 4.3.
* Las instrucciones para abrir el proyecto, abrir la herramienta y correr los tests, indicando la versión de Unity utilizada.

Pueden cambiar la versión de Unity respecto del TP2 (por ejemplo, Graph Toolkit necesita 6000.6 o posterior). Si lo hacen, indíquenlo en el README y comprueben que los tests del TP2 sigan pasando.

Para entregar, tienen que crear un tag en el repositorio y enviar **el enlace al tag en GitHub** a `diego.freijo@udelaciudad.edu.ar` antes del límite correspondiente:

| Instancia | Tag | Fecha límite |
| --- | --- | --- |
| Entrega | `tp3` | 18/11, 23:59 |
| Reentrega | `rtp3` | 25/11, 23:59 |

El tag identifica la versión que voy a evaluar: no alcanza con mandar un enlace a la rama principal. Una vez enviado, no lo muevan a otro commit.

## 3. Evaluación

Además de entregar el repositorio, van a exponer públicamente su trabajo en una **defensa oral presencial en la facultad el 25/11**, en la fecha de la reentrega.

- **1/3 de la nota** corresponde a la entrega: herramienta, tests y `AGENTS.md`.
- **2/3 de la nota** corresponden a la exposición y defensa oral.

Van a tener **20 minutos** para presentar el trabajo y después habrá **10 minutos** de preguntas y debate.

### 3.1. Mínimos para aprobar

Tanto la entrega como la defensa deben alcanzar los mínimos de aprobación. Una no compensa las insuficiencias de la otra.

* La herramienta tiene las partes pedidas en la sección 4.1 y funciona sobre los datos de su prototipo.
* Las validaciones tienen tests automatizados de Edit Mode, y pasan.
* El proyecto puede hacer un build: el código de Editor está separado del de runtime.
* Hay un `AGENTS.md` con el contenido mínimo de la sección 4.3, escrito para su proyecto.
* Pueden explicar el código de la herramienta y cómo trabajaron con el agente, incluido lo que hizo mal y cómo se dieron cuenta.

### 3.2. Qué voy a evaluar

Que la herramienta sea útil para sus datos, que el diseño sea claro, que las validaciones estén en un solo lugar y bien testeadas, y que el `AGENTS.md` le dé a un agente lo que necesita para trabajar en su proyecto.

Durante la defensa tienen que mostrar la herramienta funcionando y apoyarse en el código. Puedo pedirles que rompan un dato a propósito y muestren cómo lo marca la herramienta, que expliquen una parte del código o que cuenten un cambio que les propuso el agente y por qué lo aceptaron o lo rechazaron. **Que el código lo haya escrito un agente no cambia nada: lo que entregan es suyo y lo tienen que poder defender línea por línea.**

Podemos usar mi máquina para ejecutar todo lo necesario en vivo.

Les recomiendo que vayan validando conmigo el prototipo, los datos y la herramienta que eligieron lo antes posible: el plazo es de una semana.

## 4. Requerimientos / Enunciado

### 4.1. Construir la herramienta

Elegí **uno** de tus prototipos del TP2 y construí una herramienta de Editor para sus datos. Es importante que la entrega tenga una escena que se pueda abrir y probar algun prototipo del juego.

La herramienta tiene que tener:

* Una **EditorWindow** o un **editor de grafos con Graph Toolkit**.
* Un **custom Inspector** para al menos uno de los tipos de datos que edita.

Si ningún prototipo tiene datos que una persona del equipo querría editar, pueden extraerlos: pasar a `ScriptableObjects` (u otro asset) valores que hoy están en código o en la escena, y que la herramienta edite esos assets.

El código de Editor tiene que estar separado del de runtime (carpeta `Editor/` o un assembly de Editor propio). Si el runtime usa `UnityEditor`, el build falla.

### 4.2. Validaciones y tests

Las reglas que deciden si un dato es válido viven en **un solo lugar** (por ejemplo, un método `Validate()` en el ScriptableObject, como en clase14). La herramienta y los tests las leen de ahí: no dupliquen la regla en la UI.

Cada validación tiene tests de **Edit Mode** con casos válidos, inválidos y los límites relevantes. No hace falta testear la UI de la herramienta. Los tests del TP2 tienen que seguir pasando.

Correr los tests en batch, sin abrir el Editor, como en la [clase 08](../../clases/clase08/README.md), es opcional, pero es la forma en que un agente verifica su propio trabajo.

### 4.3. Trabajar con un agente

Es **obligatorio** construir la herramienta trabajando con un agente de LLM (Claude Code, Codex, Copilot en modo agente, Cursor, Gemini CLI, …): el que tengan a mano. Un chat donde ustedes copian y pegan el código no cuenta como agente. Si no consiguen acceso a ninguno, háblenlo conmigo antes de empezar.

Agreguen al repo un `AGENTS.md` con, como mínimo:

* Qué es el proyecto y la versión de Unity.
* Un mapa del código: dónde está cada cosa.
* Cómo abrir el proyecto y correr los tests.
* Las reglas del proyecto: dónde viven las validaciones, qué va en código de Editor y qué en runtime, convenciones, y qué no debe tocar el agente.

Pueden tomar como ejemplo el [`AGENTS.md` de clase14](../../clases/clase14/AGENTS.md), pero tiene que estar escrito para su proyecto: uno copiado no le sirve a un agente que trabaje en su repo.

### 4.4. Documentar lo necesario para evaluar

Actualicen el `README.md` del repo con una sección del TP3 que explique, como mínimo, qué prototipo eligieron, cómo abrir la herramienta y cómo correr los tests.

No hace falta escribir un informe ni fundamentar la herramienta como en el TP1: lo vamos a ver en la defensa.
