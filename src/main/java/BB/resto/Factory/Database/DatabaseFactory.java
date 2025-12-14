package BB.resto.Factory.Database;

import BB.resto.Config.Database.Database;
import java.sql.Connection;
import java.sql.SQLException;

public class DatabaseFactory {

    private static DatabaseFactory instance;
    private Connection connection;


    private DatabaseFactory() {

    }

    public static synchronized DatabaseFactory getInstance() {
        if (instance == null) {
            instance = new DatabaseFactory();
        }
        return instance;
    }


    public Connection getConnection() throws SQLException {
        if (connection == null || connection.isClosed()) {
            connection = Database.getConnection();
        }
        return connection;
    }


    public void beginTransaction() throws SQLException {
        Connection conn = getConnection();
        conn.setAutoCommit(false);
        System.out.println("🚀 Transaction démarrée");
    }


    public void commitTransaction() throws SQLException {
        Connection conn = getConnection();
        conn.commit();
        conn.setAutoCommit(true);
        System.out.println("✅ Transaction validée");
    }


    public void rollbackTransaction() {
        try {
            Connection conn = getConnection();
            conn.rollback();
            conn.setAutoCommit(true);
            System.out.println("↩️ Transaction annulée");
        } catch (SQLException e) {
            System.err.println("❌ Erreur rollback: " + e.getMessage());
        }
    }


    public String getConnectionStatus() {
        try {
            Connection conn = getConnection();
            if (conn == null) return "❌ Non connecté";
            if (conn.isClosed()) return "❌ Fermé";
            if (conn.isValid(2)) return "✅ Connecté et valide";
            return "⚠️ Connecté mais problème détecté";
        } catch (SQLException e) {
            return "❌ Erreur: " + e.getMessage();
        }
    }


    public void displayDatabaseInfo() {
        try {
            Connection conn = getConnection();
            var metaData = conn.getMetaData();

            System.out.println("\n=== INFORMATIONS BASE DE DONNÉES ===");
            System.out.println("📊 SGBD: " + metaData.getDatabaseProductName());
            System.out.println("📈 Version: " + metaData.getDatabaseProductVersion());
            System.out.println("👤 Utilisateur: " + metaData.getUserName());
            System.out.println("🔗 URL: " + metaData.getURL());
            System.out.println("🚀 Driver: " + metaData.getDriverName() + " v" + metaData.getDriverVersion());
            System.out.println("📁 Tables disponibles: " + getTableCount());

        } catch (SQLException e) {
            System.err.println("❌ Impossible d'afficher les infos BD: " + e.getMessage());
        }
    }


    private int getTableCount() throws SQLException {
        Connection conn = getConnection();
        var stmt = conn.createStatement();
        var rs = stmt.executeQuery(
                "SELECT COUNT(*) FROM information_schema.tables " +
                        "WHERE table_schema = DATABASE()"
        );
        return rs.next() ? rs.getInt(1) : 0;
    }


    public void shutdown() {
        try {
            if (connection != null && !connection.isClosed()) {
                connection.close();
                System.out.println("🔌 Connexion fermée");
            }
        } catch (SQLException e) {
            System.err.println("❌ Erreur fermeture connexion: " + e.getMessage());
        }
    }
}