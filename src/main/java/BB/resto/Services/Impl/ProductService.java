package BB.resto.Services.Impl;

import BB.resto.Services.Contract.IProductService;
import BB.resto.Entity.Product;
import BB.resto.Entity.Enumeration.TypeProduct;
import BB.resto.Repository.Impl.ProductRepository;
import BB.resto.Util.CloudinaryManager;
import java.io.File;
import java.util.List;
import java.util.Optional;

public class ProductService implements IProductService {
    private ProductRepository productRepository;

    public ProductService() {
        this.productRepository = new ProductRepository();
    }

    @Override
    public Product createProduct(Product product) {

        if (product.getLibelle() == null || product.getLibelle().trim().isEmpty()) {
            throw new RuntimeException("Le libellé du produit est obligatoire");
        }

        if (product.getPrix() <= 0) {
            throw new RuntimeException("Le prix doit être supérieur à 0");
        }

        if (product.getTypeProduct() == null) {
            throw new RuntimeException("Le type de produit est obligatoire");
        }

        return productRepository.save(product);
    }

    @Override
    public Product createProductWithImage(Product product, String imagePath) {

        Product createdProduct = createProduct(product);


        if (imagePath != null && !imagePath.trim().isEmpty()) {
            try {
                uploadProductImage(createdProduct.getId(), imagePath);
            } catch (Exception e) {
                System.err.println("⚠️ Erreur lors de l'upload de l'image: " + e.getMessage());

            }
        }

        return createdProduct;
    }

    @Override
    public Product updateProduct(Product product) {
        return productRepository.update(product);
    }

    @Override
    public boolean deleteProduct(int id) {

        Optional<Product> productOpt = productRepository.findById(id);
        if (productOpt.isPresent()) {
            Product product = productOpt.get();
            if (product.getCloudinaryPublicId() != null) {
                try {
                    CloudinaryManager.deleteImage(product.getCloudinaryPublicId());
                } catch (Exception e) {
                    System.err.println("⚠️ Erreur suppression image Cloudinary: " + e.getMessage());
                }
            }
        }

        return productRepository.delete(id);
    }

    @Override
    public Optional<Product> getProductById(int id) {
        return productRepository.findById(id);
    }

    @Override
    public List<Product> getAllProducts() {
        return productRepository.findAll();
    }

    @Override
    public boolean uploadProductImage(int productId, String imagePath) {
        Optional<Product> productOpt = productRepository.findById(productId);
        if (productOpt.isEmpty()) {
            throw new RuntimeException("Produit non trouvé: " + productId);
        }

        Product product = productOpt.get();
        File imageFile = new File(imagePath);

        if (!imageFile.exists()) {
            throw new RuntimeException("Fichier image non trouvé: " + imagePath);
        }

        if (imageFile.length() > 5 * 1024 * 1024) {
            throw new RuntimeException("L'image est trop volumineuse (max 5MB)");
        }

        try {

            String imageUrl = CloudinaryManager.uploadImage(imagePath, product.getLibelle());
            String publicId = product.generateCloudinaryPublicId();

            product.setCloudinaryUrl(imageUrl);
            product.setCloudinaryPublicId(publicId);

            return productRepository.update(product) != null;

        } catch (Exception e) {
            throw new RuntimeException("Erreur upload image: " + e.getMessage(), e);
        }
    }

    @Override
    public boolean deleteProductImage(int productId) {
        Optional<Product> productOpt = productRepository.findById(productId);
        if (productOpt.isEmpty()) {
            return false;
        }

        Product product = productOpt.get();
        if (product.getCloudinaryPublicId() == null) {
            return true;
        }

        try {

            boolean deleted = CloudinaryManager.deleteImage(product.getCloudinaryPublicId());

            if (deleted) {

                product.setCloudinaryUrl(null);
                product.setCloudinaryPublicId(null);
                return productRepository.update(product) != null;
            }

            return false;

        } catch (Exception e) {
            throw new RuntimeException("Erreur suppression image: " + e.getMessage(), e);
        }
    }

    @Override
    public String getProductImageUrl(int productId) {
        return getProductById(productId)
                .map(Product::getCloudinaryUrl)
                .orElse(null);
    }

    @Override
    public String getProductThumbnailUrl(int productId) {
        return getProductById(productId)
                .map(Product::getThumbnailUrl)
                .orElse(null);
    }

    @Override
    public List<Product> getProductsByType(TypeProduct type) {
        return productRepository.findByType(type);
    }

    @Override
    public List<Product> getActiveProducts() {
        return productRepository.findActiveProducts();
    }

    @Override
    public List<Product> getArchivedProducts() {
        return productRepository.findArchivedProducts();
    }

    @Override
    public List<Product> searchProductsByName(String name) {
        return productRepository.findByLibelle(name);
    }

    @Override
    public List<Product> getProductsByPriceRange(double min, double max) {
        return productRepository.findByPriceRange(min, max);
    }

    @Override
    public boolean archiveProduct(int id) {
        return productRepository.archiveProduct(id);
    }

    @Override
    public boolean unarchiveProduct(int id) {
        return productRepository.unarchiveProduct(id);
    }

    @Override
    public long countProducts() {
        return productRepository.count();
    }

    @Override
    public long countProductsByType(TypeProduct type) {
        return productRepository.countByType(type);
    }

    @Override
    public long countActiveProducts() {
        return productRepository.countActiveProducts();
    }

    @Override
    public long countArchivedProducts() {
        return productRepository.countArchivedProducts();
    }

    @Override
    public List<Product> getProductsWithImages() {
        return productRepository.findProductsWithImages();
    }

    @Override
    public List<Product> getProductsWithoutImages() {
        return productRepository.findProductsWithoutImages();
    }

    @Override
    public List<Product> getMostExpensiveProducts(int limit) {
        return productRepository.findMostExpensiveProducts(limit);
    }

    @Override
    public List<Product> getCheapestProducts(int limit) {
        return productRepository.findCheapestProducts(limit);
    }

    @Override
    public List<Product> getRecentlyAddedProducts(int limit) {
        return productRepository.findRecentlyAddedProducts(limit);
    }

    @Override
    public boolean isProductAvailable(int id) {
        return getProductById(id)
                .map(Product::isAvailable)
                .orElse(false);
    }

    @Override
    public double calculateProductPriceWithTax(int productId, double taxRate) {
        Optional<Product> productOpt = getProductById(productId);
        if (productOpt.isEmpty()) {
            return 0;
        }

        Product product = productOpt.get();
        return product.getPrix() * (1 + taxRate / 100);
    }
}