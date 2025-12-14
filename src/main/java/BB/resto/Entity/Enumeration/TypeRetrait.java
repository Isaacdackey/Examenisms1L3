package BB.resto.Entity.Enumeration;

public enum TypeRetrait {

        SUR_PLACE("Sur place"),
        A_EMPORTER("À emporter"),
        LIVRAISON("Livraison");

        private final String label;

        TypeRetrait(String label) {
            this.label = label;
        }

        public String getLabel() {
            return label;
        }
}

