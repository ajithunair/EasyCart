# EasyCart - Microservices E-Commerce Platform

## Overview

EasyCart is a microservices-based e-commerce backend application built using ASP.NET Core and modern cloud-native architectural patterns.

The project demonstrates how to design, develop, and integrate multiple independent services using API Gateway, JWT Authentication, Role-Based Authorization, Centralized Logging, Caching, Rate Limiting, Resiliency Patterns, and Shared Infrastructure components.

The solution is designed as a learning and reference project for building scalable enterprise-grade microservices using .NET.

---

# Architecture

```text
Client
   |
   v
+-------------------+
|   API Gateway     |
|     (Ocelot)      |
+-------------------+
      |
      +-------------------+
      |                   |
      v                   v
+-------------+    +-------------+
| Product API |    | Order API   |
+-------------+    +-------------+
      |
      |
      v
+-------------+
| Auth API    |
+-------------+

Each service owns its own database.
```

---

# Microservices

## 1. Authentication API

Responsible for:

* User Registration
* User Login
* JWT Token Generation
* Role Management
* Authentication & Authorization

Features:

* JWT Authentication
* Role-Based Authorization
* Secure Token Generation
* PostgreSQL Database

Example Roles:

* Admin
* User

---

## 2. Product API

Responsible for:

* Product Management
* Product CRUD Operations
* Product Search & Retrieval

Features:

* JWT Protected Endpoints
* Role-Based Authorization
* PostgreSQL Database
* Global Exception Handling
* Serilog Logging

Example:

```http
GET    /api/products
GET    /api/products/{id}
POST   /api/products
PUT    /api/products/{id}
DELETE /api/products/{id}
```

---

## 3. Order API

Responsible for:

* Order Creation
* Order Tracking
* Order Management

Features:

* JWT Authentication
* Role-Based Authorization
* PostgreSQL Database
* Polly Resiliency Policies
* Retry and Fault Handling
* Global Exception Handling

Example:

```http
GET    /api/orders
GET    /api/orders/{id}
POST   /api/orders
```

---

# API Gateway (Ocelot)

EasyCart uses Ocelot API Gateway as a single entry point for all client requests.

Responsibilities:

* Request Routing
* Authentication Validation
* Authorization
* Rate Limiting
* Response Caching
* Centralized Access Point
* API Aggregation (future)

Benefits:

* Reduced Client Complexity
* Improved Security
* Centralized Cross-Cutting Concerns

---

# Security

## JWT Authentication

Authentication API generates JWT tokens containing:

* User Information
* Roles
* Expiration
* Issuer & Audience

Example Claims:

```json
{
  "email": "admin@example.com",
  "name": "Admin User",
  "role": "Admin"
}
```

---

## Role-Based Authorization

Implemented using ASP.NET Core Authorization Policies.

Examples:

```csharp
[Authorize]
```

```csharp
[Authorize(Roles = "Admin")]
```

Protected Operations:

* Create Product
* Update Product
* Delete Product
* Administrative Actions

---

# Shared Library

A reusable Shared Library is used across all services.

Responsibilities:

## Common Components

* JWT Authentication Configuration
* Database Configuration
* Dependency Injection Registration
* Shared Models
* Shared DTOs
* Shared Utilities

## Middleware Components

### Global Exception Middleware

Provides:

* Centralized Exception Handling
* Consistent Error Responses
* Structured Logging

### API Gateway Verification Middleware

Provides:

* API Gateway Request Validation
* Internal Service Protection

### Other Shared Middleware

* Request Processing
* Common Validation Logic
* Custom Behaviors

---

# Logging

## Serilog

Centralized logging is implemented using Serilog.

Features:

* Console Logging
* Debug Logging
* File Logging
* Structured Logs
* Error Tracking

Example Log:

```text
2026-06-20 10:15:12 [INF] Product Created Successfully
```

Benefits:

* Easier Troubleshooting
* Audit Trails
* Production Diagnostics

---

# Resiliency

## Polly

Implemented in Order API.

Features:

* Retry Policies
* Fault Handling
* Temporary Failure Recovery
* Improved Service Reliability

Example Scenarios:

* Database Connectivity Issues
* Temporary Network Failures
* Downstream Service Failures

---

# Caching

Implemented through Ocelot API Gateway.

Benefits:

* Faster Response Times
* Reduced Database Calls
* Improved Scalability

Example Cached Endpoints:

```http
GET /api/products
GET /api/products/{id}
```

---

# Rate Limiting

Implemented through Ocelot.

Purpose:

* Protect APIs from abuse
* Prevent excessive requests
* Improve system stability

Benefits:

* Enhanced Security
* Fair Resource Utilization
* Protection Against Traffic Spikes

---

# Database Design

Each microservice owns its own database following the Database-Per-Service pattern.

| Service            | Database   |
| ------------------ | ---------- |
| Authentication API | PostgreSQL |
| Product API        | PostgreSQL |
| Order API          | PostgreSQL |

Environment:

* PostgreSQL running inside Docker Containers
* Independent schemas per service
* Service isolation maintained

Benefits:

* Loose Coupling
* Independent Scaling
* Better Maintainability

---

# Technology Stack

## Backend

* ASP.NET Core 8
* C#
* Entity Framework Core

## API Gateway

* Ocelot

## Security

* JWT Authentication
* Role-Based Authorization

## Database

* PostgreSQL

## Containerization

* Docker

## Logging

* Serilog

## Resiliency

* Polly

## API Testing

* Swagger/OpenAPI
* Postman / Insomnia

---

# Design Patterns & Architecture

* Microservices Architecture
* Repository Pattern
* Dependency Injection
* Middleware Pattern
* Database Per Service Pattern
* API Gateway Pattern
* Resiliency Pattern
* Centralized Logging Pattern

---

# Running the Application

## Prerequisites

* .NET 8 SDK
* Docker Desktop
* PostgreSQL Docker Containers
* Visual Studio 2022 / VS Code

## Steps

1. Start PostgreSQL containers

```bash
docker compose up -d
```

2. Run Authentication API

```bash
dotnet run
```

3. Run Product API

```bash
dotnet run
```

4. Run Order API

```bash
dotnet run
```

5. Run API Gateway

```bash
dotnet run
```

6. Access APIs through Gateway

```http
https://localhost:5003/api/products
```

---

# Future Enhancements

The following enterprise-grade features are planned:

## Azure

* Azure API Management (APIM)
* Azure Application Gateway
* Azure App Services
* Azure Container Apps
* Azure Kubernetes Service (AKS)

## Security

* Azure Key Vault
* Managed Identity
* Secret Rotation

## Caching

* Azure Redis Cache

## DevOps

* CI/CD Pipelines
* GitHub Actions
* Azure DevOps Pipelines

## Containerization

* Docker Multi-Stage Builds
* Container Registry
* Kubernetes Deployment

## Monitoring & Observability

* Azure Monitor
* Application Insights
* OpenTelemetry
* Distributed Tracing
* Centralized Log Analytics

## Reliability

* Circuit Breaker Pattern
* Service Discovery
* Health Checks

## Scalability

* Horizontal Scaling
* Load Balancing
* Auto Scaling

---

# Learning Objectives

This project demonstrates practical implementation of:

* ASP.NET Core Microservices
* Ocelot API Gateway
* JWT Authentication
* Role-Based Authorization
* Serilog Logging
* Polly Resiliency
* PostgreSQL with Docker
* Centralized Exception Handling
* Rate Limiting
* Response Caching
* Enterprise Architecture Principles

---

## Author

**Ajith Nair**


