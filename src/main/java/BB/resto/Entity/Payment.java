package BB.resto.Entity;

import BB.resto.Entity.Enumeration.MoyenPaiement;
import BB.resto.Entity.Enumeration.StatutPaiement;
import java.time.LocalDateTime;

public class Payment {
    private int id;
    private int commandeId;
    private double montant;
    private String referenceTransaction;
    private MoyenPaiement moyenPaiement;
    private StatutPaiement statutPaiement;
    private LocalDateTime datePaiement;
    private LocalDateTime createdAt;


    private Commande commande;

    public Payment() {
        this.statutPaiement = StatutPaiement.EN_ATTENTE;
        this.createdAt = LocalDateTime.now();
    }

    public Payment(int commandeId, double montant, String referenceTransaction, MoyenPaiement moyenPaiement) {
        this();
        this.commandeId = commandeId;
        this.montant = montant;
        this.referenceTransaction = referenceTransaction;
        this.moyenPaiement = moyenPaiement;
    }


    public int getId() { return id; }
    public void setId(int id) { this.id = id; }

    public int getCommandeId() { return commandeId; }
    public void setCommandeId(int commandeId) { this.commandeId = commandeId; }

    public double getMontant() { return montant; }
    public void setMontant(double montant) { this.montant = montant; }

    public String getReferenceTransaction() { return referenceTransaction; }
    public void setReferenceTransaction(String referenceTransaction) {
        this.referenceTransaction = referenceTransaction;
    }

    public MoyenPaiement getMoyenPaiement() { return moyenPaiement; }
    public void setMoyenPaiement(MoyenPaiement moyenPaiement) { this.moyenPaiement = moyenPaiement; }

    public StatutPaiement getStatutPaiement() { return statutPaiement; }
    public void setStatutPaiement(StatutPaiement statutPaiement) { this.statutPaiement = statutPaiement; }

    public LocalDateTime getDatePaiement() { return datePaiement; }
    public void setDatePaiement(LocalDateTime datePaiement) { this.datePaiement = datePaiement; }

    public LocalDateTime getCreatedAt() { return createdAt; }
    public void setCreatedAt(LocalDateTime createdAt) { this.createdAt = createdAt; }

    public Commande getCommande() { return commande; }
    public void setCommande(Commande commande) { this.commande = commande; }


    public void markAsPaid() {
        this.statutPaiement = StatutPaiement.PAYE;
        this.datePaiement = LocalDateTime.now();
    }

    public void markAsFailed() {
        this.statutPaiement = StatutPaiement.ECHEC;
    }

    public void markAsRefunded() {
        this.statutPaiement = StatutPaiement.REMBOURSE;
    }

    public boolean isPaid() {
        return statutPaiement == StatutPaiement.PAYE;
    }

    public String getFormattedMontant() {
        return String.format("%.2f FCFA", montant);
    }

    @Override
    public String toString() {
        return "Payment{" +
                "id=" + id +
                ", commandeId=" + commandeId +
                ", montant=" + montant +
                ", moyenPaiement=" + moyenPaiement +
                ", statutPaiement=" + statutPaiement +
                ", datePaiement=" + datePaiement +
                '}';
    }
}