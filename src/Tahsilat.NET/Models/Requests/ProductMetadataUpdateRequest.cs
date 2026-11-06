using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Tahsilat.NET.Models.Requests
{
    /// <summary>
    /// Body of <c>POST products/update-metadata/{id}</c>: the complete new metadata
    /// list, encoded as metadata[i][key] / metadata[i][value] by the form builder.
    /// </summary>
    internal sealed class ProductMetadataUpdateRequest
    {
        public ProductMetadataUpdateRequest(List<Dictionary<string, object>> metadata)
        {
            Metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
        }

        [JsonProperty("metadata")]
        public List<Dictionary<string, object>> Metadata { get; }
    }
}
