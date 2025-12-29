<?php

namespace App\Repository\Query;

use App\Entity\Complement;
use App\Repository\BaseRepository;
use Doctrine\Persistence\ManagerRegistry;

class ComplementRepository extends BaseRepository
{
    public function __construct(ManagerRegistry $registry)
    {
        parent::__construct($registry, Complement::class);
    }

    public function findAllOrdered(): array
    {
        return $this->createQueryBuilderAlias('c')
            ->orderBy('c.typeComplement', 'ASC')
            ->addOrderBy('c.libelle', 'ASC')
            ->getQuery()
            ->getResult();
    }

    public function findActive(): array
    {
        return $this->createQueryBuilderAlias('c')
            ->where('c.isArchived = :archived')
            ->setParameter('archived', false)
            ->orderBy('c.libelle', 'ASC')
            ->getQuery()
            ->getResult();
    }

    public function findArchived(): array
    {
        return $this->createQueryBuilderAlias('c')
            ->where('c.isArchived = :archived')
            ->setParameter('archived', true)
            ->orderBy('c.libelle', 'ASC')
            ->getQuery()
            ->getResult();
    }

    public function findByType(string $type): array
    {
        return $this->createQueryBuilderAlias('c')
            ->where('c.typeComplement = :type')
            ->andWhere('c.isArchived = false')
            ->setParameter('type', $type)
            ->orderBy('c.libelle', 'ASC')
            ->getQuery()
            ->getResult();
    }

    public function findBoissons(): array
    {
        return $this->findByType('BOISSON');
    }

    public function findFrites(): array
    {
        return $this->findByType('FRITE');
    }

    public function searchByCriteria(array $criteria): array
    {
        $qb = $this->createQueryBuilderAlias('c');

        if (!empty($criteria['libelle'])) {
            $qb->andWhere('c.libelle LIKE :libelle')
               ->setParameter('libelle', '%' . $criteria['libelle'] . '%');
        }

        if (!empty($criteria['typeComplement'])) {
            $qb->andWhere('c.typeComplement = :type')
               ->setParameter('type', $criteria['typeComplement']);
        }

        if (isset($criteria['isArchived'])) {
            $qb->andWhere('c.isArchived = :archived')
               ->setParameter('archived', $criteria['isArchived']);
        }

        return $qb->orderBy('c.libelle', 'ASC')
                  ->getQuery()
                  ->getResult();
    }

    public function countByType(string $type): int
    {
        return $this->createQueryBuilderAlias('c')
            ->select('COUNT(c.id)')
            ->where('c.typeComplement = :type')
            ->andWhere('c.isArchived = false')
            ->setParameter('type', $type)
            ->getQuery()
            ->getSingleScalarResult();
    }
}