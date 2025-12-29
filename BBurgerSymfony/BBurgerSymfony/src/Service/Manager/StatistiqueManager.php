<?php

namespace App\Service\Manager;

use App\Repository\Query\CommandeRepository;

class StatistiqueManager
{
    public function __construct(
        private CommandeRepository $commandeRepository
    ) {}

    public function getStatistiquesJournalieres(\DateTime $date): array
    {
        return $this->commandeRepository->getStatistiquesCompletes($date);
    }

    public function getDashboardData(\DateTime $date): array
    {
        return $this->commandeRepository->getDashboardData($date);
    }

    public function getRecapitulatif(\DateTime $date): array
    {
        $statistiques = $this->getStatistiquesJournalieres($date);
        
        return [
            'totalCommandes' => $statistiques['totalCommandes'],
            'recetteTotale' => $statistiques['recetteTotale'],
            'commandesAnnulees' => $statistiques['commandesAnnuleesAujourdhui'],
            'topProduit' => $this->getTopProduit($statistiques['produitsPlusVendus']),
            'meilleurLivreur' => $this->getMeilleurLivreur($statistiques['commandesParLivreur']),
        ];
    }

    private function getTopProduit(array $produitsVendus): ?array
    {
        if (empty($produitsVendus)) {
            return null;
        }

        return [
            'libelle' => $produitsVendus[0]['libelle'],
            'type' => $produitsVendus[0]['type'] ?? 'Produit',
            'quantite' => $produitsVendus[0]['total'] ?? $produitsVendus[0]['quantite'] ?? 0
        ];
    }

    private function getMeilleurLivreur(array $livreurs): ?array
    {
        if (empty($livreurs)) {
            return null;
        }

        return [
            'nom' => $livreurs[0]['nom'] ?? 'N/A',
            'prenom' => $livreurs[0]['prenom'] ?? 'N/A',
            'nbCommandes' => $livreurs[0]['count'] ?? $livreurs[0]['nbCommandes'] ?? 0,
        ];
    }

    public function getEvolutions(\DateTime $dateDebut, \DateTime $dateFin): array
    {
        $evolutions = [];
        $dateCourante = clone $dateDebut;

        while ($dateCourante <= $dateFin) {
            $evolutions[$dateCourante->format('Y-m-d')] = [
                'recette' => $this->commandeRepository->getRecetteJournaliere($dateCourante),
                'commandes' => $this->commandeRepository->getTotalCommandesParDate($dateCourante),
            ];
            $dateCourante->modify('+1 day');
        }

        return $evolutions;
    }

    
    public function getDonneesGraphique(\DateTime $date): array
    {
        $statistiques = $this->getStatistiquesJournalieres($date);
        
        return [
            'commandesParStatut' => $this->formatCommandesParStatut($statistiques['commandesParStatut']),
            'produitsPlusVendus' => $this->formatProduitsVendus($statistiques['produitsPlusVendus']),
            'burgersPlusVendus' => $this->formatBurgersVendus($statistiques['burgersPlusVendus']),
            'menusPlusVendus' => $this->formatMenusVendus($statistiques['menusPlusVendus']),
        ];
    }

    private function formatCommandesParStatut(array $commandesParStatut): array
    {
        $labels = [];
        $data = [];
        
        foreach ($commandesParStatut as $statut) {
            $labels[] = $statut['statut'];
            $data[] = $statut['count'];
        }
        
        return [
            'labels' => $labels,
            'datasets' => [
                [
                    'label' => 'Commandes par statut',
                    'data' => $data,
                    'backgroundColor' => ['#FF6384', '#36A2EB', '#FFCE56', '#4BC0C0', '#9966FF', '#FF9F40'],
                ]
            ]
        ];
    }

    private function formatProduitsVendus(array $produitsVendus): array
    {
        $labels = [];
        $data = [];
        
        foreach ($produitsVendus as $produit) {
            $labels[] = $produit['libelle'] . ' (' . ($produit['type'] ?? 'Produit') . ')';
            $data[] = $produit['total'] ?? 0;
        }
        
        return [
            'labels' => $labels,
            'datasets' => [
                [
                    'label' => 'Quantités vendues',
                    'data' => $data,
                    'backgroundColor' => '#36A2EB',
                ]
            ]
        ];
    }

    private function formatBurgersVendus(array $burgersVendus): array
    {
        $labels = [];
        $data = [];
        
        foreach ($burgersVendus as $burger) {
            $labels[] = $burger['libelle'];
            $data[] = $burger['total'] ?? 0;
        }
        
        return [
            'labels' => $labels,
            'datasets' => [
                [
                    'label' => 'Burgers vendus',
                    'data' => $data,
                    'backgroundColor' => '#FF6384',
                ]
            ]
        ];
    }

    private function formatMenusVendus(array $menusVendus): array
    {
        $labels = [];
        $data = [];
        
        foreach ($menusVendus as $menu) {
            $labels[] = $menu['libelle'];
            $data[] = $menu['total'] ?? 0;
        }
        
        return [
            'labels' => $labels,
            'datasets' => [
                [
                    'label' => 'Menus vendus',
                    'data' => $data,
                    'backgroundColor' => '#4BC0C0',
                ]
            ]
        ];
    }
}