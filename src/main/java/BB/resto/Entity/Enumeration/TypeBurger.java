package BB.resto.Entity.Enumeration;

public enum TypeBurger {

    CHEESEBURGER("Cheese Burger"),
    FISHBURGER("Fish Burger"),
    CHICKENBURGER("Chicken Burger"),
    BACONBURGER("Bacon Burger"),
    VEGGIEBURGER("Veggie Burger");

    private final String label;

    TypeBurger(String label) {
        this.label = label;
    }

    public String getLabel() {
        return label;
    }
}
