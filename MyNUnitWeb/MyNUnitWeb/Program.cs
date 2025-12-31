// </copyright file="Program.cs" author="Alina Letyagina">
// under MIT License.
// </copyright>

using Microsoft.EntityFrameworkCore;
using MyNUnitWeb.Data;
using MyNUnitWeb.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddScoped<TestRunnerService>();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite("Data Source=App_Data/testruns.db"));

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
    .WithStaticAssets();
app.MapControllers();

var dbPath = Path.Combine(app.Environment.ContentRootPath, "App_Data");
if (!Directory.Exists(dbPath))
{
    Directory.CreateDirectory(dbPath);
}

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.EnsureCreated();
}

app.Run();