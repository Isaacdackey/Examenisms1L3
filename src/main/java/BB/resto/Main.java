package BB.resto;

import BB.resto.Views.*;
import java.util.Scanner;

public class Main {
    private static Scanner scanner = new Scanner(System.in);

    public static void main(String[] args) {
        System.out.println("═══════════════════════════════════════════════");
        System.out.println("   BRASIL BURGER - GESTION DES RESSOURCES");
        System.out.println("      Java Console - L3 ISM - Semestre 1");
        System.out.println("═══════════════════════════════════════════════");


        BurgerView burgerView = new BurgerView();
        MenuView menuView = new MenuView();
        ComplementView complementView = new ComplementView();
        ProductView productView = new ProductView();

        while (true) {
            System.out.println("\n=== MENU PRINCIPAL ===");
            System.out.println("1. 🍔 Gérer les Burgers");
            System.out.println("2. 🍟🥤 Gérer les Menus");
            System.out.println("3. 🥤🍟 Gérer les Compléments");
            System.out.println("4. 📦 Voir le catalogue complet");
            System.out.println("0. 🚪 Quitter");
            System.out.print("👉 Votre choix : ");

            int choice = getIntInput();

            switch (choice) {
                case 1:
                    burgerView.showMenu();
                    break;
                case 2:
                    menuView.showMenu();
                    break;
                case 3:
                    complementView.showMenu();
                    break;
                case 4:
                    productView.showProducts();
                    break;
                case 0:
                    System.out.println("👋 Au revoir !");
                    scanner.close();
                    return;
                default:
                    System.out.println("❌ Choix invalide !");
            }
        }
    }

    private static int getIntInput() {
        try {
            return Integer.parseInt(scanner.nextLine());
        } catch (NumberFormatException e) {
            return -1;
        }
    }
}