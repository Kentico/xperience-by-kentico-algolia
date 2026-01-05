namespace DancingGoat.Models;

public record ProductListItemViewModel(string Name, string ImagePath, string Url, decimal Price, string Tag)
{
    public static ProductListItemViewModel GetViewModel(IProductFields product, string urlPath, string tag) => new(
                        product.ProductFieldName,
                        product.ProductFieldImage.FirstOrDefault()?.ImageFile.Url,
                        urlPath,
                        product.ProductFieldPrice,
                        tag);
}
