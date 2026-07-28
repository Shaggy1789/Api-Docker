using Microsoft.AspNetCore.Diagnostics;

namespace Catalog_Api.Exceptions
{
    public class CustomExceptionHandler : IExceptionHandler
    {
        //ilogger se utiliza para controlar las excepciones 
        private readonly ILogger<CustomExceptionHandler> _logger;

        public CustomExceptionHandler(
            ILogger<CustomExceptionHandler> logger)
        {
            _logger = logger;
        }

        //Este metodo se encarga de manejar las excepciones. 
        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            _logger.LogError(
                exception,
                "Excepcion Capturada");

                var statusCode = StatusCodes.Status500InternalServerError;
            
            if(exception is ValidationException)
            {
                statusCode = StatusCodes.Status400BadRequest;
            }

            httpContext.Response.StatusCode = statusCode;
            
            /* Este metodo devuelve un json  como respuesta  */
            await httpContext.Response.WriteAsJsonAsync(
                new
                {
                    Tittle = exception.GetType().Name,
                    Status = statusCode,
                    Detail = exception.Message
                },
                cancellationToken);

            return true;
        }

    }
}
