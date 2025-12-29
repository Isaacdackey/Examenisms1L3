<?php

namespace App\Form;

use App\Entity\Burger;
use Symfony\Component\Form\AbstractType;
use Symfony\Component\Form\FormBuilderInterface;
use Symfony\Component\OptionsResolver\OptionsResolver;
use Symfony\Component\Form\Extension\Core\Type\FileType;
use Symfony\Component\Form\Extension\Core\Type\TextType;
use Symfony\Component\Form\Extension\Core\Type\NumberType;
use Symfony\Component\Form\Extension\Core\Type\TextareaType;
use Symfony\Component\Validator\Constraints\File;

class BurgerType extends AbstractType
{
    public function buildForm(FormBuilderInterface $builder, array $options): void
    {
        $builder
            ->add('libelle', TextType::class, [
                'label' => 'Nom du burger'
            ])
            ->add('prix', NumberType::class, [
                'label' => 'Prix (FCFA)'
            ])
            ->add('description', TextareaType::class, [
                'label' => 'Description',
                'required' => false
            ])
            ->add('typeBurger', TextType::class, [
                'label' => 'Type de burger',
                'required' => false,
                'attr' => ['placeholder' => 'Ex: Classique, Poulet, Végétarien']
            ])
            ->add('calories', NumberType::class, [
                'label' => 'Calories',
                'required' => false,
                'html5' => true
            ])
            ->add('tempsPreparation', NumberType::class, [
                'label' => 'Temps de préparation (minutes)',
                'required' => false,
                'html5' => true
            ])
            ->add('imageFile', FileType::class, [
                'label' => 'Image du burger',
                'mapped' => false,
                'required' => false,
                'constraints' => [
                    new File([
                        'maxSize' => '2M',
                        'mimeTypes' => [
                            'image/jpeg',
                            'image/png',
                            'image/webp',
                        ],
                        'mimeTypesMessage' => 'Veuillez uploader une image valide (JPEG, PNG, WebP)',
                    ])
                ],
            ]);
    }

    public function configureOptions(OptionsResolver $resolver): void
    {
        $resolver->setDefaults([
            'data_class' => Burger::class,
        ]);
    }
}