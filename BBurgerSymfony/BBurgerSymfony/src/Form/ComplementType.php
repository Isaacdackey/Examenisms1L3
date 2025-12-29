<?php

namespace App\Form;

use App\Entity\Complement;
use Symfony\Component\Form\AbstractType;
use Symfony\Component\Form\FormBuilderInterface;
use Symfony\Component\OptionsResolver\OptionsResolver;
use Symfony\Component\Form\Extension\Core\Type\FileType;
use Symfony\Component\Form\Extension\Core\Type\TextType;
use Symfony\Component\Form\Extension\Core\Type\ChoiceType;
use Symfony\Component\Form\Extension\Core\Type\NumberType;
use Symfony\Component\Form\Extension\Core\Type\TextareaType;
use Symfony\Component\Validator\Constraints\File;

class ComplementType extends AbstractType
{
    public function buildForm(FormBuilderInterface $builder, array $options): void
    {
        $builder
            ->add('libelle', TextType::class, [
                'label' => 'Nom du complément',
                'attr' => [
                    'class' => 'form-control',
                    'placeholder' => 'Ex: Frites Maison, Coca-Cola 50cl'
                ],
                'required' => true
            ])
            ->add('prix', NumberType::class, [
                'label' => 'Prix (FCFA)',
                'attr' => [
                    'class' => 'form-control',
                    'placeholder' => 'Ex: 1000',
                    'step' => '50',
                    'min' => '0'
                ],
                'required' => true,
                'html5' => true
            ])
            ->add('description', TextareaType::class, [
                'label' => 'Description',
                'required' => false,
                'attr' => [
                    'class' => 'form-control',
                    'rows' => 3,
                    'placeholder' => 'Description détaillée du complément...'
                ]
            ])
            ->add('typeComplement', ChoiceType::class, [
                'label' => 'Type de complément',
                'choices' => [
                        'Boisson' => 'BOISSON',
                        'Frites' => 'FRITE',
                ],
                'placeholder' => 'Sélectionnez un type',
                'attr' => [
                    'class' => 'form-select'
                ],
                'required' => true
            ])
            ->add('volume', ChoiceType::class, [
                'label' => 'Volume/Taille',
                'required' => false,
                'choices' => [
                    'Taille' => [
                        'Petite' => 'petite',
                        'Moyenne' => 'moyenne',
                        'Grande' => 'grande',
                        'Familiale' => 'familiale'
                    ],
                    'Volume (boissons)' => [
                        '20cl' => '20cl',
                        '33cl' => '33cl',
                        '50cl' => '50cl',
                        '1L' => '1L',
                        '1.5L' => '1.5L'
                    ],
                    'Portion' => [
                        'Portion normale' => 'normale',
                        'Double portion' => 'double'
                    ]
                ],
                'placeholder' => 'Sélectionnez un volume/taille',
                'attr' => [
                    'class' => 'form-select'
                ]
            ])
            ->add('imageFile', FileType::class, [
                'label' => 'Image du complément',
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
                'attr' => [
                    'class' => 'form-control',
                    'accept' => 'image/*'
                ],
                'help' => 'Format accepté: JPG, PNG, WebP. Taille max: 2MB.'
            ]);
    }

    public function configureOptions(OptionsResolver $resolver): void
    {
        $resolver->setDefaults([
            'data_class' => Complement::class,
            'attr' => ['class' => 'needs-validation', 'novalidate' => 'novalidate']
        ]);
    }
}