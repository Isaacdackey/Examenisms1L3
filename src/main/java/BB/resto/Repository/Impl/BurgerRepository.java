package BB.resto.Repository.Impl;

import BB.resto.Entity.Enumeration.TypeProduct;
import BB.resto.Repository.Contract.IBurgerRepository;
import BB.resto.Entity.Burger;
import BB.resto.Entity.Enumeration.TypeBurger;
import BB.resto.Config.Database.Database;
import java.sql.*;
import java.util.ArrayList;
import java.util.List;
import java.util.Optional;

public class BurgerRepository implements IBurgerRepository {
    private Connection connection;

    public BurgerRepository() {
        try {
            this.connection = Database.getConnection();
        } catch (SQLException e) {
            throw new RuntimeException("Erreur connexion DB", e);
        }
    }

    @Override
    public Burger save(Burger burger) {

        ProductRepository ProductRepo = new ProductRepository();
        burger = (Burger) ProductRepo.save(burger);

        String sql = "INSERT INTO burger (id, type_burger) VALUES (?, ?)";

        try (PreparedStatement stmt = connection.prepareStatement(sql)) {
            stmt.setInt(1, burger.getId());
            stmt.setString(2, burger.getTypeBurger().name());
            stmt.executeUpdate();
            return burger;
        } catch (SQLException e) {
            throw new RuntimeException("Erreur sauvegarde burger: " + e.getMessage(), e);
        }
    }

    @Override
    public Optional<Burger> findById(int id) {
        String sql = "SELECT p.*, b.type_burger FROM Product p " +
                "JOIN burger b ON p.id = b.id WHERE p.id = ?";

        try (PreparedStatement stmt = connection.prepareStatement(sql)) {
            stmt.setInt(1, id);
            ResultSet rs = stmt.executeQuery();

            if (rs.next()) {
                return Optional.of(mapResultSetToBurger(rs));
            }
            return Optional.empty();
        } catch (SQLException e) {
            throw new RuntimeException("Erreur recherche burger: " + e.getMessage(), e);
        }
    }

    @Override
    public List<Burger> findAll() {
        List<Burger> burgers = new ArrayList<>();
        String sql = "SELECT p.*, b.type_burger FROM Product p " +
                "JOIN burger b ON p.id = b.id " +
                "WHERE p.type_Product = 'BURGER' AND p.is_archived = FALSE " +
                "ORDER BY p.libelle";

        try (Statement stmt = connection.createStatement();
             ResultSet rs = stmt.executeQuery(sql)) {

            while (rs.next()) {
                burgers.add(mapResultSetToBurger(rs));
            }
            return burgers;
        } catch (SQLException e) {
            throw new RuntimeException("Erreur liste burgers: " + e.getMessage(), e);
        }
    }

    @Override
    public List<Burger> findByType(TypeBurger type) {
        List<Burger> burgers = new ArrayList<>();
        String sql = "SELECT p.*, b.type_burger FROM Product p " +
                "JOIN burger b ON p.id = b.id " +
                "WHERE b.type_burger = ? AND p.is_archived = FALSE " +
                "ORDER BY p.libelle";

        try (PreparedStatement stmt = connection.prepareStatement(sql)) {
            stmt.setString(1, type.name());
            ResultSet rs = stmt.executeQuery();

            while (rs.next()) {
                burgers.add(mapResultSetToBurger(rs));
            }
            return burgers;
        } catch (SQLException e) {
            throw new RuntimeException("Erreur recherche par type: " + e.getMessage(), e);
        }
    }

    @Override
    public List<Burger> findActiveBurgers() {
        return findAll();
    }

    @Override
    public List<Burger> findByCaloriesMax(int maxCalories) {
        List<Burger> burgers = new ArrayList<>();
        String sql = "SELECT p.*, b.type_burger FROM Product p " +
                "JOIN burger b ON p.id = b.id " +
                "WHERE b.calories <= ? AND p.is_archived = FALSE " +
                "ORDER BY b.calories";

        try (PreparedStatement stmt = connection.prepareStatement(sql)) {
            stmt.setInt(1, maxCalories);
            ResultSet rs = stmt.executeQuery();

            while (rs.next()) {
                burgers.add(mapResultSetToBurger(rs));
            }
            return burgers;
        } catch (SQLException e) {
            throw new RuntimeException("Erreur recherche par calories: " + e.getMessage(), e);
        }
    }

    @Override
    public List<Burger> findByPreparationTimeMax(int maxTime) {
        List<Burger> burgers = new ArrayList<>();
        String sql = "SELECT p.*, b.type_burger FROM Product p " +
                "JOIN burger b ON p.id = b.id " +
                "WHERE b.temps_preparation <= ? AND p.is_archived = FALSE " +
                "ORDER BY b.temps_preparation";

        try (PreparedStatement stmt = connection.prepareStatement(sql)) {
            stmt.setInt(1, maxTime);
            ResultSet rs = stmt.executeQuery();

            while (rs.next()) {
                burgers.add(mapResultSetToBurger(rs));
            }
            return burgers;
        } catch (SQLException e) {
            throw new RuntimeException("Erreur recherche par temps: " + e.getMessage(), e);
        }
    }

    @Override
    public Burger update(Burger burger) {

        ProductRepository ProductRepo = new ProductRepository();
        burger = (Burger) ProductRepo.update(burger);

        if (burger != null) {
            String sql = "UPDATE burger SET type_burger = ? WHERE id = ?";

            try (PreparedStatement stmt = connection.prepareStatement(sql)) {
                stmt.setString(1, burger.getTypeBurger().name());
                stmt.setInt(2, burger.getId());
                stmt.executeUpdate();
                return burger;
            } catch (SQLException e) {
                throw new RuntimeException("Erreur mise à jour burger: " + e.getMessage(), e);
            }
        }
        return null;
    }


    @Override
    public boolean delete(int id) {


        ProductRepository productRepo = new ProductRepository();
        boolean archived = productRepo.delete(id);

        if (archived) {
            System.out.println("✅ Burger ID " + id + " archivé (is_archived = TRUE)");
        } else {
            System.out.println("❌ Échec archivage burger ID " + id);
        }

        return archived;
    }

    @Override
    public long count() {
        String sql = "SELECT COUNT(*) FROM burger";

        try (Statement stmt = connection.createStatement();
             ResultSet rs = stmt.executeQuery(sql)) {

            return rs.next() ? rs.getLong(1) : 0;
        } catch (SQLException e) {
            throw new RuntimeException("Erreur comptage burgers: " + e.getMessage(), e);
        }
    }

    private Burger mapResultSetToBurger(ResultSet rs) throws SQLException {
        Burger burger = new Burger();
        burger.setId(rs.getInt("id"));
        burger.setLibelle(rs.getString("libelle"));
        burger.setDescription(rs.getString("description"));
        burger.setPrix(rs.getDouble("prix"));
        burger.setTypeBurger(TypeBurger.valueOf(rs.getString("type_burger")));
        burger.setArchived(rs.getBoolean("is_archived"));
        burger.setCreatedAt(rs.getTimestamp("created_at").toLocalDateTime());
        burger.setTypeProduct(TypeProduct.valueOf(rs.getString("type_product")));
        return burger;
    }
}