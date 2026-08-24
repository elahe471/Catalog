using System;
using System.Collections.Generic;
using System.Text;

namespace Catalog.Contracts.IntegrationEvents
{
    public record CatalogItemDeletedEvent(
 string Slug);
}
