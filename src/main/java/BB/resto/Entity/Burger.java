package BB.resto.Entity;

import BB.resto.Entity.Enumeration.TypeBurger;
import BB.resto.Entity.Enumeration.TypeProduct;

public class Burger extends Product {
    private TypeBurger typeBurger;
    private int calories;
    private int tempsPreparation;


    public Burger() {
        super();
    }

    public Burger(String libelle, String description, double prix, TypeBurger typeBurger, int calories, int tempsPreparation) {
        super(libelle, description, prix, TypeProduct.BURGER);
        this.typeBurger = typeBurger;
        this.calories = calories;
        this.tempsPreparation = tempsPreparation;
    }


    public TypeBurger getTypeBurger() { return typeBurger; }
    public void setTypeBurger(TypeBurger typeBurger) { this.typeBurger = typeBurger; }

    public int getCalories() { return calories; }
    public void setCalories(int calories) { this.calories = calories; }

    public int getTempsPreparation() { return tempsPreparation; }
    public void setTempsPreparation(int tempsPreparation) { this.tempsPreparation = tempsPreparation; }

    @Override
    public String toString() {
        return "Burger{" +
                "id=" + getId() +
                ", libelle='" + getLibelle() + '\'' +
                ", typeBurger=" + typeBurger +
                ", calories=" + calories +
                ", tempsPreparation=" + tempsPreparation + "min" +
                ", prix=" + getPrix() +
                '}';
    }
}