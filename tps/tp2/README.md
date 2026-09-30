# Trabajo Práctico 2: Implementación y Tests en Unity

- **Integrantes por equipo:** 1
- **Fecha límite de entrega:** 28/10, 23:59
- **Fecha límite de reentrega:** 04/11, 23:59
- **Exposición pública presencial:** 04/11
- **Se aprueba con:** 4

## 1. Objetivo

En el [TP1](../tp1/README.md) eligieron problemas técnicos de sus juegos y diseñaron soluciones. En este trabajo van a implementar esas soluciones y validar su comportamiento con tests automatizados.

La idea es que tomen los diseños del TP1 como un encargo que les pasó el líder de un equipo: tienen que llevarlos a una implementación real, respetando lo acordado y explicando los ajustes que sean necesarios.

No espero que entreguen un juego completo, ni siquiera que las funcionalidades puedan jugarse de forma aislada. Espero **3 pruebas de concepto, o 2 si una corresponde a un megaproblema**, técnicamente muy pulidas: bien implementadas, bien diseñadas y bien testeadas dentro de un alcance claro.

El pulido que buscamos está en el código, sus contratos y sus pruebas; no hace falta agregar presentación visual o contenido que no ayude a demostrar la solución.

Recuerden que, por lo general, la solución más simple suele ser la mejor.

## 2. Condiciones de entrega

La entrega consiste en un **repositorio de GitHub** que incluya:

* La entrega del **TP1**, como referencia para comparar el diseño con la implementación.
* El código de los prototipos y sus tests automatizados.
* Las instrucciones necesarias para abrir los proyectos, ejecutar los prototipos y correr los tests, indicando la versión de Unity utilizada.
* Documentación breve del alcance, las exclusiones y las decisiones de cada prototipo, según lo indicado en la sección 4.

El código debe compilar y las instrucciones deben permitir reproducir las pruebas. Los prototipos pueden estar en un mismo proyecto o en proyectos separados, siempre que quede claro cómo ejecutar y evaluar cada uno.

Para entregar, tienen que crear un tag en el repositorio y enviar **el enlace al tag en GitHub** a `diego.freijo@udelaciudad.edu.ar` antes del límite correspondiente:

| Instancia | Tag | Fecha límite |
| --- | --- | --- |
| Entrega | `tp2` | 28/10, 23:59 |
| Reentrega | `rtp2` | 04/11, 23:59 |

El tag identifica la versión que voy a evaluar: no alcanza con mandar un enlace a la rama principal. Una vez enviado, no lo muevan a otro commit.

## 3. Evaluación

Además de entregar el repositorio, otra vez van a exponer públicamente sus soluciones en una **defensa oral presencial en la facultad el 04/11**, en la fecha del recuperatorio.

- **1/3 de la nota** corresponde a la entrega: implementación, tests y documentación.
- **2/3 de la nota** corresponden a la exposición y defensa oral.

Van a tener **20 minutos** para presentar el trabajo y después habrá **10 minutos** de preguntas y debate.

### 3.1. Mínimos para aprobar

Tanto la entrega como la defensa deben alcanzar los mínimos de aprobación. Una no compensa las insuficiencias de la otra. Los mínimos de implementación y tests se exigen para cada prototipo:

* Los comportamientos comprometidos deben estar implementados y funcionar.
* Debe haber tests automatizados relevantes que validen esos comportamientos, y deben pasar.
* Deben poder explicar la implementación, las pruebas y las decisiones centrales de cada solución.

Pueden elegir ustedes el detalle de como presentan cada proptotipo. Recomiendo que cada una de las tres funcionalidades esté en una carpeta independiente del resto. Y que tenga una sola escena que ejecute la prueba.

Un test que detecta un defecto puede estar bien elegido, pero el comportamiento que falla sigue pendiente de corrección. A la inversa, que todos los tests pasen no demuestra por sí solo que sean suficientes o pertinentes.

### 3.2. Qué voy a evaluar

El cumplimiento del alcance acordado, la claridad del diseño, la calidad de los tests y la fundamentación de las decisiones. No buscamos cantidad de código, patrones o pruebas, sino soluciones adecuadas al problema.

Durante la defensa tienen que apoyarse en el código y los tests. Puedo pedirles que ejecuten una prueba, expliquen qué error detectaría, recorran una decisión de diseño o analicen un caso límite. No hace falta recordar cada línea de memoria, pero sí entender lo que entregaron.

Podemos usar mi maquina para ejecutar todo lo necesario en vivo.

Les recomiendo que vayan validando conmigo el alcance, las decisiones y las pruebas durante las clases anteriores a la entrega.

## 4. Requerimientos / Enunciado

### 4.1. Implementar las soluciones del TP1

Implementá las soluciones elegidas para los problemas técnicos del TP1. Mantené los **3 problemas**, o los **2 si uno fue acordado como megaproblema**.

Para cada prototipo, dejá explícito:

* Qué problema resuelve y qué solución del TP1 implementa.
* Qué comportamientos concretos deben funcionar.
* Qué supuestos necesita y qué situaciones quedan fuera del alcance, justificando esas exclusiones.

No hace falta resolver cualquier situación imaginable, pero el alcance debe ser coherente con el problema original. Excluir un caso necesita una justificación; no alcanza con dejarlo afuera porque la implementación no lo resuelve.

Los ajustes internos que aparezcan al implementar pueden documentarse brevemente en la entrega. Si necesitás cambiar sustancialmente la arquitectura, reemplazar una funcionalidad o reducir el alcance, **validalo conmigo antes de avanzar**. Explicá qué cambió respecto del TP1, qué descubriste al implementar y por qué el ajuste tiene sentido.

### 4.2. Validar con tests automatizados

Usá lo trabajado en la [clase 08](../../clases/clase08/README.md) para validar las implementaciones. Cada prototipo debe incluir tests de sus comportamientos importantes y de los casos límite relevantes para su alcance.

No hay una cantidad fija de tests ni es obligatorio incluir todas las categorías. Elegí las pruebas según lo que necesitás comprobar y explicá:

* Qué comportamiento verifica cada prueba o grupo de pruebas.
* Qué casos elegiste y por qué son relevantes.
* Qué errores de implementación permitirían detectar.

Las pruebas deben poder ejecutarse de forma reproducible, preparar lo que necesitan y limpiar los recursos que crean cuando corresponda. Evitá que dependan del orden de ejecución o de una preparación manual que las instrucciones no contemplen.

Recuerden que unitario/integración, funcional y Edit Mode/Play Mode describen aspectos distintos de una prueba. No hace falta usar Play Mode en todos los prototipos. Pero si el comportamiento depende de física, del ciclo de vida de Unity o de otra interacción con el motor, deben validar esa integración relevante: llamar directamente a un método puede no alcanzar para demostrar que funciona cuando Unity lo invoca. **Elegir que testear y cómo testearlo es una parte fundamental de lo que quiero evaluar**.

### 4.3. Demostrar las propiedades del diseño

Algunos problemas del TP1 no se resuelven solamente mostrando una funcionalidad en ejecución. Si la solución promete una propiedad de diseño, presentá evidencia de que la cumple. Por ejemplo:

* Si busca extensibilidad, mostrás cómo agregar una variante.
* Si busca desacoplamiento, mostrás cómo sustituir una dependencia.
* Si busca controlar el estado, comprobás quién puede modificarlo y qué reglas deben preservarse.

La demostración puede formar parte de los tests o de la exposición, según corresponda. Lo importante es que permita comprobar la propiedad que justificó la elección del diseño.

### 4.4. Fundamentar las decisiones de código

Los conceptos de la clase 09 —contratos, `null`, mutabilidad, transparencia referencial, entre otros— van a ser una **lupa para analizar sus soluciones**, no una lista de recursos que deban incorporar obligatoriamente.

Tienen que poder explicar las decisiones relevantes: qué entradas acepta una operación, qué garantiza, qué estados son válidos, quién puede modificarlos y cómo se representa la ausencia de un valor.

Por ejemplo, usar `null` no está mal por sí mismo: deberían poder explicar por qué lo eligieron en lugar de representar ese caso con un objeto y cómo se maneja. Del mismo modo, una excepción puede tener sentido cuando no se cumple una precondición, siempre que puedan explicar ese contrato, quién debe cumplirlo y por qué resulta adecuado.

Se evalúa que las decisiones tengan sentido para el problema, que el código respete los contratos elegidos y que las pruebas comprueben las reglas relevantes.

### 4.5. Documentar lo necesario para evaluar

Agreguen al repo un `README.md` que explique lo que haga falta. Mínimo, debería explicar cómo ejecutar las pruebas y cómo correr los tests.

No hace falta escribir otro informe extenso como el del TP1. Incluí una explicación breve por prototipo que reúna el alcance y sus exclusiones justificadas, los cambios respecto del diseño original, las decisiones relevantes y la relación entre los comportamientos esperados y los tests que los validan. 


Podés usar texto, diagramas o referencias al código. La documentación y las instrucciones deben permitir entender qué implementaste, cómo comprobarlo y qué vas a defender durante la exposición.
