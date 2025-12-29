<?php

namespace App\Controller\Gestionnaire;

use App\Entity\ZoneLivraison;
use App\Entity\User;
use App\Service\Manager\LivraisonManager;
use App\Service\Manager\CommandeManager;
use Doctrine\ORM\EntityManagerInterface;
use Symfony\Bundle\FrameworkBundle\Controller\AbstractController;
use Symfony\Component\HttpFoundation\Request;
use Symfony\Component\HttpFoundation\Response;
use Symfony\Component\Routing\Annotation\Route;

#[Route('/gestionnaire/livraisons')]
class LivraisonController extends AbstractController
{
    public function __construct(
        private LivraisonManager $livraisonManager,
        private CommandeManager $commandeManager,
        private EntityManagerInterface $em
    ) {}

    #[Route('/', name: 'gestionnaire_livraisons')]
    public function index(Request $request): Response
    {
        $this->denyAccessUnlessGranted('ROLE_GESTIONNAIRE');

        $statutFiltre = $request->query->get('statut');
        $commandesParZone = $this->livraisonManager->getCommandesParZone($statutFiltre);
        $livreurs = $this->livraisonManager->getLivreursDisponibles();

        return $this->render('gestionnaire/livraisons.html.twig', [
            'commandesParZone' => $commandesParZone,
            'livreurs' => $livreurs,
            'statutFiltre' => $statutFiltre
        ]);
    }

    #[Route('/assigner-zone', name: 'gestionnaire_assigner_zone', methods: ['POST'])]
    public function assignerZone(Request $request): Response
    {
        $this->denyAccessUnlessGranted('ROLE_GESTIONNAIRE');

        $zoneId = (int) $request->request->get('zone_id');
        $livreurId = (int) $request->request->get('livreur_id');
        $commandesIds = array_filter($request->request->all('commandes'), fn($id) => !empty($id) && is_numeric($id));

        if ($zoneId <= 0 || $livreurId <= 0 || empty($commandesIds)) {
            $this->addFlash('error', 'Données manquantes ou invalides.');
            return $this->redirectToRoute('gestionnaire_livraisons');
        }

        $zone = $this->em->getRepository(ZoneLivraison::class)->find($zoneId);
        $livreur = $this->em->getRepository(User::class)->find($livreurId);

        if (!$zone || !$livreur || $livreur->getRoleString() !== 'LIVREUR') {
            $this->addFlash('error', 'Zone ou livreur invalide.');
            return $this->redirectToRoute('gestionnaire_livraisons');
        }

        $result = $this->commandeManager->assignerZoneLivraison($commandesIds, $zone, $livreur);

        if ($result['success'] > 0) {
            $this->addFlash('success', "{$result['success']} commande(s) assignée(s) au livreur {$livreur->getFullName()} !");
            
            if (!empty($result['failed'])) {
                $this->addFlash('warning', count($result['failed']) . " commande(s) non éligible(s)");
            }
        } else {
            $this->addFlash('error', 'Aucune commande n\'a pu être assignée.');
        }

        return $this->redirectToRoute('gestionnaire_livraisons');
    }

    #[Route('/statistiques', name: 'gestionnaire_livraisons_statistiques')]
    public function statistiques(): Response
    {
        $this->denyAccessUnlessGranted('ROLE_GESTIONNAIRE');

        $today = new \DateTime();
        $statistiques = $this->livraisonManager->getStatistiquesLivraisons($today);
        $tournees = $this->livraisonManager->calculerTourneesOptimisees(
            $this->em->getRepository(\App\Entity\Commande::class)->findCommandesPourLivraison('PRETE')
        );

        return $this->render('gestionnaire/livraisons_statistiques.html.twig', [
            'statistiques' => $statistiques,
            'tournees' => $tournees,
            'today' => $today,
        ]);
    }

    
    
}