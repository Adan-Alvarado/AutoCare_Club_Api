
namespace AutoCare_Club_Api.Dtos.Common
{
    public class ResponseDto<T>
    {
        public int StatusCode { get; set; } 
        public string Message { get; set; } = string.Empty;
        public bool Status { get; set; } // Verdadero para respuestas sin errores y sino falso
        public T? Data { get; set; } 
    
    }
}