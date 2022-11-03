using NUnit.Framework;
using Ucu.Poo.TelegramBot;
using Telegram.Bot.Types;

namespace ProgramTests
{
    // Clase de prueba de la clase AddressHandler.
    public class AddressHandlerTests
    {
        // Resultado de las búsquedas de prueba; sólo se usa el atributo Found
        private class TestAddressResult : IAddressResult
        {
            public bool Found { get; set; }
            public double Latitude { get; }
            public double Longitude { get; }
        }

        // Buscador de direcciones de prueba
        private class TestAddressFinder : IAddressFinder
        {
            // Retorna true si la dirección es "true" y false en caso contrario.
            public IAddressResult GetLocation(string address)
            {
                return new TestAddressResult() { Found = address.Equals("true") };
            }
        }

        AddressHandler handler;
        Message message;
        TestAddressFinder finder = new TestAddressFinder();

        [SetUp]
        public void Setup()
        {
            handler = new AddressHandler(finder, null);
            message = new Message();
        }

        [Test]
        // Prueba que el comando sea procesado.
        public void TestAddressHandled()
        {
            message.Text = handler.Keywords[0];
            string response;

            IHandler result = handler.Handle(message, out response);

            Assert.That(result, Is.Not.Null);
            Assert.That(handler.State, Is.EqualTo(AddressHandler.AddressState.AddressPrompt));
        }

        [Test]
        // Prueba que el comando sea procesado, pida una dirección que existe, y la encuentre.
        public void TestAddressFound()
        {
            message.Text = this.handler.Keywords[0];
            string response;
            this.handler.Handle(message, out response);

            message.Text = "true";
            IHandler result = this.handler.Handle(message, out response);

            Assert.That(result, Is.Not.Null);
            Assert.That(this.handler.State, Is.EqualTo(AddressHandler.AddressState.Start));
            Assert.That(this.handler.Data.Result.Found, Is.True);
        }

        [Test]
        // Prueba que el comando sea procesado, pide una dirección que no existe, y no la encuentre.
        public void TestAddressNotFound()
        {
            message.Text = this.handler.Keywords[0];
            string response;
            this.handler.Handle(message, out response);

            message.Text = "false";
            IHandler result = this.handler.Handle(message, out response);

            Assert.That(result, Is.Not.Null);
            Assert.That(handler.State, Is.EqualTo(AddressHandler.AddressState.AddressPrompt));
            Assert.That(handler.Data.Result.Found, Is.False);

            message.Text = "true";
            result = handler.Handle(message, out response);

            Assert.That(result, Is.Not.Null);
            Assert.That(this.handler.State, Is.EqualTo(AddressHandler.AddressState.Start));
            Assert.That(this.handler.Data.Result.Found, Is.True);
        }

        [Test]
        // Prueba que el comando sea cancelado.
        public void TestCancel()
        {
            handler.Cancel();

            Assert.That(handler.State, Is.EqualTo(AddressHandler.AddressState.Start));
            Assert.That(handler.Data.Address, Is.EqualTo(default(string)));
            Assert.That(handler.Data.Result, Is.Null);
        }

        [Test]
        // Prueba que ocurre cuando el comando se ejecuta sin que haya un buscador de direcciones asignado.
        public void TestNoFinder()
        {
            handler = new AddressHandler(null, null);

            message.Text = handler.Keywords[0];
            string response;
            handler.Handle(message, out response);

            message.Text = string.Empty;
            IHandler result = handler.Handle(message, out response);

            Assert.That(result, Is.Not.Null);
            Assert.That(handler.State, Is.EqualTo(AddressHandler.AddressState.Start));
        }
    }
}