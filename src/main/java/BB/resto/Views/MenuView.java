package BB.resto.Views;

import BB.resto.Entity.Enumeration.TypeProduct;
import BB.resto.Entity.Menu;
import BB.resto.Entity.Burger;
import BB.resto.Entity.Complement;
import BB.resto.Services.Impl.MenuService;
import BB.resto.Services.Impl.BurgerService;
import BB.resto.Services.Impl.ComplementService;
import java.util.*;

public class MenuView {
    private Scanner scanner = new Scanner(System.in);
    private MenuService menuService = new MenuService();
    private BurgerService burgerService = new BurgerService();
    private ComplementService complementService = new ComplementService();

    public void showMenu() {
        while (true) {
            System.out.println("\n═══════════════════════════════════════════════");
            System.out.println("             GESTION DES MENUS 🍟🥤           ");
            System.out.println("═══════════════════════════════════════════════");
            System.out.println("1. Créer un menu (burger + boisson + frites)");
            System.out.println("2. Modifier un menu");
            System.out.println("3. Archiver un menu");
            System.out.println("4. Lister les menus actifs");
            System.out.println("5. Voir les détails d'un menu");
            System.out.println("0. Retour au menu principal");
            System.out.print("👉 Votre choix : ");

            int choice = Integer.parseInt(scanner.nextLine());

            switch (choice) {
                case 1: createMenu(); break;
                case 2: updateMenu(); break;
                case 3: archiveMenu(); break;
                case 4: listActiveMenus(); break;
                case 5: showMenuDetails(); break;
                case 0: return;
                default: System.out.println("❌ Choix invalide !");
            }
        }
    }

    private void createMenu() {
        System.out.println("\n➕ CRÉER UN MENU");


        List<Burger> burgers = burgerService.getActiveBurgers();
        if (burgers.isEmpty()) {
            System.out.println("❌ Aucun burger disponible !");
            return;
        }

        System.out.println("\n🍔 CHOISISSEZ UN BURGER :");
        for (int i = 0; i < burgers.size(); i++) {
            Burger b = burgers.get(i);
            System.out.printf("%d. %-25s %-8.2f FCFA\n",
                    i+1, b.getLibelle(), b.getPrix());
        }
        System.out.print("👉 Votre choix : ");
        int burgerChoice = Integer.parseInt(scanner.nextLine()) - 1;
        Burger selectedBurger = burgers.get(burgerChoice);


        List<Complement> boissons = complementService.getBoissons();
        if (boissons.isEmpty()) {
            System.out.println("❌ Aucune boisson disponible !");
            return;
        }

        System.out.println("\n🥤 CHOISISSEZ UNE BOISSON :");
        for (int i = 0; i < boissons.size(); i++) {
            Complement c = boissons.get(i);
            System.out.printf("%d. %-25s %-8.2f FCFA\n",
                    i+1, c.getLibelle(), c.getPrix());
        }
        System.out.print("👉 Votre choix : ");
        int boissonChoice = Integer.parseInt(scanner.nextLine()) - 1;
        Complement selectedBoisson = boissons.get(boissonChoice);


        List<Complement> frites = complementService.getFrites();
        if (frites.isEmpty()) {
            System.out.println("❌ Aucune frite disponible !");
            return;
        }

        System.out.println("\n🍟 CHOISISSEZ DES FRITES :");
        for (int i = 0; i < frites.size(); i++) {
            Complement c = frites.get(i);
            System.out.printf("%d. %-25s %-8.2f FCFA\n",
                    i+1, c.getLibelle(), c.getPrix());
        }
        System.out.print("👉 Votre choix : ");
        int fritesChoice = Integer.parseInt(scanner.nextLine()) - 1;
        Complement selectedFrites = frites.get(fritesChoice);


        System.out.print("\n📝 Nom du menu : ");
        String nom = scanner.nextLine();

        System.out.print("📋 Description : ");
        String description = scanner.nextLine();

        try {

            int burgerId = selectedBurger.getId();
            int boissonId = selectedBoisson.getId();
            int fritesId = selectedFrites.getId();

            Menu menu = menuService.createMenuFromComponents(
                    selectedBurger.getId(),
                    selectedBoisson.getId(),
                    selectedFrites.getId(),
                    nom,
                    description
            );
            menu.setTypeProduct(TypeProduct.MENU);
            System.out.println("\n✅ MENU CRÉÉ AVEC SUCCÈS !");
            System.out.println("   Nom : " + menu.getLibelle());
            System.out.println("   Prix total : " + menu.getFormattedPrix() + " (10% de réduction incluse)");
            System.out.println("   Composants :");
            System.out.println("     - " + selectedBurger.getLibelle());
            System.out.println("     - " + selectedBoisson.getLibelle());
            System.out.println("     - " + selectedFrites.getLibelle());

        } catch (Exception e) {
            System.out.println("❌ Erreur : " + e.getMessage());
            e.printStackTrace();
        }
    }

    private void updateMenu() {
        System.out.println("\n✏️ MODIFIER UN MENU");
        listActiveMenus();

        System.out.print("\nID du menu à modifier : ");
        int id = Integer.parseInt(scanner.nextLine());

        var menuOpt = menuService.getMenuById(id);
        if (menuOpt.isEmpty()) {
            System.out.println("❌ Menu non trouvé !");
            return;
        }

        Menu menu = menuOpt.get();

        System.out.print("Nouveau nom [" + menu.getLibelle() + "] : ");
        String newNom = scanner.nextLine();
        if (!newNom.isEmpty()) menu.setLibelle(newNom);

        System.out.print("Nouvelle description [" + menu.getDescription() + "] : ");
        String newDesc = scanner.nextLine();
        if (!newDesc.isEmpty()) menu.setDescription(newDesc);

        try {
            menuService.updateMenu(menu);
            System.out.println("✅ Menu modifié avec succès !");
        } catch (Exception e) {
            System.out.println("❌ Erreur : " + e.getMessage());
        }
    }

    private void archiveMenu() {
        System.out.println("\n📦 ARCHIVER UN MENU");
        listActiveMenus();

        System.out.print("\nID du menu à archiver : ");
        int id = Integer.parseInt(scanner.nextLine());

        System.out.print("Êtes-vous sûr ? (o/n) : ");
        String confirm = scanner.nextLine();

        if (!confirm.equalsIgnoreCase("o")) {
            System.out.println("❌ Archivage annulé !");
            return;
        }

        try {
            if (menuService.deleteMenu(id)) {
                System.out.println("✅ Menu archivé avec succès !");
            } else {
                System.out.println("❌ Échec de l'archivage !");
            }
        } catch (Exception e) {
            System.out.println("❌ Erreur : " + e.getMessage());
        }
    }

    private void listActiveMenus() {
        System.out.println("\n🍟🥤 MENUS ACTIFS :");
        System.out.println("═══════════════════════════════════════════════");

        List<Menu> menus = menuService.getActiveMenus();
        if (menus.isEmpty()) {
            System.out.println("Aucun menu actif.");
        } else {
            for (Menu m : menus) {
                System.out.printf("ID: %-4d %-25s %-10s\n",
                        m.getId(), m.getLibelle(), m.getFormattedPrix());
            }
        }
    }

    private void showMenuDetails() {
        System.out.println("\n🔍 DÉTAILS D'UN MENU");
        listActiveMenus();

        System.out.print("\nID du menu à afficher : ");
        int id = Integer.parseInt(scanner.nextLine());

        var menuOpt = menuService.getMenuById(id);
        if (menuOpt.isEmpty()) {
            System.out.println("❌ Menu non trouvé !");
            return;
        }

        Menu menu = menuOpt.get();

        System.out.println("\n📋 DÉTAILS DU MENU :");
        System.out.println("   Nom : " + menu.getLibelle());
        System.out.println("   Description : " + menu.getDescription());
        System.out.println("   Prix : " + menu.getFormattedPrix());

        System.out.println("\nAppuyez sur Entrée pour continuer...");
        scanner.nextLine();
    }
}