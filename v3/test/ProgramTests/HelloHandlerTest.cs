using NUnit.Framework;
using Ucu.Poo.TelegramBot;
using Telegram.Bot.Types;

namespace ProgramTests
{
    public class HelloHandlerTests
    {
        HelloHandler handler;
        Message message;

        [SetUp]
        public void Setup()
        {
            handler = new HelloHandler(null);
            message = new Message();
        }

        [Test]
        public void TestHandle()
        {
            message.Text = handler.Keywords[0];
            string response;

            IHandler result = handler.Handle(message, out response);

            Assert.That(result, Is.Not.Null);
            Assert.That(response, Is.EqualTo("¡Hola! ¿Cómo estás?"));
        }

        [Test]
        public void TestDoesNotHandle()
        {
            message.Text = "adios";
            string response;

            IHandler result = handler.Handle(message, out response);

            Assert.That(result, Is.Null);
            Assert.That(response, Is.Empty);
        }
    }
}