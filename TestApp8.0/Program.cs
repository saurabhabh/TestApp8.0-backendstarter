using TestApp8._0.Domain;
using TestApp8._0.Repository;
using TestApp8._0.Service;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers().AddNewtonsoftJson();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpContextAccessor();
builder.Services.AddDbContext<DataContext>();

var services = builder.Services;

//Added Automapper
services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
services.AddScoped(typeof(IRepository<,>), typeof(Repository<,>));
services.AddScoped<IStudentService, StudentService>();


// Allow all origins
services.AddCors(p => p.AddPolicy("corsapp", builder =>
{
    // Check if this is working
    builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
}));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
