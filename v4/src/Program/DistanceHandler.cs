using Telegram.Bot.Types;

namespace Ucu.Poo.TelegramBot
{
    /// <summary>
    /// Un "handler" del patrón Chain of Responsibility que implementa el comando "distancia".
    /// </summary>
    public class DistanceHandler : BaseHandler
    {
        // Un calculador de distancias entre dos direcciones. Permite que la forma de calcular distancias se determine en
        // tiempo de ejecución: en el código final se asigna un objeto que use una API para buscar distancias; y en los
        // casos de prueba se asigne un objeto que retorne un resultado que puede ser configurado desde el caso de prueba.
        private IDistanceCalculator calculator;

        /// <summary>
        /// El estado del comando.
        /// </summary>
        public DistanceState State { get; private set; }

        /// <summary>
        /// Los datos que va obteniendo el comando en los diferentes estados.
        /// </summary>
        public DistanceData Data { get; private set; } = new DistanceData();

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="DistanceHandler"/>.
        /// </summary>
        /// <param name="calculator">Un calculador de distancias.</param>
        /// <param name="next">El próximo "handler".</param>
        public DistanceHandler(IDistanceCalculator calculator, DistanceHandler next)
            : base(new string[] { "distancia" }, next)
        {
            this.calculator = calculator;
            this.State = DistanceState.Start;
        }

        /// <summary>
        /// Determina si este "handler" puede procesar el mensaje. En el primer mensaje cuando
        /// <see cref="DistanceHandler.State"/> es <see cref="DistanceState.Start"/> usa
        /// <see cref="BaseHandler.Keywords"/> para buscar el texto en el mensaje ignorando mayúsculas y minúsculas. En
        /// caso contrario eso implica que los sucesivos mensajes son parámetros del comando y se procesan siempre.
        /// </summary>
        /// <param name="message">El mensaje a procesar.</param>
        /// <returns>true si el mensaje puede ser pocesado; false en caso contrario.</returns>
        protected override bool CanHandle(Message message)
        {
            if (this.State == DistanceState.Start)
            {
                return base.CanHandle(message);
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// Procesa todos los mensajes y retorna true siempre.
        /// </summary>
        /// <param name="message">El mensaje a procesar.</param>
        /// <param name="response">La respuesta al mensaje procesado indicando que el mensaje no pudo se procesado.</param>
        /// <returns>true si el mensaje fue procesado; false en caso contrario.</returns>
        protected override void InternalHandle(Message message, out string response)
        {
            if (this.State ==  DistanceState.Start)
            {
                // En el estado Start le pide la dirección de origen y pasa al estado FromAddressPrompt
                this.State = DistanceState.FromAddressPrompt;
                response = "Vamos a calcular una distancia. Decime la dirección de origen por favor";
            }
            else if ((this.State == DistanceState.FromAddressPrompt) && (this.calculator != null))
            {
                // En el estado FromAddressPrompt el mensaje recibido es la respuesta con la dirección de origen
                this.Data.FromAddress = message.Text;
                this.State = DistanceState.ToAddressPrompt;
                response = "Ahora decime la dirección de destino por favor";
            }
            else if ((this.State == DistanceState.ToAddressPrompt) && (this.calculator != null))
            {
                this.Data.ToAddress = message.Text;
                this.Data.Result = this.calculator.CalculateDistance(this.Data.FromAddress, this.Data.ToAddress);

                if (this.Data.Result.FromExists && this.Data.Result.ToExists)
                {
                    // Si encuentra ambas direcciones retorna la distancia y pasa nuevamente al estado Start
                    response = $"La distancia es {this.Data.Result.Distance}";
                    this.State = DistanceState.Start;
                }
                else
                {
                    // Si no encuentra alguna de las direcciones se las pide de nuevo y vuelve al estado FromAddressPrompt.
                    // Una versión más sofisticada podría determinar cuál de las dos direcciones no existe y volver al
                    // estado en el que se pide la dirección que falta.
                    response = "No encuentro alguna de las direcciones. Decime la dirección de origen por favor";
                    this.State = DistanceState.FromAddressPrompt;
                }
            }
            else if (((State == DistanceState.FromAddressPrompt) || (State == DistanceState.FromAddressPrompt))
                && (this.calculator == null))
            {
                // En los estados FromAddressPrompt o ToAddressPrompt si no hay un buscador de direcciones hay que
                // responder que hubo un error y volver al estado inicial.
                response = "Error interno de configuración, no puedo calcular distancias";
                this.State = DistanceState.Start;
            }
            else
            {
                response = string.Empty;
            }
        }

        /// <summary>
        /// Retorna este "handler" al estado inicial.
        /// </summary>
        protected override void InternalCancel()
        {
            this.State = DistanceState.Start;
            this.Data = new DistanceData();
        }

        /// <summary>
        /// Indica los diferentes estados que puede tener el comando DistanceHandler.
        /// - Start: El estado inicial del comando. En este estado el comando pide la dirección de origen y pasa al
        /// siguiente estado.
        /// - FromAddressPrompt: Luego de pedir la dirección de origen. En este estado el comando pide la dirección de
        /// destino y pasa al siguiente estado.
        /// - ToAddressPrompt: Luego de pedir la dirección de destino. En este estado el comando calcula la distancia
        /// y vuelve al estado Start.
        /// </summary>
        public enum DistanceState
        {
            Start,
            FromAddressPrompt,
            ToAddressPrompt
        }

        /// <summary>
        /// Representa los datos que va obteniendo el comando DistanceHandler en los diferentes estados.
        /// </summary>
        public class DistanceData
        {
            /// <summary>
            /// La dirección que se ingresó en el estado DistanceState.FromAddressPrompt.
            /// </summary>
            public string FromAddress { get; set; }

            /// <summary>
            /// La dirección que se ingresó en el estado DistanceState.ToAddressPrompt.
            /// </summary>
            public string ToAddress { get; set; }

            /// <summary>
            /// El resultado del cálculo de la distancia entre las direcciones ingresadas.
            /// </summary>
            public IDistanceResult Result { get; set; }
        }
    }
}