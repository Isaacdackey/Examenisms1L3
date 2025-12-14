package BB.resto.Services.Impl;

import BB.resto.Services.Contract.IMenuService;
import BB.resto.Entity.Menu;
import BB.resto.Entity.Burger;
import BB.resto.Entity.Complement;
import BB.resto.Entity.Enumeration.TypeProduct;
import BB.resto.Repository.Impl.MenuRepository;
import BB.resto.Repository.Impl.BurgerRepository;
import BB.resto.Repository.Impl.ComplementRepository;
import java.util.List;
import java.util.Optional;

public class MenuService implements IMenuService {
    private MenuRepository menuRepository;
    private BurgerRepository burgerRepository;
    private ComplementRepository complementRepository;
    private ProductService productService;

    public MenuService() {
        this.menuRepository = new MenuRepository();
        this.burgerRepository = new BurgerRepository();
        this.complementRepository = new ComplementRepository();
        this.productService = new ProductService();
    }

    @Override
    public Menu createMenu(Menu menu) {
        if (!isValidMenuComposition(menu.getBurgerId(), menu.getBoissonId(), menu.getFritesId())) {
            throw new RuntimeException("Composition de menu invalide");
        }

        return menuRepository.save(menu);
    }

    @Override
    public Menu createMenuFromComponents(int burgerId, int boissonId, int fritesId, String libelle, String description) {

        double menuPrice = calculateMenuPriceWithDiscount(burgerId, boissonId, fritesId, 10.0);

        Menu menu = new Menu();
        menu.setLibelle(libelle);
        menu.setDescription(description);
        menu.setPrix(menuPrice);
        menu.setBurgerId(burgerId);
        menu.setBoissonId(boissonId);
        menu.setFritesId(fritesId);
        menu.setTypeProduct(TypeProduct.MENU);

        return createMenu(menu);
    }

    @Override
    public Menu updateMenu(Menu menu) {
        return menuRepository.update(menu);
    }

    @Override
    public boolean deleteMenu(int id) {
        return menuRepository.delete(id);
    }

    @Override
    public Optional<Menu> getMenuById(int id) {
        return menuRepository.findById(id);
    }

    @Override
    public List<Menu> getAllMenus() {
        return menuRepository.findAll();
    }

    @Override
    public double calculateMenuPrice(int burgerId, int boissonId, int fritesId) {
        return menuRepository.calculateMenuPrice(burgerId, boissonId, fritesId);
    }

    @Override
    public double calculateMenuPriceWithDiscount(int burgerId, int boissonId, int fritesId, double discountPercent) {
        double basePrice = calculateMenuPrice(burgerId, boissonId, fritesId);
        return basePrice * (1 - discountPercent / 100);
    }

    @Override
    public List<Menu> getActiveMenus() {
        return menuRepository.findActiveMenus();
    }

    @Override
    public Optional<Menu> getCompleteMenuDetails(int id) {
        return menuRepository.findCompleteMenu(id);
    }

    @Override
    public boolean isValidMenuComposition(int burgerId, int boissonId, int fritesId) {

        Optional<Burger> burgerOpt = burgerRepository.findById(burgerId);
        Optional<Complement> boissonOpt = complementRepository.findById(boissonId);
        Optional<Complement> fritesOpt = complementRepository.findById(fritesId);

        if (burgerOpt.isEmpty() || boissonOpt.isEmpty() || fritesOpt.isEmpty()) {
            return false;
        }

        Burger burger = burgerOpt.get();
        Complement boisson = boissonOpt.get();
        Complement frites = fritesOpt.get();


        if (burger.isArchived() || boisson.isArchived() || frites.isArchived()) {
            return false;
        }

        return true;
    }

    @Override
    public long countMenus() {
        return menuRepository.count();
    }

    @Override
    public boolean uploadMenuImage(int menuId, String imagePath) {
        Optional<Menu> menuOpt = getMenuById(menuId);
        if (menuOpt.isEmpty()) {
            return false;
        }

        return productService.uploadProductImage(menuId, imagePath);
    }
}