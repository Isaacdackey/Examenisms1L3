package BB.resto.Repository.Contract;

import BB.resto.Entity.Product;
import BB.resto.Entity.Enumeration.TypeProduct;
import java.util.List;

public interface IProductRepository extends IBaseRepository<Product> {


    boolean unarchive(int id);

    List<Product> findActiveProducts();


    List<Product> findArchivedProducts();


    List<Product> findByType(TypeProduct type);


    List<Product> findByLibelle(String libelle);


    List<Product> findByPriceRange(double min, double max);


    boolean archiveProduct(int id);
    boolean unarchiveProduct(int id);


    boolean updateCloudinaryImage(int productId, String cloudinaryUrl, String publicId);


    long countByType(TypeProduct type);
    long countActiveProducts();
    long countArchivedProducts();


    List<Product> searchProducts(String keyword, TypeProduct type, Double minPrice, Double maxPrice);

    List<Product> findProductsWithImages();
    List<Product> findProductsWithoutImages();


    List<Product> findMostExpensiveProducts(int limit);
    List<Product> findCheapestProducts(int limit);


    List<Product> findRecentlyAddedProducts(int limit);
    List<Product> findRecentlyUpdatedProducts(int limit);
}