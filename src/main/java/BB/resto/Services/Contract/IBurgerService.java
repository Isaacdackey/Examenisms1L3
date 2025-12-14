package BB.resto.Services.Contract;

import BB.resto.Entity.Burger;
import BB.resto.Entity.Enumeration.TypeBurger;
import java.util.List;
import java.util.Optional;

public interface IBurgerService {

    Burger createBurger(Burger burger);
    Burger createBurgerWithImage(Burger burger, String imagePath);
    Burger updateBurger(Burger burger);
    boolean deleteBurger(int id);
    Optional<Burger> getBurgerById(int id);
    List<Burger> getAllBurgers();


    List<Burger> getBurgersByType(TypeBurger type);
    List<Burger> getActiveBurgers();
    List<Burger> getBurgersByCaloriesMax(int maxCalories);
    List<Burger> getBurgersByPreparationTimeMax(int maxTime);


    boolean uploadBurgerImage(int burgerId, String imagePath);


    long countBurgers();
    long countBurgersByType(TypeBurger type);


    boolean isBurgerAvailable(int id);
    double calculateBurgerPriceWithComplement(int burgerId, int complementId);
}