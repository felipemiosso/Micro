using Micro.API.Endpoints.Application;
using Micro.API.Endpoints.HealthCheck;
using Micro.API.Endpoints.JobPosting;
using Micro.API.Endpoints.Requisition;
using Micro.API.Endpoints.UserProfile;
using Micro.API.Endpoints.Candidate;
using Micro.API.Endpoints.Role;
using Micro.API.Endpoints.User;
using Micro.API.Infrastructure.Auth;
using Micro.API.Infrastructure.Database;
using Micro.API.Infrastructure.Logging;
using Micro.API.Infrastructure.OpenApi;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Add infrastructure services
builder.Host.AddSerilog();
builder.Services.AddDatabase(builder.Configuration);
builder.Services.AddAuth(builder.Configuration, builder.Environment);
builder.Services.AddSwagger();

var app = builder.Build();

app.UseSerilogRequestLogging();

app.UseFirebaseEmulatorAuth(builder.Configuration, builder.Environment);

app.UseSwagger();

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

// Map endpoints
app.MapUserProfileEndpoints();
app.MapCandidateEndpoints();
app.MapRoleEndpoints();
app.MapUserEndpoints();
app.MapHealthCheckEndpoints().AllowAnonymous();
app.MapRequisitionEndpoints();
app.MapJobPostingEndpoints();
app.MapApplicationEndpoints();

// Initialize database
await app.ApplyMigrationsAndSeed();

app.Run();

public partial class Program { }
