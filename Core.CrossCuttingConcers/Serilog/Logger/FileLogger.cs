using System;
using Core.CrossCuttingConcers.Serilog.ConfigurationModels;
using Core.CrossCuttingConcers.Serilog.Messages;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace Core.CrossCuttingConcers.Serilog.Logger
{
	public class FileLogger : LoggerServiceBase
	{
		private readonly IConfiguration _configuration;

        public FileLogger(IConfiguration configuration)
        {
            _configuration = configuration;

            FileLogConfiguration logConfig = configuration.GetSection("SerilogLogConfiguration:FileLogConfiguration").Get<FileLogConfiguration>() ?? throw new Exception(SeriLogMessages.NullOptionsMessage);

            string logFilePath = string.Format("{0}{1}", arg0: Directory.GetCurrentDirectory() + logConfig.FolderPath, arg1: ".txt");
            //rolling interval.DAy hergün yeni bir dosya ooluştur
            Logger = new LoggerConfiguration().WriteTo.File(
                logFilePath, rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: null,
                fileSizeLimitBytes: 500000,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level}] {Message}{Newline}{Exception}").CreateLogger();
        }
    }
}

