package BB.resto.Entity;

import java.time.LocalDateTime;
import java.util.ArrayList;
import java.util.List;

public class ZoneLivraison {
    private int id;
    private String nom;
    private double prixLivraison;
    private String quartiersCouverts;
    private LocalDateTime createdAt;


    private List<String> quartiersList;


    public ZoneLivraison() {
        this.createdAt = LocalDateTime.now();
        this.quartiersList = new ArrayList<>();
    }

    public ZoneLivraison(String nom, double prixLivraison, String quartiersCouverts) {
        this();
        this.nom = nom;
        this.prixLivraison = prixLivraison;
        this.quartiersCouverts = quartiersCouverts;
        parseQuartiers();
    }


    public int getId() { return id; }
    public void setId(int id) { this.id = id; }

    public String getNom() { return nom; }
    public void setNom(String nom) { this.nom = nom; }

    public double getPrixLivraison() { return prixLivraison; }
    public void setPrixLivraison(double prixLivraison) { this.prixLivraison = prixLivraison; }

    public String getQuartiersCouverts() { return quartiersCouverts; }
    public void setQuartiersCouverts(String quartiersCouverts) {
        this.quartiersCouverts = quartiersCouverts;
        parseQuartiers();
    }

    public LocalDateTime getCreatedAt() { return createdAt; }
    public void setCreatedAt(LocalDateTime createdAt) { this.createdAt = createdAt; }


    private void parseQuartiers() {
        quartiersList = new ArrayList<>();
        if (quartiersCouverts != null && !quartiersCouverts.trim().isEmpty()) {
            String[] parts = quartiersCouverts.split(",");
            for (String part : parts) {
                quartiersList.add(part.trim());
            }
        }
    }

    public List<String> getQuartiersList() {
        if (quartiersList == null || quartiersList.isEmpty()) {
            parseQuartiers();
        }
        return quartiersList;
    }

    public void setQuartiersList(List<String> quartiersList) {
        this.quartiersList = quartiersList;
        this.quartiersCouverts = String.join(", ", quartiersList);
    }

    public boolean couvreQuartier(String quartier) {
        return getQuartiersList().stream()
                .anyMatch(q -> q.equalsIgnoreCase(quartier.trim()));
    }

    public String getFormattedPrixLivraison() {
        return String.format("%.2f FCFA", prixLivraison);
    }

    @Override
    public String toString() {
        return "ZoneLivraison{" +
                "id=" + id +
                ", nom='" + nom + '\'' +
                ", prixLivraison=" + prixLivraison +
                ", quartiersCouverts='" + quartiersCouverts + '\'' +
                '}';
    }
}