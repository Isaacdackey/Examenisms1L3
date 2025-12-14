package BB.resto.Services.Contract;

import BB.resto.Entity.Menu;
import BB.resto.Entity.Burger;
import BB.resto.Entity.Complement;
import java.util.List;
import java.util.Optional;

public interface IMenuService {

    Menu createMenu(Menu menu);
    Menu createMenuFromComponents(int burgerId, int boissonId, int fritesId, String libelle, String description);
    Menu updateMenu(Menu menu);
    boolean deleteMenu(int id);
    Optional<Menu> getMenuById(int id);
    List<Menu> getAllMenus();


    double calculateMenuPrice(int burgerId, int boissonId, int fritesId);
    double calculateMenuPriceWithDiscount(int burgerId, int boissonId, int fritesId, double discountPercent);


    List<Menu> getActiveMenus();
    Optional<Menu> getCompleteMenuDetails(int id);


    boolean isValidMenuComposition(int burgerId, int boissonId, int fritesId);


    long countMenus();


    boolean uploadMenuImage(int menuId, String imagePath);
}