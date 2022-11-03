using System;
using Telegram.Bot.Types;

namespace Ucu.Poo.TelegramBot
{
    /// <summary>
    /// Un progrma de demostración que permite escribir comandos en la consola emulando el comportamiento de un bot. Los
    /// comandos son procesados usando el patrón Chain or Responsiblity al igual que como podrían ser procesados en un bot.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// Punto de entrada al programa.
        /// </summary>
        public static void Main()
        {
            IHandler handler =
                new HelloHandler(
                new GoodByeHandler(
                new PhotoHandler(null, null)
            ));
            Message message = new Message();
            string response;

            Console.WriteLine("Escribí un comando o 'salir':");
            Console.Write("> ");

            while (true)
            {
                message.Text = Console.ReadLine();
                if (message.Text.Equals("salir", StringComparison.InvariantCultureIgnoreCase))
                {
                    Console.WriteLine("Salimos");
                    return;
                }

                IHandler result = handler.Handle(message, out response);

                if (result == null)
                {
                    Console.WriteLine("No entiendo");
                    Console.Write("> ");
                }
                else
                {
                    Console.WriteLine(response);
                    Console.Write("> ");
                }
            }
        }
    }
}