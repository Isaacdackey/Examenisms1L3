package BB.resto.Views;

import BB.resto.Entity.Complement;
import BB.resto.Entity.Enumeration.TypeComplement;
import BB.resto.Entity.Enumeration.TypeProduct;
import BB.resto.Services.Impl.ComplementService;
import java.util.*;

public class ComplementView {
    private Scanner scanner = new Scanner(System.in);
    private ComplementService complementService = new ComplementService();

    public void showMenu() {
        while (true) {
            System.out.println("\n═══════════════════════════════════════════════");
            System.out.println("         GESTION DES COMPLÉMENTS 🥤🍟         ");
            System.out.println("═══════════════════════════════════════════════");
            System.out.println("1. Ajouter un complément");
            System.out.println("2. Modifier un complément");
            System.out.println("3. Archiver un complément");
            System.out.println("4. Lister les boissons");
            System.out.println("5. Lister les frites");
            System.out.println("6. Lister tous les compléments");
            System.out.println("0. Retour au menu principal");
            System.out.print("👉 Votre choix : ");

            int choice = Integer.parseInt(scanner.nextLine());

            switch (choice) {
                case 1: createComplement(); break;
                case 2: updateComplement(); break;
                case 3: archiveComplement(); break;
                case 4: listBoissons(); break;
                case 5: listFrites(); break;
                case 6: listAllComplements(); break;
                case 0: return;
                default: System.out.println("❌ Choix invalide !");
            }
        }
    }

    private void createComplement() {
        System.out.println("\n➕ AJOUTER UN COMPLÉMENT");

        System.out.print("Nom : ");
        String nom = scanner.nextLine();

        System.out.print("Description : ");
        String description = scanner.nextLine();

        System.out.print("Prix : ");
        double prix = Double.parseDouble(scanner.nextLine());

        System.out.println("Type (1: BOISSON, 2: FRITE) : ");
        int typeChoice = Integer.parseInt(scanner.nextLine());
        TypeComplement type = (typeChoice == 1) ? TypeComplement.BOISSON : TypeComplement.FRITE;

        try {
            Complement complement = new Complement();
            complement.setLibelle(nom);
            complement.setDescription(description);
            complement.setPrix(prix);
            complement.setTypeComplement(type);
            complement.setTypeProduct(TypeProduct.COMPLEMENT);
            Complement created = complementService.createComplement(complement);
            System.out.println("✅ Complément créé avec succès ! ID: " + created.getId());

        } catch (Exception e) {
            System.out.println("❌ Erreur : " + e.getMessage());
        }
    }

    private void updateComplement() {
        System.out.println("\n✏️ MODIFIER UN COMPLÉMENT");
        listAllComplements();

        System.out.print("\nID du complément à modifier : ");
        int id = Integer.parseInt(scanner.nextLine());

        var complementOpt = complementService.getComplementById(id);
        if (complementOpt.isEmpty()) {
            System.out.println("❌ Complément non trouvé !");
            return;
        }

        Complement complement = complementOpt.get();


        complement.setTypeProduct(BB.resto.Entity.Enumeration.TypeProduct.COMPLEMENT);

        System.out.print("Nouveau nom [" + complement.getLibelle() + "] : ");
        String newNom = scanner.nextLine();
        if (!newNom.isEmpty()) complement.setLibelle(newNom);

        System.out.print("Nouvelle description [" + complement.getDescription() + "] : ");
        String newDesc = scanner.nextLine();
        if (!newDesc.isEmpty()) complement.setDescription(newDesc);

        System.out.print("Nouveau prix [" + complement.getPrix() + "] : ");
        String newPrixStr = scanner.nextLine();
        if (!newPrixStr.isEmpty()) complement.setPrix(Double.parseDouble(newPrixStr));

        try {
            complementService.updateComplement(complement);
            System.out.println("✅ Complément modifié avec succès !");
        } catch (Exception e) {
            System.out.println("❌ Erreur : " + e.getMessage());
        }
    }

    private void archiveComplement() {
        System.out.println("\n📦 ARCHIVER UN COMPLÉMENT");
        listAllComplements();

        System.out.print("\nID du complément à archiver : ");
        int id = Integer.parseInt(scanner.nextLine());

        System.out.print("Êtes-vous sûr ? (o/n) : ");
        String confirm = scanner.nextLine();

        if (!confirm.equalsIgnoreCase("o")) {
            System.out.println("❌ Archivage annulé !");
            return;
        }

        try {
            if (complementService.deleteComplement(id)) {
                System.out.println("✅ Complément archivé avec succès !");
            } else {
                System.out.println("❌ Échec de l'archivage !");
            }
        } catch (Exception e) {
            System.out.println("❌ Erreur : " + e.getMessage());
        }
    }

    private void listBoissons() {
        System.out.println("\n🥤 BOISSONS DISPONIBLES :");
        List<Complement> boissons = complementService.getBoissons();
        if (boissons.isEmpty()) {
            System.out.println("Aucune boisson disponible.");
        } else {
            for (Complement c : boissons) {
                System.out.printf("ID: %-4d %-25s %-8.2f FCFA\n",
                        c.getId(), c.getLibelle(), c.getPrix());
            }
        }
    }

    private void listFrites() {
        System.out.println("\n🍟 FRITES DISPONIBLES :");
        List<Complement> frites = complementService.getFrites();
        if (frites.isEmpty()) {
            System.out.println("Aucune frite disponible.");
        } else {
            for (Complement c : frites) {
                System.out.printf("ID: %-4d %-25s %-8.2f FCFA\n",
                        c.getId(), c.getLibelle(), c.getPrix());
            }
        }
    }

    private void listAllComplements() {
        System.out.println("\n📦 TOUS LES COMPLÉMENTS :");
        List<Complement> allComplements = complementService.getAllComplements();
        if (allComplements.isEmpty()) {
            System.out.println("Aucun complément disponible.");
        } else {
            for (Complement c : allComplements) {
                String status = c.isArchived() ? "[Archivé]" : "[Actif]";
                System.out.printf("ID: %-4d %-25s %-8.2f FCFA %-10s %s\n",
                        c.getId(), c.getLibelle(), c.getPrix(),
                        c.getTypeComplement().getLabel(), status);
            }
        }
    }
}