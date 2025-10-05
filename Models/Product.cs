using Azure;
using Azure.Data.Tables;

namespace DesignerCloset.Models
{

    public class Product : ITableEntity
    {
        public string PartitionKey { get; set; } = "Product";
        public string? RowKey { get; set; }

        public string? ImageUrl { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }


        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }
    }

}