<?php

namespace App\Controller\Gestionnaire;

use App\Entity\Complement;
use App\Form\ComplementType;
use App\Service\Core\ImageUploaderService;
use App\Repository\Query\ComplementRepository;
use Doctrine\ORM\EntityManagerInterface;
use Symfony\Bundle\FrameworkBundle\Controller\AbstractController;
use Symfony\Component\HttpFoundation\Request;
use Symfony\Component\HttpFoundation\Response;
use Symfony\Component\Routing\Annotation\Route;

#[Route('/gestionnaire/complements')]
class ComplementController extends AbstractController
{
    public function __construct(
        private ComplementRepository $complementRepository,
        private ImageUploaderService $imageUploader,
        private EntityManagerInterface $em
    ) {}

    #[Route('/', name: 'gestionnaire_complement_index', methods: ['GET'])]
    public function index(): Response
    {
        $this->denyAccessUnlessGranted('ROLE_GESTIONNAIRE');
        $complements = $this->complementRepository->findAllOrdered();

        return $this->render('gestionnaire/complement/index.html.twig', [
            'complements' => $complements,
        ]);
    }

    #[Route('/new', name: 'gestionnaire_complement_new', methods: ['GET', 'POST'])]
    public function new(Request $request): Response
    {
        $this->denyAccessUnlessGranted('ROLE_GESTIONNAIRE');

        $complement = new Complement();
        $form = $this->createForm(ComplementType::class, $complement);
        $form->handleRequest($request);

        if ($form->isSubmitted() && $form->isValid()) {
            $complement->setUpdatedAt(new \DateTime());

            $imageFile = $form->get('imageFile')->getData();
            if ($imageFile) {
                $uploadResult = $this->imageUploader->upload($imageFile);
                if ($uploadResult['success']) {
                    $complement->setCloudinaryUrl($uploadResult['url']);
                    $complement->setCloudinaryPublicId($uploadResult['public_id']);
                    $this->addFlash('success', 'Image uploadée avec succès !');
                }
            }

            $this->complementRepository->save($complement, true);
            $this->addFlash('success', 'Complément créé avec succès !');
            return $this->redirectToRoute('gestionnaire_complement_index');
        }

        return $this->render('gestionnaire/complement/new.html.twig', [
            'form' => $form->createView(),
        ]);
    }

    #[Route('/{id}/edit', name: 'gestionnaire_complement_edit', methods: ['GET', 'POST'])]
    public function edit(Request $request, Complement $complement): Response
    {
        $this->denyAccessUnlessGranted('ROLE_GESTIONNAIRE');

        $oldPublicId = $complement->getCloudinaryPublicId();
        $form = $this->createForm(ComplementType::class, $complement);
        $form->handleRequest($request);

        if ($form->isSubmitted() && $form->isValid()) {
            $complement->setUpdatedAt(new \DateTime());

            $imageFile = $form->get('imageFile')->getData();
            if ($imageFile) {
                if ($oldPublicId) {
                    $this->imageUploader->delete($oldPublicId);
                }
                $uploadResult = $this->imageUploader->upload($imageFile);
                if ($uploadResult['success']) {
                    $complement->setCloudinaryUrl($uploadResult['url']);
                    $complement->setCloudinaryPublicId($uploadResult['public_id']);
                }
            }

            $this->complementRepository->save($complement, true);
            $this->addFlash('success', 'Complément modifié avec succès !');
            return $this->redirectToRoute('gestionnaire_complement_index');
        }

        return $this->render('gestionnaire/complement/edit.html.twig', [
            'complement' => $complement,
            'form' => $form->createView(),
        ]);
    }

    #[Route('/{id}/archive', name: 'gestionnaire_complement_archive', methods: ['POST'])]
    public function archive(Request $request, Complement $complement): Response
    {
        $this->denyAccessUnlessGranted('ROLE_GESTIONNAIRE');

        if ($this->isCsrfTokenValid('archive' . $complement->getId(), $request->request->get('_token'))) {
            $complement->setIsArchived(true);
            $complement->setUpdatedAt(new \DateTime());
            $this->complementRepository->save($complement, true);
            $this->addFlash('success', 'Complément archivé avec succès !');
        }

        return $this->redirectToRoute('gestionnaire_complement_index');
    }

    #[Route('/{id}/restore', name: 'gestionnaire_complement_restore', methods: ['POST'])]
    public function restore(Request $request, Complement $complement): Response
    {
        $this->denyAccessUnlessGranted('ROLE_GESTIONNAIRE');

        if ($this->isCsrfTokenValid('restore' . $complement->getId(), $request->request->get('_token'))) {
            $complement->setIsArchived(false);
            $complement->setUpdatedAt(new \DateTime());
            $this->complementRepository->save($complement, true);
            $this->addFlash('success', 'Complément restauré avec succès !');
        }

        return $this->redirectToRoute('gestionnaire_complement_index');
    }
}