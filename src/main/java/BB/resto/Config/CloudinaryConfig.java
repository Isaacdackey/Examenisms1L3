package BB.resto.Config;

import com.cloudinary.Cloudinary;
import com.cloudinary.utils.ObjectUtils;
import java.io.IOException;
import java.io.InputStream;
import java.util.Map;
import java.util.Properties;

public class CloudinaryConfig {

        private static Cloudinary cloudinary;
        private static Properties properties = new Properties();

        static {

            try (InputStream input = CloudinaryConfig.class.getClassLoader()
                    .getResourceAsStream("application.properties")) {
                if (input != null) {
                    properties.load(input);
                }
            } catch (IOException e) {
                System.err.println("Erreur chargement configuration Cloudinary: " + e.getMessage());
            }


            Map config = ObjectUtils.asMap(
                    "cloud_name", properties.getProperty("cloudinary.cloud_name"),
                    "api_key", properties.getProperty("cloudinary.api_key"),
                    "api_secret", properties.getProperty("cloudinary.api_secret"),
                    "secure", true
            );
            cloudinary = new Cloudinary(config);
        }

        public static Cloudinary getCloudinary() {
            return cloudinary;
        }

        public static String getCloudName() {
            return properties.getProperty("cloudinary.cloud_name");
        }

        public static String getApiKey() {
            return properties.getProperty("cloudinary.api_key");
        }

}
