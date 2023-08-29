using System;
using System.Transactions;
using MediatR;

namespace Core.Application.Pipelines.Transaction
{
    public class TransactionScopeBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>, ITransactionRequest
    {


        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            using TransactionScope transactionScope = new(TransactionScopeAsyncFlowOption.Enabled);
            TResponse response;

            try
            {
                response = await next();
                transactionScope.Complete();
            }
            catch (Exception ex)
            {
                transactionScope.Dispose();
                throw;
            }

            return response;
            
        }


    }
}

