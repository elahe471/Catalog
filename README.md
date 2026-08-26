# Catalog Service

A lightweight **Catalog Service** built with **.NET 10** and ASP.NET Core Minimal APIs.

The main purpose of this service is to generate and manage sample catalog data for testing and implementing the **Search as a Service** project.

## Purpose

The Catalog Service is used as the source of product data and integration events.

It was created mainly to demonstrate and test:

* Catalog data management
* Message Broker integration
* Asynchronous communication
* RabbitMQ
* MassTransit
* Integration Events
* Elasticsearch indexing
* Synchronization between Catalog and Search services

## Architecture

```text
Catalog Service
      |
      | Integration Events
      v
   RabbitMQ
      |
      v
 Search Service
      |
      v
 Elasticsearch
```

The Catalog Service remains the source of truth for catalog data.

When a catalog item is created, updated, or deleted, an integration event is published through RabbitMQ.

The Search Service consumes these events and synchronizes the corresponding document in Elasticsearch.

## Integration Events

The service publishes events such as:

```text
CatalogItemAddedEvent
CatalogItemChangedEvent
CatalogItemDeletedEvent
```

This allows the Catalog and Search services to remain loosely coupled.

## Catalog APIs

The service provides APIs for managing:

* Catalog Items
* Catalog Brands
* Catalog Categories


## Why This Service Exists

The goal of this project is not to build a complete production-ready e-commerce catalog.

Instead, it provides realistic sample data and event flows required to implement and demonstrate the **Search as a Service** architecture.

The combination of the Catalog and Search services demonstrates how data can flow asynchronously from a relational database to Elasticsearch through a message broker.

## Tech Stack

* .NET 10
* ASP.NET Core Minimal APIs
* Entity Framework Core
* RabbitMQ
* MassTransit
* Docker

## Related Project

The catalog data produced by this service is consumed by the **Search Service**, where Elasticsearch capabilities such as full-text search, filtering, pagination, autocomplete, facets, and highlighting are implemented.
