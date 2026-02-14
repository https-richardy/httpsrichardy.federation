# REST API Reference

This document provides a clear reference to consume our REST API, including standardization rules, serialization, and OpenAPI specification.

Our goal is to make it easy for developers to integrate with our Identity Provider without surprises.
Think of this as your **hands-on guide** to using the API, not just a reference.

## Table of Contents

- [Standardization](#standardization)
- [JSON Serialization](#json-serialization)
- [OpenAPI Specification](#openapi-specification)

## Standardization

Our API is consistent and predictable. When things go well, you’ll always get the **payload** you expect. When something goes wrong, you’ll receive the appropriate **HTTP status code** along with an **error object** — and this object is always present for all expected errors.

The error object follows this structure:

```json
{
  "code": "VINDER-IDP-ERR-XXX-XXX",
  "description": "A clear description of what went wrong."
}
```

* **code**: a unique identifier for the error (check out [errors-reference.md](/Documentation/errors-reference.md) for all available codes).
* **description**: a human-readable message explaining what happened.

This makes it easy to programmatically handle errors, log issues, or show meaningful messages in your apps.
No surprises, no hidden structures — just a clean and consistent pattern across the entire API.

## JSON Serialization

All our responses are serialized in camelCase, the standard for web APIs.
We found it important to mention this because we've worked with APIs where some endpoints returned camelCase while others returned snake_case — which is confusing.

With our provider, you can rely on camelCase everywhere, consistently, making it easier to consume and integrate with your frontends or other services.

## OpenAPI Specification

Our API is fully documented using the **OpenAPI 3 (OAS3) standard**.
This means you can explore all available endpoints, request/response schemas requirements programmatically or using tools like **Swagger UI**, **Postman**, or any OpenAPI-compatible client.

You can access the OpenAPI JSON directly at: `https://<your-host>/openapi/v1.json`

# Accessing the OpenAPI Specification via /scalar

You can also access and interact with the OpenAPI documentation using the `/scalar` endpoint:

- `https://<your-host>/scalar`

This endpoint provides a user-friendly interface for exploring, testing, and visualizing all available API endpoints and schemas directly in your browser. Tools like Swagger UI or Scalar UI can be used for this purpose, making integration and discovery even easier for developers.
