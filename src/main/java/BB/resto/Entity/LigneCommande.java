package BB.resto.Entity;

import java.time.LocalDateTime;

public class LigneCommande {
    private int id;
    private int commandeId;
    private int productId;
    private int quantite;
    private double prixUnitaire;
    private double sousTotal;
    private LocalDateTime createdAt;
    private Product product;

    public LigneCommande() {
        this.quantite = 1;
        this.createdAt = LocalDateTime.now();
    }

    public LigneCommande(int commandeId, int productId, int quantite, double prixUnitaire) {
        this();
        this.commandeId = commandeId;
        this.productId = productId;
        this.quantite = quantite;
        this.prixUnitaire = prixUnitaire;
        calculerSousTotal();
    }


    public int getId() { return id; }
    public void setId(int id) { this.id = id; }

    public int getCommandeId() { return commandeId; }
    public void setCommandeId(int commandeId) { this.commandeId = commandeId; }

    public int getProductId() { return productId; }
    public void setProductId(int productId) {
        this.productId = productId;
    }

    public int getQuantite() { return quantite; }
    public void setQuantite(int quantite) {
        this.quantite = quantite;
        calculerSousTotal();
    }

    public double getPrixUnitaire() { return prixUnitaire; }
    public void setPrixUnitaire(double prixUnitaire) {
        this.prixUnitaire = prixUnitaire;
        calculerSousTotal();
    }

    public double getSousTotal() { return sousTotal; }
    public void setSousTotal(double sousTotal) { this.sousTotal = sousTotal; }

    public LocalDateTime getCreatedAt() { return createdAt; }
    public void setCreatedAt(LocalDateTime createdAt) { this.createdAt = createdAt; }

    public Product getProduct() { return product; }
    public void setProduct(Product product) { this.product = product; }



    private void calculerSousTotal() {
        this.sousTotal = this.quantite * this.prixUnitaire;
    }

    public void incrementerQuantite() {
        this.quantite++;
        calculerSousTotal();
    }

    public void decrementerQuantite() {
        if (this.quantite > 1) {
            this.quantite--;
            calculerSousTotal();
        }
    }

    public String getFormattedSousTotal() {
        return String.format("%.2f FCFA", sousTotal);
    }

    @Override
    public String toString() {
        return "LigneCommande{" +
                "id=" + id +
                ", commandeId=" + commandeId +
                ", productId=" + productId +
                ", quantite=" + quantite +
                ", prixUnitaire=" + prixUnitaire +
                ", sousTotal=" + sousTotal +
                '}';
    }
}