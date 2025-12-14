package BB.resto.Entity;

import BB.resto.Entity.Enumeration.TypeComplement;
import BB.resto.Entity.Enumeration.TypeProduct;

public class Complement extends Product {
    private TypeComplement typeComplement;
    private String volume;


    public Complement() {
        super();
    }

    public Complement(String libelle, String description, double prix, TypeComplement typeComplement, String volume) {
        super(libelle, description, prix, TypeProduct.COMPLEMENT);
        this.typeComplement = typeComplement;
        this.volume = volume;
    }


    public TypeComplement getTypeComplement() { return typeComplement; }
    public void setTypeComplement(TypeComplement typeComplement) { this.typeComplement = typeComplement; }

    public String getVolume() { return volume; }
    public void setVolume(String volume) { this.volume = volume; }


    public boolean isBoisson() {
        return typeComplement == TypeComplement.BOISSON;
    }

    public boolean isFrite() {
        return typeComplement == TypeComplement.FRITE;
    }

    @Override
    public String toString() {
        return "Complement{" +
                "id=" + getId() +
                ", libelle='" + getLibelle() + '\'' +
                ", typeComplement=" + typeComplement +
                ", volume='" + volume + '\'' +
                ", prix=" + getPrix() +
                '}';
    }
}