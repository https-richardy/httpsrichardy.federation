# ERRORS REFERENCE

Federation Gateway is designed to provide clear, actionable error feedback for every stage of the identity and access management lifecycle. This document serves as a comprehensive reference for all error codes that may be encountered when interacting with the platform, whether through the backend or SDK. Each error is accompanied by a description, a likely cause, and guidance on how to resolve or avoid the issue.

---

## AUTHENTICATION ERRORS

| Code           | Description                                                        | Cause                                                      | Resolution                                  |
|----------------|--------------------------------------------------------------------|------------------------------------------------------------|----------------------------------------------|
| #ERROR-32B37   | The token format is invalid or the token is malformed.             | Token is missing, corrupted, or not JWT.                   | Ensure a valid token is provided.            |
| #ERROR-5F736   | The token has expired.                                             | Token lifetime exceeded.                                   | Request a new token.                        |
| #ERROR-FB8E4   | The token signature is invalid.                                    | Token was tampered or signed with wrong key.               | Check signing keys and token integrity.      |
| #ERROR-1A9C3   | The token issuer is invalid.                                       | Token was issued by an untrusted authority.                | Use tokens from the configured authority.    |
| #ERROR-2C0D9   | The provided refresh token is invalid, expired, or already used.   | Refresh token is wrong, expired, or reused.                | Request a new refresh token.                |
| #ERROR-60CBC   | Logout failed: the refresh token is invalid, expired, or reused.   | Logout attempted with invalid/expired refresh token.       | Re-authenticate and try again.              |
| #ERROR-A7E7C   | The provided credentials are invalid.                              | Wrong client/user credentials.                             | Check credentials and try again.            |
| #ERROR-0AF50   | The client was not found.                                          | ClientId does not exist or is misconfigured.               | Verify client registration.                 |
| #ERROR-D5D7C   | The provided client credentials are invalid.                       | Client secret is wrong or missing.                         | Check client credentials.                   |
| #ERROR-9B3E1   | Does not contain valid authentication credentials.                 | No Authorization header or invalid credentials.            | Provide valid credentials.                  |
| #ERROR-04A2F   | The user was not found.                                            | User does not exist or was deleted.                        | Check user existence or create user.        |

---

## AUTHORIZATION ERRORS

| Code           | Description                                                        | Cause                                                      | Resolution                                  |
|----------------|--------------------------------------------------------------------|------------------------------------------------------------|----------------------------------------------|
| #ERROR-F8EBC   | The specified redirect URI is not registered or allowed for realm. | Redirect URI not whitelisted for client/realm.             | Register the redirect URI.                  |
| #ERROR-C9D0A   | The provided authorization code is invalid, expired, or used.      | Code is wrong, expired, or already used.                   | Request a new authorization code.           |
| #ERROR-F4EB5   | The specified authorization grant type is not supported.           | Grant type not implemented or allowed.                     | Use a supported grant type.                 |
| #ERROR-5F5B3   | The authorization code has expired.                                | Code lifetime exceeded.                                    | Request a new code.                         |
| #ERROR-DDA70   | The provided code verifier does not match the code challenge.      | PKCE code verifier mismatch.                               | Use correct code verifier.                  |

---

## GROUP ERRORS

| Code           | Description                                                        | Cause                                                      | Resolution                                  |
|----------------|--------------------------------------------------------------------|------------------------------------------------------------|----------------------------------------------|
| #ERROR-1C6F3   | The group with the specified name already exists.                  | Group name conflict.                                       | Use a different group name.                 |
| #ERROR-9C69E   | The group already has the specified permission assigned.           | Permission already linked to group.                        | No action needed or remove duplicate.       |
| #ERROR-4D2E2   | The group with the specified ID does not exist.                    | Group ID is wrong or deleted.                              | Check group existence.                      |
| #ERROR-C2FB0   | The group does not have the specified permission assigned.         | Permission not linked to group.                            | Assign permission before removing.          |

---

## IDENTITY ERRORS

| Code           | Description                                                        | Cause                                                      | Resolution                                  |
|----------------|--------------------------------------------------------------------|------------------------------------------------------------|----------------------------------------------|
| #ERROR-DC3B1   | The user with the specified username already exists.               | Username conflict.                                         | Use a unique username.                      |

---

## PERMISSION ERRORS

| Code           | Description                                                        | Cause                                                      | Resolution                                  |
|----------------|--------------------------------------------------------------------|------------------------------------------------------------|----------------------------------------------|
| #ERROR-93F4A   | The permission with the specified name already exists.             | Permission name conflict.                                  | Use a different permission name.            |
| #ERROR-7B1E2   | The permission name is reserved by the system.                     | Attempt to use reserved name.                              | Choose another name.                        |
| #ERROR-93697   | The specified permission does not exist.                           | Permission not found.                                      | Check permission existence.                 |

---

## REALM ERRORS

| Code           | Description                                                        | Cause                                                      | Resolution                                  |
|----------------|--------------------------------------------------------------------|------------------------------------------------------------|----------------------------------------------|
| #ERROR-8B1C5   | No HTTP context available to retrieve realm information.           | Request outside HTTP context.                              | Ensure request is within HTTP context.       |
| #ERROR-2FB9A   | The specified realm does not exist.                                | Realm not found.                                           | Check realm existence.                      |
| #ERROR-B2E42   | Realm header is missing from the HTTP request.                     | Missing or malformed header.                               | Add correct realm header.                   |
| #ERROR-F98CE   | A realm with the same name already exists.                         | Realm name conflict.                                       | Use a unique realm name.                    |
| #ERROR-F23E2   | The realm already has the specified permission assigned.           | Permission already linked to realm.                        | No action needed or remove duplicate.       |
| #ERROR-C2FB0   | The realm does not have the specified permission assigned.         | Permission not linked to realm.                            | Assign permission before removing.          |

---

## USER ERRORS

| Code           | Description                                                        | Cause                                                      | Resolution                                  |
|----------------|--------------------------------------------------------------------|------------------------------------------------------------|----------------------------------------------|
| #ERROR-E6B32   | The specified user does not exist.                                 | User not found.                                            | Check user existence.                       |
| #ERROR-33066   | The user is already a member of the specified group.               | User already in group.                                     | No action needed or remove duplicate.       |
| #ERROR-44DEC   | The user already has the specified permission assigned.            | Permission already linked to user.                         | No action needed or remove duplicate.       |
| #ERROR-C2FB0   | The user does not have the specified permission assigned.          | Permission not linked to user.                             | Assign permission before removing.          |
| #ERROR-0E56E   | The user is not a member of the specified group.                   | User not in group.                                         | Add user to group before removing.          |

---

This reference is intended to help developers and integrators quickly identify, understand, and resolve errors encountered when working with Federation Gateway. For further details, consult the main documentation or open an issue with the error code and context.
