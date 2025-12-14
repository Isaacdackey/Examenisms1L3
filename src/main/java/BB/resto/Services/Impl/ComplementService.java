package BB.resto.Services.Impl;

import BB.resto.Services.Contract.IComplementService;
import BB.resto.Entity.Complement;
import BB.resto.Entity.Enumeration.TypeComplement;
import BB.resto.Repository.Impl.ComplementRepository;
import java.util.List;
import java.util.Optional;

public class ComplementService implements IComplementService {
    private ComplementRepository complementRepository;

    public ComplementService() {
        this.complementRepository = new ComplementRepository();
    }

    @Override
    public Complement createComplement(Complement complement) {
        return complementRepository.save(complement);
    }

    @Override
    public Complement updateComplement(Complement complement) {
        return complementRepository.update(complement);
    }

    @Override
    public boolean deleteComplement(int id) {
        return complementRepository.delete(id);
    }

    @Override
    public Optional<Complement> getComplementById(int id) {
        return complementRepository.findById(id);
    }

    @Override
    public List<Complement> getAllComplements() {
        return complementRepository.findAll();
    }

    @Override
    public List<Complement> getComplementsByType(TypeComplement type) {
        return complementRepository.findByType(type);
    }

    @Override
    public List<Complement> getBoissons() {
        return complementRepository.findBoissons();
    }

    @Override
    public List<Complement> getFrites() {
        return complementRepository.findFrites();
    }

    @Override
    public List<Complement> getActiveComplements() {
        return complementRepository.findActiveComplements();
    }

    @Override
    public long countComplements() {
        return complementRepository.count();
    }

    @Override
    public long countComplementsByType(TypeComplement type) {
        return complementRepository.findByType(type).size();
    }

    @Override
    public List<Complement> getAvailableBoissonsForMenu() {
        return getBoissons().stream()
                .filter(complement -> !complement.isArchived())
                .toList();
    }

    @Override
    public List<Complement> getAvailableFritesForMenu() {
        return getFrites().stream()
                .filter(complement -> !complement.isArchived())
                .toList();
    }
}