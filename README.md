# EasyCart - Enterprise Microservices E-Commerce Platform

## Overview

EasyCart is a microservices-based e-commerce backend application built using ASP.NET Core and modern cloud-native architectural patterns.

The project demonstrates enterprise-grade concepts including:

* Microservices Architecture
* API Gateway Pattern
* JWT Authentication & Authorization
* Database per Service
* Event-Driven Architecture
* Distributed Caching
* Observability & Distributed Tracing
* Centralized Logging
* Resiliency Patterns

---

# Architecture

```text
Client
   |
   v
+-----------------------+
|    API Gateway        |
|      Ocelot           |
+-----------------------+
            |
    -------------------------
    |         |         |
    v         v         v
+--------+ +--------+ +------------+
| Auth   | |Product | |   Order    |
| API    | | API    | |    API     |
+--------+ +--------+ +------------+
               |            |
               |            |
           Redis Cache      |
                            |
                       RabbitMQ
                            |
                      MassTransit
                            |
                    +---------------+
                    | Inventory API |
                    +---------------+

Each service owns its own database.
```

---

# Microservices

## Authentication API

Responsible for:

* User Registration
* User Login
* JWT Token Generation
* Role Management

Features:

* JWT Authentication
* Role-Based Authorization
* PostgreSQL Database

---

## Product API

Responsible for:

* Product CRUD Operations
* Product Retrieval
* Product Management

Features:

* Redis Cache
* PostgreSQL
* OpenTelemetry
* Serilog Logging

Endpoints:

```http
GET    /api/products
GET    /api/products/{id}
POST   /api/products
PUT    /api/products/{id}
DELETE /api/products/{id}
```

---

## Order API

Responsible for:

* Order Creation
* Order Management

Features:

* PostgreSQL
* Polly
* RabbitMQ Publisher
* OpenTelemetry

Endpoints:

```http
GET    /api/orders
POST   /api/orders
```

---

## Inventory API

Responsible for:

* Inventory Management
* Stock Reduction
* Stock Updates

Features:

* RabbitMQ Consumer
* MassTransit
* PostgreSQL
* OpenTelemetry

Endpoints:

```http
GET    /api/inventory/{productId}
POST   /api/inventory
POST   /api/inventory/{productId}/add-stock
POST   /api/inventory/{productId}/reduce-stock
```

---

# API Gateway

Ocelot is used as the API Gateway.

Responsibilities:

* Request Routing
* JWT Authentication
* Rate Limiting
* Response Caching
* Centralized Access Point

---

# Security

## JWT Authentication

Features:

* Token-based authentication
* Role-based authorization
* Secure API access

Example:

```json
{
  "email": "admin@example.com",
  "role": "Admin"
}
```

---

# Event-Driven Architecture

RabbitMQ and MassTransit are used for asynchronous communication.

## Order Processing Flow

```text
Client
   ↓
Order API
   ↓
OrderPlacedEvent
   ↓
RabbitMQ
   ↓
Inventory Service
   ↓
Inventory Updated
```

Benefits:

* Loose coupling
* Better scalability
* Improved reliability
* Asynchronous processing

---

# Redis Distributed Cache

Implemented in Product API.

Strategy:

* Cache Aside Pattern

Benefits:

* Faster product retrieval
* Reduced database load
* Improved response times

---

# Observability

EasyCart uses OpenTelemetry for distributed tracing.

Features:

* HTTP Request Tracing
* Database Query Tracing
* RabbitMQ Messaging Tracing
* Consumer Tracing
* Cross-Service Trace Propagation

Technologies:

* OpenTelemetry
* OTLP Exporter
* Jaeger

---

# Distributed Tracing

```text
Client
   ↓
Ocelot Gateway
   ↓
Order Service
   ↓
PostgreSQL
   ↓
RabbitMQ
   ↓
Inventory Service
   ↓
PostgreSQL
```

---

# Shared Library

The shared library provides:

* JWT Configuration
* OpenTelemetry Configuration
* Serilog Configuration
* Global Exception Middleware
* Trace Middleware
* Dependency Injection Extensions

---

# Trace Middleware

Implemented using shared middleware.

Features:

* Correlation ID generation
* Cross-service propagation
* Request tracking

Header:

```text
X-Correlation-Id
```

---

# Logging

Serilog provides:

* Structured Logging
* Console Logging
* File Logging
* Error Tracking

---

# Resiliency

Polly is implemented in Order API.

Features:

* Retry Policies
* Fault Handling
* Temporary Failure Recovery

---

# Database Design

Database-per-service pattern.

| Service     | Database  |
| ----------- | --------- |
| Auth API    | authdb    |
| Product API | productdb |
| Order API   | orderdb   |

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

## Messaging

* RabbitMQ
* MassTransit

## Caching

* Redis

## Observability

* OpenTelemetry
* Jaeger

## Logging

* Serilog

## Resiliency

* Polly

## Containerization

* Docker

---

# Design Patterns

* Microservices Architecture
* Repository Pattern
* Dependency Injection
* API Gateway Pattern
* Database Per Service Pattern
* Event-Driven Architecture
* Publisher-Subscriber Pattern
* Distributed Cache Pattern
* Middleware Pattern
* Observability Pattern

---

# Running the Application

## Prerequisites

* .NET 8 SDK
* Docker Desktop
* PostgreSQL
* RabbitMQ
* Redis
* Jaeger

---

## Start Infrastructure

```bash
docker compose up -d
```

---

## Run Services

```bash
dotnet run
```

Run:

* Authentication API
* Product API
* Order API
* Inventory API
* API Gateway

---

## Access Through Gateway

```http
https://localhost:5003/api/products
```

---

# Jaeger Dashboard

```text
http://localhost:16686
```

Provides:

* End-to-end tracing
* Database spans
* RabbitMQ spans
* Cross-service observability

---

# Future Enhancements

* Azure API Management
* Azure Application Insights
* Azure Service Bus
* Azure Redis Cache
* Azure Key Vault
* Azure App Service
* Azure Container Apps
* AKS
* CI/CD Pipelines
* Health Checks
* Notification Service

---

# Learning Objectives

This project demonstrates:

* ASP.NET Core Microservices
* Ocelot API Gateway
* JWT Authentication
* RabbitMQ Messaging
* MassTransit
* Redis Caching
* OpenTelemetry
* Jaeger
* Distributed Tracing
* Event-Driven Architecture
* Serilog Logging
* Polly Resiliency
* PostgreSQL
* Docker

---

## Author

**Ajith Nair**
