package BB.resto.Repository.Contract;

import BB.resto.Entity.Burger;
import BB.resto.Entity.Enumeration.TypeBurger;
import java.util.List;

public interface IBurgerRepository extends IBaseRepository<Burger> {
    List<Burger> findByType(TypeBurger type);
    List<Burger> findActiveBurgers();
    List<Burger> findByCaloriesMax(int maxCalories);
    List<Burger> findByPreparationTimeMax(int maxTime);
}