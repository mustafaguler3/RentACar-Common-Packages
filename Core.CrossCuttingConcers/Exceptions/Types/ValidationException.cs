using System;
namespace Core.CrossCuttingConcers.Exceptions.Types
{
	public class ValidationException : Exception
	{
		public IEnumerable<ValidationExceptionModel> Errors { get; }

		public ValidationException():base()
		{
			Errors = Array.Empty<ValidationExceptionModel>();
		}

		public ValidationException(string? message):base(message)
		{
			Errors = Array.Empty<ValidationExceptionModel>();				
		}

		public ValidationException(string? message,Exception? innerException)
		{
			Errors = Array.Empty<ValidationExceptionModel>();
		}

		public ValidationException(IEnumerable<ValidationExceptionModel> errors):base()
		{	
			
		}

		private static string BuildErrorMessage(IEnumerable<ValidationExceptionModel> errors)
		{
			IEnumerable<string> arr = errors.Select(x => $"{Environment.NewLine} -- {x.Property}: {string.Join(Environment.NewLine, values: x.Errors)}");

			return $"Validation failed: {string.Join(string.Empty, arr)}";
		}
	}

	public class ValidationExceptionModel
	{
		public string Property { get; set; }
		public IEnumerable<string>? Errors { get; set; }
	}
}

