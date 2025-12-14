package BB.resto.Entity;

import BB.resto.Entity.Enumeration.TypeProduct;

import java.util.ArrayList;
import java.util.List;

public class Menu extends Product {
    private int burgerId;
    private int boissonId;
    private int fritesId;
    private double reductionPourcentage;


    private Burger burger;
    private Complement boisson;
    private Complement frites;
    private List<Complement> complementsAdditionnels;


    public Menu() {
        super();
        this.reductionPourcentage = 10.0;
        this.complementsAdditionnels = new ArrayList<>();
    }

    public Menu(String libelle, String description, int burgerId, int boissonId, int fritesId) {
        super(libelle, description, 0.0, TypeProduct.MENU);
        this.burgerId = burgerId;
        this.boissonId = boissonId;
        this.fritesId = fritesId;
        this.reductionPourcentage = 10.0;
        this.complementsAdditionnels = new ArrayList<>();
    }


    public int getBurgerId() { return burgerId; }
    public void setBurgerId(int burgerId) { this.burgerId = burgerId; }

    public int getBoissonId() { return boissonId; }
    public void setBoissonId(int boissonId) { this.boissonId = boissonId; }

    public int getFritesId() { return fritesId; }
    public void setFritesId(int fritesId) { this.fritesId = fritesId; }

    public double getReductionPourcentage() { return reductionPourcentage; }
    public void setReductionPourcentage(double reductionPourcentage) {
        this.reductionPourcentage = reductionPourcentage;
    }


    public Burger getBurger() { return burger; }
    public void setBurger(Burger burger) { this.burger = burger; }

    public Complement getBoisson() { return boisson; }
    public void setBoisson(Complement boisson) { this.boisson = boisson; }

    public Complement getFrites() { return frites; }
    public void setFrites(Complement frites) { this.frites = frites; }

    public List<Complement> getComplementsAdditionnels() { return complementsAdditionnels; }
    public void setComplementsAdditionnels(List<Complement> complementsAdditionnels) {
        this.complementsAdditionnels = complementsAdditionnels;
    }


    public void addComplementAdditionnel(Complement complement) {
        this.complementsAdditionnels.add(complement);
    }

    public double calculerPrixOriginal() {
        double total = 0;
        if (burger != null) total += burger.getPrix();
        if (boisson != null) total += boisson.getPrix();
        if (frites != null) total += frites.getPrix();

        for (Complement complement : complementsAdditionnels) {
            total += complement.getPrix();
        }

        return total;
    }

    public double calculerPrixAvecReduction() {
        double prixOriginal = calculerPrixOriginal();
        return prixOriginal * (1 - reductionPourcentage / 100);
    }

    @Override
    public String toString() {
        return "Menu{" +
                "id=" + getId() +
                ", libelle='" + getLibelle() + '\'' +
                ", burgerId=" + burgerId +
                ", boissonId=" + boissonId +
                ", fritesId=" + fritesId +
                ", reduction=" + reductionPourcentage + "%" +
                ", prix=" + getPrix() +
                '}';
    }
}