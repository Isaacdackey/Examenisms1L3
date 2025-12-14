package BB.resto.Entity.Enumeration;

public enum TypeRole {

        GESTIONNAIRE("Gestionnaire"),
        CLIENT("Client"),
        LIVREUR("Livreur");

        private final String label;

        TypeRole(String label) {
            this.label = label;
        }

        public String getLabel() {
            return label;
        }

        public static TypeRole fromString(String text) {
            for (TypeRole role : TypeRole.values()) {
                if (role.name().equalsIgnoreCase(text)) {
                    return role;
                }
            }
            return CLIENT;
        }
}