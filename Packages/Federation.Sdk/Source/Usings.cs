global using System.Text;
global using System.Text.Json;
global using System.Net.Http.Headers;
global using System.Net.Http.Json;
global using System.Reflection;

global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.AspNetCore.Authentication.JwtBearer;

global using HttpsRichardy.Internal.Essentials.Patterns;

global using HttpsRichardy.Federation.Sdk.Serializers;
global using HttpsRichardy.Federation.Sdk.Helpers;
global using HttpsRichardy.Federation.Sdk.Clients;
global using HttpsRichardy.Federation.Sdk.Configurations;
global using HttpsRichardy.Federation.Sdk.Interceptors;

global using HttpsRichardy.Federation.Sdk.Contracts.Errors;
global using HttpsRichardy.Federation.Sdk.Contracts.Clients;
global using HttpsRichardy.Federation.Sdk.Contracts.Payloads.Identity;
global using HttpsRichardy.Federation.Sdk.Contracts.Payloads.Group;
global using HttpsRichardy.Federation.Sdk.Contracts.Payloads.Permission;
global using HttpsRichardy.Federation.Sdk.Contracts.Payloads.Common;
global using HttpsRichardy.Federation.Sdk.Contracts.Payloads.Realm;
global using HttpsRichardy.Federation.Sdk.Contracts.Payloads.User;
