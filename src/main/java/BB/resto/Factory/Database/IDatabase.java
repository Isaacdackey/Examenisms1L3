package BB.resto.Factory.Database;

import java.sql.ResultSet;
import java.time.LocalDate;
import java.time.LocalDateTime;
import java.util.List;
import java.util.function.Function;

public interface IDatabase {


    <T> T executeQuery(String sql, Function<ResultSet, T> resultMapper, Object... params);


    <T> List<T> executeQueryList(String sql, Function<ResultSet, T> rowMapper, Object... params);


    int executeUpdate(String sql, Object... params);

    boolean execute(String sql, Object... params);


    void executeBatch(List<String> sqlStatements);


    int count(String tableName, String condition, Object... params);


    boolean exists(String tableName, String condition, Object... params);


    double sum(String tableName, String column, String condition, Object... params);


    void createTable(String sql);
    void dropTable(String tableName);
    void truncateTable(String tableName);
    List<String> getTableNames();


    void testConnection();
}