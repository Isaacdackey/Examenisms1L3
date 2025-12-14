package BB.resto.Entity.Enumeration;

public enum StatutPaiement {

        EN_ATTENTE("En attente"),
        PAYE("Payé"),
        ECHEC("Échec"),
        REMBOURSE("Remboursé");

        private final String label;

        StatutPaiement(String label) {
            this.label = label;
        }

        public String getLabel() {
            return label;
        }
}



