package BB.resto.Services.Impl;

import BB.resto.Services.Contract.IBurgerService;
import BB.resto.Entity.Burger;
import BB.resto.Entity.Enumeration.TypeBurger;
import BB.resto.Repository.Impl.BurgerRepository;
import java.util.List;
import java.util.Optional;

public class BurgerService implements IBurgerService {
    private BurgerRepository burgerRepository;
    private ProductService productService;

    public BurgerService() {
        this.burgerRepository = new BurgerRepository();
        this.productService = new ProductService();
    }

    @Override
    public Burger createBurger(Burger burger) {
        return burgerRepository.save(burger);
    }

    @Override
    public Burger createBurgerWithImage(Burger burger, String imagePath) {
        Burger createdBurger = createBurger(burger);

        if (imagePath != null && !imagePath.trim().isEmpty()) {
            try {
                uploadBurgerImage(createdBurger.getId(), imagePath);
            } catch (Exception e) {
                System.err.println("⚠️ Erreur upload image burger: " + e.getMessage());
            }
        }

        return createdBurger;
    }

    @Override
    public Burger updateBurger(Burger burger) {
        return burgerRepository.update(burger);
    }

    @Override
    public boolean deleteBurger(int id) {
        return burgerRepository.delete(id);
    }

    @Override
    public Optional<Burger> getBurgerById(int id) {
        return burgerRepository.findById(id);
    }

    @Override
    public List<Burger> getAllBurgers() {
        return burgerRepository.findAll();
    }

    @Override
    public List<Burger> getBurgersByType(TypeBurger type) {
        return burgerRepository.findByType(type);
    }

    @Override
    public List<Burger> getActiveBurgers() {
        return burgerRepository.findActiveBurgers();
    }

    @Override
    public List<Burger> getBurgersByCaloriesMax(int maxCalories) {
        return burgerRepository.findByCaloriesMax(maxCalories);
    }

    @Override
    public List<Burger> getBurgersByPreparationTimeMax(int maxTime) {
        return burgerRepository.findByPreparationTimeMax(maxTime);
    }

    @Override
    public boolean uploadBurgerImage(int burgerId, String imagePath) {
        Optional<Burger> burgerOpt = getBurgerById(burgerId);
        if (burgerOpt.isEmpty()) {
            return false;
        }

        return productService.uploadProductImage(burgerId, imagePath);
    }

    @Override
    public long countBurgers() {
        return burgerRepository.count();
    }

    @Override
    public long countBurgersByType(TypeBurger type) {
        return burgerRepository.findByType(type).size();
    }

    @Override
    public boolean isBurgerAvailable(int id) {
        return getBurgerById(id)
                .map(burger -> !burger.isArchived() && burger.getPrix() > 0)
                .orElse(false);
    }

    @Override
    public double calculateBurgerPriceWithComplement(int burgerId, int complementId) {
        Optional<Burger> burgerOpt = getBurgerById(burgerId);
        Optional<Burger> complementOpt = getBurgerById(complementId);

        if (burgerOpt.isEmpty()) {
            return 0;
        }

        double total = burgerOpt.get().getPrix();


        if (complementOpt.isPresent()) {
            total += complementOpt.get().getPrix();
        }

        return total;
    }

}