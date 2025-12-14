package BB.resto.Entity.Enumeration;

public enum TypeComplement {

        BOISSON("Boisson"),
        FRITE("Frite");

        private final String label;

        TypeComplement(String label) {
            this.label = label;
        }

        public String getLabel() {
            return label;
        }
}

