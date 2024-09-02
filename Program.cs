using Microsoft.EntityFrameworkCore;
using BookStoreApi.Data;
using BookStoreApi.Models;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add ServiceDbContextContext with IHttpContextAccessor
builder.Services.AddDbContext<ServiceDbContextContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register IHttpContextAccessor
builder.Services.AddHttpContextAccessor();

// Add Swagger services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

WebApplication app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Ensure the database is created
using (IServiceScope scope = app.Services.CreateScope())
{
    ServiceDbContextContext dbContext = scope.ServiceProvider.GetRequiredService<ServiceDbContextContext>();
    dbContext.Database.EnsureCreated();
}

// Define endpoints
app.MapGet("/", () => "Hello World!");

// CRUD operations for books
app.MapGet("/books", async (ServiceDbContextContext db) =>
{
    return await db.Books.ToListAsync();
});

app.MapGet("/books/{id}", async (Guid id, ServiceDbContextContext db) =>
{
    Book? book = await db.Books.FindAsync(id);
    return book is not null ? Results.Ok(book) : Results.NotFound();
});

app.MapPost("/books", async (Book book, ServiceDbContextContext db) =>
{
    db.Books.Add(book);
    await db.SaveChangesAsync();
    return Results.Created($"/books/{book.ID}", book);
});

app.MapPut("/books/{id}", async (Guid id, Book updatedBook, ServiceDbContextContext db) =>
{
    Book? book = await db.Books.FindAsync(id);
    if (book is null) return Results.NotFound();

    book.Title = updatedBook.Title;
    book.Author = updatedBook.Author;
    book.Genre = updatedBook.Genre;
    book.PublicationYear = updatedBook.PublicationYear;
    book.ISBN = updatedBook.ISBN;
    book.Price = updatedBook.Price;
    book.Availability = updatedBook.Availability;
    book.Publisher = updatedBook.Publisher;
    book.Format = updatedBook.Format;
    book.Pages = updatedBook.Pages;
    book.Language = updatedBook.Language;
    book.Edition = updatedBook.Edition;

    await db.SaveChangesAsync();
    return Results.Ok(book);
});

app.MapDelete("/books/{id}", async (Guid id, ServiceDbContextContext db) =>
{
    Book? book = await db.Books.FindAsync(id);
    if (book is null) return Results.NotFound();

    db.Books.Remove(book);
    await db.SaveChangesAsync();
    return Results.NoContent();
});


// CRUD operations for cars
app.MapGet("/cars", async (ServiceDbContextContext db) =>
{
    return await db.Cars.ToListAsync();
});

app.MapGet("/cars/{id}", async (Guid id, ServiceDbContextContext db) =>
{
    Car? car = await db.Cars.FindAsync(id);
    return car is not null ? Results.Ok(car) : Results.NotFound();
});

app.MapPost("/cars", async (Car car, ServiceDbContextContext db) =>
{
    db.Cars.Add(car);
    await db.SaveChangesAsync();
    return Results.Created($"/cars/{car.ID}", car);
});

app.MapPut("/cars/{id}", async (Guid id, Car updatedCar, ServiceDbContextContext db) =>
{
    Car? car = await db.Cars.FindAsync(id);
    if (car is null) return Results.NotFound();

    car.Make = updatedCar.Make;
    car.Model = updatedCar.Model;
    car.Year = updatedCar.Year;
    car.VIN = updatedCar.VIN;

    await db.SaveChangesAsync();
    return Results.Ok(car);
});

app.MapDelete("/cars/{id}", async (Guid id, ServiceDbContextContext db) =>
{
    Car? car = await db.Cars.FindAsync(id);
    if (car is null) return Results.NotFound();

    db.Cars.Remove(car);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

app.Run();
