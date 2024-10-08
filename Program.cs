using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Minimal_API.Dominio.Servicos;
using Minimal_API.Dominio.Interfaces;
using Minimal_API.Dominio.DTO;
using Minimal_API.Infraestrutura.DB;
using Minimal_API.Dominio.ModelViews;
using Minimal_API.Dominio.Entidades;
using Minimal_API.Dominio.Enuns;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.ComponentModel.DataAnnotations;

#region Builder
var builder = WebApplication.CreateBuilder(args);

var key = builder.Configuration.GetSection("Jwt").ToString();
if(string.IsNullOrEmpty(key)) key = "123456";

builder.Services.AddAuthentication(option => {
    option.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    option.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    }).AddJwtBearer(options => {
        options.TokenValidationParameters = new TokenValidationParameters{
            ValidateLifetime = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key))
        };
});

builder.Services.AddAuthorization();

builder.Services.AddScoped<IAdministradorServico, AdministradorServico>();
builder.Services.AddScoped<IVeiculoServico, VeiculoServico>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<DbContexto>(options =>
{
    options.UseMySql(
        builder.Configuration.GetConnectionString("mysql"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("mysql"))
    );
});

var app = builder.Build();
#endregion

#region Home
app.MapGet("/", () => Results.Json(new Home())).WithTags("Home");
#endregion

#region Administradores

string GerarTokemJwt(Administrador administrador){
    if(string.IsNullOrEmpty(key)) return string.Empty;
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>()
        {
            new Claim("Email", administrador.Email),
            new Claim("Perfil", administrador.Perfil)
        };

        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.Now.AddDays(1),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

//############################################################################
app.MapPost("administradores/login", ([FromBody] LoginDTO loginDTO, IAdministradorServico admServico) =>
{
    var adm = admServico.Login(loginDTO);
    if (adm != null)
    {
        string token = GerarTokemJwt(adm);
        return Results.Ok(new AdministradorLogado{
            Email = adm.Email,
            Perfil = adm.Perfil,
            Token = token
        });
    }
    else
        return Results.Unauthorized();
}).WithTags("Administradores");

//############################################################################
ErrosDeValidacao validaAdnDTO(AdministradorDTO administradorDTO)
{
    var vakidacao = new ErrosDeValidacao{
        Mensagens = new List<string>()
    };

    if(string.IsNullOrEmpty(administradorDTO.Email))
        vakidacao.Mensagens.Add("O email não pode ser vazio");

    if(string.IsNullOrEmpty(administradorDTO.Senha))
        vakidacao.Mensagens.Add("A senha não pode ser vazia");

    if(administradorDTO.Perfil == null)
        vakidacao.Mensagens.Add("O perfil não pode ser vazio");

    return vakidacao;
}

//############################################################################
app.MapPost("/administradores", ([FromBody] AdministradorDTO admDTO, IAdministradorServico admServico) =>
{
    var vakidacao = validaAdnDTO(admDTO);
        if(vakidacao.Mensagens.Count > 0)
        return Results.BadRequest(vakidacao);

    var admin = new Administrador{
        Email = admDTO.Email,
        Senha = admDTO.Senha,
        Perfil = admDTO.Perfil.ToString() ?? Perfil.Editor.ToString()
    };

    admServico.Incluir(admin);

    return Results.Created($"/administrador/{admin.Id}", new AdministradorModelView{
        Id = admin.Id,
        Email = admin.Email,
        Perfil = admin.Perfil
    });

}).RequireAuthorization().WithTags("Administradores");

//############################################################################
app.MapGet("administradores", ([FromQuery] int? pagina, IAdministradorServico admServico) =>{
    var adms = new List<AdministradorModelView>();
    var administradores = admServico.Todos(pagina);
    foreach(var admin in administradores)
    {
        adms.Add(new AdministradorModelView{
            Id = admin.Id,
            Email = admin.Email,
            Perfil = admin.Perfil
        });
    }
    return Results.Ok(adms);
}).RequireAuthorization().WithTags("Administradores");

//############################################################################
app.MapGet("/administradores/{id}", ([FromRoute] int id, IAdministradorServico admServico) =>
{
    var admin = admServico.BuscaPorId(id);
    if(admin == null) return Results.NotFound();

    return Results.Ok(new AdministradorModelView{
            Id = admin.Id,
            Email = admin.Email,
            Perfil = admin.Perfil
        });

}).RequireAuthorization().WithTags("Administradores");
#endregion

#region Veiculos

ErrosDeValidacao validaDTO(VeiculoDTO veiculoDTO)
{
    var vakidacao = new ErrosDeValidacao{
        Mensagens = new List<string>()
    };

    if(string.IsNullOrEmpty(veiculoDTO.Nome))
        vakidacao.Mensagens.Add("O nome não pode ser vazio");

    if(string.IsNullOrEmpty(veiculoDTO.Marca))
        vakidacao.Mensagens.Add("O marca não pode ser vazia");

    if(veiculoDTO.Ano <= 1950)
        vakidacao.Mensagens.Add("Veículo muito antigo. Valido apenas superior ha 1950");

    return vakidacao;
}

app.MapPost("/veiculos", ([FromBody] VeiculoDTO veiculoDTO, IVeiculoServico veiculoServico) =>
{
    var vakidacao = validaDTO(veiculoDTO);
        if(vakidacao.Mensagens.Count > 0)
        return Results.BadRequest(vakidacao);

    var veiculo = new Veiculo{
        Nome = veiculoDTO.Nome,
        Marca = veiculoDTO.Marca,
        Ano = veiculoDTO.Ano
    };

    veiculoServico.Incluir(veiculo);

    return Results.Created($"/veiculo/{veiculo.Id}", veiculo);

}).RequireAuthorization().WithTags("Veículos");

app.MapGet("/veiculos", ([FromQuery] int? pagina, IVeiculoServico veiculoServico) =>
{
    var veiculos = veiculoServico.Todos(pagina);

    return Results.Ok(veiculos);

}).RequireAuthorization().WithTags("Veículos");

app.MapGet("/veiculos/{id}", ([FromRoute] int id, IVeiculoServico veiculoServico) =>
{
    var veiculo = veiculoServico.BuscaPorId(id);
    if(veiculo == null) return Results.NotFound();

    return Results.Ok(veiculo);

}).RequireAuthorization().WithTags("Veículos");

app.MapPut("/veiculos/{id}", ([FromRoute] int id, VeiculoDTO veiculoDTO, IVeiculoServico veiculoServico) =>
{
    var veiculo = veiculoServico.BuscaPorId(id);
    if(veiculo == null) return Results.NotFound();
    
    var vakidacao = validaDTO(veiculoDTO);
    if(vakidacao.Mensagens.Count > 0)
    return Results.BadRequest(vakidacao);

    veiculo.Nome = veiculoDTO.Nome;
    veiculo.Marca = veiculoDTO.Marca;
    veiculo.Ano = veiculoDTO.Ano;

    veiculoServico.Atualizar(veiculo);

    return Results.Ok(veiculo);

}).RequireAuthorization().WithTags("Veículos");

app.MapDelete("/veiculos/{id}", ([FromRoute] int id, IVeiculoServico veiculoServico) =>
{
    var veiculo = veiculoServico.BuscaPorId(id);
    if(veiculo == null) return Results.NotFound();

    veiculoServico.Apagar(veiculo);

    return Results.NoContent();

}).RequireAuthorization().WithTags("Veículos");

#endregion

#region App
app.UseSwagger();
app.UseSwaggerUI();
app.UseAuthentication();
app.UseAuthorization();

app.Run();
#endregion