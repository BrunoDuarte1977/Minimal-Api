using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Minimal_API;
using Minimal_API.Dominio.DTO;
using Minimal_API.Dominio.Entidades;
using Minimal_API.Dominio.Enuns;
using Minimal_API.Dominio.Interfaces;
using Minimal_API.Dominio.ModelViews;
using Minimal_API.Dominio.Servicos;
using Minimal_API.Infraestrutura.DB;

public class Startup
{
    public Startup(IConfiguration configuration)
    {
        configuration = configuration;
        key = Configuration?.GetSection("Jwt")?.ToString() ?? "";
    }

    private string key = "";

    public IConfiguration Configuration {get;set;} = default!;

    public void ConfigureServices(IServiceCollection services)
    {
        if(string.IsNullOrEmpty(key)) key = "SuaSuperChaveSeguraDe32Caracteres";

        services.AddAuthentication(option => {
        option.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        option.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(options => {
            options.TokenValidationParameters = new TokenValidationParameters{
                ValidateLifetime = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                ValidateIssuer = false,
                ValidateAudience = false
            };
        });

        services.AddAuthorization();

        services.AddScoped<IAdministradorServico, AdministradorServico>();
        services.AddScoped<IVeiculoServico, VeiculoServico>();

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options => {
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme{
                Name = "Administration",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Insira o token JWT"
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme{
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    new string[]{}
                }
            });
        });

        services.AddDbContext<DbContexto>(options =>
        {
            options.UseMySql(
                Configuration.GetConnectionString("MySql"),
                ServerVersion.AutoDetect(Configuration.GetConnectionString("MySql"))
            );
        });
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.UseSwagger();
        app.UseSwaggerUI();

        app.UseRouting();
        
        app.UseAuthentication();
        app.UseAuthorization();

        app.UseEndpoints(endpoints => {

            #region Home
            endpoints.MapGet("/", () => Results.Json(new Home())).AllowAnonymous().WithTags("Home");
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
                        new Claim("Perfil", administrador.Perfil),
                        new Claim(ClaimTypes.Role, administrador.Perfil)
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
            endpoints.MapPost("administradores/login", ([FromBody] LoginDTO loginDTO, IAdministradorServico admServico) =>
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
            }).AllowAnonymous().WithTags("Administradores");

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
            endpoints.MapPost("/administradores", ([FromBody] AdministradorDTO admDTO, IAdministradorServico admServico) =>
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

            }).RequireAuthorization()
            .RequireAuthorization(new AuthorizeAttribute{Roles = "Adm"})
            .WithTags("Administradores");

            //############################################################################
            endpoints.MapGet("administradores", ([FromQuery] int? pagina, IAdministradorServico admServico) =>{
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
            }).RequireAuthorization()
            .RequireAuthorization(new AuthorizeAttribute{Roles = "Adm"})
            .WithTags("Administradores");

            //############################################################################
            endpoints.MapGet("/administradores/{id}", ([FromRoute] int id, IAdministradorServico admServico) =>
            {
                var admin = admServico.BuscaPorId(id);
                if(admin == null) return Results.NotFound();

                return Results.Ok(new AdministradorModelView{
                        Id = admin.Id,
                        Email = admin.Email,
                        Perfil = admin.Perfil
                    });

            }).RequireAuthorization()
            .RequireAuthorization(new AuthorizeAttribute{Roles = "Adm"})
            .WithTags("Administradores");
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

            endpoints.MapPost("/veiculos", ([FromBody] VeiculoDTO veiculoDTO, IVeiculoServico veiculoServico) =>
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

            }).RequireAuthorization()
            .RequireAuthorization(new AuthorizeAttribute{Roles = "Adm,Editor"})
            .WithTags("Veículos");

            endpoints.MapGet("/veiculos", ([FromQuery] int? pagina, IVeiculoServico veiculoServico) =>
            {
                var veiculos = veiculoServico.Todos(pagina);

                return Results.Ok(veiculos);

            }).RequireAuthorization()
            .RequireAuthorization(new AuthorizeAttribute{Roles = "Adm,Editor"})
            .WithTags("Veículos");

            endpoints.MapGet("/veiculos/{id}", ([FromRoute] int id, IVeiculoServico veiculoServico) =>
            {
                var veiculo = veiculoServico.BuscaPorId(id);
                if(veiculo == null) return Results.NotFound();

                return Results.Ok(veiculo);

            }).RequireAuthorization()
            .RequireAuthorization(new AuthorizeAttribute{Roles = "Adm,Editor"})
            .WithTags("Veículos");

            endpoints.MapPut("/veiculos/{id}", ([FromRoute] int id, VeiculoDTO veiculoDTO, IVeiculoServico veiculoServico) =>
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

            }).RequireAuthorization()
            .RequireAuthorization(new AuthorizeAttribute{Roles = "Adm"})
            .WithTags("Veículos");

            endpoints.MapDelete("/veiculos/{id}", ([FromRoute] int id, IVeiculoServico veiculoServico) =>
            {
                var veiculo = veiculoServico.BuscaPorId(id);
                if(veiculo == null) return Results.NotFound();

                veiculoServico.Apagar(veiculo);

                return Results.NoContent();

            }).RequireAuthorization()
            .RequireAuthorization(new AuthorizeAttribute{Roles = "Adm"})
            .WithTags("Veículos");

            #endregion
        });
    }
}