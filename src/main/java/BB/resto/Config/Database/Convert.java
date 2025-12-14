package BB.resto.Config.Database;

import java.sql.ResultSet;
import java.sql.SQLException;
import java.time.LocalDateTime;
import java.time.format.DateTimeFormatter;
import java.util.ArrayList;
import java.util.List;
import java.time.LocalDateTime;


public class
Convert {


    public static <T> List<T> toList(ResultSet rs, RowMapper<T> mapper) throws SQLException {
        List<T> list = new ArrayList<>();
        while (rs.next()) {
            list.add(mapper.mapRow(rs));
        }
        return list;
    }


    public static <T> T toObject(ResultSet rs, RowMapper<T> mapper) throws SQLException {
        if (rs.next()) {
            return mapper.mapRow(rs);
        }
        return null;
    }


    public interface RowMapper<T> {
        T mapRow(ResultSet rs) throws SQLException;
    }


    public static String toSqlDateTime(LocalDateTime dateTime) {
        if (dateTime == null) return null;
        DateTimeFormatter formatter = DateTimeFormatter.ofPattern("yyyy-MM-dd HH:mm:ss");
        return dateTime.format(formatter);
    }


    public static LocalDateTime fromSqlDateTime(String sqlDateTime) {
        if (sqlDateTime == null) return null;
        DateTimeFormatter formatter = DateTimeFormatter.ofPattern("yyyy-MM-dd HH:mm:ss");
        return LocalDateTime.parse(sqlDateTime, formatter);
    }


    public static String formatPrice(double price) {
        return String.format("%.2f FCFA", price);
    }


    public static String formatDate(LocalDateTime dateTime) {
        if (dateTime == null) return "Non défini";
        DateTimeFormatter formatter = DateTimeFormatter.ofPattern("dd/MM/yyyy HH:mm");
        return dateTime.format(formatter);
    }


    public static String formatShortDate(LocalDateTime dateTime) {
        if (dateTime == null) return "N/A";
        DateTimeFormatter formatter = DateTimeFormatter.ofPattern("dd/MM/yyyy");
        return dateTime.format(formatter);
    }


    public static String timeAgo(LocalDateTime dateTime) {
        if (dateTime == null) return "N/A";

        LocalDateTime now = LocalDateTime.now();
        long seconds = java.time.Duration.between(dateTime, now).getSeconds();

        if (seconds < 60) return "À l'instant";
        if (seconds < 3600) return "Il y a " + (seconds / 60) + " min";
        if (seconds < 86400) return "Il y a " + (seconds / 3600) + " h";
        if (seconds < 2592000) return "Il y a " + (seconds / 86400) + " j";
        if (seconds < 31536000) return "Il y a " + (seconds / 2592000) + " mois";
        return "Il y a " + (seconds / 31536000) + " ans";
    }


    public static boolean isNullOrEmpty(String str) {
        return str == null || str.trim().isEmpty();
    }


    public static String normalize(String str) {
        if (isNullOrEmpty(str)) return "";

        str = str.trim();
        if (str.length() > 1) {
            return str.substring(0, 1).toUpperCase() + str.substring(1).toLowerCase();
        }
        return str.toUpperCase();
    }
}