using System;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace Core.CrossCuttingConcers.Exceptions.Extensions
{
	public static class ProblemDetailsExtensions
	{
		public static string AsJson<IProblemDetail>(this IProblemDetail details)
			where IProblemDetail : ProblemDetails => JsonSerializer.Serialize(details);
	}
}

