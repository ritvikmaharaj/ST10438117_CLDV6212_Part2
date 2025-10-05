using Azure;
using Azure.Data.Tables;

namespace DesignerCloset.Models
{
    public class Order : ITableEntity
    {
        public string PartitionKey { get; set; } = "OrderPartition";
        public string RowKey { get; set; } = Guid.NewGuid().ToString();

           
        public string ProductId { get; set; }    
        public string ShoeSize { get; set; }
        public int Quantity { get; set; }

        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        public ETag ETag { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
    }
}
