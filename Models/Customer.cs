using Azure;
using Azure.Data.Tables;
using System.ComponentModel.DataAnnotations;

namespace DesignerCloset.Models
{
    public class Customer : ITableEntity
    {
        [Key]
     
        public string CustomerName { get; set; }
        public string CustomerSurname { get; set; }
        public string CustomerCell { get; set; }

        public int Price { get; set; }

        // ITableEntity Implementation
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }

        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }
    }

}
