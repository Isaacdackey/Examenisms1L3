<?php

namespace App\Service\Core;

use App\Entity\Menu;

class PrixCalculatorService
{
    public function calculerPrixMenu(Menu $menu): float
    {
        if (!$menu->getBurger() || !$menu->getBoisson() || !$menu->getFrites()) {
            return (float) ($menu->getPrix() ?: 0);
        }

        $prixBurger = (float) $menu->getBurger()->getPrix();
        $prixBoisson = (float) $menu->getBoisson()->getPrix();
        $prixFrites = (float) $menu->getFrites()->getPrix();

        if ($prixBurger <= 0 || $prixBoisson <= 0 || $prixFrites <= 0) {
            return (float) ($menu->getPrix() ?: 0);
        }

        $prixTotal = $prixBurger + $prixBoisson + $prixFrites;

        if ($menu->getReductionPourcentage() && is_numeric($menu->getReductionPourcentage())) {
            $reduction = $prixTotal * ((float) $menu->getReductionPourcentage() / 100);
            $prixTotal -= $reduction;
        }

        return round($prixTotal, 2);
    }

    public function calculerTotalCommande(array $lignesCommande): float
    {
        $total = 0;
        foreach ($lignesCommande as $ligne) {
            $total += (float) $ligne->getPrixUnitaire() * $ligne->getQuantite();
        }
        return round($total, 2);
    }

    public function appliquerReduction(float $montant, float $pourcentage): float
    {
        $reduction = $montant * ($pourcentage / 100);
        return round($montant - $reduction, 2);
    }

    public function calculerFraisLivraison(float $montantCommande, float $fraisBase = 0): float
    {
        
        if ($montantCommande > 5000) {
            return $fraisBase;
        }
        return $fraisBase + ($montantCommande * 0.05);
    }
}