using AplicacaoAtividade03.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Registra os controllers (as classes que expõem os endpoints REST).
builder.Services.AddControllers();

// Swagger: tela web para testar os endpoints sem precisar de Postman.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Lê a connection string do appsettings.json.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' não encontrada.");

// Registra o DbContext (o "gerente" do EF Core) usando o provedor Pomelo para MySQL.
// AutoDetect descobre a versão do servidor MySQL conectando uma vez na inicialização.
builder.Services.AddDbContext<BdAulaBackendContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();