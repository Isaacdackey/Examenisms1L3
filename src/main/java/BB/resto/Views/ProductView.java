package BB.resto.Views;

import BB.resto.Services.Impl.BurgerService;
import BB.resto.Services.Impl.MenuService;
import BB.resto.Services.Impl.ComplementService;
import java.util.Scanner;

public class ProductView {
    private Scanner scanner = new Scanner(System.in);
    private BurgerService burgerService = new BurgerService();
    private MenuService menuService = new MenuService();
    private ComplementService complementService = new ComplementService();

    public void showProducts() {
        System.out.println("\n═══════════════════════════════════════════════");
        System.out.println("            📦 CATALOGUE COMPLET               ");
        System.out.println("═══════════════════════════════════════════════");

        System.out.println("\n🍔 BURGERS :");
        burgerService.getActiveBurgers().forEach(b ->
                System.out.printf("  %-25s %-8.2f FCFA\n", b.getLibelle(), b.getPrix()));

        System.out.println("\n🍟🥤 MENUS :");
        menuService.getActiveMenus().forEach(m ->
                System.out.printf("  %-25s %-8.2f FCFA\n", m.getLibelle(), m.getPrix()));

        System.out.println("\n🥤 BOISSONS :");
        complementService.getBoissons().forEach(b ->
                System.out.printf("  %-25s %-8.2f FCFA\n", b.getLibelle(), b.getPrix()));

        System.out.println("\n🍟 FRITES :");
        complementService.getFrites().forEach(f ->
                System.out.printf("  %-25s %-8.2f FCFA\n", f.getLibelle(), f.getPrix()));

        System.out.println("\nAppuyez sur Entrée pour continuer...");
        scanner.nextLine();
    }
}