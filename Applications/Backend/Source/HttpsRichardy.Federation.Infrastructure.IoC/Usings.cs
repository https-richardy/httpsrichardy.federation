global using System.Diagnostics.CodeAnalysis;
global using System.Security.Cryptography;

global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Configuration;

global using HttpsRichardy.Federation.Common.Configuration;
global using HttpsRichardy.Federation.Domain.Collections;
global using HttpsRichardy.Federation.Domain.Aggregates;
global using HttpsRichardy.Federation.Domain.Policies;

global using HttpsRichardy.Federation.Application.Services;
global using HttpsRichardy.Federation.Application.Providers;
global using HttpsRichardy.Federation.Application.Contracts;
global using HttpsRichardy.Federation.Application.Handlers.Authorization;

global using HttpsRichardy.Federation.Application.Payloads.Identity;
global using HttpsRichardy.Federation.Application.Payloads.Authorization;
global using HttpsRichardy.Federation.Application.Payloads.Group;
global using HttpsRichardy.Federation.Application.Payloads.Permission;
global using HttpsRichardy.Federation.Application.Payloads.Realm;
global using HttpsRichardy.Federation.Application.Payloads.User;

global using HttpsRichardy.Federation.Application.Validators.Permission;
global using HttpsRichardy.Federation.Application.Validators.Group;
global using HttpsRichardy.Federation.Application.Validators.Identity;
global using HttpsRichardy.Federation.Application.Validators.Authorization;
global using HttpsRichardy.Federation.Application.Validators.Realm;
global using HttpsRichardy.Federation.Application.Validators.User;

global using HttpsRichardy.Federation.Application.Handlers.Identity;
global using HttpsRichardy.Federation.Application.Policies;

global using HttpsRichardy.Federation.Infrastructure.Providers;
global using HttpsRichardy.Federation.Infrastructure.Persistence;
global using HttpsRichardy.Federation.Infrastructure.Security;
global using HttpsRichardy.Dispatcher.Extensions;

global using MongoDB.Driver;
global using FluentValidation;
