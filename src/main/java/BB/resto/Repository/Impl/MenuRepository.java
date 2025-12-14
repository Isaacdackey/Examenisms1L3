package BB.resto.Repository.Impl;

import BB.resto.Repository.Contract.IMenuRepository;
import BB.resto.Entity.Menu;
import BB.resto.Entity.Product;
import BB.resto.Config.Database.Database;
import BB.resto.Repository.Impl.ProductRepository;
import java.sql.*;
import java.util.ArrayList;
import java.util.List;
import java.util.Optional;

public class MenuRepository implements IMenuRepository {
    private Connection connection;

    public MenuRepository() {
        try {
            this.connection = Database.getConnection();
        } catch (SQLException e) {
            throw new RuntimeException("Erreur connexion DB", e);
        }
    }

    @Override
    public Menu save(Menu menu) {
        double prixMenu = calculateMenuPrice(menu.getBurgerId(), menu.getBoissonId(), menu.getFritesId());
        menu.setPrix(prixMenu);

        ProductRepository productRepo = new ProductRepository();
        menu = (Menu) productRepo.save(menu);

        String sql = "INSERT INTO menu (id, burger_id, boisson_id, frites_id) VALUES (?, ?, ?, ?)";

        try (PreparedStatement stmt = connection.prepareStatement(sql)) {
            stmt.setInt(1, menu.getId());
            stmt.setInt(2, menu.getBurgerId());
            stmt.setInt(3, menu.getBoissonId());
            stmt.setInt(4, menu.getFritesId());
            stmt.executeUpdate();
            return menu;
        } catch (SQLException e) {
            throw new RuntimeException("Erreur sauvegarde menu: " + e.getMessage(), e);
        }
    }

    @Override
    public Optional<Menu> findById(int id) {
        String sql = "SELECT p.*, m.burger_id, m.boisson_id, m.frites_id FROM product p " +
                "JOIN menu m ON p.id = m.id WHERE p.id = ?";

        try (PreparedStatement stmt = connection.prepareStatement(sql)) {
            stmt.setInt(1, id);
            ResultSet rs = stmt.executeQuery();

            if (rs.next()) {
                return Optional.of(mapResultSetToMenu(rs));
            }
            return Optional.empty();
        } catch (SQLException e) {
            throw new RuntimeException("Erreur recherche menu: " + e.getMessage(), e);
        }
    }

    @Override
    public Optional<Menu> findCompleteMenu(int id) {
        return findById(id);
    }

    @Override
    public List<Menu> findAll() {
        List<Menu> menus = new ArrayList<>();
        String sql = "SELECT p.*, m.burger_id, m.boisson_id, m.frites_id FROM product p " +
                "JOIN menu m ON p.id = m.id " +
                "WHERE p.type_product = 'MENU' AND p.is_archived = FALSE " +
                "ORDER BY p.libelle";

        try (Statement stmt = connection.createStatement();
             ResultSet rs = stmt.executeQuery(sql)) {

            while (rs.next()) {
                menus.add(mapResultSetToMenu(rs));
            }
            return menus;
        } catch (SQLException e) {
            throw new RuntimeException("Erreur liste menus: " + e.getMessage(), e);
        }
    }

    @Override
    public List<Menu> findActiveMenus() {
        return findAll();
    }

    @Override
    public double calculateMenuPrice(int burgerId, int boissonId, int fritesId) {

        double total = 0;

        ProductRepository productRepo = new ProductRepository();

        Optional<Product> burgerOpt = productRepo.findById(burgerId);
        Optional<Product> boissonOpt = productRepo.findById(boissonId);
        Optional<Product> fritesOpt = productRepo.findById(fritesId);

        if (burgerOpt.isPresent()) {
            total += burgerOpt.get().getPrix();
        }
        if (boissonOpt.isPresent()) {
            total += boissonOpt.get().getPrix();
        }
        if (fritesOpt.isPresent()) {
            total += fritesOpt.get().getPrix();
        }

        return total * 0.90;
    }

    @Override
    public Menu update(Menu menu) {
        double prixMenu = calculateMenuPrice(menu.getBurgerId(), menu.getBoissonId(), menu.getFritesId());
        menu.setPrix(prixMenu);

        ProductRepository productRepo = new ProductRepository();
        menu = (Menu) productRepo.update(menu);

        if (menu != null) {
            String sql = "UPDATE menu SET burger_id = ?, boisson_id = ?, frites_id = ? WHERE id = ?";

            try (PreparedStatement stmt = connection.prepareStatement(sql)) {
                stmt.setInt(1, menu.getBurgerId());
                stmt.setInt(2, menu.getBoissonId());
                stmt.setInt(3, menu.getFritesId());
                stmt.setInt(4, menu.getId());
                stmt.executeUpdate();
                return menu;
            } catch (SQLException e) {
                throw new RuntimeException("Erreur mise à jour menu: " + e.getMessage(), e);
            }
        }
        return null;
    }

    @Override
    public boolean delete(int id) {
        String sql = "DELETE FROM menu WHERE id = ?";

        try (PreparedStatement stmt = connection.prepareStatement(sql)) {
            stmt.setInt(1, id);
            boolean deleted = stmt.executeUpdate() > 0;

            if (deleted) {
                ProductRepository productRepo = new ProductRepository();
                return productRepo.delete(id);
            }
            return false;
        } catch (SQLException e) {
            throw new RuntimeException("Erreur suppression menu: " + e.getMessage(), e);
        }
    }

    @Override
    public long count() {
        String sql = "SELECT COUNT(*) FROM menu";

        try (Statement stmt = connection.createStatement();
             ResultSet rs = stmt.executeQuery(sql)) {

            return rs.next() ? rs.getLong(1) : 0;
        } catch (SQLException e) {
            throw new RuntimeException("Erreur comptage menus: " + e.getMessage(), e);
        }
    }

    private Menu mapResultSetToMenu(ResultSet rs) throws SQLException {
        Menu menu = new Menu();
        menu.setId(rs.getInt("id"));
        menu.setLibelle(rs.getString("libelle"));
        menu.setDescription(rs.getString("description"));
        menu.setPrix(rs.getDouble("prix"));
        menu.setBurgerId(rs.getInt("burger_id"));
        menu.setBoissonId(rs.getInt("boisson_id"));
        menu.setFritesId(rs.getInt("frites_id"));
        menu.setArchived(rs.getBoolean("is_archived"));
        menu.setCreatedAt(rs.getTimestamp("created_at").toLocalDateTime());
        return menu;
    }
}