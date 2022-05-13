using HotChocolate.AspNetCore.Authorization;
using ProductService.Models;

namespace ProductService.GraphQL
{
    public class Query
    {
        [Authorize]
        public IQueryable<Product> GetProducts([Service] Latihan4Context context) =>
            context.Products;
    }
}
