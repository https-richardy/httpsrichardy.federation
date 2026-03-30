global using System.Text.Json.Serialization;
global using System.Security.Cryptography;

global using HttpsRichardy.Internal.Essentials.Patterns;
global using HttpsRichardy.Internal.Essentials.Filtering;

global using HttpsRichardy.Federation.Domain.Errors;
global using HttpsRichardy.Federation.Domain.Policies;
global using HttpsRichardy.Federation.Domain.Concepts;
global using HttpsRichardy.Federation.Common.Constants;

global using HttpsRichardy.Federation.Domain.Aggregates;
global using HttpsRichardy.Federation.Domain.Filtering;
global using HttpsRichardy.Federation.Domain.Filtering.Builders;
global using HttpsRichardy.Federation.Domain.Collections;

global using HttpsRichardy.Federation.Application.Payloads.Common;
global using HttpsRichardy.Federation.Application.Payloads.Identity;
global using HttpsRichardy.Federation.Application.Payloads.Authorization;
global using HttpsRichardy.Federation.Application.Payloads.Group;
global using HttpsRichardy.Federation.Application.Payloads.Permission;
global using HttpsRichardy.Federation.Application.Payloads.Realm;
global using HttpsRichardy.Federation.Application.Payloads.User;
global using HttpsRichardy.Federation.Application.Payloads.Client;
global using HttpsRichardy.Federation.Application.Payloads.Connect;

global using HttpsRichardy.Federation.Application.Services;
global using HttpsRichardy.Federation.Application.Contracts;
global using HttpsRichardy.Federation.Application.Providers;
global using HttpsRichardy.Federation.Application.Mappers;
global using HttpsRichardy.Federation.Application.Utilities;

global using FluentValidation;
global using HttpsRichardy.Dispatcher.Contracts;
