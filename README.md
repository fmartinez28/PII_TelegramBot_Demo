![UCU](https://github.com/ucudal/PII_Conceptos_De_POO/raw/master/Assets/logo-ucu.png)

### FIT - Universidad Católica del Uruguay

<br>

# Demo de bots de Telegram

Pequeña demo de un bot de Telegram en C#.

Para probar el bot:

1. Clona este repo.

2. Crea un nuevo bot en Telegram; anota el token que te indica el [@BotFather](https://telegram.me/BotFather).

3. Crea un archivo `secrets.json` en las siguientes ubicaciones dependiendo de tu sistema operativo; si no existe alguna de las carpetas en la ruta deberás crearla -`%APPDATA%` en Windows siempre existe, así como `~` siempre existe en Linux/macOS-:

- **Windows**: `%APPDATA%\\Microsoft\\UserSecrets\\PII_TelegramBot_Demo\\secrets.json`
- **Linux/macOs**: `~/.microsoft/usersecrets/PII_TelegramBot_Demo/secrets.json`

4. Edita el archivo `secrets.json` para que contenga la configuración que aparece a continuación, donde reemplazas `<token>` por el que te dio el BotFather:
    ```json
    {
    "BotSecret:Token": "<token>"
    }
    ```

> 🤔 ¿Porqué la complicamos?
>
> De esta forma vas a poder subir el código de tu bot a repositorios públicos de GitHub sin compartir el token de tu bot. No vas a tener que hacerlo ahora, pero si en algún momento quieres ejecutar tu bot en otro ambiente como un servidor de producción o en Azure, podrás configurar el token secreto en forma segura.

### v1

Pequeña demo de un bot de telegram en C#. Este bot responde a los siguientes mensajes de texto:

- "Hola"
- "Chau"
- "Foto"

### v2

Ídem v1 pero usando el patrón [Chain of Responsibility](https://refactoring.guru/design-patterns/chain-of-responsibility) para implementar los tres comandos del bot.

### v3

Ídem v2 pero sin usar un bot, es decir, prueba los comandos mediante casos de prueba. También es posible probar los comandos de forma interactiva por la consola, emulando un bot.

### v4

Ídem v3 agregando comandos que piden uno o dos datos antes de ejecutar vía _prompts_:

- "Dirección"
- "Distancia"

 Estos comandos tienen estado y datos capturados durante la ejecución. Estos nuevos comandos también son probados mediante casos de prueba o de forma interactiva en la consola.