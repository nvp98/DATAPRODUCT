using Data_Product.Controllers;
using Data_Product.Repositorys;
using Data_Product.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Quartz;
using System.Globalization;



var builder = WebApplication.CreateBuilder(args);

// Thiết lập CultureInfo
var cultureInfo = new CultureInfo("en-GB"); // dd/MM/yyyy
CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

// Add services to the container.
builder.Services.AddControllersWithViews();
// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie();
builder.Services.AddSession();

//builder.Services.AddScoped<BM_11Controller>();
builder.Services.AddScoped<IChiaGangService, ChiaGangService>();

builder.Services.AddDbContext<DataContext>(options =>
options.UseSqlServer(builder.Configuration.GetConnectionString("ConnectionString") ?? throw new InvalidOperationException("Connection string 'ConnectionString' not found.")));


////Đăng ký BackgroundService tự động tạo phiếu
//builder.Services.AddHostedService<TaoPhieuTuDongBackgroundService>();
//builder.Services.AddQuartz(q =>
//{
//    q.UseMicrosoftDependencyInjectionJobFactory();

//    // Đăng ký Job
//    var jobKey = new JobKey("TaoPhieuTuDongJob");
//    q.AddJob<TaoPhieuTuDongJob>(opts => opts.WithIdentity(jobKey));

//    // Tạo Trigger: chạy mỗi 15 phút
//    q.AddTrigger(opts => opts
//        .ForJob(jobKey)
//        .WithIdentity("TaoPhieuTuDongJob-trigger")
//        .WithSimpleSchedule(x => x
//            .WithIntervalInMinutes(20)
//            .RepeatForever()
//        ));
//    q.AddTrigger(opts => opts
//       .ForJob(jobKey)
//       .WithIdentity("TaoPhieuTuDongJob-trigger-beforeDayShift")
//       .WithCronSchedule("0 55 7 * * ?")); // 07:55

//    // Trigger chạy lúc 19:55 tối hàng ngày (trước khi kết thúc ca ngày)
//    q.AddTrigger(opts => opts
//        .ForJob(jobKey)
//        .WithIdentity("TaoPhieuTuDongJob-trigger-beforeNightShift")
//        .WithCronSchedule("0 55 19 * * ?")); // 19:55
//});

//builder.Services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

var app = builder.Build();

// Thêm Middleware cho localization
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new Microsoft.AspNetCore.Localization.RequestCulture(cultureInfo),
    SupportedCultures = new[] { cultureInfo },
    SupportedUICultures = new[] { cultureInfo }
});

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseSession();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
