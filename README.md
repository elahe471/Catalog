# Catalog Service

A lightweight **Catalog Service** built with **.NET 10** and ASP.NET Core Minimal APIs.

The main purpose of this service is to generate and manage sample catalog data for testing and implementing the **Search as a Service** and **Media as a Service** projects.

## Purpose

The Catalog Service is used as the source of product data and participates in asynchronous integration with other services.

It was created mainly to demonstrate and test:

* Catalog data management
* Message Broker integration
* Asynchronous communication
* RabbitMQ
* MassTransit
* Integration Events
* Elasticsearch indexing
* Synchronization between Catalog and Search services
* Media attachment through Media Service events

## Architecture

The Catalog Service has two different roles in the current architecture:

1. **Publisher for the Search Service**
2. **Consumer for the Media Service**

```text
                    Catalog Service
                   /               \
                  /                 \
        Publish Events           Consume Events
               |                     ^
               v                     |
            RabbitMQ              RabbitMQ
               |                     ^
               v                     |
        Search Service          Media Service
               |                     |
               v                     v
        Elasticsearch          Object Storage
                                   (MinIO)
```

The Catalog Service remains the source of truth for catalog data.

When a catalog item is created, updated, or deleted, an integration event is published through RabbitMQ. The Search Service consumes these events and synchronizes the corresponding document in Elasticsearch.

For media integration, the communication direction is reversed. The Media Service uploads the file to Object Storage and publishes a `MediaUploadedEvent`. The Catalog Service consumes this event, finds the related catalog item, and attaches the media reference to it.

## Integration Events

### Catalog → Search

The Catalog Service publishes events such as:

```text
CatalogItemAddedEvent
CatalogItemChangedEvent
CatalogItemDeletedEvent
```

The Search Service consumes these events and updates Elasticsearch.

### Media → Catalog

The Media Service publishes:

```text
MediaUploadedEvent
```

The Catalog Service consumes this event and associates the uploaded media with the related catalog item.

Conceptually:

```text
Media Service
    |
    | Upload media
    v
Object Storage
    |
    | Publish MediaUploadedEvent
    v
RabbitMQ
    |
    v
Catalog Consumer
    |
    | Find Catalog Item
    v
Attach Media Reference
```

This keeps the Catalog and Media services loosely coupled. The Media Service does not directly access the Catalog database.

## Catalog APIs

The service provides APIs for managing:

* Catalog Items
* Catalog Brands
* Catalog Categories

## Why This Service Exists

The goal of this project is not to build a complete production-ready e-commerce catalog.

Instead, it provides realistic sample data and event flows required to implement and demonstrate distributed system concepts such as **Search as a Service**, **Media as a Service**, asynchronous communication, and event-driven integration.

The Catalog Service demonstrates both sides of asynchronous messaging:

```text
Catalog → Publisher → Search
Media   → Publisher → Catalog Consumer
```

## Tech Stack

* .NET 10
* ASP.NET Core Minimal APIs
* Entity Framework Core
* RabbitMQ
* MassTransit
* Docker

## Related Projects

### Search as a Service

Catalog data is consumed by the **Search Service**, where Elasticsearch capabilities such as full-text search, filtering, pagination, autocomplete, facets, and highlighting are implemented.

### Media as a Service

The **Media Service** is responsible for validating and storing media files in Object Storage. After a successful upload, it publishes a `MediaUploadedEvent`.

The Catalog Service consumes this event and stores the relationship between the catalog item and its media reference.
