<?php

namespace App\Controller\Gestionnaire;

use App\Entity\Menu;
use App\Form\MenuType;
use App\Service\Core\ImageUploaderService;
use App\Service\Core\PrixCalculatorService;
use App\Repository\Query\MenuRepository;
use Doctrine\ORM\EntityManagerInterface;
use Symfony\Bundle\FrameworkBundle\Controller\AbstractController;
use Symfony\Component\HttpFoundation\Request;
use Symfony\Component\HttpFoundation\Response;
use Symfony\Component\Routing\Annotation\Route;

#[Route('/gestionnaire/menus')]
class MenuController extends AbstractController
{
    public function __construct(
        private MenuRepository $menuRepository,
        private ImageUploaderService $imageUploader,
        private PrixCalculatorService $prixCalculator,
        private EntityManagerInterface $em
    ) {}

    #[Route('/', name: 'gestionnaire_menu_index', methods: ['GET'])]
    public function index(): Response
    {
        $this->denyAccessUnlessGranted('ROLE_GESTIONNAIRE');
        $menus = $this->menuRepository->findAllOrdered();

        return $this->render('gestionnaire/menu/index.html.twig', [
            'menus' => $menus,
        ]);
    }

    #[Route('/new', name: 'gestionnaire_menu_new', methods: ['GET', 'POST'])]
    public function new(Request $request): Response
    {
        $this->denyAccessUnlessGranted('ROLE_GESTIONNAIRE');

        $menu = new Menu();
        $form = $this->createForm(MenuType::class, $menu);
        $form->handleRequest($request);

        if ($form->isSubmitted() && $form->isValid()) {
            if (!$menu->getBurger() || !$menu->getBoisson() || !$menu->getFrites()) {
                $this->addFlash('error', 'Tous les composants (burger, boisson, frites) doivent être sélectionnés.');
                return $this->render('gestionnaire/menu/new.html.twig', [
                    'menu' => $menu,
                    'form' => $form->createView(),
                ]);
            }

            $prixTotal = $this->prixCalculator->calculerPrixMenu($menu);
            $menu->setPrix((string) $prixTotal);
            $menu->setUpdatedAt(new \DateTime());

            $imageFile = $form->get('imageFile')->getData();
            if ($imageFile) {
                $uploadResult = $this->imageUploader->upload($imageFile);
                if ($uploadResult['success']) {
                    $menu->setCloudinaryUrl($uploadResult['url']);
                    $menu->setCloudinaryPublicId($uploadResult['public_id']);
                    $this->addFlash('success', 'Image uploadée avec succès !');
                }
            }

            $this->menuRepository->save($menu, true);
            $this->addFlash('success', 'Menu créé avec succès !');
            return $this->redirectToRoute('gestionnaire_menu_index');
        }

        return $this->render('gestionnaire/menu/new.html.twig', [
            'menu' => $menu,
            'form' => $form->createView(),
        ]);
    }

    #[Route('/{id}/edit', name: 'gestionnaire_menu_edit', methods: ['GET', 'POST'])]
    public function edit(Request $request, Menu $menu): Response
    {
        $this->denyAccessUnlessGranted('ROLE_GESTIONNAIRE');

        $oldPublicId = $menu->getCloudinaryPublicId();
        $form = $this->createForm(MenuType::class, $menu);
        $form->handleRequest($request);

        if ($form->isSubmitted() && $form->isValid()) {
            if (!$menu->getBurger() || !$menu->getBoisson() || !$menu->getFrites()) {
                $this->addFlash('error', 'Tous les composants (burger, boisson, frites) doivent être sélectionnés.');
                return $this->render('gestionnaire/menu/edit.html.twig', [
                    'menu' => $menu,
                    'form' => $form->createView(),
                ]);
            }

            $prixTotal = $this->prixCalculator->calculerPrixMenu($menu);
            $menu->setPrix((string) $prixTotal);
            $menu->setUpdatedAt(new \DateTime());

            $imageFile = $form->get('imageFile')->getData();
            if ($imageFile) {
                if ($oldPublicId) {
                    $this->imageUploader->delete($oldPublicId);
                }
                $uploadResult = $this->imageUploader->upload($imageFile);
                if ($uploadResult['success']) {
                    $menu->setCloudinaryUrl($uploadResult['url']);
                    $menu->setCloudinaryPublicId($uploadResult['public_id']);
                }
            }

            $this->menuRepository->save($menu, true);
            $this->addFlash('success', 'Menu modifié avec succès !');
            return $this->redirectToRoute('gestionnaire_menu_index');
        }

        return $this->render('gestionnaire/menu/edit.html.twig', [
            'menu' => $menu,
            'form' => $form->createView(),
        ]);
    }

    #[Route('/{id}/archive', name: 'gestionnaire_menu_archive', methods: ['POST'])]
    public function archive(Request $request, Menu $menu): Response
    {
        $this->denyAccessUnlessGranted('ROLE_GESTIONNAIRE');

        if ($this->isCsrfTokenValid('archive'.$menu->getId(), $request->request->get('_token'))) {
            $menu->setIsArchived(true);
            $menu->setUpdatedAt(new \DateTime());
            $this->menuRepository->save($menu, true);
            $this->addFlash('success', 'Menu archivé avec succès !');
        }

        return $this->redirectToRoute('gestionnaire_menu_index');
    }

    #[Route('/{id}/restore', name: 'gestionnaire_menu_restore', methods: ['POST'])]
    public function restore(Request $request, Menu $menu): Response
    {
        $this->denyAccessUnlessGranted('ROLE_GESTIONNAIRE');

        if ($this->isCsrfTokenValid('restore'.$menu->getId(), $request->request->get('_token'))) {
            $menu->setIsArchived(false);
            $menu->setUpdatedAt(new \DateTime());
            $this->menuRepository->save($menu, true);
            $this->addFlash('success', 'Menu restauré avec succès !');
        }

        return $this->redirectToRoute('gestionnaire_menu_index');
    }
}