using OtpNet;

namespace RendicionesPrimar.Services
{
    public interface IMfaService
    {
        string GenerarSecretKey();
        string GenerarCodigoVerificacion();
        bool VerificarCodigoVerificacion(string codigoIngresado, string codigoAlmacenado, DateTime? expira);
        bool VerificarCodigoTOTP(string secretKey, string codigo);
    }

    public class MfaService : IMfaService
    {
        public string GenerarSecretKey()
        {
            var key = KeyGeneration.GenerateRandomKey(20);
            return Base32Encoding.ToString(key);
        }

        public string GenerarCodigoVerificacion()
        {
            var random = new Random();
            return random.Next(100000, 999999).ToString();
        }

        public bool VerificarCodigoVerificacion(string codigoIngresado, string codigoAlmacenado, DateTime? expira)
        {
            if (string.IsNullOrEmpty(codigoIngresado) || string.IsNullOrEmpty(codigoAlmacenado))
                return false;

            if (!expira.HasValue || expira.Value < DateTime.UtcNow)
                return false;

            return codigoIngresado.Trim() == codigoAlmacenado.Trim();
        }

        public bool VerificarCodigoTOTP(string secretKey, string codigo)
        {
            try
            {
                var key = Base32Encoding.ToBytes(secretKey);
                var totp = new Totp(key);
                
                // Verificar el código actual y los códigos de los últimos 2 períodos (1 minuto)
                return totp.VerifyTotp(codigo, out _, new VerificationWindow(previous: 1, future: 1));
            }
            catch
            {
                return false;
            }
        }
    }
} 