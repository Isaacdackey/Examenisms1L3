<?php

namespace App\Repository\Query;

use App\Entity\Burger;
use App\Repository\BaseRepository;
use Doctrine\Persistence\ManagerRegistry;

class BurgerRepository extends BaseRepository
{
    public function __construct(ManagerRegistry $registry)
    {
        parent::__construct($registry, Burger::class);
    }

    public function findAllOrdered(string $orderBy = 'libelle', string $direction = 'ASC'): array
    {
        return $this->createQueryBuilderAlias('b')
            ->orderBy('b.' . $orderBy, $direction)
            ->getQuery()
            ->getResult();
    }

    public function findActive(): array
    {
        return $this->createQueryBuilderAlias('b')
            ->where('b.isArchived = :archived')
            ->setParameter('archived', false)
            ->orderBy('b.libelle', 'ASC')
            ->getQuery()
            ->getResult();
    }

    public function findArchived(): array
    {
        return $this->createQueryBuilderAlias('b')
            ->where('b.isArchived = :archived')
            ->setParameter('archived', true)
            ->orderBy('b.libelle', 'ASC')
            ->getQuery()
            ->getResult();
    }

    public function searchByCriteria(array $criteria): array
    {
        $qb = $this->createQueryBuilderAlias('b');

        if (!empty($criteria['libelle'])) {
            $qb->andWhere('b.libelle LIKE :libelle')
               ->setParameter('libelle', '%' . $criteria['libelle'] . '%');
        }

        if (isset($criteria['typeBurger'])) {
            $qb->andWhere('b.typeBurger = :type')
               ->setParameter('type', $criteria['typeBurger']);
        }

        if (isset($criteria['isArchived'])) {
            $qb->andWhere('b.isArchived = :archived')
               ->setParameter('archived', $criteria['isArchived']);
        }

        return $qb->orderBy('b.libelle', 'ASC')
                  ->getQuery()
                  ->getResult();
    }

    public function countActive(): int
    {
        return $this->createQueryBuilderAlias('b')
            ->select('COUNT(b.id)')
            ->where('b.isArchived = :archived')
            ->setParameter('archived', false)
            ->getQuery()
            ->getSingleScalarResult();
    }

    public function findWithPagination(int $page = 1, int $limit = 10, array $criteria = []): array
    {
        $offset = ($page - 1) * $limit;
        
        $qb = $this->createQueryBuilderAlias('b');

        foreach ($criteria as $field => $value) {
            if (!empty($value)) {
                $qb->andWhere("b.$field = :$field")
                   ->setParameter($field, $value);
            }
        }

        return $qb->orderBy('b.libelle', 'ASC')
                  ->setFirstResult($offset)
                  ->setMaxResults($limit)
                  ->getQuery()
                  ->getResult();
    }
}