<?php

namespace App\Repository\Query;

use App\Entity\Commande;
use App\Repository\BaseRepository;
use Doctrine\Persistence\ManagerRegistry;

class StatistiqueRepository extends BaseRepository
{
    public function __construct(ManagerRegistry $registry)
    {
        parent::__construct($registry, Commande::class);
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

    public function getStatistiquesCompletes(\DateTime $date): array
    {
        $commandeRepo = $this->_em->getRepository(Commande::class);
        
        $statistiques = $commandeRepo->getStatistiquesCompletes($date);
        
        return [
            'burgersPlusVendus' => $statistiques['burgersPlusVendus'],
            'menusPlusVendus' => $statistiques['menusPlusVendus'],
            'commandesParStatut' => $statistiques['commandesParStatut'],
            'commandesParTypeRetrait' => $commandeRepo->getCommandesParTypeRetrait($date),
            'produitsPlusVendus' => $statistiques['produitsPlusVendus'],
            'commandesParLivreur' => $commandeRepo->getCommandesParLivreur($date),
            'recetteTotale' => $statistiques['recetteTotale'],
            'totalCommandes' => $statistiques['totalCommandes'],
            'commandesAnnuleesAujourdhui' => $statistiques['commandesAnnuleesAujourdhui'],
        ];
    }
}