<?php

namespace App\Repository;

use Doctrine\Bundle\DoctrineBundle\Repository\ServiceEntityRepository;
use Doctrine\Persistence\ManagerRegistry;

abstract class BaseRepository extends ServiceEntityRepository
{
    protected $_em;
    
    public function __construct(ManagerRegistry $registry, string $entityClass)
    {
        parent::__construct($registry, $entityClass);
        $this->_em = $this->getEntityManager();
    }

    protected function createQueryBuilderAlias(string $alias = 'entity')
    {
        return $this->createQueryBuilder($alias);
    }

    public function save(object $entity, bool $flush = true): void
    {
        $this->_em->persist($entity);
        if ($flush) {
            $this->_em->flush();
        }
    }

    public function remove(object $entity, bool $flush = true): void
    {
        $this->_em->remove($entity);
        if ($flush) {
            $this->_em->flush();
        }
    }

    public function flush(): void
    {
        $this->_em->flush();
    }

    public function countBy(array $criteria = []): int
    {
        $qb = $this->createQueryBuilderAlias('e')
            ->select('COUNT(e.id)');

        foreach ($criteria as $field => $value) {
            $qb->andWhere("e.$field = :$field")
               ->setParameter($field, $value);
        }

        return (int) $qb->getQuery()->getSingleScalarResult();
    }
}