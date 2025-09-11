using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using ScanDrive.Api.Authorization;
using ScanDrive.Domain.Settings;
using ScanDrive.Infrastructure.Context;
using ScanDrive.Api.Logging;
using System.Text;
using System.Text.Json.Serialization;

namespace ScanDrive.Api
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(connectionString));

            // Configure Identity
            builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
            {
                options.Password.RequiredLength = 6;
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = true;
            })
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

            // Configure JWT
            var jwtSettingsSection = builder.Configuration.GetSection("JwtSettings");
            var jwtSettings = jwtSettingsSection.Get<JwtSettings>();
            builder.Services.Configure<JwtSettings>(jwtSettingsSection);

            // Configure OpenAI
            var openAISettingsSection = builder.Configuration.GetSection("OpenAI");
            builder.Services.Configure<OpenAISettings>(openAISettingsSection);

            if (jwtSettings != null)
            {
                builder.Services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = jwtSettings.Issuer,
                        ValidAudience = jwtSettings.Audience,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey))
                    };
                });
            }

            // Registra o handler de autorização para Admin
            builder.Services.AddSingleton<IAuthorizationHandler, AdminAuthorizationHandler>();

            // Configure Authorization Policies
            builder.Services.AddAuthorization(options =>
            {
                // Política para chat
                options.AddPolicy("Module.Chat:Permission.View", policy =>
                    policy.RequireClaim("Permission", $"{Claims.Modules.Chat}:{Claims.Permissions.View}"));

                // Política para administração
                options.AddPolicy("Module.Administration:Permission.View", policy =>
                    policy.RequireClaim("Permission", $"{Claims.Modules.Administration}:{Claims.Permissions.View}"));
                options.AddPolicy("Module.Administration:Permission.Create", policy =>
                    policy.RequireClaim("Permission", $"{Claims.Modules.Administration}:{Claims.Permissions.Create}"));
                options.AddPolicy("Module.Administration:Permission.Edit", policy =>
                    policy.RequireClaim("Permission", $"{Claims.Modules.Administration}:{Claims.Permissions.Edit}"));
                options.AddPolicy("Module.Administration:Permission.Delete", policy =>
                    policy.RequireClaim("Permission", $"{Claims.Modules.Administration}:{Claims.Permissions.Delete}"));

                // Política para relatórios
                options.AddPolicy("Module.Reports:Permission.View", policy =>
                    policy.RequireClaim("Permission", $"{Claims.Modules.Reports}:{Claims.Permissions.View}"));
                options.AddPolicy("Module.Reports:Permission.Export", policy =>
                    policy.RequireClaim("Permission", $"{Claims.Modules.Reports}:{Claims.Permissions.Export}"));

                // Política para financeiro
                options.AddPolicy("Module.Financial:Permission.View", policy =>
                    policy.RequireClaim("Permission", $"{Claims.Modules.Financial}:{Claims.Permissions.View}"));
                options.AddPolicy("Module.Financial:Permission.Edit", policy =>
                    policy.RequireClaim("Permission", $"{Claims.Modules.Financial}:{Claims.Permissions.Edit}"));
                options.AddPolicy("Module.Financial:Permission.Approve", policy =>
                    policy.RequireClaim("Permission", $"{Claims.Modules.Financial}:{Claims.Permissions.Approve}"));

                // Política para funcionários
                options.AddPolicy("Module.Employees:Permission.View", policy =>
                    policy.RequireClaim("Permission", $"{Claims.Modules.Employees}:{Claims.Permissions.View}"));
                options.AddPolicy("Module.Employees:Permission.Create", policy =>
                    policy.RequireClaim("Permission", $"{Claims.Modules.Employees}:{Claims.Permissions.Create}"));
                options.AddPolicy("Module.Employees:Permission.Edit", policy =>
                    policy.RequireClaim("Permission", $"{Claims.Modules.Employees}:{Claims.Permissions.Edit}"));
                options.AddPolicy("Module.Employees:Permission.Delete", policy =>
                    policy.RequireClaim("Permission", $"{Claims.Modules.Employees}:{Claims.Permissions.Delete}"));

                // Política para histórico
                options.AddPolicy("Module.History:Permission.View", policy =>
                    policy.RequireClaim("Permission", $"{Claims.Modules.History}:{Claims.Permissions.View}"));

                // Política para veículos
                options.AddPolicy("Module.Vehicles:Permission.View", policy =>
                    policy.RequireClaim("Permission", $"{Claims.Modules.Vehicles}:{Claims.Permissions.View}"));
                options.AddPolicy("Module.Vehicles:Permission.Create", policy =>
                    policy.RequireClaim("Permission", $"{Claims.Modules.Vehicles}:{Claims.Permissions.Create}"));
                options.AddPolicy("Module.Vehicles:Permission.Edit", policy =>
                    policy.RequireClaim("Permission", $"{Claims.Modules.Vehicles}:{Claims.Permissions.Edit}"));
                options.AddPolicy("Module.Vehicles:Permission.Delete", policy =>
                    policy.RequireClaim("Permission", $"{Claims.Modules.Vehicles}:{Claims.Permissions.Delete}"));

                // Política para lojas
                options.AddPolicy("Module.Shops:Permission.View", policy =>
                    policy.RequireClaim("Permission", $"{Claims.Modules.Shops}:{Claims.Permissions.View}"));
                options.AddPolicy("Module.Shops:Permission.Create", policy =>
                    policy.RequireClaim("Permission", $"{Claims.Modules.Shops}:{Claims.Permissions.Create}"));
                options.AddPolicy("Module.Shops:Permission.Edit", policy =>
                    policy.RequireClaim("Permission", $"{Claims.Modules.Shops}:{Claims.Permissions.Edit}"));
                options.AddPolicy("Module.Shops:Permission.Update", policy =>
                    policy.RequireClaim("Permission", $"{Claims.Modules.Shops}:{Claims.Permissions.Edit}"));
                options.AddPolicy("Module.Shops:Permission.Delete", policy =>
                    policy.RequireClaim("Permission", $"{Claims.Modules.Shops}:{Claims.Permissions.Delete}"));

                // Política para leads
                options.AddPolicy("Module.Leads:Permission.View", policy =>
                    policy.RequireClaim("Permission", $"{Claims.Modules.Leads}:{Claims.Permissions.View}"));
                options.AddPolicy("Module.Leads:Permission.Create", policy =>
                    policy.RequireClaim("Permission", $"{Claims.Modules.Leads}:{Claims.Permissions.Create}"));
                options.AddPolicy("Module.Leads:Permission.Edit", policy =>
                    policy.RequireClaim("Permission", $"{Claims.Modules.Leads}:{Claims.Permissions.Edit}"));
                options.AddPolicy("Module.Leads:Permission.Delete", policy =>
                    policy.RequireClaim("Permission", $"{Claims.Modules.Leads}:{Claims.Permissions.Delete}"));

                // Política para test drives
                options.AddPolicy("Module.TestDrives:Permission.View", policy =>
                    policy.RequireClaim("Permission", $"{Claims.Modules.TestDrives}:{Claims.Permissions.View}"));
                options.AddPolicy("Module.TestDrives:Permission.Create", policy =>
                    policy.RequireClaim("Permission", $"{Claims.Modules.TestDrives}:{Claims.Permissions.Create}"));
                options.AddPolicy("Module.TestDrives:Permission.Edit", policy =>
                    policy.RequireClaim("Permission", $"{Claims.Modules.TestDrives}:{Claims.Permissions.Edit}"));
                options.AddPolicy("Module.TestDrives:Permission.Delete", policy =>
                    policy.RequireClaim("Permission", $"{Claims.Modules.TestDrives}:{Claims.Permissions.Delete}"));

                // Política para perguntas do chat
                options.AddPolicy("Module.ChatQuestions:Permission.View", policy =>
                    policy.RequireClaim("Permission", $"{Claims.Modules.ChatQuestions}:{Claims.Permissions.View}"));
                options.AddPolicy("Module.ChatQuestions:Permission.Create", policy =>
                    policy.RequireClaim("Permission", $"{Claims.Modules.ChatQuestions}:{Claims.Permissions.Create}"));
                options.AddPolicy("Module.ChatQuestions:Permission.Edit", policy =>
                    policy.RequireClaim("Permission", $"{Claims.Modules.ChatQuestions}:{Claims.Permissions.Edit}"));
                options.AddPolicy("Module.ChatQuestions:Permission.Delete", policy =>
                    policy.RequireClaim("Permission", $"{Claims.Modules.ChatQuestions}:{Claims.Permissions.Delete}"));

                // Outras políticas baseadas em módulos
                foreach (var module in typeof(Claims.Modules).GetFields())
                {
                    var moduleName = module.GetValue(null)?.ToString();
                    if (string.IsNullOrEmpty(moduleName)) continue;

                    foreach (var permission in typeof(Claims.Permissions).GetFields())
                    {
                        var permissionName = permission.GetValue(null)?.ToString();
                        if (string.IsNullOrEmpty(permissionName)) continue;

                        var policyName = $"{moduleName}:{permissionName}";
                        options.AddPolicy(policyName, policy =>
                            policy.RequireClaim("Permission", policyName));
                    }
                }
            });

            // Configure CORS to Frontend
            var corsSettingsSection = builder.Configuration.GetSection("CorsSettings");
            var corsSettings = corsSettingsSection.Get<CorsSettings>();
            builder.Services.Configure<CorsSettings>(corsSettingsSection);

            var corsPolicyName = "AllowFrontend";
            builder.Services.AddCors(options =>
            {
                options.AddPolicy(name: corsPolicyName,
                    policy =>
                    {
                        policy
                            .AllowAnyOrigin()
                            .AllowAnyMethod()
                            .AllowAnyHeader();
                        // .AllowCredentials() não pode ser usado com AllowAnyOrigin
                    });
            });

            builder.Services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                });

            // Configure Swagger
            var swaggerSettingsSection = builder.Configuration.GetSection("SwaggerSettings");
            var swaggerSettings = swaggerSettingsSection.Get<SwaggerSettings>();
            builder.Services.Configure<SwaggerSettings>(swaggerSettingsSection);

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc(swaggerSettings?.Version ?? "v1", new OpenApiInfo
                {
                    Title = swaggerSettings?.Title ?? "ScanDrive API",
                    Version = swaggerSettings?.Version ?? "v1",
                    Description = swaggerSettings?.Description ?? "API para gerenciamento de lojas e anúncios de veículos",
                    Contact = new OpenApiContact
                    {
                        Name = swaggerSettings?.Contact?.Name ?? "ScanDrive Team",
                        Email = swaggerSettings?.Contact?.Email ?? "contato@scandrive.com"
                    }
                });

                // Adiciona suporte para documentação XML
                var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);

                // Configuração do JWT no Swagger
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = swaggerSettings?.Security?.Description ?? "JWT Authorization header using the Bearer scheme. Example: 'Bearer {token}'",
                    Name = swaggerSettings?.Security?.Name ?? "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = swaggerSettings?.Security?.Scheme ?? "Bearer"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });

                // Adiciona suporte para exibir descrições dos enums
                c.UseInlineDefinitionsForEnums();
            });

            // Adiciona HttpClient
            builder.Services.AddHttpClient();

            builder.Logging
                .ClearProviders()
                .AddConsole()
                .AddDebug()
                .AddDatabaseLogging(); // Adiciona o provedor de logging no banco de dados

            var app = builder.Build();

            // Setup CORs
            app.UseCors(corsPolicyName);

            // Configure the HTTP request pipeline.
            // Temporariamente habilitando Swagger em todos os ambientes
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "ScanDrive API v1");
                c.RoutePrefix = "swagger";
            });

            app.UseHttpsRedirection();

            // Adiciona middleware de autenticação antes da autorização
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            // Cria roles padrão e claims se não existirem
            try
            {
                using (var scope = app.Services.CreateScope())
                {
                    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
                    
                    // Adicionando todas as roles do sistema
                    foreach (var role in Roles.All)
                    {
                        if (!roleManager.RoleExistsAsync(role).Result)
                        {
                            roleManager.CreateAsync(new IdentityRole(role)).Wait();
                        }

                        // Adiciona as claims padrão para cada role
                        if (Claims.DefaultClaims.RoleClaims.ContainsKey(role))
                        {
                            var existingRole = roleManager.FindByNameAsync(role).Result;
                            if (existingRole != null)
                            {
                                var existingClaims = roleManager.GetClaimsAsync(existingRole).Result;
                                
                                foreach (var claim in Claims.DefaultClaims.RoleClaims[role])
                                {
                                    if (!existingClaims.Any(c => c.Type == "Permission" && c.Value == claim))
                                    {
                                        roleManager.AddClaimAsync(existingRole, new System.Security.Claims.Claim("Permission", claim)).Wait();
                                    }
                                }
                            }
                        }
                    }

                    // Inicializa os dados de seed
                    await ScanDrive.Infrastructure.Seeds.InitialSeed.SeedData(app.Services, builder.Configuration);
                }
            }
            catch (Exception ex)
            {
                var logger = app.Services.GetRequiredService<ILogger<Program>>();
                logger.LogError(ex, "Ocorreu um erro durante a inicialização do banco de dados.");
            }

            app.Run();
        }
    }
}
