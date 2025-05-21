using Microsoft.EntityFrameworkCore;
using NasaApp.Database;
using NasaApp.Jobs;
using NasaApp.Services;
using Quartz;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<AppDbContext>(options =>
	options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddHttpClient();
builder.Services.AddScoped<MeteoriteDataService>();
builder.Services.AddQuartz(q =>
{
	q.UseMicrosoftDependencyInjectionJobFactory();

	var jobKey = new JobKey("MeteoriteDataUpdateJob");
	q.AddJob<MeteoriteDataUpdateJob>(opts => opts.WithIdentity(jobKey));

	q.AddTrigger(opts => opts
		.ForJob(jobKey)
		.WithIdentity("MeteoriteDataUpdateJob-trigger")
		//.WithCronSchedule("0 0 2 * * ?")); // ��������� ��������� � 2:00
		.WithSimpleSchedule(x => x
			.WithIntervalInMinutes(30)
			.RepeatForever())
		.StartNow());
});

builder.Services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);
builder.Services.AddControllers();
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

using (var scope = app.Services.CreateScope())
{
	var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
	await dbContext.Database.EnsureCreatedAsync();

	// ���������, ���� �� ������ � ����
	if (!await dbContext.Meteorites.AnyAsync())
	{
		var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
		logger.LogInformation("������ � ���������� �� �������. ����������� �������������� ��������...");

		try
		{
			var dataService = scope.ServiceProvider.GetRequiredService<MeteoriteDataService>();
			await dataService.FetchAndSaveMeteoriteDataAsync();
			logger.LogInformation("�������������� �������� ������ � ���������� ��������� �������");
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "������ ��� ��������� �������� ������ � ����������");
		}
	}
}

app.Run();
