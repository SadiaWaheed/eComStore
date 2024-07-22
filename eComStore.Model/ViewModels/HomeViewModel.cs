namespace eComStore.Model.ViewModels
{
    public class HomeViewModel
    {
        public IEnumerable<Product> Products { get; set; }
        public IEnumerable<ShoppingCart> ListCart {  get; set; } 
    }
}
