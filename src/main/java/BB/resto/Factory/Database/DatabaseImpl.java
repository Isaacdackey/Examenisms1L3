package BB.resto.Factory.Database;

import java.sql.*;
import java.time.LocalDate;
import java.time.LocalDateTime;
import java.util.ArrayList;
import java.util.List;
import java.util.function.Function;

public class DatabaseImpl implements IDatabase {

    private DatabaseFactory factory;

    public DatabaseImpl() {
        this.factory = DatabaseFactory.getInstance();
    }

    @Override
    public <T> T executeQuery(String sql, Function<ResultSet, T> resultMapper, Object... params) {
        try (Connection conn = factory.getConnection();
             PreparedStatement stmt = conn.prepareStatement(sql)) {

            setParameters(stmt, params);
            ResultSet rs = stmt.executeQuery();
            return resultMapper.apply(rs);

        } catch (SQLException e) {
            throw new RuntimeException("Erreur requête: " + e.getMessage(), e);
        }
    }

    @Override
    public <T> List<T> executeQueryList(String sql, Function<ResultSet, T> rowMapper, Object... params) {
        return executeQuery(sql, rs -> {
            List<T> list = new ArrayList<>();
            try {
                while (rs.next()) {
                    list.add(rowMapper.apply(rs));
                }
            } catch (SQLException e) {
                throw new RuntimeException("Erreur mapping résultat", e);
            }
            return list;
        }, params);
    }

    @Override
    public int executeUpdate(String sql, Object... params) {
        try (Connection conn = factory.getConnection();
             PreparedStatement stmt = conn.prepareStatement(sql, Statement.RETURN_GENERATED_KEYS)) {

            setParameters(stmt, params);
            int affectedRows = stmt.executeUpdate();


            if (affectedRows > 0 && sql.trim().toUpperCase().startsWith("INSERT")) {
                ResultSet rs = stmt.getGeneratedKeys();
                if (rs.next()) {
                    return rs.getInt(1);
                }
            }

            return affectedRows;

        } catch (SQLException e) {
            throw new RuntimeException("Erreur mise à jour: " + e.getMessage(), e);
        }
    }

    @Override
    public boolean execute(String sql, Object... params) {
        try (Connection conn = factory.getConnection();
             PreparedStatement stmt = conn.prepareStatement(sql)) {

            setParameters(stmt, params);
            return stmt.execute();

        } catch (SQLException e) {
            throw new RuntimeException("Erreur exécution: " + e.getMessage(), e);
        }
    }

    @Override
    public void executeBatch(List<String> sqlStatements) {
        try (Connection conn = factory.getConnection();
             Statement stmt = conn.createStatement()) {

            conn.setAutoCommit(false);
            for (String sql : sqlStatements) {
                stmt.addBatch(sql);
            }
            stmt.executeBatch();
            conn.commit();

        } catch (SQLException e) {
            factory.rollbackTransaction();
            throw new RuntimeException("Erreur batch: " + e.getMessage(), e);
        }
    }

    @Override
    public int count(String tableName, String condition, Object... params) {
        String sql = "SELECT COUNT(*) FROM " + tableName;
        if (condition != null && !condition.trim().isEmpty()) {
            sql += " WHERE " + condition;
        }

        return executeQuery(sql, rs -> {
            try {
                return rs.next() ? rs.getInt(1) : 0;
            } catch (SQLException e) {
                throw new RuntimeException("Erreur comptage", e);
            }
        }, params);
    }

    @Override
    public boolean exists(String tableName, String condition, Object... params) {
        return count(tableName, condition, params) > 0;
    }

    @Override
    public double sum(String tableName, String column, String condition, Object... params) {
        String sql = "SELECT COALESCE(SUM(" + column + "), 0) FROM " + tableName;
        if (condition != null && !condition.trim().isEmpty()) {
            sql += " WHERE " + condition;
        }

        return executeQuery(sql, rs -> {
            try {
                return rs.next() ? rs.getDouble(1) : 0;
            } catch (SQLException e) {
                throw new RuntimeException("Erreur somme", e);
            }
        }, params);
    }

    @Override
    public void createTable(String sql) {
        execute(sql);
    }

    @Override
    public void dropTable(String tableName) {
        execute("DROP TABLE IF EXISTS " + tableName);
    }

    @Override
    public void truncateTable(String tableName) {
        execute("TRUNCATE TABLE " + tableName);
    }

    @Override
    public List<String> getTableNames() {
        String sql = "SELECT table_name FROM information_schema.tables " +
                "WHERE table_schema = DATABASE() ORDER BY table_name";

        return executeQueryList(sql, rs -> {
            try {
                return rs.getString("table_name");
            } catch (SQLException e) {
                throw new RuntimeException("Erreur récupération tables", e);
            }
        });
    }

    @Override
    public void testConnection() {
        try {
            Connection conn = factory.getConnection();
            if (conn.isValid(5)) {
                System.out.println("✅ Test de connexion réussi !");
            }
        } catch (SQLException e) {
            throw new RuntimeException("Test connexion échoué", e);
        }
    }


    private void setParameters(PreparedStatement stmt, Object... params) throws SQLException {
        if (params != null) {
            for (int i = 0; i < params.length; i++) {
                Object param = params[i];
                int paramIndex = i + 1;

                if (param == null) {
                    stmt.setNull(paramIndex, Types.NULL);
                } else if (param instanceof Integer) {
                    stmt.setInt(paramIndex, (Integer) param);
                } else if (param instanceof Double) {
                    stmt.setDouble(paramIndex, (Double) param);
                } else if (param instanceof String) {
                    stmt.setString(paramIndex, (String) param);
                } else if (param instanceof Boolean) {
                    stmt.setBoolean(paramIndex, (Boolean) param);
                } else if (param instanceof java.sql.Date) {
                    stmt.setDate(paramIndex, (java.sql.Date) param);
                } else if (param instanceof java.sql.Timestamp) {
                    stmt.setTimestamp(paramIndex, (java.sql.Timestamp) param);
                } else if (param instanceof LocalDate) {
                    stmt.setDate(paramIndex, java.sql.Date.valueOf((LocalDate) param));
                } else if (param instanceof LocalDateTime) {
                    stmt.setTimestamp(paramIndex, java.sql.Timestamp.valueOf((LocalDateTime) param));
                } else {
                    stmt.setObject(paramIndex, param);
                }
            }
        }
    }
}