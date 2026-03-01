using Microsoft.EntityFrameworkCore;
using arabella.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=arabella.db"));
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(2);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
    // Add Pet columns if they were added to the model after the DB was first created
    try { db.Database.ExecuteSqlRaw("ALTER TABLE Pets ADD COLUMN Size TEXT;"); }
    catch (Microsoft.Data.Sqlite.SqliteException ex) when (ex.Message?.Contains("duplicate column", StringComparison.OrdinalIgnoreCase) == true) { }
    try { db.Database.ExecuteSqlRaw("ALTER TABLE Pets ADD COLUMN Color TEXT;"); }
    catch (Microsoft.Data.Sqlite.SqliteException ex) when (ex.Message?.Contains("duplicate column", StringComparison.OrdinalIgnoreCase) == true) { }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();
app.UseSession();
app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();
