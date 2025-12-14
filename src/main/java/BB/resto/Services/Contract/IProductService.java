package BB.resto.Services.Contract;

import BB.resto.Entity.Product;
import BB.resto.Entity.Enumeration.TypeProduct;
import java.util.List;
import java.util.Optional;

public interface IProductService {

    Product createProduct(Product product);
    Product createProductWithImage(Product product, String imagePath);
    Product updateProduct(Product product);
    boolean deleteProduct(int id);
    Optional<Product> getProductById(int id);
    List<Product> getAllProducts();


    boolean uploadProductImage(int productId, String imagePath);
    boolean deleteProductImage(int productId);
    String getProductImageUrl(int productId);
    String getProductThumbnailUrl(int productId);


    List<Product> getProductsByType(TypeProduct type);
    List<Product> getActiveProducts();
    List<Product> getArchivedProducts();
    List<Product> searchProductsByName(String name);
    List<Product> getProductsByPriceRange(double min, double max);


    boolean archiveProduct(int id);
    boolean unarchiveProduct(int id);


    long countProducts();
    long countProductsByType(TypeProduct type);
    long countActiveProducts();
    long countArchivedProducts();


    List<Product> getProductsWithImages();
    List<Product> getProductsWithoutImages();
    List<Product> getMostExpensiveProducts(int limit);
    List<Product> getCheapestProducts(int limit);
    List<Product> getRecentlyAddedProducts(int limit);


    boolean isProductAvailable(int id);
    double calculateProductPriceWithTax(int productId, double taxRate);
}