package BB.resto.Entity;

import BB.resto.Entity.Enumeration.TypeRole;
import java.time.LocalDateTime;

public class User {
    private int id;
    private String nom;
    private String prenom;
    private String tel;
    private String email;
    private String password;
    private String adresse;
    private String quartier;
    private TypeRole role;
    private String vehicule;
    private boolean isActive;
    private LocalDateTime createdAt;
    private LocalDateTime updatedAt;

    public User() {
        this.isActive = true;  // OK
        this.createdAt = LocalDateTime.now();
    }

    public User(String nom, String prenom, String tel, String email, String password, TypeRole role) {
        this();
        this.nom = nom;
        this.prenom = prenom;
        this.tel = tel;
        this.email = email;
        this.password = password;
        this.role = role;
    }


    public int getId() { return id; }
    public void setId(int id) { this.id = id; }

    public String getNom() { return nom; }
    public void setNom(String nom) { this.nom = nom; }

    public String getPrenom() { return prenom; }
    public void setPrenom(String prenom) { this.prenom = prenom; }

    public String getTel() { return tel; }
    public void setTel(String tel) { this.tel = tel; }

    public String getEmail() { return email; }
    public void setEmail(String email) { this.email = email; }

    public String getPassword() { return password; }
    public void setPassword(String password) { this.password = password; }

    public String getAdresse() { return adresse; }
    public void setAdresse(String adresse) { this.adresse = adresse; }

    public String getQuartier() { return quartier; }
    public void setQuartier(String quartier) { this.quartier = quartier; }

    public TypeRole getRole() { return role; }
    public void setRole(TypeRole role) { this.role = role; }

    public String getVehicule() { return vehicule; }
    public void setVehicule(String vehicule) { this.vehicule = vehicule; }


    public boolean isActive() {
        return isActive;
    }

    public void setActive(boolean active) {
        this.isActive = active;
    }

    public LocalDateTime getCreatedAt() { return createdAt; }
    public void setCreatedAt(LocalDateTime createdAt) { this.createdAt = createdAt; }

    public LocalDateTime getUpdatedAt() { return updatedAt; }
    public void setUpdatedAt(LocalDateTime updatedAt) { this.updatedAt = updatedAt; }


    public String getFullName() {
        return prenom + " " + nom;
    }

    public boolean isGestionnaire() {
        return role == TypeRole.GESTIONNAIRE;
    }

    public boolean isClient() {
        return role == TypeRole.CLIENT;
    }

    public boolean isLivreur() {
        return role == TypeRole.LIVREUR;
    }

    @Override
    public String toString() {
        return "User{" +
                "id=" + id +
                ", nom='" + nom + '\'' +
                ", prenom='" + prenom + '\'' +
                ", tel='" + tel + '\'' +
                ", email='" + email + '\'' +
                ", role=" + role +
                ", isActive=" + isActive +  // OK
                '}';
    }
}