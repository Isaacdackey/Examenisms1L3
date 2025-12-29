<?php

namespace App\Controller\Gestionnaire;

use App\Service\Manager\StatistiqueManager;
use Symfony\Bundle\FrameworkBundle\Controller\AbstractController;
use Symfony\Component\HttpFoundation\Response;
use Symfony\Component\Routing\Annotation\Route;

#[Route('/gestionnaire/statistiques')]
class StatistiqueController extends AbstractController
{
    public function __construct(
        private StatistiqueManager $statistiqueManager
    ) {}

    #[Route('/', name: 'gestionnaire_statistiques')]
    public function index(): Response
    {
        $this->denyAccessUnlessGranted('ROLE_GESTIONNAIRE');

        $today = new \DateTime();
        $statistiques = $this->statistiqueManager->getStatistiquesJournalieres($today);

        return $this->render('gestionnaire/statistiques.html.twig', [
            'burgersPlusVendus' => $statistiques['burgersPlusVendus'],
            'menusPlusVendus' => $statistiques['menusPlusVendus'],
            'commandesParStatut' => $statistiques['commandesParStatut'],
            'commandesParTypeRetrait' => $statistiques['commandesParTypeRetrait'],
            'produitsPlusVendus' => $statistiques['produitsPlusVendus'],
            'commandesParLivreur' => $statistiques['commandesParLivreur'],
            'recetteTotale' => $statistiques['recetteTotale'],
            'totalCommandes' => $statistiques['totalCommandes'],
            'commandesAnnuleesAujourdhui' => $statistiques['commandesAnnuleesAujourdhui'],
            'today' => $today,
        ]);
    }
}