package BB.resto.Entity;

import BB.resto.Entity.Enumeration.TypeProduct;
import java.time.LocalDateTime;

public class Product {
    private int id;
    private String libelle;
    private String description;
    private double prix;


    private String cloudinaryUrl;
    private String cloudinaryPublicId;
    private String imageFormat;

    private boolean isArchived;
    private TypeProduct typeProduct;
    private LocalDateTime createdAt;
    private LocalDateTime updatedAt;


    public Product() {
        this.isArchived = false;
        this.createdAt = LocalDateTime.now();
        this.updatedAt = LocalDateTime.now();
    }

    public Product(String libelle, String description, double prix, TypeProduct typeProduct) {
        this();
        this.libelle = libelle;
        this.description = description;
        this.prix = prix;
        this.typeProduct = typeProduct;
    }

    public Product(String libelle, String description, double prix, TypeProduct typeProduct,
                   String cloudinaryUrl, String cloudinaryPublicId) {
        this(libelle, description, prix, typeProduct);
        this.cloudinaryUrl = cloudinaryUrl;
        this.cloudinaryPublicId = cloudinaryPublicId;
    }


    public int getId() { return id; }
    public void setId(int id) { this.id = id; }

    public String getLibelle() { return libelle; }
    public void setLibelle(String libelle) { this.libelle = libelle; }

    public String getDescription() { return description; }
    public void setDescription(String description) { this.description = description; }

    public double getPrix() { return prix; }
    public void setPrix(double prix) { this.prix = prix; }

    public boolean isArchived() { return isArchived; }
    public void setArchived(boolean archived) { isArchived = archived; }

    public TypeProduct getTypeProduct() { return typeProduct; }
    public void setTypeProduct(TypeProduct typeProduct) { this.typeProduct = typeProduct; }

    public LocalDateTime getCreatedAt() { return createdAt; }
    public void setCreatedAt(LocalDateTime createdAt) { this.createdAt = createdAt; }

    public LocalDateTime getUpdatedAt() { return updatedAt; }
    public void setUpdatedAt(LocalDateTime updatedAt) { this.updatedAt = updatedAt; }


    public String getCloudinaryUrl() { return cloudinaryUrl; }
    public void setCloudinaryUrl(String cloudinaryUrl) {
        this.cloudinaryUrl = cloudinaryUrl;
        if (cloudinaryUrl != null && cloudinaryUrl.contains(".")) {
            String[] parts = cloudinaryUrl.split("\\.");
            if (parts.length > 0) {
                this.imageFormat = parts[parts.length - 1].toLowerCase();
            }
        }
    }

    public String getCloudinaryPublicId() { return cloudinaryPublicId; }
    public void setCloudinaryPublicId(String cloudinaryPublicId) {
        this.cloudinaryPublicId = cloudinaryPublicId;
    }

    public String getImageFormat() { return imageFormat; }
    public void setImageFormat(String imageFormat) { this.imageFormat = imageFormat; }


    @Deprecated
    public String getImageUrl() {
        return cloudinaryUrl;
    }


    @Deprecated
    public void setImageUrl(String imageUrl) {
        this.cloudinaryUrl = imageUrl;
    }


    public boolean hasCloudinaryImage() {
        return cloudinaryUrl != null && !cloudinaryUrl.trim().isEmpty();
    }


    public boolean hasLocalImage() {
        return cloudinaryUrl != null &&
                cloudinaryUrl.startsWith("/") &&
                !cloudinaryUrl.contains("cloudinary.com");
    }


    public String generateCloudinaryPublicId() {
        if (id > 0) {
            return "product_" + id + "_" + libelle.toLowerCase()
                    .replace(" ", "_")
                    .replace("'", "")
                    .replace("é", "e")
                    .replace("è", "e")
                    .replace("ê", "e");
        }
        return "product_" + System.currentTimeMillis() + "_" +
                libelle.toLowerCase().replace(" ", "_");
    }


    public String getOptimizedImageUrl(int width, int height) {
        if (!hasCloudinaryImage() || cloudinaryPublicId == null) {
            return cloudinaryUrl;
        }


        if (cloudinaryUrl.contains("cloudinary.com")) {
            String baseUrl = cloudinaryUrl.substring(0, cloudinaryUrl.indexOf("/upload/") + 8);
            return baseUrl + "w_" + width + ",h_" + height + ",c_fill/q_auto/" +
                    cloudinaryPublicId + "." + (imageFormat != null ? imageFormat : "jpg");
        }
        return cloudinaryUrl;
    }


    public String getDefaultImageUrl() {
        return getOptimizedImageUrl(300, 300);
    }


    public String getThumbnailUrl() {
        return getOptimizedImageUrl(150, 150);
    }


    public String getFormattedPrix() {
        return String.format("%,.2f XOF", prix);
    }


    public boolean isAvailable() {
        return !isArchived && prix > 0;
    }


    public boolean isBurger() {
        return typeProduct == TypeProduct.BURGER;
    }


    public boolean isMenu() {
        return typeProduct == TypeProduct.MENU;
    }


    public boolean isComplement() {
        return typeProduct == TypeProduct.COMPLEMENT;
    }

    public void applyDiscount(double percentage) {
        if (percentage > 0 && percentage <= 100) {
            this.prix = this.prix * (1 - percentage / 100);
        }
    }


    public void applyTVA(double tvaRate) {
        if (tvaRate > 0) {
            this.prix = this.prix * (1 + tvaRate / 100);
        }
    }

    @Override
    public String toString() {
        return "Product{" +
                "id=" + id +
                ", libelle='" + libelle + '\'' +
                ", prix=" + getFormattedPrix() +
                ", type=" + typeProduct +
                ", archived=" + isArchived +
                ", hasImage=" + hasCloudinaryImage() +
                '}';
    }


    public String toDetailedString() {
        StringBuilder sb = new StringBuilder();
        sb.append("🍔 ").append(libelle).append("\n");
        sb.append("   📝 ").append(description).append("\n");
        sb.append("   💰 ").append(getFormattedPrix()).append("\n");
        sb.append("   🏷️  Type: ").append(typeProduct).append("\n");
        if (hasCloudinaryImage()) {
            sb.append("   🖼️  Image: Disponible\n");
        }
        if (isArchived) {
            sb.append("   📦 Statut: Archivé\n");
        }
        return sb.toString();
    }


    public String toOrderLineString(int quantity) {
        return String.format("%dx %s - %s",
                quantity, libelle, getFormattedPrix());
    }

    @Override
    public boolean equals(Object o) {
        if (this == o) return true;
        if (o == null || getClass() != o.getClass()) return false;
        Product product = (Product) o;
        return id == product.id;
    }

    @Override
    public int hashCode() {
        return Integer.hashCode(id);
    }


    public Product cloneWithoutId() {
        Product clone = new Product();
        clone.setLibelle(this.libelle);
        clone.setDescription(this.description);
        clone.setPrix(this.prix);
        clone.setCloudinaryUrl(this.cloudinaryUrl);
        clone.setCloudinaryPublicId(this.cloudinaryPublicId);
        clone.setImageFormat(this.imageFormat);
        clone.setArchived(this.isArchived);
        clone.setTypeProduct(this.typeProduct);
        clone.setCreatedAt(this.createdAt);
        clone.setUpdatedAt(this.updatedAt);
        return clone;
    }
}