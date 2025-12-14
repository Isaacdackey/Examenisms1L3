package BB.resto.Repository.Contract;

import BB.resto.Entity.Menu;
import java.util.List;
import java.util.Optional;

public interface IMenuRepository extends IBaseRepository<Menu> {

    List<Menu> findActiveMenus();


    double calculateMenuPrice(int burgerId, int boissonId, int fritesId);


    Optional<Menu> findCompleteMenu(int id);
}