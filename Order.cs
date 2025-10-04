using Azure;
using Azure.Data.Tables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace QueueFunction
{
    public class Order : ITableEntity
    {
        public string PartitionKey { get; set; } = "Orders";
        public string RowKey { get; set; } = Guid.NewGuid().ToString();

        public string CustomerName { get; set; }
        public string ProductName { get; set; }
        public double Total { get; set; }

        public string SneakerPictureUrl { get; set; }

        public ETag ETag { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
    }
}
