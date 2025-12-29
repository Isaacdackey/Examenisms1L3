<?php

namespace App\Controller\Gestionnaire;

use App\Entity\Burger;
use App\Form\BurgerType;
use App\Service\Core\ImageUploaderService;
use App\Repository\Query\BurgerRepository;
use Doctrine\ORM\EntityManagerInterface;
use Symfony\Bundle\FrameworkBundle\Controller\AbstractController;
use Symfony\Component\HttpFoundation\Request;
use Symfony\Component\HttpFoundation\Response;
use Symfony\Component\Routing\Annotation\Route;

#[Route('/gestionnaire/burgers')]
class BurgerController extends AbstractController
{
    public function __construct(
        private BurgerRepository $burgerRepository,
        private ImageUploaderService $imageUploader,
        private EntityManagerInterface $em
    ) {}

    #[Route('/', name: 'gestionnaire_burger_index', methods: ['GET'])]
    public function index(): Response
    {
        $this->denyAccessUnlessGranted('ROLE_GESTIONNAIRE');
        $burgers = $this->burgerRepository->findAllOrdered();

        return $this->render('gestionnaire/burger/index.html.twig', [
            'burgers' => $burgers,
        ]);
    }

    #[Route('/new', name: 'gestionnaire_burger_new')]
    public function new(Request $request): Response
    {
        $this->denyAccessUnlessGranted('ROLE_GESTIONNAIRE');

        $burger = new Burger();
        $form = $this->createForm(BurgerType::class, $burger);
        $form->handleRequest($request);

        if ($form->isSubmitted() && $form->isValid()) {
            $burger->setUpdatedAt(new \DateTime());

            $imageFile = $form->get('imageFile')->getData();
            if ($imageFile) {
                $uploadResult = $this->imageUploader->upload($imageFile);
                if ($uploadResult['success']) {
                    $burger->setCloudinaryUrl($uploadResult['url']);
                    $burger->setCloudinaryPublicId($uploadResult['public_id']);
                    $this->addFlash('success', 'Image uploadée avec succès !');
                } else {
                    $this->addFlash('warning', 'Image non uploadée : ' . $uploadResult['error']);
                }
            }

            $this->burgerRepository->save($burger, true);

            $this->addFlash('success', 'Burger créé avec succès !');
            return $this->redirectToRoute('gestionnaire_burger_index');
        }

        return $this->render('gestionnaire/burger/new.html.twig', [
            'form' => $form->createView(),
        ]);
    }

    #[Route('/{id}/edit', name: 'gestionnaire_burger_edit')]
    public function edit(Request $request, Burger $burger): Response
    {
        $this->denyAccessUnlessGranted('ROLE_GESTIONNAIRE');

        $oldPublicId = $burger->getCloudinaryPublicId();
        $form = $this->createForm(BurgerType::class, $burger);
        $form->handleRequest($request);

        if ($form->isSubmitted() && $form->isValid()) {
            $burger->setUpdatedAt(new \DateTime());

            $imageFile = $form->get('imageFile')->getData();
            if ($imageFile) {
                if ($oldPublicId) {
                    $deleteResult = $this->imageUploader->delete($oldPublicId);
                    if (!$deleteResult['success']) {
                        $this->addFlash('warning', 'Ancienne image non supprimée : ' . $deleteResult['error']);
                    }
                }

                $uploadResult = $this->imageUploader->upload($imageFile);
                if ($uploadResult['success']) {
                    $burger->setCloudinaryUrl($uploadResult['url']);
                    $burger->setCloudinaryPublicId($uploadResult['public_id']);
                    $this->addFlash('success', 'Image mise à jour !');
                } else {
                    $this->addFlash('warning', 'Image non uploadée : ' . $uploadResult['error']);
                }
            }

            $this->burgerRepository->save($burger, true);
            $this->addFlash('success', 'Burger modifié avec succès !');
            return $this->redirectToRoute('gestionnaire_burger_index');
        }

        return $this->render('gestionnaire/burger/edit.html.twig', [
            'burger' => $burger,
            'form' => $form->createView(),
        ]);
    }

    #[Route('/{id}/archive', name: 'gestionnaire_burger_archive', methods: ['POST'])]
    public function archive(Request $request, Burger $burger): Response
    {
        $this->denyAccessUnlessGranted('ROLE_GESTIONNAIRE');

        if ($this->isCsrfTokenValid('archive'.$burger->getId(), $request->request->get('_token'))) {
            $burger->setIsArchived(true);
            $burger->setUpdatedAt(new \DateTime());
            $this->burgerRepository->save($burger, true);
            $this->addFlash('success', 'Burger archivé avec succès !');
        }

        return $this->redirectToRoute('gestionnaire_burger_index');
    }

    #[Route('/{id}/restore', name: 'gestionnaire_burger_restore', methods: ['POST'])]
    public function restore(Request $request, Burger $burger): Response
    {
        $this->denyAccessUnlessGranted('ROLE_GESTIONNAIRE');

        if ($this->isCsrfTokenValid('restore'.$burger->getId(), $request->request->get('_token'))) {
            $burger->setIsArchived(false);
            $burger->setUpdatedAt(new \DateTime());
            $this->burgerRepository->save($burger, true);
            $this->addFlash('success', 'Burger restauré avec succès !');
        }

        return $this->redirectToRoute('gestionnaire_burger_index');
    }
}