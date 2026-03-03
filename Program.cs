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

// Log whether Azure Storage is configured (helps troubleshoot photo upload on Azure)
var azureConn = builder.Configuration["AzureStorage:ConnectionString"];
var azureConfigured = !string.IsNullOrWhiteSpace(azureConn);
app.Logger.LogInformation("Azure Storage for pet photos: {Configured}", azureConfigured ? "configured" : "NOT configured (set AzureStorage__ConnectionString in App settings)");

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
    // Infractions: if table has old Description column (NOT NULL), drop and recreate with new schema (no Description, Type only)
    try
    {
        var conn2 = db.Database.GetDbConnection();
        if (conn2.State != System.Data.ConnectionState.Open) conn2.Open();
        using var cmd2 = conn2.CreateCommand();
        cmd2.CommandText = "SELECT name FROM pragma_table_info('Infractions');";
        var infColumns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        using (var r2 = cmd2.ExecuteReader())
        {
            while (r2.Read())
                infColumns.Add(r2.GetString(0));
        }
        if (infColumns.Contains("Description"))
        {
            cmd2.CommandText = "DROP TABLE IF EXISTS Infractions;";
            cmd2.ExecuteNonQuery();
            cmd2.CommandText = "CREATE TABLE Infractions (Id INTEGER PRIMARY KEY AUTOINCREMENT, UnitNumber TEXT NOT NULL, Date TEXT NOT NULL, Type TEXT NOT NULL, FOREIGN KEY(UnitNumber) REFERENCES Units(UnitNumber));";
            cmd2.ExecuteNonQuery();
        }
        else if (!infColumns.Contains("Type"))
        {
            cmd2.CommandText = "ALTER TABLE Infractions ADD COLUMN Type TEXT;";
            cmd2.ExecuteNonQuery();
        }
    }
    catch (Exception) { /* Infractions table may not exist yet */ }
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
