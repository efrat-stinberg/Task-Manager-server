using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging; 
using TodoApi; 

var builder = WebApplication.CreateBuilder(args);

builder.Logging.AddFile("Logs/ToDoAPI-{Date}.txt");

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins",
        builder =>
        {
            builder.AllowAnyOrigin()
                   .AllowAnyMethod()
                   .AllowAnyHeader();
        });
});

builder.Services.AddControllers();

builder.Services.AddDbContext<MyDbContext>(options =>
{
    var connectionString = Environment.GetEnvironmentVariable("DATABASE_URL") ?? 
        "server=localhost;user=root;password=Es@90906;database=ToDoDB";
    options.UseMySql(connectionString, 
        new MySqlServerVersion(new Version(8, 0, 21)),
        options => options.EnableRetryOnFailure());
});

var app = builder.Build(); 

app.Use(async (context, next) =>
{
    try
    {
        await next.Invoke(); 
    }
    catch (Exception ex)
    {
        var logger = context.RequestServices.GetService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while processing the request.");
        throw; 
    }
    finally
    {
        var logger = context.RequestServices.GetService<ILogger<Program>>();
        logger.LogInformation($"Response Status Code: {context.Response.StatusCode}");
    }
});


app.UseCors("AllowAllOrigins");

app.MapGet("/items", async (MyDbContext context) => {
    var items = await context.Items.ToListAsync();
    return Results.Ok(items); 
});

app.MapGet("/items/{id}", async (int id, MyDbContext context) => {
    var item = await context.Items.FindAsync(id);
    if (item == null)
    {
        return Results.NotFound(); 
    }
    return Results.Ok(item);
});

app.MapPost("/items", async (Item newItem, MyDbContext context) => {
    context.Items.Add(newItem);
    await context.SaveChangesAsync(); 
    var items = await context.Items.ToListAsync();
    return Results.Ok(items); 
});


app.MapPut("/items/{id}", async (int id, bool inputItem, MyDbContext context) => {
    var existingItem = await context.Items.FindAsync(id);
    if (existingItem == null)
    {
        return Results.NotFound(); 
    }
    existingItem.IsComplete = inputItem;
    await context.SaveChangesAsync(); 
    return Results.Ok("Item updated"); 
});


app.MapDelete("/items/{id}", async (int id, MyDbContext context) => {
    var existingItem = await context.Items.FindAsync(id);
    if (existingItem == null)
    {
        return Results.NotFound(); 
    }
    context.Items.Remove(existingItem);
    await context.SaveChangesAsync(); 
    return Results.Ok("Item deleted");
});

app.Run();
