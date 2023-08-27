using System;
using Core.CrossCuttingConcers.Exceptions.Handlers;
using Microsoft.AspNetCore.Http;

namespace Core.CrossCuttingConcers.Exceptions.Middleware
{
	public class ExceptionMiddleware
	{
		private readonly RequestDelegate _next;
		private readonly HttpExceptionHandler _httpExceptionHandler;

        public ExceptionMiddleware(RequestDelegate next)
        {
            _httpExceptionHandler = new HttpExceptionHandler();
            _next = next;
        }

        public async Task Invoke(HttpContext context)//bütün kodlarımı burdan try dan geçicek bütün mimarimiz try catch den geçicek
        {
            try
            {
                await _next(context);
            }
            catch (Exception exception)
            {
                await HandleExceptionAsync(context.Response, exception);
            }
        }

        private Task HandleExceptionAsync(HttpResponse response,Exception exception)
        {
            response.ContentType = "application/json";
            _httpExceptionHandler.Response = response;

            return _httpExceptionHandler.HandleExceptionAsync(exception);
        }

    }
}

