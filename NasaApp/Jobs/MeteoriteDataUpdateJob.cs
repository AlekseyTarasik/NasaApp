using NasaApp.Services;
using Quartz;

namespace NasaApp.Jobs
{
	public class MeteoriteDataUpdateJob : IJob
	{
		private readonly MeteoriteDataService _dataService;
		private readonly ILogger<MeteoriteDataUpdateJob> _logger;

		public MeteoriteDataUpdateJob(
			MeteoriteDataService dataService,
			ILogger<MeteoriteDataUpdateJob> logger)
		{
			_dataService = dataService;
			_logger = logger;
		}

		public async Task Execute(IJobExecutionContext context)
		{
			_logger.LogInformation("Запущено задание по обновлению метеоритных данных");
			try
			{
				await _dataService.FetchAndSaveMeteoriteDataAsync();
				_logger.LogInformation("Задание по обновлению метеоритных данных выполнено успешно");
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Ошибка в задании обновления метеоритных данных");
			}
		}
	}
}
