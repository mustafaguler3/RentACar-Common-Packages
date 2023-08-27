using System;
using Core.CrossCuttingConcers.Exceptions.Types;

namespace Core.CrossCuttingConcers.Exceptions.Handlers
{
	public abstract class ExceptionHandler
	{
		public Task HandleExceptionAsync(Exception exception) =>
			exception switch
			{
				BusinessException businessException => HandleException(businessException),
				_ => HandleException(exception)
			};

		protected abstract Task HandleException(BusinessException businessException);

        protected abstract Task HandleException(Exception exception);
    }
}

