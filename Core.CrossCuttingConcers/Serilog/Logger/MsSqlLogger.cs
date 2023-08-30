using System;
using Core.CrossCuttingConcers.Serilog.ConfigurationModels;
using Core.CrossCuttingConcers.Serilog.Messages;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Sinks.MSSqlServer;

namespace Core.CrossCuttingConcers.Serilog.Logger
{
	public class MsSqlLogger : LoggerServiceBase
	{
		public MsSqlLogger(IConfiguration configuration)
		{
			MsSqlConfiguration logConfiguration = configuration.GetSection("SerilogConfiguration:MsSqlConfiguration").Get<MsSqlConfiguration>() ?? throw new Exception(SeriLogMessages.NullOptionsMessage);

			MSSqlServerSinkOptions sinkOptions = new()
			{
				TableName = logConfiguration.TableName,
				AutoCreateSqlDatabase = logConfiguration.AutoCreateSqlTable
			};

			ColumnOptions columnOptions = new();

			global::Serilog.Core.Logger serilogConfig = new LoggerConfiguration().WriteTo
				.MSSqlServer(logConfiguration.ConnectionString, sinkOptions, columnOptions: columnOptions).CreateLogger();

			Logger = serilogConfig;
		}
	}
}

