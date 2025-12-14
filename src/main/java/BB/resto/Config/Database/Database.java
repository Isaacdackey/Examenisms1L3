package BB.resto.Config.Database;

import java.sql.Connection;
import java.sql.DriverManager;
import java.sql.SQLException;

public class Database {

    private static final String URL = "jdbc:postgresql://ep-green-flower-a41njh0k-pooler.us-east-1.aws.neon.tech/neondb";
    private static final String USER = "neondb_owner";
    private static final String PASSWORD = "npg_ZSfnwt30JeKI";

    static {
        try {

            Class.forName("org.postgresql.Driver");
            System.out.println("✅ Driver PostgreSQL chargé");
        } catch (ClassNotFoundException e) {
            throw new RuntimeException("❌ Driver PostgreSQL non trouvé. Ajoutez la dépendance dans pom.xml", e);
        }
    }


    public static Connection getConnection() throws SQLException {

        String fullUrl = URL + "?sslmode=require&channel_binding=require";

        System.out.println("🔗 Tentative de connexion à Neon...");
        Connection conn = DriverManager.getConnection(fullUrl, USER, PASSWORD);


        conn.setAutoCommit(true);

        return conn;
    }


    public static boolean testConnection() {
        try (Connection conn = getConnection()) {
            return conn != null && !conn.isClosed();
        } catch (SQLException e) {
            System.err.println("❌ Test connexion échoué: " + e.getMessage());
            return false;
        }
    }


    public static void printConnectionInfo() throws SQLException {
        try (Connection conn = getConnection()) {
            System.out.println("=== INFORMATIONS CONNEXION ===");
            System.out.println("Base: " + conn.getCatalog());
            System.out.println("URL: " + conn.getMetaData().getURL());
            System.out.println("User: " + conn.getMetaData().getUserName());
            System.out.println("Driver: " + conn.getMetaData().getDriverName());
            System.out.println("Version DB: " + conn.getMetaData().getDatabaseProductVersion());
            System.out.println("Version Driver: " + conn.getMetaData().getDriverVersion());
            System.out.println("=============================");
        }
    }
}