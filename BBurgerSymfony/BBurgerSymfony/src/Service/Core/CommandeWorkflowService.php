<?php

namespace App\Service\Core;

use App\Entity\Commande;

class CommandeWorkflowService
{
    private const STATUTS_VALIDES = [
        'EN_ATTENTE' => ['VALIDEE', 'ANNULEE'],
        'VALIDEE' => ['EN_PREPARATION', 'ANNULEE'],
        'EN_PREPARATION' => ['PRETE', 'ANNULEE'],
        'PRETE' => ['EN_LIVRAISON', 'TERMINEE'],
        'EN_LIVRAISON' => ['LIVREE', 'TERMINEE'],
        'LIVREE' => ['TERMINEE'],
        'TERMINEE' => [],
        'ANNULEE' => []
    ];

    public function peutChangerStatut(string $statutActuel, string $nouveauStatut): bool
    {
        return in_array($nouveauStatut, self::STATUTS_VALIDES[$statutActuel] ?? []);
    }

    public function getStatutsPossibles(string $statutActuel): array
    {
        return self::STATUTS_VALIDES[$statutActuel] ?? [];
    }

    public function validerTransition(Commande $commande, string $nouveauStatut): bool
    {
        $statutActuel = $commande->getStatut();
        
        if (!$this->peutChangerStatut($statutActuel, $nouveauStatut)) {
            return false;
        }

        
        if ($nouveauStatut === 'EN_LIVRAISON' && $commande->getTypeRetrait() !== 'LIVRAISON') {
            return false;
        }

        if ($nouveauStatut === 'EN_LIVRAISON' && !$commande->getLivreur()) {
            return false;
        }

        return true;
    }

    public function getStatutLabel(string $statut): string
    {
        $labels = [
            'EN_ATTENTE' => 'En attente',
            'VALIDEE' => 'Validée',
            'EN_PREPARATION' => 'En préparation',
            'PRETE' => 'Prête',
            'EN_LIVRAISON' => 'En livraison',
            'LIVREE' => 'Livrée',
            'TERMINEE' => 'Terminée',
            'ANNULEE' => 'Annulée',
        ];

        return $labels[$statut] ?? $statut;
    }

    public function getBadgeClass(string $statut): string
    {
        $classes = [
            'EN_ATTENTE' => 'bg-warning text-dark',
            'VALIDEE' => 'bg-info',
            'EN_PREPARATION' => 'bg-primary',
            'PRETE' => 'bg-success',
            'EN_LIVRAISON' => 'bg-dark',
            'LIVREE' => 'bg-success',
            'TERMINEE' => 'bg-secondary',
            'ANNULEE' => 'bg-danger',
        ];

        return $classes[$statut] ?? 'bg-secondary';
    }
}