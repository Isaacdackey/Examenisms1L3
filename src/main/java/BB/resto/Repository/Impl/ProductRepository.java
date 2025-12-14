package BB.resto.Repository.Impl;

import BB.resto.Repository.Contract.IProductRepository;
import BB.resto.Entity.Product;
import BB.resto.Entity.Enumeration.TypeProduct;
import BB.resto.Config.Database.Database;
import java.sql.*;
import java.util.ArrayList;
import java.util.List;
import java.util.Optional;

public class ProductRepository implements IProductRepository {
    private Connection connection;

    public ProductRepository() {
        try {
            this.connection = Database.getConnection();
        } catch (SQLException e) {
            throw new RuntimeException("Erreur connexion DB", e);
        }
    }



    @Override
    public Product save(Product product) {
        String sql = "INSERT INTO product (libelle, description, prix, cloudinary_url, " +
                "cloudinary_public_id, is_archived, type_product) " +
                "VALUES (?, ?, ?, ?, ?, ?, ?)";

        try (PreparedStatement stmt = connection.prepareStatement(sql, Statement.RETURN_GENERATED_KEYS)) {
            stmt.setString(1, product.getLibelle());
            stmt.setString(2, product.getDescription());
            stmt.setDouble(3, product.getPrix());
            stmt.setString(4, product.getCloudinaryUrl());
            stmt.setString(5, product.getCloudinaryPublicId());
            stmt.setBoolean(6, product.isArchived());
            stmt.setString(7, product.getTypeProduct().name());

            int affectedRows = stmt.executeUpdate();
            if (affectedRows > 0) {
                try (ResultSet rs = stmt.getGeneratedKeys()) {
                    if (rs.next()) {
                        product.setId(rs.getInt(1));
                    }
                }
            }
            return product;
        } catch (SQLException e) {
            throw new RuntimeException("Erreur sauvegarde produit: " + e.getMessage(), e);
        }
    }

    @Override
    public Optional<Product> findById(int id) {
        String sql = "SELECT * FROM product WHERE id = ?";

        try (PreparedStatement stmt = connection.prepareStatement(sql)) {
            stmt.setInt(1, id);
            ResultSet rs = stmt.executeQuery();

            if (rs.next()) {
                return Optional.of(mapResultSetToProduct(rs));
            }
            return Optional.empty();
        } catch (SQLException e) {
            throw new RuntimeException("Erreur recherche produit: " + e.getMessage(), e);
        }
    }

    @Override
    public List<Product> findAll() {
        List<Product> products = new ArrayList<>();
        String sql = "SELECT * FROM product ORDER BY libelle";

        try (Statement stmt = connection.createStatement();
             ResultSet rs = stmt.executeQuery(sql)) {

            while (rs.next()) {
                products.add(mapResultSetToProduct(rs));
            }
            return products;
        } catch (SQLException e) {
            throw new RuntimeException("Erreur liste produits: " + e.getMessage(), e);
        }
    }

    @Override
    public Product update(Product product) {
        String sql = "UPDATE product SET libelle = ?, description = ?, prix = ?, " +
                "cloudinary_url = ?, cloudinary_public_id = ?, " +
                "is_archived = ?, type_product = ?, updated_at = CURRENT_TIMESTAMP " +
                "WHERE id = ?";

        try (PreparedStatement stmt = connection.prepareStatement(sql)) {
            stmt.setString(1, product.getLibelle());
            stmt.setString(2, product.getDescription());
            stmt.setDouble(3, product.getPrix());
            stmt.setString(4, product.getCloudinaryUrl());
            stmt.setString(5, product.getCloudinaryPublicId());
            stmt.setBoolean(6, product.isArchived());
            stmt.setString(7, product.getTypeProduct().name());
            stmt.setInt(8, product.getId());

            return stmt.executeUpdate() > 0 ? product : null;
        } catch (SQLException e) {
            throw new RuntimeException("Erreur mise à jour produit: " + e.getMessage(), e);
        }
    }

    @Override

    public boolean delete(int id) {

        String sql = "UPDATE product SET is_archived = TRUE WHERE id = ?";

        try (PreparedStatement stmt = connection.prepareStatement(sql)) {
            stmt.setInt(1, id);
            int rowsUpdated = stmt.executeUpdate();
            System.out.println("🔧 DEBUG: Archivage produit ID " + id + " - " + rowsUpdated + " ligne(s) mise(s) à jour");
            return rowsUpdated > 0;
        } catch (SQLException e) {
            throw new RuntimeException("Erreur archivage produit: " + e.getMessage(), e);
        }
    }

    @Override
    public boolean unarchive(int id) {
        String sql = "UPDATE product SET is_archived = FALSE WHERE id = ?";

        try (PreparedStatement stmt = connection.prepareStatement(sql)) {
            stmt.setInt(1, id);
            return stmt.executeUpdate() > 0;
        } catch (SQLException e) {
            throw new RuntimeException("Erreur désarchivage produit: " + e.getMessage(), e);
        }
    }

    @Override
    public long count() {
        String sql = "SELECT COUNT(*) FROM product";

        try (Statement stmt = connection.createStatement();
             ResultSet rs = stmt.executeQuery(sql)) {

            return rs.next() ? rs.getLong(1) : 0;
        } catch (SQLException e) {
            throw new RuntimeException("Erreur comptage produits: " + e.getMessage(), e);
        }
    }



    @Override
    public List<Product> findActiveProducts() {
        List<Product> products = new ArrayList<>();
        String sql = "SELECT * FROM product WHERE is_archived = FALSE ORDER BY libelle";

        try (Statement stmt = connection.createStatement();
             ResultSet rs = stmt.executeQuery(sql)) {

            while (rs.next()) {
                products.add(mapResultSetToProduct(rs));
            }
            return products;
        } catch (SQLException e) {
            throw new RuntimeException("Erreur liste produits actifs: " + e.getMessage(), e);
        }
    }

    @Override
    public List<Product> findArchivedProducts() {
        List<Product> products = new ArrayList<>();
        String sql = "SELECT * FROM product WHERE is_archived = TRUE ORDER BY libelle";

        try (Statement stmt = connection.createStatement();
             ResultSet rs = stmt.executeQuery(sql)) {

            while (rs.next()) {
                products.add(mapResultSetToProduct(rs));
            }
            return products;
        } catch (SQLException e) {
            throw new RuntimeException("Erreur liste produits archivés: " + e.getMessage(), e);
        }
    }

    @Override
    public List<Product> findByType(TypeProduct type) {
        List<Product> products = new ArrayList<>();
        String sql = "SELECT * FROM product WHERE type_product = ? AND is_archived = FALSE ORDER BY libelle";

        try (PreparedStatement stmt = connection.prepareStatement(sql)) {
            stmt.setString(1, type.name());
            ResultSet rs = stmt.executeQuery();

            while (rs.next()) {
                products.add(mapResultSetToProduct(rs));
            }
            return products;
        } catch (SQLException e) {
            throw new RuntimeException("Erreur recherche par type: " + e.getMessage(), e);
        }
    }

    @Override
    public List<Product> findByLibelle(String libelle) {
        List<Product> products = new ArrayList<>();
        String sql = "SELECT * FROM product WHERE libelle ILIKE ? AND is_archived = FALSE ORDER BY libelle";

        try (PreparedStatement stmt = connection.prepareStatement(sql)) {
            stmt.setString(1, "%" + libelle + "%");
            ResultSet rs = stmt.executeQuery();

            while (rs.next()) {
                products.add(mapResultSetToProduct(rs));
            }
            return products;
        } catch (SQLException e) {
            throw new RuntimeException("Erreur recherche par libellé: " + e.getMessage(), e);
        }
    }

    @Override
    public List<Product> findByPriceRange(double min, double max) {
        List<Product> products = new ArrayList<>();
        String sql = "SELECT * FROM product WHERE prix BETWEEN ? AND ? AND is_archived = FALSE ORDER BY prix";

        try (PreparedStatement stmt = connection.prepareStatement(sql)) {
            stmt.setDouble(1, min);
            stmt.setDouble(2, max);
            ResultSet rs = stmt.executeQuery();

            while (rs.next()) {
                products.add(mapResultSetToProduct(rs));
            }
            return products;
        } catch (SQLException e) {
            throw new RuntimeException("Erreur recherche par prix: " + e.getMessage(), e);
        }
    }

    @Override
    public boolean archiveProduct(int id) {
        String sql = "UPDATE product SET is_archived = TRUE, updated_at = CURRENT_TIMESTAMP WHERE id = ?";

        try (PreparedStatement stmt = connection.prepareStatement(sql)) {
            stmt.setInt(1, id);
            return stmt.executeUpdate() > 0;
        } catch (SQLException e) {
            throw new RuntimeException("Erreur archivage produit: " + e.getMessage(), e);
        }
    }

    @Override
    public boolean unarchiveProduct(int id) {
        String sql = "UPDATE product SET is_archived = FALSE, updated_at = CURRENT_TIMESTAMP WHERE id = ?";

        try (PreparedStatement stmt = connection.prepareStatement(sql)) {
            stmt.setInt(1, id);
            return stmt.executeUpdate() > 0;
        } catch (SQLException e) {
            throw new RuntimeException("Erreur désarchivage produit: " + e.getMessage(), e);
        }
    }

    @Override
    public boolean updateCloudinaryImage(int productId, String cloudinaryUrl, String publicId) {
        String sql = "UPDATE product SET cloudinary_url = ?, cloudinary_public_id = ?, " +
                "updated_at = CURRENT_TIMESTAMP WHERE id = ?";

        try (PreparedStatement stmt = connection.prepareStatement(sql)) {
            stmt.setString(1, cloudinaryUrl);
            stmt.setString(2, publicId);
            stmt.setInt(3, productId);

            return stmt.executeUpdate() > 0;
        } catch (SQLException e) {
            throw new RuntimeException("Erreur mise à jour image: " + e.getMessage(), e);
        }
    }

    @Override
    public long countByType(TypeProduct type) {
        String sql = "SELECT COUNT(*) FROM product WHERE type_product = ?";

        try (PreparedStatement stmt = connection.prepareStatement(sql)) {
            stmt.setString(1, type.name());
            ResultSet rs = stmt.executeQuery();

            return rs.next() ? rs.getLong(1) : 0;
        } catch (SQLException e) {
            throw new RuntimeException("Erreur comptage par type: " + e.getMessage(), e);
        }
    }

    @Override
    public long countActiveProducts() {
        String sql = "SELECT COUNT(*) FROM product WHERE is_archived = FALSE";

        try (Statement stmt = connection.createStatement();
             ResultSet rs = stmt.executeQuery(sql)) {

            return rs.next() ? rs.getLong(1) : 0;
        } catch (SQLException e) {
            throw new RuntimeException("Erreur comptage produits actifs: " + e.getMessage(), e);
        }
    }

    @Override
    public long countArchivedProducts() {
        String sql = "SELECT COUNT(*) FROM product WHERE is_archived = TRUE";

        try (Statement stmt = connection.createStatement();
             ResultSet rs = stmt.executeQuery(sql)) {

            return rs.next() ? rs.getLong(1) : 0;
        } catch (SQLException e) {
            throw new RuntimeException("Erreur comptage produits archivés: " + e.getMessage(), e);
        }
    }

    @Override
    public List<Product> searchProducts(String keyword, TypeProduct type, Double minPrice, Double maxPrice) {
        List<Product> products = new ArrayList<>();
        StringBuilder sql = new StringBuilder("SELECT * FROM product WHERE is_archived = FALSE");
        List<Object> params = new ArrayList<>();

        if (keyword != null && !keyword.trim().isEmpty()) {
            sql.append(" AND (libelle ILIKE ? OR description ILIKE ?)");
            params.add("%" + keyword + "%");
            params.add("%" + keyword + "%");
        }

        if (type != null) {
            sql.append(" AND type_product = ?");
            params.add(type.name());
        }

        if (minPrice != null) {
            sql.append(" AND prix >= ?");
            params.add(minPrice);
        }

        if (maxPrice != null) {
            sql.append(" AND prix <= ?");
            params.add(maxPrice);
        }

        sql.append(" ORDER BY libelle");

        try (PreparedStatement stmt = connection.prepareStatement(sql.toString())) {
            for (int i = 0; i < params.size(); i++) {
                stmt.setObject(i + 1, params.get(i));
            }

            ResultSet rs = stmt.executeQuery();
            while (rs.next()) {
                products.add(mapResultSetToProduct(rs));
            }
            return products;
        } catch (SQLException e) {
            throw new RuntimeException("Erreur recherche avancée: " + e.getMessage(), e);
        }
    }

    @Override
    public List<Product> findProductsWithImages() {
        List<Product> products = new ArrayList<>();
        String sql = "SELECT * FROM product WHERE cloudinary_url IS NOT NULL AND is_archived = FALSE ORDER BY libelle";

        try (Statement stmt = connection.createStatement();
             ResultSet rs = stmt.executeQuery(sql)) {

            while (rs.next()) {
                products.add(mapResultSetToProduct(rs));
            }
            return products;
        } catch (SQLException e) {
            throw new RuntimeException("Erreur liste produits avec images: " + e.getMessage(), e);
        }
    }

    @Override
    public List<Product> findProductsWithoutImages() {
        List<Product> products = new ArrayList<>();
        String sql = "SELECT * FROM product WHERE cloudinary_url IS NULL AND is_archived = FALSE ORDER BY libelle";

        try (Statement stmt = connection.createStatement();
             ResultSet rs = stmt.executeQuery(sql)) {

            while (rs.next()) {
                products.add(mapResultSetToProduct(rs));
            }
            return products;
        } catch (SQLException e) {
            throw new RuntimeException("Erreur liste produits sans images: " + e.getMessage(), e);
        }
    }

    @Override
    public List<Product> findMostExpensiveProducts(int limit) {
        List<Product> products = new ArrayList<>();
        String sql = "SELECT * FROM product WHERE is_archived = FALSE ORDER BY prix DESC LIMIT ?";

        try (PreparedStatement stmt = connection.prepareStatement(sql)) {
            stmt.setInt(1, limit);
            ResultSet rs = stmt.executeQuery();

            while (rs.next()) {
                products.add(mapResultSetToProduct(rs));
            }
            return products;
        } catch (SQLException e) {
            throw new RuntimeException("Erreur liste produits chers: " + e.getMessage(), e);
        }
    }

    @Override
    public List<Product> findCheapestProducts(int limit) {
        List<Product> products = new ArrayList<>();
        String sql = "SELECT * FROM product WHERE is_archived = FALSE ORDER BY prix ASC LIMIT ?";

        try (PreparedStatement stmt = connection.prepareStatement(sql)) {
            stmt.setInt(1, limit);
            ResultSet rs = stmt.executeQuery();

            while (rs.next()) {
                products.add(mapResultSetToProduct(rs));
            }
            return products;
        } catch (SQLException e) {
            throw new RuntimeException("Erreur liste produits moins chers: " + e.getMessage(), e);
        }
    }

    @Override
    public List<Product> findRecentlyAddedProducts(int limit) {
        List<Product> products = new ArrayList<>();
        String sql = "SELECT * FROM product WHERE is_archived = FALSE ORDER BY created_at DESC LIMIT ?";

        try (PreparedStatement stmt = connection.prepareStatement(sql)) {
            stmt.setInt(1, limit);
            ResultSet rs = stmt.executeQuery();

            while (rs.next()) {
                products.add(mapResultSetToProduct(rs));
            }
            return products;
        } catch (SQLException e) {
            throw new RuntimeException("Erreur liste produits récents: " + e.getMessage(), e);
        }
    }

    @Override
    public List<Product> findRecentlyUpdatedProducts(int limit) {
        List<Product> products = new ArrayList<>();
        String sql = "SELECT * FROM product WHERE is_archived = FALSE ORDER BY updated_at DESC LIMIT ?";

        try (PreparedStatement stmt = connection.prepareStatement(sql)) {
            stmt.setInt(1, limit);
            ResultSet rs = stmt.executeQuery();

            while (rs.next()) {
                products.add(mapResultSetToProduct(rs));
            }
            return products;
        } catch (SQLException e) {
            throw new RuntimeException("Erreur liste produits mis à jour: " + e.getMessage(), e);
        }
    }



    private Product mapResultSetToProduct(ResultSet rs) throws SQLException {
        Product product = new Product();
        product.setId(rs.getInt("id"));
        product.setLibelle(rs.getString("libelle"));
        product.setDescription(rs.getString("description"));
        product.setPrix(rs.getDouble("prix"));
        product.setCloudinaryUrl(rs.getString("cloudinary_url"));
        product.setCloudinaryPublicId(rs.getString("cloudinary_public_id"));
        product.setArchived(rs.getBoolean("is_archived"));
        product.setTypeProduct(TypeProduct.valueOf(rs.getString("type_product")));
        product.setCreatedAt(rs.getTimestamp("created_at").toLocalDateTime());

        Timestamp updated = rs.getTimestamp("updated_at");
        if (updated != null) {
            product.setUpdatedAt(updated.toLocalDateTime());
        }

        return product;
    }
}