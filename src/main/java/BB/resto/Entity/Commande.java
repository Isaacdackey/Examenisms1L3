package BB.resto.Entity;

import BB.resto.Entity.Enumeration.EtatCommande;
import BB.resto.Entity.Enumeration.TypeRetrait;
import java.time.LocalDateTime;
import java.util.ArrayList;
import java.util.List;

public class Commande {
    private int id;
    private String numero;
    private int userId;
    private String adresse;
    private double montantTotal;
    private EtatCommande statut;
    private TypeRetrait typeRetrait;
    private Integer zoneId;
    private Integer livreurId;
    private String notes;
    private LocalDateTime createdAt;
    private LocalDateTime updatedAt;


    private User client;
    private ZoneLivraison zoneLivraison;
    private User livreur;
    private List<LigneCommande> lignesCommande;


    public Commande() {
        this.statut = EtatCommande.EN_ATTENTE;
        this.createdAt = LocalDateTime.now();
        this.lignesCommande = new ArrayList<>();
    }

    public Commande(int userId, String adresse, TypeRetrait typeRetrait) {
        this();
        this.userId = userId;
        this.adresse = adresse;
        this.typeRetrait = typeRetrait;
    }


    public int getId() { return id; }
    public void setId(int id) { this.id = id; }

    public String getNumero() { return numero; }
    public void setNumero(String numero) { this.numero = numero; }

    public int getUserId() { return userId; }
    public void setUserId(int userId) { this.userId = userId; }

    public String getAdresse() { return adresse; }
    public void setAdresse(String adresse) { this.adresse = adresse; }

    public double getMontantTotal() { return montantTotal; }
    public void setMontantTotal(double montantTotal) { this.montantTotal = montantTotal; }

    public EtatCommande getStatut() { return statut; }
    public void setStatut(EtatCommande statut) { this.statut = statut; }

    public TypeRetrait getTypeRetrait() { return typeRetrait; }
    public void setTypeRetrait(TypeRetrait typeRetrait) { this.typeRetrait = typeRetrait; }

    public Integer getZoneId() { return zoneId; }
    public void setZoneId(Integer zoneId) { this.zoneId = zoneId; }

    public Integer getLivreurId() { return livreurId; }
    public void setLivreurId(Integer livreurId) { this.livreurId = livreurId; }

    public String getNotes() { return notes; }
    public void setNotes(String notes) { this.notes = notes; }

    public LocalDateTime getCreatedAt() { return createdAt; }
    public void setCreatedAt(LocalDateTime createdAt) { this.createdAt = createdAt; }

    public LocalDateTime getUpdatedAt() { return updatedAt; }
    public void setUpdatedAt(LocalDateTime updatedAt) { this.updatedAt = updatedAt; }


    public User getClient() { return client; }
    public void setClient(User client) { this.client = client; }

    public ZoneLivraison getZoneLivraison() { return zoneLivraison; }
    public void setZoneLivraison(ZoneLivraison zoneLivraison) { this.zoneLivraison = zoneLivraison; }

    public User getLivreur() { return livreur; }
    public void setLivreur(User livreur) { this.livreur = livreur; }

    public List<LigneCommande> getLignesCommande() { return lignesCommande; }
    public void setLignesCommande(List<LigneCommande> lignesCommande) { this.lignesCommande = lignesCommande; }


    public void addLigneCommande(LigneCommande ligne) {
        this.lignesCommande.add(ligne);
        ligne.setCommandeId(this.id);
    }

    public void calculerMontantTotal() {
        double total = 0;
        for (LigneCommande ligne : lignesCommande) {
            total += ligne.getSousTotal();
        }


        if (typeRetrait == TypeRetrait.LIVRAISON && zoneLivraison != null) {
            total += zoneLivraison.getPrixLivraison();
        }

        this.montantTotal = total;
    }

    public boolean estLivrable() {
        return typeRetrait == TypeRetrait.LIVRAISON && statut != EtatCommande.LIVREE
                && statut != EtatCommande.TERMINEE && statut != EtatCommande.ANNULEE;
    }

    public boolean peutEtreAnnulee() {
        return statut == EtatCommande.EN_ATTENTE || statut == EtatCommande.VALIDEE;
    }

    public String getFormattedMontant() {
        return String.format("%.2f FCFA", montantTotal);
    }

    @Override
    public String toString() {
        return "Commande{" +
                "id=" + id +
                ", numero='" + numero + '\'' +
                ", userId=" + userId +
                ", montantTotal=" + montantTotal +
                ", statut=" + statut +
                ", typeRetrait=" + typeRetrait +
                ", createdAt=" + createdAt +
                '}';
    }
}