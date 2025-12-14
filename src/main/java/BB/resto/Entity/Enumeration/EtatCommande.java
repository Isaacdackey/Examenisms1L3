package BB.resto.Entity.Enumeration;

public enum EtatCommande {

        EN_ATTENTE("En attente"),
        VALIDEE("Validée"),
        EN_PREPARATION("En préparation"),
        PRETE("Prête"),
        EN_LIVRAISON("En livraison"),
        LIVREE("Livrée"),
        TERMINEE("Terminée"),
        ANNULEE("Annulée");

        private final String label;

        EtatCommande(String label) {
            this.label = label;
        }

        public String getLabel() {
            return label;
        }
}

