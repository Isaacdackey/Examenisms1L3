<?php

namespace App\Form;

use App\Entity\Menu;
use App\Entity\Burger;
use App\Entity\Complement;
use Symfony\Component\Form\AbstractType;
use Symfony\Component\Form\FormBuilderInterface;
use Symfony\Component\OptionsResolver\OptionsResolver;
use Symfony\Bridge\Doctrine\Form\Type\EntityType;
use Symfony\Component\Form\Extension\Core\Type\FileType;
use Symfony\Component\Form\Extension\Core\Type\TextType;
use Symfony\Component\Form\Extension\Core\Type\NumberType;
use Symfony\Component\Form\Extension\Core\Type\TextareaType;
use Doctrine\ORM\EntityRepository;
use Symfony\Component\Validator\Constraints\File;

class MenuType extends AbstractType
{
    public function buildForm(FormBuilderInterface $builder, array $options): void
    {
        $builder
            ->add('libelle', TextType::class, [
                'label' => 'Nom du menu',
                'attr' => [
                    'class' => 'form-control',
                    'placeholder' => 'Ex: Menu Classique'
                ],
                'required' => true
            ])
            ->add('description', TextareaType::class, [
                'label' => 'Description',
                'required' => false,
                'attr' => [
                    'class' => 'form-control',
                    'rows' => 3,
                    'placeholder' => 'Description du menu...'
                ]
            ])
            ->add('burger', EntityType::class, [
                'class' => Burger::class,
                'label' => 'Burger principal',
                'query_builder' => function (EntityRepository $er) {
                    return $er->createQueryBuilder('b')
                        ->where('b.isArchived = false')
                        ->orderBy('b.libelle', 'ASC');
                },
                'choice_label' => function (Burger $burger) {
                    return $burger->getLibelle() . ' - ' . $burger->getFormattedPrix();
                },
                'placeholder' => 'Sélectionnez un burger',
                'required' => true,
                'attr' => ['class' => 'form-control']
            ])
            ->add('boisson', EntityType::class, [
                'class' => Complement::class,
                'label' => 'Boisson',
                'query_builder' => function (EntityRepository $er) {
                    return $er->createQueryBuilder('c')
                        ->where('c.isArchived = false AND c.typeComplement = :type')
                        ->setParameter('type', 'BOISSON')
                        ->orderBy('c.libelle', 'ASC');
                },
                'choice_label' => function (Complement $complement) {
                    return $complement->getLibelle() . ' - ' . $complement->getFormattedPrix();
                },
                'placeholder' => 'Sélectionnez une boisson',
                'required' => true,
                'attr' => ['class' => 'form-control']
            ])
            ->add('frites', EntityType::class, [
                'class' => Complement::class,
                'label' => 'Frites/Accompagnement',
                'query_builder' => function (EntityRepository $er) {
                    return $er->createQueryBuilder('c')
                        ->where('c.isArchived = false AND c.typeComplement = :type')
                        ->setParameter('type', 'FRITE')
                        ->orderBy('c.libelle', 'ASC');
                },
                'choice_label' => function (Complement $complement) {
                    return $complement->getLibelle() . ' - ' . $complement->getFormattedPrix();
                },
                'placeholder' => 'Sélectionnez des frites',
                'required' => true,
                'attr' => ['class' => 'form-control']
            ])
            ->add('reductionPourcentage', NumberType::class, [
                'label' => 'Réduction (%)',
                'required' => false,
                'html5' => true,
                'attr' => [
                    'class' => 'form-control',
                    'min' => 0,
                    'max' => 100,
                    'step' => 1,
                    'placeholder' => '10% par défaut'
                ],
                'empty_data' => '10'
            ])
            ->add('imageFile', FileType::class, [
                'label' => 'Nouvelle image',
                'mapped' => false,
                'required' => false,
                'constraints' => [
                    new File([
                        'maxSize' => '2M',
                        'mimeTypes' => ['image/jpeg', 'image/png', 'image/webp'],
                        'mimeTypesMessage' => 'Veuillez uploader une image valide (JPEG, PNG, WebP)',
                    ])
                ],
                'attr' => [
                    'class' => 'form-control',
                    'accept' => 'image/*'
                ]
            ]);
    }

    public function configureOptions(OptionsResolver $resolver): void
    {
        $resolver->setDefaults([
            'data_class' => Menu::class,
            'attr' => ['class' => 'needs-validation', 'novalidate' => 'novalidate']
        ]);
    }
}