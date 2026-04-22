global using System.Net;
global using System.Net.Http.Headers;
global using System.Net.Http.Json;
global using System.Text.Json;

global using System.IdentityModel.Tokens.Jwt;
global using System.Security.Cryptography;
global using System.Security.Claims;

global using Microsoft.AspNetCore.Mvc.Testing;
global using Microsoft.IdentityModel.Tokens;
global using Microsoft.Extensions.DependencyInjection;

global using HttpsRichardy.Internal.Essentials.Filtering;
global using HttpsRichardy.Internal.Essentials.Patterns;
global using HttpsRichardy.Internal.Essentials.Utilities;

global using HttpsRichardy.Federation.Domain.Aggregates;
global using HttpsRichardy.Federation.Domain.Filtering;
global using HttpsRichardy.Federation.Domain.Collections;
global using HttpsRichardy.Federation.Domain.Concepts;
global using HttpsRichardy.Federation.Common.Constants;

global using HttpsRichardy.Federation.Application.Services;
global using HttpsRichardy.Federation.Application.Providers;

global using HttpsRichardy.Federation.Application.Payloads.Identity;
global using HttpsRichardy.Federation.Application.Payloads.User;
global using HttpsRichardy.Federation.Application.Payloads.Realm;
global using HttpsRichardy.Federation.Application.Payloads.Permission;
global using HttpsRichardy.Federation.Application.Payloads.Group;
global using HttpsRichardy.Federation.Application.Payloads.Secret;
global using HttpsRichardy.Federation.Application.Payloads.Common;
global using HttpsRichardy.Federation.Application.Payloads.Connect;
global using HttpsRichardy.Federation.Application.Payloads.Client;

global using HttpsRichardy.Federation.Infrastructure.Persistence;
global using HttpsRichardy.Federation.Infrastructure.Security;
global using HttpsRichardy.Federation.Infrastructure.Constants;

global using HttpsRichardy.Federation.Domain.Errors;
global using HttpsRichardy.Federation.WebApi;

global using HttpsRichardy.Federation.TestSuite.Extensions;
global using HttpsRichardy.Federation.TestSuite.Integration.Fixtures;

global using Xunit;
global using Moq;

global using DotNet.Testcontainers.Builders;
global using DotNet.Testcontainers.Containers;

global using MongoDB.Driver;
global using MongoDB.Driver.Core.Configuration;

global using AutoFixture;
