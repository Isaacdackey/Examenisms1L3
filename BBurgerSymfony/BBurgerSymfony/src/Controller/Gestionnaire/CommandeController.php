<?php

namespace App\Controller\Gestionnaire;

use App\Entity\Commande;
use App\Entity\User;
use App\Entity\Product;
use App\Service\Manager\CommandeManager;
use App\Repository\Query\CommandeRepository;
use Doctrine\ORM\EntityManagerInterface;
use Symfony\Bundle\FrameworkBundle\Controller\AbstractController;
use Symfony\Component\HttpFoundation\Request;
use Symfony\Component\HttpFoundation\Response;
use Symfony\Component\Routing\Annotation\Route;

#[Route('/gestionnaire/commandes')]
class CommandeController extends AbstractController
{
    public function __construct(
        private CommandeManager $commandeManager,
        private CommandeRepository $commandeRepository,
        private EntityManagerInterface $em
    ) {}

    #[Route('/', name: 'gestionnaire_commandes')]
    public function index(Request $request): Response
    {
        $this->denyAccessUnlessGranted('ROLE_GESTIONNAIRE');

        $paginationData = $this->commandeManager->getCommandesWithPagination($request);

        $clients = $this->em->getRepository(User::class)->createQueryBuilder('u')
            ->where('u.role = :role')
            ->setParameter('role', 'CLIENT')
            ->orderBy('u.nom', 'ASC')
            ->getQuery()
            ->getResult();

        $produits = $this->em->getRepository(Product::class)->createQueryBuilder('p')
            ->where('p.isArchived = false')
            ->orderBy('p.libelle', 'ASC')
            ->getQuery()
            ->getResult();

        return $this->render('gestionnaire/commandes.html.twig', [
            'commandes' => $paginationData['commandes'],
            'clients' => $clients,
            'produits' => $produits,
            'statutFilter' => $request->query->get('statut'),
            'dateFilter' => $request->query->get('date'),
            'clientFilter' => $request->query->get('client'),
            'produitFilter' => $request->query->get('produit'),
            'currentPage' => $paginationData['page'],
            'pagesCount' => $paginationData['pages'],
            'totalItems' => $paginationData['total'],
            'limit' => $paginationData['limit'],
        ]);
    }

    #[Route('/{id}', name: 'gestionnaire_commande_details')]
    public function details(Commande $commande): Response
    {
        $this->denyAccessUnlessGranted('ROLE_GESTIONNAIRE');
        
        return $this->render('gestionnaire/commande_details.html.twig', [
            'commande' => $commande,
        ]);
    }

    #[Route('/{id}/valider', name: 'gestionnaire_commande_valider', methods: ['POST'])]
    public function valider(Commande $commande): Response
    {
        $this->denyAccessUnlessGranted('ROLE_GESTIONNAIRE');

        if ($this->commandeManager->validerCommande($commande)) {
            $this->addFlash('success', 'Commande validée avec succès !');
        } else {
            $this->addFlash('error', 'Cette commande ne peut pas être validée.');
        }

        return $this->redirectToRoute('gestionnaire_commandes');
    }

    #[Route('/{id}/annuler', name: 'gestionnaire_commande_annuler', methods: ['POST'])]
    public function annuler(Request $request, Commande $commande): Response
    {
        $this->denyAccessUnlessGranted('ROLE_GESTIONNAIRE');

        $raison = $request->request->get('raison');
        
        if ($this->commandeManager->annulerCommande($commande, $raison)) {
            $this->addFlash('success', 'Commande annulée avec succès !');
        } else {
            $this->addFlash('error', 'Cette commande ne peut pas être annulée.');
        }

        return $this->redirectToRoute('gestionnaire_commandes');
    }

    #[Route('/{id}/prete', name: 'gestionnaire_commande_prete', methods: ['POST'])]
    public function prete(Commande $commande): Response
    {
        $this->denyAccessUnlessGranted('ROLE_GESTIONNAIRE');

        if ($this->commandeManager->marquerPrete($commande)) {
            $this->addFlash('success', 'Commande marquée comme prête !');
        } else {
            $this->addFlash('error', 'Cette commande ne peut pas être marquée comme prête.');
        }

        return $this->redirectToRoute('gestionnaire_commandes');
    }

    #[Route('/{id}/terminer', name: 'gestionnaire_commande_terminer', methods: ['POST'])]
    public function terminer(Commande $commande): Response
    {
        $this->denyAccessUnlessGranted('ROLE_GESTIONNAIRE');

        if ($this->commandeManager->terminerCommande($commande)) {
            $this->addFlash('success', 'Commande terminée avec succès !');
        } else {
            $this->addFlash('error', 'Cette commande ne peut pas être terminée.');
        }

        return $this->redirectToRoute('gestionnaire_commandes');
    }

    #[Route('/{id}/assigner-livreur', name: 'gestionnaire_commande_assigner_livreur', methods: ['POST'])]
    public function assignerLivreur(Request $request, Commande $commande): Response
    {
        $this->denyAccessUnlessGranted('ROLE_GESTIONNAIRE');

        $livreurId = $request->request->get('livreur_id');

        if (!$livreurId) {
            $this->addFlash('error', 'Veuillez sélectionner un livreur.');
            return $this->redirectToRoute('gestionnaire_commande_details', ['id' => $commande->getId()]);
        }

        $livreur = $this->em->getRepository(User::class)->find($livreurId);

        if (!$livreur || $livreur->getRoleString() !== 'LIVREUR') {
            $this->addFlash('error', 'Livreur invalide.');
            return $this->redirectToRoute('gestionnaire_commande_details', ['id' => $commande->getId()]);
        }

        if ($this->commandeManager->assignerLivreur($commande, $livreur)) {
            $this->addFlash('success', 'Livreur assigné avec succès !');
        } else {
            $this->addFlash('error', 'La commande doit être prête pour assigner un livreur.');
        }

        return $this->redirectToRoute('gestionnaire_commande_details', ['id' => $commande->getId()]);
    }
}