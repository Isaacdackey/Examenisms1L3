package BB.resto.Entity.Enumeration;

public enum MoyenPaiement {
        WAVE("Wave"),
        ORANGE_MONEY("Orange Money"),
        ESPECES("Espèces");

        private final String label;

        MoyenPaiement(String label) {
            this.label = label;
        }

        public String getLabel() {
            return label;
        }
}

