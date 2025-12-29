<?php

namespace App\Controller;

use Symfony\Bundle\FrameworkBundle\Controller\AbstractController;
use Symfony\Component\HttpFoundation\RedirectResponse;
use Symfony\Component\HttpFoundation\Response;
use Symfony\Component\Routing\Annotation\Route;
use Symfony\Component\Security\Core\Security;

class HomeController extends AbstractController
{
    #[Route('/', name: 'app_home')]
    public function index(Security $security): Response
    {

        if ($this->getUser()) {
            $user = $this->getUser();

            if (in_array('ROLE_GESTIONNAIRE', $user->getRoles())) {
                return $this->redirectToRoute('gestionnaire_dashboard');
            }

            if (in_array('ROLE_LIVREUR', $user->getRoles())) {
                return $this->redirectToRoute('livreur_dashboard');
            }

            return $this->render('home/index.html.twig', [
                'user' => $user,
            ]);
        }

    
        return $this->render('home/index.html.twig');
    }

    
    #[Route('/home', name: 'app_home_alt')]
    public function home(): Response
    {
        return $this->redirectToRoute('app_home');
    }
}