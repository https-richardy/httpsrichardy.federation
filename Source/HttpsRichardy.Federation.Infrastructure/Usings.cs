global using System.IdentityModel.Tokens.Jwt;
global using System.Security.Claims;
global using System.Security.Cryptography;

global using Microsoft.IdentityModel.Tokens;

global using HttpsRichardy.Internal.Infrastructure.Persistence;
global using HttpsRichardy.Internal.Infrastructure.Persistence.Pipelines;
global using HttpsRichardy.Internal.Essentials.Patterns;

global using HttpsRichardy.Federation.Domain.Aggregates;
global using HttpsRichardy.Federation.Domain.Errors;
global using HttpsRichardy.Federation.Domain.Collections;
global using HttpsRichardy.Federation.Domain.Filtering;
global using HttpsRichardy.Federation.Domain.Filtering.Builders;

global using HttpsRichardy.Federation.Infrastructure.Constants;
global using HttpsRichardy.Federation.Infrastructure.Pipelines;

global using HttpsRichardy.Federation.Application.Payloads.Identity;
global using HttpsRichardy.Federation.Application.Services;
global using HttpsRichardy.Federation.Application.Providers;
global using HttpsRichardy.Federation.Application.Payloads.Client;

global using SecurityToken = HttpsRichardy.Federation.Domain.Aggregates.SecurityToken;

global using MongoDB.Driver;
global using MongoDB.Bson;
global using MongoDB.Bson.Serialization;
