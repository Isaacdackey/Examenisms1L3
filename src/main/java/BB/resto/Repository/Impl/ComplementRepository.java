package BB.resto.Repository.Impl;

import BB.resto.Entity.Enumeration.TypeProduct;
import BB.resto.Repository.Contract.IComplementRepository;
import BB.resto.Entity.Complement;
import BB.resto.Entity.Enumeration.TypeComplement;
import BB.resto.Config.Database.Database;
import java.sql.*;
import java.util.ArrayList;
import java.util.List;
import java.util.Optional;

public class ComplementRepository implements IComplementRepository {
    private Connection connection;

    public ComplementRepository() {
        try {
            this.connection = Database.getConnection();
        } catch (SQLException e) {
            throw new RuntimeException("Erreur connexion DB", e);
        }
    }

    @Override
    public Complement save(Complement complement) {
        ProductRepository ProductRepo = new ProductRepository();
        complement = (Complement) ProductRepo.save(complement);

        String sql = "INSERT INTO complement (id, type_complement) VALUES (?, ?)";

        try (PreparedStatement stmt = connection.prepareStatement(sql)) {
            stmt.setInt(1, complement.getId());
            stmt.setString(2, complement.getTypeComplement().name());
            stmt.executeUpdate();
            return complement;
        } catch (SQLException e) {
            throw new RuntimeException("Erreur sauvegarde complément: " + e.getMessage(), e);
        }
    }

    @Override
    public Optional<Complement> findById(int id) {
        String sql = "SELECT p.*, c.type_complement, c.volume, p.type_product FROM Product p " +
                "JOIN complement c ON p.id = c.id WHERE p.id = ?";

        try (PreparedStatement stmt = connection.prepareStatement(sql)) {
            stmt.setInt(1, id);
            ResultSet rs = stmt.executeQuery();

            if (rs.next()) {
                return Optional.of(mapResultSetToComplement(rs));
            }
            return Optional.empty();
        } catch (SQLException e) {
            throw new RuntimeException("Erreur recherche complément: " + e.getMessage(), e);
        }
    }

    @Override
    public List<Complement> findAll() {
        List<Complement> complements = new ArrayList<>();
        String sql = "SELECT p.*, c.type_complement FROM Product p " +
                "JOIN complement c ON p.id = c.id " +
                "WHERE p.type_Product = 'COMPLEMENT' AND p.is_archived = FALSE " +
                "ORDER BY p.libelle";

        try (Statement stmt = connection.createStatement();
             ResultSet rs = stmt.executeQuery(sql)) {

            while (rs.next()) {
                complements.add(mapResultSetToComplement(rs));
            }
            return complements;
        } catch (SQLException e) {
            throw new RuntimeException("Erreur liste compléments: " + e.getMessage(), e);
        }
    }

    @Override
    public List<Complement> findByType(TypeComplement type) {
        List<Complement> complements = new ArrayList<>();
        String sql = "SELECT p.*, c.type_complement FROM Product p " +
                "JOIN complement c ON p.id = c.id " +
                "WHERE c.type_complement = ? AND p.is_archived = FALSE " +
                "ORDER BY p.libelle";

        try (PreparedStatement stmt = connection.prepareStatement(sql)) {
            stmt.setString(1, type.name());
            ResultSet rs = stmt.executeQuery();

            while (rs.next()) {
                complements.add(mapResultSetToComplement(rs));
            }
            return complements;
        } catch (SQLException e) {
            throw new RuntimeException("Erreur recherche par type: " + e.getMessage(), e);
        }
    }

    @Override
    public List<Complement> findBoissons() {
        return findByType(TypeComplement.BOISSON);
    }

    @Override
    public List<Complement> findFrites() {
        return findByType(TypeComplement.FRITE);
    }

    @Override
    public List<Complement> findActiveComplements() {
        return findAll();
    }

    @Override
    public Complement update(Complement complement) {
        ProductRepository ProductRepo = new ProductRepository();
        complement = (Complement) ProductRepo.update(complement);

        if (complement != null) {
            String sql = "UPDATE complement SET type_complement = ? WHERE id = ?";

            try (PreparedStatement stmt = connection.prepareStatement(sql)) {
                stmt.setString(1, complement.getTypeComplement().name());
                stmt.setInt(2, complement.getId());
                stmt.executeUpdate();
                return complement;
            } catch (SQLException e) {
                throw new RuntimeException("Erreur mise à jour complément: " + e.getMessage(), e);
            }
        }
        return null;
    }

    @Override
    public boolean delete(int id) {
        String sql = "DELETE FROM complement WHERE id = ?";

        try (PreparedStatement stmt = connection.prepareStatement(sql)) {
            stmt.setInt(1, id);
            boolean deleted = stmt.executeUpdate() > 0;

            if (deleted) {
                ProductRepository ProductRepo = new ProductRepository();
                return ProductRepo.delete(id);
            }
            return false;
        } catch (SQLException e) {
            throw new RuntimeException("Erreur suppression complément: " + e.getMessage(), e);
        }
    }

    @Override
    public long count() {
        String sql = "SELECT COUNT(*) FROM complement";

        try (Statement stmt = connection.createStatement();
             ResultSet rs = stmt.executeQuery(sql)) {

            return rs.next() ? rs.getLong(1) : 0;
        } catch (SQLException e) {
            throw new RuntimeException("Erreur comptage compléments: " + e.getMessage(), e);
        }
    }

    private Complement mapResultSetToComplement(ResultSet rs) throws SQLException {
        Complement complement = new Complement();
        complement.setId(rs.getInt("id"));
        complement.setLibelle(rs.getString("libelle"));
        complement.setDescription(rs.getString("description"));
        complement.setPrix(rs.getDouble("prix"));
        complement.setTypeComplement(TypeComplement.valueOf(rs.getString("type_complement")));
        complement.setArchived(rs.getBoolean("is_archived"));
        complement.setCreatedAt(rs.getTimestamp("created_at").toLocalDateTime());
        complement.setTypeProduct(TypeProduct.valueOf(rs.getString("type_product")));
        return complement;
    }
}