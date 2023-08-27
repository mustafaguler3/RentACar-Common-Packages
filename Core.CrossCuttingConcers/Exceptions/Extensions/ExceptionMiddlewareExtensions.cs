using System;
using Core.CrossCuttingConcers.Exceptions.Middleware;
using Microsoft.AspNetCore.Builder;

namespace Core.CrossCuttingConcers.Exceptions.Extensions
{
	public static class ExceptionMiddlewareExtensions
	{
		public static void ConfigureExceptionMiddleware(this IApplicationBuilder app) => app.UseMiddleware<ExceptionMiddleware>();
	}
}

