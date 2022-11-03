using NUnit.Framework;
using Ucu.Poo.TelegramBot;
using Telegram.Bot.Types;

namespace ProgramTests
{
    public class DistanceHandlerTests
    {
        private class TestDistanceResult : IDistanceResult
        {
            public bool FromExists { get; }

            public bool ToExists { get; }

            public double Distance { get; }

            public double Time { get; }

            public TestDistanceResult(bool fromExists, bool toExists, double distance, double time)
            {
                this.FromExists = fromExists;
                this.ToExists = toExists;
                this.Distance = distance;
                this.Time = time;
            }
        }

        // Un calculador de distancias de prueba
        private class TestDistanceCalculator : IDistanceCalculator
        {
            // Cuando la dirección de origen es "from" y la dirección de destino es "to" retorna una distancia y tiempo
            // predeterminados y que ambas existen; en caso contrario retorna cuál no existe.
            public IDistanceResult CalculateDistance(string fromAddress, string toAddress)
            {
                return new TestDistanceResult(fromAddress.Equals("from"), toAddress.Equals("to"), 10.0, 50.0);
            }
        }

        DistanceHandler handler;
        Message message;
        IDistanceCalculator calculator = new TestDistanceCalculator();

        [SetUp]
        public void Setup()
        {
            handler = new DistanceHandler(calculator, null);
            message = new Message();
        }

        [Test]
        // Prueba que el comando sea procesado
        public void TestDistanceHandled()
        {
            message.Text = handler.Keywords[0];
            string response;

            IHandler result = handler.Handle(message, out response);

            Assert.That(result, Is.Not.Null);
            Assert.That(handler.State, Is.EqualTo(DistanceHandler.DistanceState.FromAddressPrompt));
        }

        [Test]
        // Prueba que el comando sea procesado y pida una dirección de origen
        public void TestFromAddressHandled()
        {
            message.Text = handler.Keywords[0];
            string response;
            handler.Handle(message, out response);

            message.Text = "from";
            IHandler result = handler.Handle(message, out response);

            Assert.That(result, Is.Not.Null);
            Assert.That(handler.Data.FromAddress, Is.EqualTo("from"));
            Assert.That(handler.State, Is.EqualTo(DistanceHandler.DistanceState.ToAddressPrompt));
        }

        [Test]
        // Prueba que el comando sea procesado, pida una dirección de origen y de destino, y si ambas existen calcule la
        // distancia entre ambas
        public void TestAddressFound()
        {
            message.Text = handler.Keywords[0];
            string response;
            handler.Handle(message, out response);

            message.Text = "from";
            handler.Handle(message, out response);

            message.Text = "to";
            IHandler result = handler.Handle(message, out response);

            Assert.That(result, Is.Not.Null);
            Assert.That(handler.Data.FromAddress, Is.EqualTo("from"));
            Assert.That(handler.Data.ToAddress, Is.EqualTo("to"));
            Assert.That(handler.Data.Result.FromExists, Is.True);
            Assert.That(handler.Data.Result.ToExists, Is.True);
            Assert.That(handler.State, Is.EqualTo(DistanceHandler.DistanceState.Start));
        }

        [Test]
        // Prueba que el comando sea procesado, pida una dirección de origen y otra de destino, y si alguna no existe
        // que vuelva a pedir ambas direcciones
        public void TestAddressNotFound()
        {
            message.Text = handler.Keywords[0];
            string response;
            handler.Handle(message, out response);

            message.Text = "from";
            handler.Handle(message, out response);

            message.Text = "xyz";
            IHandler result = handler.Handle(message, out response);

            Assert.That(result, Is.Not.Null);
            Assert.That(handler.Data.FromAddress, Is.EqualTo("from"));
            Assert.That(handler.Data.ToAddress, Is.EqualTo("xyz"));
            Assert.That(handler.Data.Result.FromExists, Is.True);
            Assert.That(handler.Data.Result.ToExists, Is.False);
            Assert.That(handler.State, Is.EqualTo(DistanceHandler.DistanceState.FromAddressPrompt));
        }

        [Test]
        // Prueba que el comando sea cancelado.
        public void TestCancel()
        {
            handler.Cancel();

            Assert.That(handler.State, Is.EqualTo(DistanceHandler.DistanceState.Start));
            Assert.That(handler.Data.FromAddress, Is.EqualTo(default(string)));
            Assert.That(handler.Data.ToAddress, Is.EqualTo(default(string)));
            Assert.That(handler.Data.Result, Is.Null);
        }

        [Test]
        // Prueba que ocurre cuando el comando se ejecuta sin que haya un calculador asignado.
        public void TestNoCalculator()
        {
            DistanceHandler localHandler = new DistanceHandler(null, null);

            message.Text = localHandler.Keywords[0];
            string response;
            localHandler.Handle(message, out response);

            message.Text = "";
            IHandler result = localHandler.Handle(message, out response);

            Assert.That(result, Is.Not.Null);
            Assert.That(localHandler.State, Is.EqualTo(DistanceHandler.DistanceState.Start));
        }
    }
}