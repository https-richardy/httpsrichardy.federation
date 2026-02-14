# Errors Reference

This document provides a comprehensive catalog of error codes, messages, and their meanings within the system. It is intended to help developers, testers, and support teams quickly understand the possible errors that may occur, the scenarios in which they arise, and guidance on how to handle or resolve them.

Each error entry includes:  
- A unique error code for easy identification  
- A clear description of the error  
- Contextual information about when and why the error might occur

## Table of Contents

- [Authentication Errors (#VINDER-IDP-ERR-AUT-XXX)](#authentication-errors)
- [Tenant Errors - (#VINDER-IDP-ERR-TNT-XXX)](#tenant-errors)
- [Group Errors - (#VINDER-IDP-ERR-GRP-XXX)](#group-errors)
- [User Errors - (#VINDER-IDP-ERR-USR-XXX)](#user-errors)
- [Permission Errors - (#VINDER-IDP-ERR-PRM-XXX)](#permission-errors)
- [Identity Errors - (#VINDER-IDP-ERR-IDN-XXX)](#identity-errors)

# Authentication Errors

Errors in this category are related to user authentication and token validation processes. Each error code starting with `#VINDER-IDP-ERR-AUT-XXX` indicates an issue encountered during authentication flows such as login, token generation, validation, refresh, and logout.

| Error Code                | Description                                                     | When it Occurs                                                       | How to Handle / Fix                                              |
|---------------------------|-----------------------------------------------------------------|---------------------------------------------------------------------|-----------------------------------------------------------------|
| `#VINDER-IDP-ERR-AUT-401` | Invalid credentials                                             | When the username or password provided do not match any user       | Verify the credentials and retry login                          |
| `#VINDER-IDP-ERR-AUT-403` | Invalid client credentials                                       | When the client secret provided are incorrect                | Verify client credentials and retry                              |
| `#VINDER-IDP-ERR-AUT-404` | User not found                                                 | When the username does not exist in the system                      | Confirm the username exists or register a new account           |
| `#VINDER-IDP-ERR-AUT-405` | Invalid refresh token                                          | When the refresh token is expired, invalid, or already used        | Request a new login and refresh token                           |
| `#VINDER-IDP-ERR-AUT-408` | Client not found                                                | When the client making the request does not exist                  | Verify the client registration                                   |
| `#VINDER-IDP-ERR-AUT-409` | Logout failed                                                 | When revoking a refresh token fails due to invalid or expired token | Verify token validity before logout                             |
| `#VINDER-IDP-ERR-AUT-410` | Invalid token format                                          | When a token is malformed or incorrectly formatted                  | Ensure tokens are correctly generated and transmitted          |
| `#VINDER-IDP-ERR-AUT-411` | Token expired                                                | When the access or refresh token has passed its expiry time        | Request a new token via refresh flow or login                   |
| `#VINDER-IDP-ERR-AUT-412` | Invalid token signature                                      | When the token's signature validation fails (token tampering)      | Verify the token source and signing key                         |

# Tenant Errors

Errors in this category are related to issues with tenant identification and validation in the HTTP request context. Each error code starting with `#VINDER-IDP-ERR-TNT-XXX` indicates a problem encountered during tenant resolution and middleware processing.

| Error Code                | Description                                      | When it Occurs                                          | How to Handle / Fix                                             |
|---------------------------|------------------------------------------------|--------------------------------------------------------|----------------------------------------------------------------|
| `#VINDER-IDP-ERR-TNT-400` | Tenant header is missing from the HTTP request | When the incoming request does not include the tenant header (`X-Tenant`) | Ensure the client sends the tenant header in each request       |
| `#VINDER-IDP-ERR-TNT-404` | The specified tenant does not exist             | When the tenant header value does not match any tenant in the system | Verify tenant name correctness or register the tenant           |
| `#VINDER-IDP-ERR-TNT-409` | A tenant with the same name already exists      | When trying to create a tenant that already exists in the system | Ensure the tenant name is unique before attempting to create it |
| `#VINDER-IDP-ERR-TNT-415` | The tenant already has the specified permission assigned | When trying to assign a permission that the tenant already has | Check existing permissions before assignment to avoid duplicates |
| `#VINDER-IDP-ERR-TNT-416` | The tenant does not have the specified permission assigned | When trying to remove a permission that the tenant does not have | Verify the tenant's permissions before attempting removal |
| `#VINDER-IDP-ERR-TNT-500` | No HTTP context available to retrieve tenant information | When code runs outside of an HTTP context (e.g., background jobs like Hangfire) where `HttpContext` is not available | Ensure tenant resolution logic accounts for non-HTTP scenarios, e.g., pass tenant info explicitly in background job parameters |

# Group Errors

Errors in this category are related to issues with group management and permission assignments. Each error code starting with `#VINDER-IDP-ERR-GRP-XXX` indicates a problem encountered during group operations in the system.

| Error Code                | Description                                      | When it Occurs                                          | How to Handle / Fix                                             |
|---------------------------|------------------------------------------------|--------------------------------------------------------|----------------------------------------------------------------|
| `#VINDER-IDP-ERR-GRP-404` | The group with the specified ID does not exist | When attempting to access or modify a group that doesn't exist | Verify the group ID or create the group before performing operations |
| `#VINDER-IDP-ERR-GRP-409` | The group with the specified name already exists | When trying to create a group that already exists in the system | Ensure the group name is unique before attempting to create it |
| `#VINDER-IDP-ERR-GRP-415` | The group already has the specified permission assigned | When trying to assign a permission that the group already has | Check existing permissions before assignment to avoid duplicates |
| `#VINDER-IDP-ERR-GRP-416` | The group does not have the specified permission assigned | When trying to remove a permission that the group does not have | Verify the group's permissions before attempting removal |

# Identity Errors

Errors in this category are related to issues with user identity management. Each error code starting with `#VINDER-IDP-ERR-IDN-XXX` indicates a problem encountered during identity operations in the system.

| Error Code                | Description                                      | When it Occurs                                          | How to Handle / Fix                                             |
|---------------------------|------------------------------------------------|--------------------------------------------------------|----------------------------------------------------------------|
| `#VINDER-IDP-ERR-IDN-409` | The user with the specified username already exists | When trying to create a user with a username that is already taken | Ensure the username is unique before attempting to create the user |

# Permission Errors

Errors in this category are related to issues with permission management. Each error code starting with `#VINDER-IDP-ERR-PRM-XXX` indicates a problem encountered during permission operations in the system.

| Error Code                | Description                                      | When it Occurs                                          | How to Handle / Fix                                             |
|---------------------------|------------------------------------------------|--------------------------------------------------------|----------------------------------------------------------------|
| `#VINDER-IDP-ERR-PRM-404` | The permission with the specified ID does not exist | When attempting to access or modify a permission that doesn't exist | Verify the permission ID or create the permission before performing operations |
| `#VINDER-IDP-ERR-PRM-409` | The permission with the specified name already exists | When trying to create a permission that already exists in the system | Ensure the permission name is unique before attempting to create it |

# User Errors

Errors in this category are related to issues with user management, group membership, and permission assignments. Each error code starting with `#VINDER-IDP-ERR-USR-XXX` indicates a problem encountered during user operations in the system.

| Error Code                | Description                                      | When it Occurs                                          | How to Handle / Fix                                             |
|---------------------------|------------------------------------------------|--------------------------------------------------------|----------------------------------------------------------------|
| `#VINDER-IDP-ERR-USR-404` | The specified user does not exist              | When attempting to access or modify a user that doesn't exist | Verify the user ID or create the user before performing operations |
| `#VINDER-IDP-ERR-USR-409` | The user is already a member of the specified group | When trying to add a user to a group they already belong to | Check the user's group membership before adding                  |
| `#VINDER-IDP-ERR-USR-410` | The user already has the specified permission assigned | When trying to assign a permission the user already has | Check existing permissions before assignment                     |
| `#VINDER-IDP-ERR-USR-416` | The user does not have the specified permission assigned | When trying to remove a permission the user does not have | Verify the user's permissions before attempting removal          |
| `#VINDER-IDP-ERR-USR-417` | The user is not a member of the specified group | When trying to remove a user from a group they are not part of | Check the user's group membership before attempting removal      |
