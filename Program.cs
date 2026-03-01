using Microsoft.EntityFrameworkCore;
using arabella.Data;
using arabella.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddScoped<IPetPhotoService, PetPhotoService>();
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
    // Add Pet columns if they were added to the model after the DB was first created (no-op if columns exist)
    try
    {
        var conn = db.Database.GetDbConnection();
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT name FROM pragma_table_info('Pets');";
        var columnNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        using (var r = cmd.ExecuteReader())
        {
            while (r.Read())
                columnNames.Add(r.GetString(0));
        }
        if (!columnNames.Contains("Size")) { cmd.CommandText = "ALTER TABLE Pets ADD COLUMN Size TEXT;"; cmd.ExecuteNonQuery(); }
        if (!columnNames.Contains("Color")) { cmd.CommandText = "ALTER TABLE Pets ADD COLUMN Color TEXT;"; cmd.ExecuteNonQuery(); }
        if (!columnNames.Contains("PhotoUrl")) { cmd.CommandText = "ALTER TABLE Pets ADD COLUMN PhotoUrl TEXT;"; cmd.ExecuteNonQuery(); }
    }
    catch (Exception) { /* Pets table may not exist yet; EnsureCreated will have created it with current schema */ }
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
