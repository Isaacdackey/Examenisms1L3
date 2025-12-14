package BB.resto.Util;

import com.cloudinary.Cloudinary;
import com.cloudinary.utils.ObjectUtils;
import BB.resto.Config.CloudinaryConfig;
import java.io.File;
import java.io.IOException;
import java.util.Map;

public class CloudinaryManager {

        private static Cloudinary cloudinary = CloudinaryConfig.getCloudinary();


        public static Map uploadImage(File imageFile, String publicId) throws IOException {
            return cloudinary.uploader().upload(imageFile,
                    ObjectUtils.asMap(
                            "public_id", publicId,
                            "folder", "brasil-burger/products",
                            "overwrite", true,
                            "resource_type", "image"
                    )
            );
        }


        public static String uploadImage(String filePath, String productName) throws IOException {
            File imageFile = new File(filePath);
            String publicId = "product_" + System.currentTimeMillis() + "_" +
                    productName.toLowerCase().replace(" ", "_");

            Map uploadResult = uploadImage(imageFile, publicId);
            return (String) uploadResult.get("secure_url");
        }


        public static boolean deleteImage(String publicId) throws IOException {
            Map result = cloudinary.uploader().destroy(publicId, ObjectUtils.emptyMap());
            return "ok".equals(result.get("result"));
        }


        public static String getOptimizedUrl(String publicId, int width, int height) {
            return cloudinary.url()
                    .transformation(new com.cloudinary.Transformation()
                            .width(width)
                            .height(height)
                            .crop("fill")
                            .quality("auto"))
                    .generate(publicId);
        }

        public static String extractPublicIdFromUrl(String url) {
            if (url == null || !url.contains("cloudinary.com")) {
                return null;
            }

            String[] parts = url.split("/");
            if (parts.length > 0) {
                String lastPart = parts[parts.length - 1];
                return lastPart.substring(0, lastPart.lastIndexOf('.'));
            }
            return null;
        }

}
