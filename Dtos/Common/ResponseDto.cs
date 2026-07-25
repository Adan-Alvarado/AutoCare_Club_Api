
namespace AutoCare_Club_Api.Dtos.Common
{
    public class ResponseDto<T>
    {
        public int StatusCode { get; set; } // Código de respuesta
        public string Message { get; set; } // Mensaje de la respuesta
        public bool Status { get; set; } // Verdadero para respuestas sin errores y sino falso
        public T Data { get; set; }
    
    }
}