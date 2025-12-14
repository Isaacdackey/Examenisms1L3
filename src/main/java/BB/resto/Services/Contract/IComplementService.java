package BB.resto.Services.Contract;

import BB.resto.Entity.Complement;
import BB.resto.Entity.Enumeration.TypeComplement;
import java.util.List;
import java.util.Optional;

public interface IComplementService {

    Complement createComplement(Complement complement);
    Complement updateComplement(Complement complement);
    boolean deleteComplement(int id);
    Optional<Complement> getComplementById(int id);
    List<Complement> getAllComplements();


    List<Complement> getComplementsByType(TypeComplement type);
    List<Complement> getBoissons();
    List<Complement> getFrites();
    List<Complement> getActiveComplements();

    long countComplements();
    long countComplementsByType(TypeComplement type);


    List<Complement> getAvailableBoissonsForMenu();
    List<Complement> getAvailableFritesForMenu();
}