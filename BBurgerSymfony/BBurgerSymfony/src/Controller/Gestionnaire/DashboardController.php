<?php

namespace App\Controller\Gestionnaire;

use App\Service\Manager\StatistiqueManager;
use Symfony\Bundle\FrameworkBundle\Controller\AbstractController;
use Symfony\Component\HttpFoundation\Response;
use Symfony\Component\Routing\Annotation\Route;

#[Route('/gestionnaire')]
class DashboardController extends AbstractController
{
    public function __construct(
        private StatistiqueManager $statistiqueManager
    ) {}

    #[Route('/', name: 'gestionnaire_dashboard')]
    public function dashboard(): Response
    {
        $this->denyAccessUnlessGranted('ROLE_GESTIONNAIRE');
        
        $today = new \DateTime();
        $dashboardData = $this->statistiqueManager->getDashboardData($today);

        return $this->render('gestionnaire/dashboard.html.twig', [
            'commandesEnCours' => $dashboardData['commandesEnCours'],
            'commandesValidees' => $dashboardData['commandesValidees'],
            'commandesAnnulees' => $dashboardData['commandesAnnulees'],
            'recettesJournalieres' => $dashboardData['recettesJournalieres'],
            'nbCommandesEnCours' => $dashboardData['nbCommandesEnCours'],
            'nbCommandesValidees' => $dashboardData['nbCommandesValidees'],
            'nbCommandesAnnulees' => $dashboardData['nbCommandesAnnulees'],
        ]);
    }
}