<?php

namespace App\Service\Manager;

use App\Entity\Commande;
use App\Entity\User;
use App\Entity\ZoneLivraison;
use App\Repository\Query\CommandeRepository;
use App\Service\Core\CommandeWorkflowService;
use Doctrine\ORM\EntityManagerInterface;
use Symfony\Component\HttpFoundation\Request;

class CommandeManager
{
    public function __construct(
        private CommandeRepository $commandeRepository,
        private CommandeWorkflowService $workflowService,
        private EntityManagerInterface $em
    ) {}

    public function validerCommande(Commande $commande): bool
    {
        if (!$this->workflowService->validerTransition($commande, 'VALIDEE')) {
            return false;
        }

        $commande->setStatut('VALIDEE');
        $commande->setUpdatedAt(new \DateTime());
        
        $this->em->flush();
        return true;
    }

    public function annulerCommande(Commande $commande, ?string $raison = null): bool
    {
        if (!$this->workflowService->validerTransition($commande, 'ANNULEE')) {
            return false;
        }

        $commande->setStatut('ANNULEE');
        $commande->setUpdatedAt(new \DateTime());

        if ($raison) {
            $commande->setNotes($raison);
        }

        $this->em->flush();
        return true;
    }

    public function marquerPrete(Commande $commande): bool
    {
        if (!$this->workflowService->validerTransition($commande, 'PRETE')) {
            return false;
        }

        $commande->setStatut('PRETE');
        $commande->setUpdatedAt(new \DateTime());
        $this->em->flush();
        return true;
    }

    public function assignerLivreur(Commande $commande, User $livreur): bool
    {
        if (!$this->workflowService->validerTransition($commande, 'EN_LIVRAISON')) {
            return false;
        }

        $commande->setLivreur($livreur);
        $commande->setStatut('EN_LIVRAISON');
        $commande->setUpdatedAt(new \DateTime());
        $this->em->flush();
        return true;
    }

    public function terminerCommande(Commande $commande): bool
    {
        $statutValide = $commande->getStatut() === 'PRETE' && $commande->getTypeRetrait() !== 'LIVRAISON';
        $livraisonValide = $commande->getStatut() === 'EN_LIVRAISON';

        if (!$statutValide && !$livraisonValide) {
            return false;
        }

        $nouveauStatut = $livraisonValide ? 'LIVREE' : 'TERMINEE';
        
        if (!$this->workflowService->validerTransition($commande, $nouveauStatut)) {
            return false;
        }

        $commande->setStatut($nouveauStatut);
        $commande->setUpdatedAt(new \DateTime());
        $this->em->flush();
        return true;
    }

    public function assignerZoneLivraison(array $commandesIds, ZoneLivraison $zone, User $livreur): array
    {
        $results = [
            'success' => 0,
            'failed' => [],
            'errors' => []
        ];

        foreach ($commandesIds as $commandeId) {
            $commande = $this->em->getRepository(Commande::class)->find($commandeId);

            if (!$commande) {
                $results['failed'][] = "Commande #$commandeId introuvable";
                continue;
            }

            if (!$this->workflowService->validerTransition($commande, 'EN_LIVRAISON')) {
                $results['failed'][] = "Commande #{$commande->getNumero()} (statut: {$commande->getStatut()})";
                continue;
            }

            if ($commande->getTypeRetrait() !== 'LIVRAISON') {
                $results['failed'][] = "Commande #{$commande->getNumero()} (type: {$commande->getTypeRetrait()})";
                continue;
            }

            $commande->setZone($zone);
            $commande->setLivreur($livreur);
            $commande->setStatut('EN_LIVRAISON');
            $commande->setUpdatedAt(new \DateTime());
            $results['success']++;
        }

        if ($results['success'] > 0) {
            $this->em->flush();
        }

        return $results;
    }

    public function getCommandesWithPagination(Request $request): array
    {
        $filters = [
            'statut' => $request->query->get('statut'),
            'date' => $request->query->get('date'),
            'client_id' => $request->query->get('client'),
            'produit' => $request->query->get('produit'),
        ];

        $page = $request->query->getInt('page', 1);
        $limit = $request->query->getInt('limit', 15);
        $offset = ($page - 1) * $limit;

        $commandes = $this->commandeRepository->findCommandesWithFilters($filters, $limit, $offset);
        $total = $this->commandeRepository->countCommandesWithFilters($filters);

        return [
            'commandes' => $commandes,
            'total' => $total,
            'page' => $page,
            'limit' => $limit,
            'pages' => ceil($total / $limit)
        ];
    }

    public function getCommandesEnCours(): array
    {
        $today = new \DateTime('today');
        $todayEnd = clone $today;
        $todayEnd->modify('+1 day');

        return $this->commandeRepository->createQueryBuilderAlias('c')
            ->leftJoin('c.user', 'u')
            ->addSelect('u')
            ->where('c.createdAt >= :start AND c.createdAt < :end')
            ->andWhere('c.statut IN (:statuts)')
            ->setParameter('start', $today)
            ->setParameter('end', $todayEnd)
            ->setParameter('statuts', ['EN_ATTENTE', 'VALIDEE', 'EN_PREPARATION', 'PRETE', 'EN_LIVRAISON'])
            ->orderBy('c.createdAt', 'DESC')
            ->getQuery()
            ->getResult();
    }

    public function getDashboardData(\DateTime $date): array
    {
        return $this->commandeRepository->getDashboardData($date);
    }
}