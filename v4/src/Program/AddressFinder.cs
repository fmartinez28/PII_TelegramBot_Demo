using Ucu.Poo.Locations.Client;
using Nito.AsyncEx;

namespace Ucu.Poo.TelegramBot
{
    /// <summary>
    /// Un buscador de direcciones concreto que usa una API de localización.
    /// </summary>
    public class AddressFinder : IAddressFinder
    {
        private LocationApiClient client;

        /// <summary>
        /// Inicializa una nueva instancia de AddressFinder.
        /// </summary>
        /// <param name="client">El cliente de la API de localización.</param>
        public AddressFinder(LocationApiClient client)
        {
            this.client = client;
        }

        /// <summary>
        /// Determina si existe una dirección.
        /// </summary>
        /// <param name="address">La dirección a buscar.</param>
        /// <returns>Una instancia de AddressResult con el resultado de la búsqueda, que incluye si la dirección se
        /// encontró o no, y si se encontró, la latitud y la longitud de la dirección.</returns>
        public IAddressResult GetLocation(string address)
        {
            Location location = AsyncContext.Run(() => this.client.GetLocationAsync(address));
            AddressResult result = new AddressResult(location);

            return result;
        }
    }

    /// <summary>
    /// Una implementación concreta del resutlado de buscar una dirección. Además de las propiedades definidas en
    /// IAddressResult esta clase agrega una propiedad Location para acceder a las coordenadas de la dirección buscada.
    /// </summary>
    public class AddressResult : IAddressResult
    {
        public Location Location { get; }

        public AddressResult(Location location)
        {
            this.Location = location;
        }

        public bool Found
        {
            get
            {
                return this.Location.Found;
            }
        }

        public double Latitude
        {
            get
            {
                return this.Location.Latitude;
            }
        }

        public double Longitude
        {
            get
            {
                return this.Location.Longitude;
            }
        }
    }
}