<?php

namespace App\Repository\Query;

use App\Entity\Menu;
use App\Repository\BaseRepository;
use Doctrine\Persistence\ManagerRegistry;

class MenuRepository extends BaseRepository
{
    public function __construct(ManagerRegistry $registry)
    {
        parent::__construct($registry, Menu::class);
    }

    public function findAllOrdered(string $orderBy = 'libelle', string $direction = 'ASC'): array
    {
        return $this->createQueryBuilderAlias('m')
            ->leftJoin('m.burger', 'b')
            ->leftJoin('m.boisson', 'bo')
            ->leftJoin('m.frites', 'f')
            ->addSelect('b', 'bo', 'f')
            ->orderBy('m.' . $orderBy, $direction)
            ->getQuery()
            ->getResult();
    }

    public function findActive(): array
    {
        return $this->createQueryBuilderAlias('m')
            ->leftJoin('m.burger', 'b')
            ->leftJoin('m.boisson', 'bo')
            ->leftJoin('m.frites', 'f')
            ->addSelect('b', 'bo', 'f')
            ->where('m.isArchived = :archived')
            ->setParameter('archived', false)
            ->orderBy('m.libelle', 'ASC')
            ->getQuery()
            ->getResult();
    }

    public function findArchived(): array
    {
        return $this->createQueryBuilderAlias('m')
            ->leftJoin('m.burger', 'b')
            ->leftJoin('m.boisson', 'bo')
            ->leftJoin('m.frites', 'f')
            ->addSelect('b', 'bo', 'f')
            ->where('m.isArchived = :archived')
            ->setParameter('archived', true)
            ->orderBy('m.libelle', 'ASC')
            ->getQuery()
            ->getResult();
    }

    public function findWithComposition(): array
    {
        return $this->createQueryBuilderAlias('m')
            ->innerJoin('m.burger', 'b')
            ->innerJoin('m.boisson', 'bo')
            ->innerJoin('m.frites', 'f')
            ->addSelect('b', 'bo', 'f')
            ->where('m.isArchived = false')
            ->andWhere('b.isArchived = false')
            ->andWhere('bo.isArchived = false')
            ->andWhere('f.isArchived = false')
            ->orderBy('m.libelle', 'ASC')
            ->getQuery()
            ->getResult();
    }

    public function countByBurger(int $burgerId): int
    {
        return $this->createQueryBuilderAlias('m')
            ->select('COUNT(m.id)')
            ->where('m.burger = :burgerId')
            ->andWhere('m.isArchived = false')
            ->setParameter('burgerId', $burgerId)
            ->getQuery()
            ->getSingleScalarResult();
    }
}