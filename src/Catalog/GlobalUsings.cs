// microsoft
global using Microsoft.EntityFrameworkCore;
global using Microsoft.EntityFrameworkCore.Metadata.Builders;
global using Microsoft.Extensions.Diagnostics.HealthChecks;
global using Microsoft.AspNetCore.Diagnostics.HealthChecks;
global using System.Text.RegularExpressions;
global using System.Reflection;
global using Microsoft.AspNetCore.Http.HttpResults;
global using Microsoft.Extensions.Options;


// third-party
global using MassTransit;
global using FluentValidation;

// solution
global using Catalog.Models;
global using Catalog.Infrastructure.Extensions;
global using Catalog.Infrastructure.Exceptions;
global using Catalog.Services;
global using Catalog.Infrastructure;
global using Catalog.Endpoints.Contracts;
global using Catalog.Endpoints;
global using Catalog.Infrastructure.HealthChecks;
global using Catalog.Contracts.IntegrationEvents;
global using Catalog.Infrastructure.CustomModel;
