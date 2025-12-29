<?php

namespace App\Repository\Query;

use App\Entity\Commande;
use App\Repository\BaseRepository;
use Doctrine\Persistence\ManagerRegistry;
use App\Entity\ZoneLivraison;
use App\Entity\User;

class CommandeRepository extends BaseRepository
{
    public function __construct(ManagerRegistry $registry)
    {
        parent::__construct($registry, Commande::class);
    }

    public function findAllOrdered(string $orderBy = 'createdAt', string $direction = 'DESC'): array
    {
        return $this->createQueryBuilderAlias('c')
            ->leftJoin('c.user', 'u')
            ->leftJoin('c.zone', 'z')
            ->leftJoin('c.livreur', 'l')
            ->addSelect('u', 'z', 'l')
            ->orderBy('c.' . $orderBy, $direction)
            ->getQuery()
            ->getResult();
    }

    public function findTodayCommandes(): array
    {
        $todayStart = new \DateTime('today');
        $todayEnd = clone $todayStart;
        $todayEnd->modify('+1 day');

        return $this->createQueryBuilderAlias('c')
            ->leftJoin('c.user', 'u')
            ->leftJoin('c.zone', 'z')
            ->leftJoin('c.livreur', 'l')
            ->addSelect('u', 'z', 'l')
            ->where('c.createdAt >= :start AND c.createdAt < :end')
            ->setParameter('start', $todayStart)
            ->setParameter('end', $todayEnd)
            ->orderBy('c.createdAt', 'DESC')
            ->getQuery()
            ->getResult();
    }

    public function findCommandesByStatut(string $statut, ?\DateTime $date = null): array
    {
        $qb = $this->createQueryBuilderAlias('c')
            ->leftJoin('c.user', 'u')
            ->leftJoin('c.zone', 'z')
            ->addSelect('u', 'z')
            ->where('c.statut = :statut')
            ->setParameter('statut', $statut);

        if ($date) {
            $dateStart = clone $date;
            $dateEnd = clone $date;
            $dateEnd->modify('+1 day');

            $qb->andWhere('c.createdAt >= :start AND c.createdAt < :end')
               ->setParameter('start', $dateStart)
               ->setParameter('end', $dateEnd);
        }

        return $qb->orderBy('c.createdAt', 'DESC')
                  ->getQuery()
                  ->getResult();
    }

    public function findCommandesWithFilters(array $filters = [], int $limit = 15, int $offset = 0): array
    {
        $qb = $this->createQueryBuilderAlias('c')
            ->leftJoin('c.user', 'u')
            ->leftJoin('c.livreur', 'l')
            ->leftJoin('c.zone', 'z')
            ->leftJoin('c.lignesCommande', 'lc')
            ->leftJoin('lc.produit', 'p')
            ->addSelect('u', 'l', 'z', 'lc', 'p');

        if (!empty($filters['statut'])) {
            $qb->andWhere('c.statut = :statut')
               ->setParameter('statut', $filters['statut']);
        }

        if (!empty($filters['date'])) {
            $date = new \DateTime($filters['date']);
            $dateEnd = clone $date;
            $dateEnd->modify('+1 day');

            $qb->andWhere('c.createdAt >= :date_start AND c.createdAt < :date_end')
               ->setParameter('date_start', $date)
               ->setParameter('date_end', $dateEnd);
        }

        if (!empty($filters['client_id'])) {
            $qb->andWhere('u.id = :client_id')
               ->setParameter('client_id', $filters['client_id']);
        }

        if (!empty($filters['produit'])) {
            $qb->andWhere('p.libelle LIKE :produit OR p.id = :produit_id')
               ->setParameter('produit', '%' . $filters['produit'] . '%')
               ->setParameter('produit_id', is_numeric($filters['produit']) ? $filters['produit'] : 0);
        }

        return $qb->orderBy('c.createdAt', 'DESC')
                  ->setFirstResult($offset)
                  ->setMaxResults($limit)
                  ->getQuery()
                  ->getResult();
    }


    public function findCommandesPourLivraison($filtre = null): array
{
    if ($filtre instanceof ZoneLivraison) {
        
        return $this->findCommandesFiltrees(['zone' => $filtre]);
    } elseif (is_string($filtre)) {
        
        return $this->findCommandesFiltrees(['statut' => $filtre]);
    } else {
    
        return $this->findCommandesFiltrees();
    }
}


    public function findCommandesFiltrees(array $filtres = []): array
    {
        $qb = $this->createQueryBuilderAlias('c')
            ->leftJoin('c.user', 'u')
            ->leftJoin('c.zone', 'z')
            ->leftJoin('c.livreur', 'l')
            ->leftJoin('c.lignesCommande', 'lc')
            ->leftJoin('lc.produit', 'p')
            ->addSelect('u', 'z', 'l', 'lc', 'p')
            ->orderBy('c.createdAt', 'ASC');

        if (!empty($filtres['statut']) && $filtres['statut'] !== 'TOUS') {
            $qb->andWhere('c.statut = :statut')
            ->setParameter('statut', $filtres['statut']);
        } elseif (empty($filtres['statut']) || $filtres['statut'] === 'TOUS') {
            $qb->andWhere('c.statut IN (:statuts)')
            ->setParameter('statuts', ['EN_ATTENTE', 'VALIDEE', 'EN_PREPARATION', 'PRETE']);
        }

        if (!empty($filtres['zone']) && $filtres['zone'] instanceof ZoneLivraison) {
            $qb->andWhere('c.zone = :zone')
            ->setParameter('zone', $filtres['zone']);
        }

        if (!empty($filtres['livreur']) && $filtres['livreur'] instanceof User) {
            $qb->andWhere('c.livreur = :livreur')
            ->setParameter('livreur', $filtres['livreur']);
        }

        return $qb->getQuery()->getResult();
    }

    public function countCommandesWithFilters(array $filters = []): int
    {
        $qb = $this->createQueryBuilderAlias('c')
            ->select('COUNT(c.id)');

        if (!empty($filters['statut'])) {
            $qb->andWhere('c.statut = :statut')
               ->setParameter('statut', $filters['statut']);
        }

        if (!empty($filters['date'])) {
            $date = new \DateTime($filters['date']);
            $dateEnd = clone $date;
            $dateEnd->modify('+1 day');

            $qb->andWhere('c.createdAt >= :date_start AND c.createdAt < :date_end')
               ->setParameter('date_start', $date)
               ->setParameter('date_end', $dateEnd);
        }

        if (!empty($filters['client_id'])) {
            $qb->andWhere('c.user = :client_id')
               ->setParameter('client_id', $filters['client_id']);
        }

        return $qb->getQuery()->getSingleScalarResult();
    }

    public function getDashboardData(\DateTime $date): array
    {
        $todayStart = clone $date;
        $todayStart->setTime(0, 0, 0);
        $todayEnd = clone $date;
        $todayEnd->setTime(23, 59, 59);

        $commandesEnCours = $this->createQueryBuilderAlias('c')
            ->leftJoin('c.user', 'u')
            ->addSelect('u')
            ->where('c.createdAt >= :start AND c.createdAt <= :end')
            ->andWhere('c.statut IN (:statuts)')
            ->setParameter('start', $todayStart)
            ->setParameter('end', $todayEnd)
            ->setParameter('statuts', ['EN_ATTENTE', 'VALIDEE', 'EN_PREPARATION', 'PRETE', 'EN_LIVRAISON'])
            ->orderBy('c.createdAt', 'DESC')
            ->getQuery()
            ->getResult();

        $commandesValidees = $this->createQueryBuilderAlias('c')
            ->leftJoin('c.user', 'u')
            ->addSelect('u')
            ->where('c.createdAt >= :start AND c.createdAt <= :end')
            ->andWhere('c.statut = :statut')
            ->setParameter('start', $todayStart)
            ->setParameter('end', $todayEnd)
            ->setParameter('statut', 'VALIDEE')
            ->getQuery()
            ->getResult();

        $commandesAnnulees = $this->createQueryBuilderAlias('c')
            ->leftJoin('c.user', 'u')
            ->addSelect('u')
            ->where('c.createdAt >= :start AND c.createdAt <= :end')
            ->andWhere('c.statut = :statut')
            ->setParameter('start', $todayStart)
            ->setParameter('end', $todayEnd)
            ->setParameter('statut', 'ANNULEE')
            ->getQuery()
            ->getResult();

        $recettesJournalieres = $this->_em->createQuery(
            'SELECT SUM(c.montantTotal) as total
            FROM App\Entity\Commande c
            WHERE c.createdAt >= :today_start AND c.createdAt <= :today_end
            AND c.statut IN (:statuts)'
        )
            ->setParameter('today_start', $todayStart)
            ->setParameter('today_end', $todayEnd)
            ->setParameter('statuts', ['VALIDEE', 'LIVREE', 'TERMINEE'])
            ->getSingleScalarResult();

        return [
            'commandesEnCours' => $commandesEnCours,
            'commandesValidees' => $commandesValidees,
            'commandesAnnulees' => $commandesAnnulees,
            'recettesJournalieres' => $recettesJournalieres ?? 0,
            'nbCommandesEnCours' => count($commandesEnCours),
            'nbCommandesValidees' => count($commandesValidees),
            'nbCommandesAnnulees' => count($commandesAnnulees),
        ];
    }

    public function getRecetteJournaliere(\DateTime $date): float
    {
        $dateStart = clone $date;
        $dateStart->setTime(0, 0, 0);
        $dateEnd = clone $date;
        $dateEnd->setTime(23, 59, 59);

        $result = $this->createQueryBuilderAlias('c')
            ->select('SUM(c.montantTotal) as total')
            ->where('c.createdAt >= :start AND c.createdAt <= :end')
            ->andWhere('c.statut IN (:statuts)')
            ->setParameter('start', $dateStart)
            ->setParameter('end', $dateEnd)
            ->setParameter('statuts', ['VALIDEE', 'LIVREE', 'TERMINEE'])
            ->getQuery()
            ->getSingleScalarResult();

        return (float) ($result ?? 0);
    }

    public function getStatistiquesCompletes(\DateTime $date): array
    {
        $dateStart = clone $date;
        $dateStart->setTime(0, 0, 0);
        $dateEnd = clone $date;
        $dateEnd->setTime(23, 59, 59);

        $burgersPlusVendus = $this->_em->createQuery(
            'SELECT p.libelle, SUM(lc.quantite) as total
            FROM App\Entity\LigneCommande lc
            JOIN lc.produit p
            JOIN lc.commande c
            WHERE c.createdAt >= :start
            AND c.createdAt <= :end
            AND p INSTANCE OF App\Entity\Burger
            AND c.statut != :statut
            GROUP BY p.id, p.libelle
            ORDER BY total DESC'
        )
            ->setParameter('start', $dateStart)
            ->setParameter('end', $dateEnd)
            ->setParameter('statut', 'ANNULEE')
            ->setMaxResults(10)
            ->getResult();

        $menusPlusVendus = $this->_em->createQuery(
            'SELECT p.libelle, SUM(lc.quantite) as total
            FROM App\Entity\LigneCommande lc
            JOIN lc.produit p
            JOIN lc.commande c
            WHERE c.createdAt >= :start
            AND c.createdAt <= :end
            AND p INSTANCE OF App\Entity\Menu
            AND c.statut != :statut
            GROUP BY p.id, p.libelle
            ORDER BY total DESC'
        )
            ->setParameter('start', $dateStart)
            ->setParameter('end', $dateEnd)
            ->setParameter('statut', 'ANNULEE')
            ->setMaxResults(5)
            ->getResult();

        $commandesParStatut = $this->createQueryBuilderAlias('c')
            ->select('c.statut, COUNT(c.id) as count')
            ->where('c.createdAt >= :start AND c.createdAt <= :end')
            ->setParameter('start', $dateStart)
            ->setParameter('end', $dateEnd)
            ->groupBy('c.statut')
            ->orderBy('c.statut', 'ASC')
            ->getQuery()
            ->getResult();

        $commandesParTypeRetrait = $this->getCommandesParTypeRetrait($date);

        $produitsPlusVendus = $this->_em->createQuery(
            'SELECT p.libelle, 
                CASE 
                    WHEN p INSTANCE OF App\Entity\Burger THEN \'Burger\'
                    WHEN p INSTANCE OF App\Entity\Menu THEN \'Menu\'
                    WHEN p INSTANCE OF App\Entity\Complement THEN \'Complément\'
                    ELSE \'Produit\'
                END as type,
                SUM(lc.quantite) as total
            FROM App\Entity\LigneCommande lc
            JOIN lc.produit p
            JOIN lc.commande c
            WHERE c.createdAt >= :start
            AND c.createdAt <= :end
            AND c.statut != :statut
            GROUP BY p.id, p.libelle
            ORDER BY total DESC'
        )
            ->setParameter('start', $dateStart)
            ->setParameter('end', $dateEnd)
            ->setParameter('statut', 'ANNULEE')
            ->setMaxResults(15)
            ->getResult();

        $commandesParLivreur = $this->getCommandesParLivreur($date);

        $totalCommandes = $this->createQueryBuilderAlias('c')
            ->select('COUNT(c.id)')
            ->where('c.createdAt >= :start AND c.createdAt <= :end')
            ->setParameter('start', $dateStart)
            ->setParameter('end', $dateEnd)
            ->getQuery()
            ->getSingleScalarResult();

        $commandesAnnulees = $this->createQueryBuilderAlias('c')
            ->select('COUNT(c.id)')
            ->where('c.createdAt >= :start AND c.createdAt <= :end')
            ->andWhere('c.statut = :statut')
            ->setParameter('start', $dateStart)
            ->setParameter('end', $dateEnd)
            ->setParameter('statut', 'ANNULEE')
            ->getQuery()
            ->getSingleScalarResult();

        return [
            'burgersPlusVendus' => $burgersPlusVendus,
            'menusPlusVendus' => $menusPlusVendus,
            'commandesParStatut' => $commandesParStatut,
            'commandesParTypeRetrait' => $commandesParTypeRetrait,
            'produitsPlusVendus' => $produitsPlusVendus,
            'commandesParLivreur' => $commandesParLivreur,
            'recetteTotale' => $this->getRecetteJournaliere($date),
            'totalCommandes' => $totalCommandes,
            'commandesAnnuleesAujourdhui' => $commandesAnnulees,
        ];
    }

    public function getBurgersPlusVendus(\DateTime $date): array
    {
        $dateStart = clone $date;
        $dateStart->setTime(0, 0, 0);
        $dateEnd = clone $date;
        $dateEnd->setTime(23, 59, 59);

        return $this->_em->createQuery(
            'SELECT p.libelle, SUM(lc.quantite) as total
            FROM App\Entity\LigneCommande lc
            JOIN lc.produit p
            JOIN lc.commande c
            WHERE c.createdAt >= :start
            AND c.createdAt <= :end
            AND p INSTANCE OF App\Entity\Burger
            AND c.statut != :statut
            GROUP BY p.id, p.libelle
            ORDER BY total DESC'
        )
            ->setParameter('start', $dateStart)
            ->setParameter('end', $dateEnd)
            ->setParameter('statut', 'ANNULEE')
            ->setMaxResults(10)
            ->getResult();
    }

    public function getMenusPlusVendus(\DateTime $date): array
    {
        $dateStart = clone $date;
        $dateStart->setTime(0, 0, 0);
        $dateEnd = clone $date;
        $dateEnd->setTime(23, 59, 59);

        return $this->_em->createQuery(
            'SELECT p.libelle, SUM(lc.quantite) as total
            FROM App\Entity\LigneCommande lc
            JOIN lc.produit p
            JOIN lc.commande c
            WHERE c.createdAt >= :start
            AND c.createdAt <= :end
            AND p INSTANCE OF App\Entity\Menu
            AND c.statut != :statut
            GROUP BY p.id, p.libelle
            ORDER BY total DESC'
        )
            ->setParameter('start', $dateStart)
            ->setParameter('end', $dateEnd)
            ->setParameter('statut', 'ANNULEE')
            ->setMaxResults(5)
            ->getResult();
    }

    public function getCommandesParTypeRetrait(\DateTime $date): array
    {
        $dateStart = clone $date;
        $dateStart->setTime(0, 0, 0);
        $dateEnd = clone $date;
        $dateEnd->setTime(23, 59, 59);

        return $this->createQueryBuilderAlias('c')
            ->select('c.typeRetrait, COUNT(c.id) as count, SUM(c.montantTotal) as total')
            ->where('c.createdAt >= :start AND c.createdAt <= :end')
            ->setParameter('start', $dateStart)
            ->setParameter('end', $dateEnd)
            ->groupBy('c.typeRetrait')
            ->getQuery()
            ->getResult();
    }

    public function getTopProduitsVendus(\DateTime $date): array
    {
        $dateStart = clone $date;
        $dateStart->setTime(0, 0, 0);
        $dateEnd = clone $date;
        $dateEnd->setTime(23, 59, 59);

        return $this->_em->createQuery(
            'SELECT p.libelle, 
                   CASE 
                       WHEN p INSTANCE OF App\Entity\Burger THEN \'Burger\'
                       WHEN p INSTANCE OF App\Entity\Menu THEN \'Menu\'
                       WHEN p INSTANCE OF App\Entity\Complement THEN \'Complément\'
                       ELSE \'Produit\'
                   END as type,
                   SUM(lc.quantite) as total
            FROM App\Entity\LigneCommande lc
            JOIN lc.produit p
            JOIN lc.commande c
            WHERE c.createdAt >= :start
            AND c.createdAt <= :end
            AND c.statut != :statut
            GROUP BY p.id, p.libelle
            ORDER BY total DESC'
        )
            ->setParameter('start', $dateStart)
            ->setParameter('end', $dateEnd)
            ->setParameter('statut', 'ANNULEE')
            ->setMaxResults(15)
            ->getResult();
    }

    public function getCommandesParLivreur(\DateTime $date): array
    {
        $dateStart = clone $date;
        $dateStart->setTime(0, 0, 0);
        $dateEnd = clone $date;
        $dateEnd->setTime(23, 59, 59);

        return $this->createQueryBuilderAlias('c')
            ->select('l.nom, l.prenom, COUNT(c.id) as nbCommandes, SUM(c.montantTotal) as totalLivraisons')
            ->leftJoin('c.livreur', 'l')
            ->where('c.createdAt >= :start AND c.createdAt <= :end')
            ->andWhere('c.livreur IS NOT NULL')
            ->setParameter('start', $dateStart)
            ->setParameter('end', $dateEnd)
            ->groupBy('l.id', 'l.nom', 'l.prenom')
            ->orderBy('nbCommandes', 'DESC')
            ->getQuery()
            ->getResult();
    }

    public function getTotalCommandesParDate(\DateTime $date): int
    {
        $dateStart = clone $date;
        $dateStart->setTime(0, 0, 0);
        $dateEnd = clone $date;
        $dateEnd->setTime(23, 59, 59);

        return $this->createQueryBuilderAlias('c')
            ->select('COUNT(c.id)')
            ->where('c.createdAt >= :start AND c.createdAt <= :end')
            ->setParameter('start', $dateStart)
            ->setParameter('end', $dateEnd)
            ->getQuery()
            ->getSingleScalarResult();
    }
}