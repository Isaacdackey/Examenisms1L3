package BB.resto.Repository.Contract;

import BB.resto.Entity.Complement;
import BB.resto.Entity.Enumeration.TypeComplement;
import java.util.List;

public interface IComplementRepository extends IBaseRepository<Complement> {

    List<Complement> findByType(TypeComplement type);
    List<Complement> findBoissons();
    List<Complement> findFrites();
    List<Complement> findActiveComplements();
}