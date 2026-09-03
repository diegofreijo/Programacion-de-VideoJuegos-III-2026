# Posibles Problemas Para Elegir en el TP1

## Problemas generales

### 1. Gestión del estado del juego

* Cómo representar el estado actual del juego y sus diferentes partes.
* Qué sucede cuando el juego cambia entre situaciones como gameplay, pausa, menú o game over.
* Cómo evitar que diferentes sistemas modifiquen el estado de manera descontrolada.
* Ejemplos: vida de los personajes, inventario, progreso de una partida, estado de una misión.

### 2. Comunicación entre sistemas

* Cómo hacer que diferentes sistemas puedan comunicarse sin quedar fuertemente acoplados.
* Qué sucede cuando un sistema necesita informar a otros que ocurrió algo.
* Cómo evitar dependencias innecesarias entre sistemas que no deberían conocerse entre sí.
* Ejemplos: un enemigo muere y deben reaccionar la UI, el audio y el sistema de puntuación.

### 3. Entrada del jugador

* Cómo recibir y procesar las acciones del jugador.
* Cómo evitar que la lógica del juego dependa directamente de un dispositivo o botón específico.
* Cómo manejar diferentes esquemas de control o dispositivos.
* Ejemplos: teclado, gamepad, mouse o controles táctiles.

### 4. Configuración y datos

* Cómo almacenar y acceder a los datos que utiliza el juego.
* Cómo separar los datos de configuración de la lógica que los utiliza.
* Cómo modificar valores sin tener que cambiar código.
* Ejemplos: estadísticas de enemigos, armas, niveles de dificultad o configuración del jugador.

### 5. Guardado y carga

* Cómo guardar el estado de una partida y recuperarlo posteriormente.
* Qué información debe persistir y cuál puede reconstruirse.
* Cómo evitar que el sistema de guardado tenga que conocer todos los detalles de los sistemas del juego.
* Ejemplos: progreso, inventario, posición del jugador, configuración o desbloqueables.

### 6. UI y gameplay

* Cómo comunicar la lógica del juego con la interfaz de usuario.
* Cómo evitar que el gameplay dependa directamente de elementos concretos de la UI.
* Qué sucede cuando diferentes partes del juego necesitan actualizar la interfaz.
* Ejemplos: barras de vida, inventarios, menús, diálogos o indicadores.

### 7. Creación y ciclo de vida de objetos

* Cómo crear, inicializar, reutilizar y destruir objetos durante el juego.
* Cómo controlar quién es responsable de crear y destruir cada objeto.
* Cómo evitar problemas cuando existen muchos objetos o se crean y destruyen frecuentemente.
* Ejemplos: enemigos, proyectiles, partículas, objetos recolectables o unidades.

### 8. Escenas y transiciones

* Cómo organizar las escenas que componen el juego.
* Qué objetos deben mantenerse al cambiar de escena y cuáles deben destruirse.
* Cómo manejar las transiciones entre diferentes partes del juego.
* Ejemplos: pasar de un menú a una partida, cambiar de nivel o cargar una zona.

### 9. Extensibilidad

* Cómo agregar nuevas funcionalidades sin tener que modificar grandes partes del código existente.
* Cómo evitar que agregar contenido genere cada vez más código específico.
* Qué partes del sistema deberían poder cambiar o extenderse con facilidad.
* Ejemplos: nuevos tipos de enemigos, armas, habilidades, niveles o reglas de gameplay.

### 10. Soportar mods

* Cómo permitir que terceros agreguen o modifiquen contenido del juego.
* Qué partes del juego deberían estar disponibles para los mods y cuáles deberían permanecer protegidas.
* Cómo cargar contenido creado externamente sin acoplarlo directamente al juego.
* Ejemplos: nuevos mapas, personajes, objetos, reglas o contenido creado por la comunidad.

### 11. Multiplayer

* Cómo sincronizar el estado del juego entre diferentes jugadores.
* Qué información debe enviarse por la red y cuál puede calcularse localmente.
* Cómo manejar situaciones en las que los jugadores tienen diferentes estados o conexiones.
* Ejemplos: movimiento, combate, acciones de jugadores o partidas cooperativas.

---

## Problemas específicos de Roguelikes

### 12. Randomness y reproducibilidad

* Cómo utilizar aleatoriedad sin perder la posibilidad de reproducir un resultado.
* Cómo controlar el azar para poder probar y depurar el juego.
* Cómo generar diferentes partidas manteniendo reglas consistentes.
* Ejemplos: generación de niveles, enemigos, recompensas o eventos aleatorios.

### 13. Efectos y modificadores

* Cómo representar efectos que modifican temporal o permanentemente el comportamiento de entidades.
* Cómo combinar múltiples efectos sin crear una implementación específica para cada combinación.
* Cómo manejar efectos que se aplican, acumulan, reemplazan o eliminan entre sí.
* Ejemplos: buffs, debuffs, estados alterados, objetos o habilidades pasivas.

### 14. Contenido combinatorio

* Cómo permitir que diferentes elementos del juego interactúen entre sí de distintas maneras.
* Cómo evitar tener que implementar manualmente cada combinación posible.
* Cómo mantener estas interacciones manejables a medida que aumenta la cantidad de contenido.
* Ejemplos: objetos + habilidades, cartas + efectos, armas + modificadores o habilidades + estados.

---

## Problemas específicos de juegos de estrategia por turnos

### 15. Flujo de turnos

* Cómo representar el orden en que los jugadores y sistemas realizan sus acciones.
* Cómo manejar las diferentes fases que puede tener un turno.
* Cómo controlar las transiciones entre turnos y fases.
* Ejemplos: inicio de turno, acciones del jugador, fase de combate, mantenimiento y fin de turno.

### 16. Reglas de interacción

* Cómo representar las reglas que determinan qué ocurre cuando diferentes elementos interactúan.
* Cómo validar si una determinada interacción es posible.
* Cómo evitar que las reglas queden distribuidas y duplicadas entre diferentes objetos.
* Ejemplos: ataques, movimiento, captura de territorios, recursos o efectos entre unidades.

### 17. Acciones y órdenes

* Cómo representar las acciones que puede realizar un jugador o una unidad.
* Cómo validar, ejecutar y eventualmente cancelar una acción.
* Cómo separar la decisión de realizar una acción de su ejecución.
* Ejemplos: mover una unidad, atacar, construir, investigar o utilizar una habilidad.

### 18. IA

* Cómo separar la decisión que toma la IA de la ejecución de las acciones dentro del juego.
* Cómo permitir diferentes comportamientos o estrategias sin duplicar la lógica de gameplay.
* Cómo hacer que la IA pueda analizar el estado actual del juego para tomar decisiones.
* Ejemplos: movimiento de unidades, selección de objetivos, gestión de recursos o planificación de turnos.

### 19. Deshacer, repetición o simulación

* Cómo reconstruir estados anteriores o futuros del juego.
* Cómo representar las acciones necesarias para pasar de un estado a otro.
* Cómo ejecutar acciones sin afectar permanentemente el estado real de la partida.
* Ejemplos: *undo*, *replay*, previsualizar un movimiento o simular una acción antes de ejecutarla.
