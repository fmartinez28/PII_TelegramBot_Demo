using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;

namespace Ucu.Poo.TelegramBot
{
    /// <summary>
    /// Un programa que implementa un bot de Telegram.
    /// </summary>
    public class Program
    {
        // La instancia del bot.
        private static TelegramBotClient Bot;

        // El token provisto por Telegram al crear el bot. Mira el archivo README.md en la raíz de este repo para
        // obtener indicaciones sobre cómo configurarlo.
        private static string token;

        // Esta clase es un POCO -vean https://en.wikipedia.org/wiki/Plain_old_CLR_object- para representar el token
        // secreto del bot.
        private class BotSecret
        {
            public string Token { get; set; }
        }

        // Una interfaz requerida para configurar el servicio que lee el token secreto del bot.
        private interface ISecretService
        {
            string Token { get; }
        }

        // Una clase que provee el servicio de leer el token secreto del bot.
        private class SecretService : ISecretService
        {
            private readonly BotSecret _secrets;

            public SecretService(IOptions<BotSecret> secrets)
            {
                _secrets = secrets.Value ?? throw new ArgumentNullException(nameof(secrets));
            }

            public string Token { get { return _secrets.Token; } }
        }

        // Configura la aplicación.
        private static void Start()
        {
            // Lee una variable de entorno NETCORE_ENVIRONMENT que si no existe o tiene el valor 'development' indica
            // que estamos en un ambiente de desarrollo.
            var developmentEnvironment = Environment.GetEnvironmentVariable("NETCORE_ENVIRONMENT");
            var isDevelopment =
                string.IsNullOrEmpty(developmentEnvironment) ||
                developmentEnvironment.ToLower() == "development";

            var builder = new ConfigurationBuilder();
            builder
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            // En el ambiente de desarrollo el token secreto del bot se toma de la configuración secreta
            if (isDevelopment)
            {
                builder.AddUserSecrets<Program>();
            }

            var configuration = builder.Build();

            IServiceCollection services = new ServiceCollection();

            // Mapeamos la implementación de las clases para  inyección de dependencias
            services
                .Configure<BotSecret>(configuration.GetSection(nameof(BotSecret)))
                .AddSingleton<ISecretService, SecretService>();

            var serviceProvider = services.BuildServiceProvider();
            var revealer = serviceProvider.GetService<ISecretService>();
            token = revealer.Token;
        }

        private static IHandler firstHandler;

        /// <summary>
        /// Punto de entrada al programa.
        /// </summary>
        public static void Main()
        {
            Start();

            Bot = new TelegramBotClient(token);

            firstHandler =
                new HelloHandler(
                new GoodByeHandler(
                new PhotoHandler(Bot, null)
            ));

            var cts = new CancellationTokenSource();

            // Comenzamos a escuchar mensajes. Esto se hace en otro hilo (en background). El primer método
            // HandleUpdateAsync es invocado por el bot cuando se recibe un mensaje. El segundo método HandleErrorAsync
            // es invocado cuando ocurre un error.
            Bot.StartReceiving(
                HandleUpdateAsync,
                HandleErrorAsync,
                new ReceiverOptions()
                {
                    AllowedUpdates = Array.Empty<UpdateType>()
                },
                cts.Token
            );

            Console.WriteLine($"Bot is up!");

            // Esperamos a que el usuario aprete Enter en la consola para terminar el bot.
            Console.ReadLine();

            // Terminamos el bot.
            cts.Cancel();
        }

        /// <summary>
        /// Maneja las actualizaciones del bot (todo lo que llega), incluyendo mensajes, ediciones de mensajes,
        /// respuestas a botones, etc. En este ejemplo sólo manejamos mensajes de texto.
        /// </summary>
        public static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            try
            {
                // Sólo respondemos a mensajes de texto
                if (update.Type == UpdateType.Message)
                {
                    await HandleMessageReceived(botClient, update.Message);
                }
            }
            catch(Exception e)
            {
                await HandleErrorAsync(botClient, e, cancellationToken);
            }
        }

        /// <summary>
        /// Maneja los mensajes que se envían al bot a través de handlers de una chain of responsibility.
        /// </summary>
        /// <param name="message">El mensaje recibido</param>
        /// <returns></returns>
        private static async Task HandleMessageReceived(ITelegramBotClient botClient, Message message)
        {
            Console.WriteLine($"Received a message from {message.From.FirstName} saying: {message.Text}");

            string response = string.Empty;

            firstHandler.Handle(message, out response);

            if (!string.IsNullOrEmpty(response))
            {
                await Bot.SendTextMessageAsync(message.Chat.Id, response);
            }
        }

        /// <summary>
        /// Manejo de excepciones. Por ahora simplemente la imprimimos en la consola.
        /// </summary>
        public static Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            Console.WriteLine(exception.Message);
            return Task.CompletedTask;
        }
    }
}