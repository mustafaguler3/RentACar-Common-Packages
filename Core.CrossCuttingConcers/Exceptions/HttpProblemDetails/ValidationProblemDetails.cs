using System;
using Microsoft.AspNetCore.Http;
using System.Net.NetworkInformation;
using Core.CrossCuttingConcers.Exceptions.Types;
using Microsoft.AspNetCore.Mvc;

namespace Core.CrossCuttingConcers.Exceptions.HttpProblemDetails
{
	public class ValidationProblemDetails : ProblemDetails
	{

        public IEnumerable<ValidationExceptionModel> Errors { get; set; }

        public ValidationProblemDetails(IEnumerable<ValidationExceptionModel> errors)
        {
            Title = "Validation error(s)";
            Detail = "One or mora validation errors occured";
            Errors = errors;
            Status = StatusCodes.Status400BadRequest;
            Type = "https://example.com/probs/validation";
        }
    }
}

