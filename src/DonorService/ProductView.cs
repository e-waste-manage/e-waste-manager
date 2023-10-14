using Amazon.DynamoDBv2.Model;

namespace DonorService
{
    public class ProductView
    {
        List<Product> products;
        Dictionary<string, AttributeValue> lastEvaluatedKey;

        public ProductView(List<Product> products, Dictionary<string, AttributeValue> lastEvaluatedKey)
        {
            this.products = products;
            this.lastEvaluatedKey = lastEvaluatedKey;
        }
    }

}
