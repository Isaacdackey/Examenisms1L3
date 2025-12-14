package BB.resto.Views;

import BB.resto.Entity.Burger;
import BB.resto.Entity.Enumeration.TypeBurger;
import BB.resto.Services.Impl.BurgerService;

import java.sql.Connection;
import java.sql.ResultSet;
import java.sql.Statement;
import java.util.*;

public class BurgerView {
    private Scanner scanner = new Scanner(System.in);
    private BurgerService burgerService = new BurgerService();

    public void showMenu() {
        while (true) {
            System.out.println("\n═══════════════════════════════════════════════");
            System.out.println("            GESTION DES BURGERS 🍔            ");
            System.out.println("═══════════════════════════════════════════════");
            System.out.println("1. Ajouter un burger");
            System.out.println("2. Modifier un burger");
            System.out.println("3. Archiver un burger");
            System.out.println("4. Désarchiver un burger");
            System.out.println("5. Lister les burgers actifs");
            System.out.println("6. Lister les burgers archivés");
            System.out.println("7. Rechercher un burger par type");
            System.out.println("0. Retour au menu principal");
            System.out.print("👉 Votre choix : ");

            int choice = Integer.parseInt(scanner.nextLine());

            switch (choice) {
                case 1: createBurger(); break;
                case 2: updateBurger(); break;
                case 3: archiveBurger(); break;
                case 4: unarchiveBurger(); break;
                case 5: listActiveBurgers(); break;
                case 6: listArchivedBurgers(); break;
                case 7: searchByType(); break;
                case 0: return;
                default: System.out.println("❌ Choix invalide !");
            }
        }
    }

    private void createBurger() {
        System.out.println("\n➕ AJOUTER UN BURGER");

        System.out.print("Nom : ");
        String nom = scanner.nextLine();

        System.out.print("Description : ");
        String description = scanner.nextLine();

        System.out.print("Prix : ");
        double prix = Double.parseDouble(scanner.nextLine());

        System.out.println("\n📋 TYPES DE BURGER DISPONIBLES :");
        System.out.println("1. CHEESEBURGER");
        System.out.println("2. FISHBURGER");
        System.out.println("3. CHICKENBURGER");
        System.out.println("4. BACONBURGER");
        System.out.println("5. VEGGIEBURGER");
        System.out.print("👉 Votre choix (1-5) : ");

        int typeChoice = Integer.parseInt(scanner.nextLine());
        TypeBurger type;

        switch (typeChoice) {
            case 1: type = TypeBurger.CHEESEBURGER; break;
            case 2: type = TypeBurger.FISHBURGER; break;
            case 3: type = TypeBurger.CHICKENBURGER; break;
            case 4: type = TypeBurger.BACONBURGER; break;
            case 5: type = TypeBurger.VEGGIEBURGER; break;
            default:
                System.out.println("❌ Choix invalide, par défaut : CHEESEBURGER");
                type = TypeBurger.CHEESEBURGER;
        }

        System.out.print("Calories : ");
        int calories = Integer.parseInt(scanner.nextLine());

        System.out.print("Temps de préparation (min) : ");
        int prepTime = Integer.parseInt(scanner.nextLine());

        try {
            Burger burger = new Burger();
            burger.setLibelle(nom);
            burger.setDescription(description);
            burger.setPrix(prix);
            burger.setTypeBurger(type);
            burger.setCalories(calories);
            burger.setTempsPreparation(prepTime);
            burger.setTypeProduct(BB.resto.Entity.Enumeration.TypeProduct.BURGER);
            Burger created = burgerService.createBurger(burger);
            System.out.println("✅ Burger créé avec succès ! ID: " + created.getId());
        } catch (Exception e) {
            System.out.println("❌ Erreur : " + e.getMessage());
        }
    }

    private void updateBurger() {
        System.out.println("\n✏️ MODIFIER UN BURGER");
        listActiveBurgers();

        System.out.print("\nID du burger à modifier : ");
        int id = Integer.parseInt(scanner.nextLine());

        var burgerOpt = burgerService.getBurgerById(id);
        if (burgerOpt.isEmpty()) {
            System.out.println("❌ Burger non trouvé !");
            return;
        }

        Burger burger = burgerOpt.get();
        burger.setTypeProduct(BB.resto.Entity.Enumeration.TypeProduct.BURGER);
        System.out.print("Nouveau nom [" + burger.getLibelle() + "] : ");
        String newNom = scanner.nextLine();
        if (!newNom.isEmpty()) burger.setLibelle(newNom);

        System.out.print("Nouvelle description [" + burger.getDescription() + "] : ");
        String newDesc = scanner.nextLine();
        if (!newDesc.isEmpty()) burger.setDescription(newDesc);

        System.out.print("Nouveau prix [" + burger.getPrix() + "] : ");
        String newPrixStr = scanner.nextLine();
        if (!newPrixStr.isEmpty()) burger.setPrix(Double.parseDouble(newPrixStr));

        try {
            burgerService.updateBurger(burger);
            System.out.println("✅ Burger modifié avec succès !");
        } catch (Exception e) {
            System.out.println("❌ Erreur : " + e.getMessage());
        }
    }

    private void archiveBurger() {
        System.out.println("\n📦 ARCHIVER UN BURGER");
        listActiveBurgers();

        System.out.print("\nID du burger à archiver : ");
        int id = Integer.parseInt(scanner.nextLine());

        System.out.print("Êtes-vous sûr ? (o/n) : ");
        String confirm = scanner.nextLine();

        if (!confirm.equalsIgnoreCase("o")) {
            System.out.println("❌ Archivage annulé !");
            return;
        }

        try {
            if (burgerService.deleteBurger(id)) {
                System.out.println("✅ Burger archivé avec succès !");
            } else {
                System.out.println("❌ Échec de l'archivage !");
            }
        } catch (Exception e) {
            System.out.println("❌ Erreur : " + e.getMessage());
        }
    }

    private void unarchiveBurger() {
        System.out.println("\n🔄 DÉSARCHIVER UN BURGER");
        listArchivedBurgers();

        System.out.print("\nID du burger à désarchiver : ");
        int id = Integer.parseInt(scanner.nextLine());

        var burgerOpt = burgerService.getBurgerById(id);
        if (burgerOpt.isEmpty()) {
            System.out.println("❌ Burger non trouvé !");
            return;
        }

        Burger burger = burgerOpt.get();


        burger.setArchived(false);
        burger.setTypeProduct(BB.resto.Entity.Enumeration.TypeProduct.BURGER);

        try {
            burgerService.updateBurger(burger);
            System.out.println("✅ Burger désarchivé avec succès !");
        } catch (Exception e) {
            System.out.println("❌ Erreur : " + e.getMessage());
        }
    }

    private void listActiveBurgers() {
        System.out.println("\n🍔 BURGERS ACTIFS :");
        System.out.println("═══════════════════════════════════════════════");

        List<Burger> burgers = burgerService.getActiveBurgers();
        if (burgers.isEmpty()) {
            System.out.println("Aucun burger actif.");
        } else {
            for (Burger b : burgers) {
                System.out.printf("ID: %-4d %-25s %-8.2f FCFA Type: %-15s\n",
                        b.getId(), b.getLibelle(), b.getPrix(), b.getTypeBurger().getLabel());
            }
        }
    }

    private void listArchivedBurgers() {
        System.out.println("\n📦 BURGERS ARCHIVÉS :");
        System.out.println("═══════════════════════════════════════════════");

        try {

            String sql = "SELECT p.id, p.libelle, p.prix, b.type_burger " +
                    "FROM Product p " +
                    "JOIN burger b ON p.id = b.id " +
                    "WHERE p.type_Product = 'BURGER' AND p.is_archived = TRUE " +
                    "ORDER BY p.libelle";

            Connection conn = BB.resto.Config.Database.Database.getConnection();
            Statement stmt = conn.createStatement();
            ResultSet rs = stmt.executeQuery(sql);

            boolean hasResults = false;
            while (rs.next()) {
                hasResults = true;
                System.out.printf("ID: %-4d %-25s %-8.2f FCFA Type: %s\n",
                        rs.getInt("id"),
                        rs.getString("libelle"),
                        rs.getDouble("prix"),
                        rs.getString("type_burger"));
            }

            if (!hasResults) {
                System.out.println("Aucun burger archivé.");
            }

            rs.close();
            stmt.close();
        } catch (Exception e) {
            System.out.println("❌ Erreur : " + e.getMessage());
        }
    }

    private void searchByType() {
        System.out.println("\n🔍 RECHERCHER PAR TYPE");
        System.out.println("1. CHEESEBURGER");
        System.out.println("2. FISHBURGER");
        System.out.println("3. CHICKENBURGER");
        System.out.println("4. BACONBURGER");
        System.out.println("5. VEGGIEBURGER");
        System.out.print("👉 Type (1-5) : ");

        int typeChoice = Integer.parseInt(scanner.nextLine());
        TypeBurger type;

        switch (typeChoice) {
            case 1: type = TypeBurger.CHEESEBURGER; break;
            case 2: type = TypeBurger.FISHBURGER; break;
            case 3: type = TypeBurger.CHICKENBURGER; break;
            case 4: type = TypeBurger.BACONBURGER; break;
            case 5: type = TypeBurger.VEGGIEBURGER; break;
            default:
                System.out.println("❌ Choix invalide !");
                return;
        }

        List<Burger> burgers = burgerService.getBurgersByType(type);

        System.out.println("\n🍔 BURGERS " + type.getLabel() + " :");
        if (burgers.isEmpty()) {
            System.out.println("Aucun burger de ce type.");
        } else {
            for (Burger b : burgers) {
                System.out.printf("  %-25s %-8.2f FCFA\n", b.getLibelle(), b.getPrix());
            }
        }
    }
}