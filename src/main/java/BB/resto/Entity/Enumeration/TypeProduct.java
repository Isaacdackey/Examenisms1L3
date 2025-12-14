package BB.resto.Entity.Enumeration;

public enum TypeProduct {

        BURGER("Burger"),
        COMPLEMENT("Complément"),
        MENU("Menu");

        private final String label;

        TypeProduct(String label) {
            this.label = label;
        }

        public String getLabel() {
            return label;
        }
}

