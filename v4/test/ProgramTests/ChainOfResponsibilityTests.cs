using NUnit.Framework;
using Ucu.Poo.TelegramBot;
using Telegram.Bot.Types;

namespace ProgramTests
{
    public class ChainOfResponsibilityTests
    {
        IHandler handler;
        HelloHandler helloHandler = new HelloHandler(null);
        GoodByeHandler goodByeHandler = new GoodByeHandler(null);
        PhotoHandler photoHandler = new PhotoHandler(null, null);
        Message message = new Message();

        [SetUp]
        public void Setup()
        {
            helloHandler.Next = goodByeHandler;
            goodByeHandler.Next = photoHandler;

            handler = helloHandler;

            message = new Message();
        }

        [Test]
        public void TestHelloHandlesHello()
        {
            message.Text = helloHandler.Keywords[0];
            string response;

            IHandler result = handler.Handle(message, out response);

            Assert.That(result, Is.Not.Null);
            Assert.That(response, Is.EqualTo("¡Hola! ¿Cómo estás?"));
        }

        [Test]
        public void TestGoodByeHandlesGoodBye()
        {
            message.Text = goodByeHandler.Keywords[0];
            string response;

            IHandler result = handler.Handle(message, out response);

            Assert.That(result, Is.Not.Null);
            Assert.That(response, Is.EqualTo("¡Chau! ¡Qué andes bien!"));
        }

        [Test]
        public void TestPictureHandlesPhoto()
        {
            message.Text = photoHandler.Keywords[0];
            string response;

            IHandler result = handler.Handle(message, out response);

            Assert.That(result, Is.Not.Null);
            Assert.That(response, Is.Empty);
        }

        [Test]
        public void NoOneHandlesNoCommand()
        {
            message.Text = "xyz";
            string response;

            IHandler result = handler.Handle(message, out response);

            Assert.That(result, Is.Null);
            Assert.That(response, Is.Empty);
        }

        [Test]
        public void TestCancel()
        {
            handler.Cancel();
        }
    }
}