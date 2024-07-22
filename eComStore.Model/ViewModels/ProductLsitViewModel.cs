namespace eComStore.Model.ViewModels
{
    public class ProductListViewModel
    {
        public IEnumerable<Product> Products { get; set; }
        public IEnumerable<ShoppingCart> ListCarts {  get; set; } 
        public IEnumerable<WishList> WishLists { get; set; }
    }
}
