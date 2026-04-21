global using System.Diagnostics.CodeAnalysis;
global using System.Text.Json;
global using System.Text.RegularExpressions;
global using System.Net.Mime;
global using System.Security.Claims;

global using Microsoft.AspNetCore.Mvc;
global using Microsoft.AspNetCore.Mvc.ApiExplorer;
global using Microsoft.AspNetCore.Mvc.ModelBinding;
global using Microsoft.AspNetCore.Mvc.RazorPages;

global using Microsoft.AspNetCore.Authorization;
global using Microsoft.AspNetCore.Authentication.JwtBearer;
global using Microsoft.Extensions.Caching.Memory;

global using Microsoft.OpenApi.Models;
global using Microsoft.IdentityModel.Tokens;

global using HttpsRichardy.Federation.Domain.Aggregates;
global using HttpsRichardy.Federation.Domain.Filtering;
global using HttpsRichardy.Federation.Domain.Collections;

global using HttpsRichardy.Federation.Common.Constants;
global using HttpsRichardy.Federation.Common.Configuration;
global using HttpsRichardy.Federation.Domain.Errors;
global using HttpsRichardy.Federation.Domain.Concepts;

global using HttpsRichardy.Federation.Application.Payloads.Group;
global using HttpsRichardy.Federation.Application.Payloads.Identity;
global using HttpsRichardy.Federation.Application.Payloads.Authorization;
global using HttpsRichardy.Federation.Application.Payloads.Permission;
global using HttpsRichardy.Federation.Application.Payloads.Realm;
global using HttpsRichardy.Federation.Application.Payloads.User;
global using HttpsRichardy.Federation.Application.Payloads.Connect;
global using HttpsRichardy.Federation.Application.Payloads.Client;
global using HttpsRichardy.Federation.Application.Payloads.Common;

global using HttpsRichardy.Federation.Application.Providers;
global using HttpsRichardy.Federation.Application.Services;
global using HttpsRichardy.Federation.Infrastructure.IoC.Extensions;

global using HttpsRichardy.Federation.WebApi.Extensions;
global using HttpsRichardy.Federation.WebApi.Middlewares;
global using HttpsRichardy.Federation.WebApi.Attributes;
global using HttpsRichardy.Federation.WebApi.Binders;
global using HttpsRichardy.Federation.WebApi.Providers;
global using HttpsRichardy.Federation.WebApi.Workers;
global using HttpsRichardy.Federation.WebApi.Constants;
global using HttpsRichardy.Federation.WebApi.Conventions;

global using HttpsRichardy.Internal.Essentials.Utilities;
global using HttpsRichardy.Internal.Essentials.Patterns;
global using HttpsRichardy.Dispatcher.Contracts;

global using Scalar.AspNetCore;
global using FluentValidation.AspNetCore;
