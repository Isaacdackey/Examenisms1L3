<?php

namespace App\Service\Manager;

use App\Entity\Commande;
use App\Entity\User;
use App\Entity\ZoneLivraison;
use App\Repository\Query\CommandeRepository;
use Doctrine\ORM\EntityManagerInterface;

class LivraisonManager
{
    public function __construct(
        private CommandeRepository $commandeRepository,
        private EntityManagerInterface $em
    ) {}

    public function getCommandesParZone(?string $statutFiltre = null): array
    {
        $commandesPourLivraison = $this->commandeRepository->findCommandesPourLivraison($statutFiltre);

        $commandesParZone = [];
        foreach ($commandesPourLivraison as $commande) {
            $zoneId = $commande->getZone() ? $commande->getZone()->getId() : 0;
            $zoneNom = $commande->getZone() ? $commande->getZone()->getNom() : 'Sans zone';

            if (!isset($commandesParZone[$zoneId])) {
                $commandesParZone[$zoneId] = [
                    'zone' => $commande->getZone(),
                    'nom' => $zoneNom,
                    'commandes' => [],
                    'total' => 0,
                    'pretes' => 0
                ];
            }

            $commandesParZone[$zoneId]['commandes'][] = $commande;
            $commandesParZone[$zoneId]['total']++;
            
            if ($commande->getStatut() === 'PRETE') {
                $commandesParZone[$zoneId]['pretes']++;
            }
        }

        return $commandesParZone;
    }

    public function getLivreursDisponibles(): array
    {
        return $this->em->getRepository(User::class)->createQueryBuilder('u')
            ->where('u.role = :role')
            ->andWhere('u.isActive = true')
            ->setParameter('role', 'LIVREUR')
            ->orderBy('u.nom', 'ASC')
            ->getQuery()
            ->getResult();
    }

    public function getZonesLivraison(): array
    {
        return $this->em->getRepository(ZoneLivraison::class)->createQueryBuilder('z')
            ->orderBy('z.nom', 'ASC')
            ->getQuery()
            ->getResult();
    }

    public function calculerTourneesOptimisees(array $commandes): array
    {
        $tournees = [];
        
        foreach ($commandes as $commande) {
            $zoneId = $commande->getZone() ? $commande->getZone()->getId() : 0;
            
            if (!isset($tournees[$zoneId])) {
                $tournees[$zoneId] = [
                    'zone' => $commande->getZone(),
                    'commandes' => [],
                    'nombre' => 0,
                    'montantTotal' => 0
                ];
            }
            
            $tournees[$zoneId]['commandes'][] = $commande;
            $tournees[$zoneId]['nombre']++;
            $tournees[$zoneId]['montantTotal'] += (float) $commande->getMontantTotal();
        }

        
        usort($tournees, function($a, $b) {
            return $b['nombre'] <=> $a['nombre'];
        });

        return $tournees;
    }


    public function getStatistiquesLivraisons(\DateTime $date): array
    {
        $commandesLivrees = $this->commandeRepository->findCommandesByStatut('LIVREE', $date);
        $commandesEnLivraison = $this->commandeRepository->findCommandesByStatut('EN_LIVRAISON', $date);
        
        $totalLivraisons = count($commandesLivrees) + count($commandesEnLivraison);
        $tauxReussite = $totalLivraisons > 0 ? (count($commandesLivrees) / $totalLivraisons) * 100 : 0;

        return [
            'livraisonsReussies' => count($commandesLivrees),
            'livraisonsEnCours' => count($commandesEnLivraison),
            'tauxReussite' => round($tauxReussite, 2),
            'totalLivraisons' => $totalLivraisons,
            'chiffreAffairesLivraison' => array_sum(array_map(
                fn($c) => (float) $c->getMontantTotal(),
                $commandesLivrees
            ))
        ];
    }
}