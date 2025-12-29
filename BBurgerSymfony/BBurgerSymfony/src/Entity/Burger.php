<?php
namespace App\Entity;

use Doctrine\ORM\Mapping as ORM;

#[ORM\Entity]
#[ORM\Table(name: 'burger')]
class Burger extends Product
{
    #[ORM\Column(length: 20, nullable: true)]
    private ?string $typeBurger = null;

    #[ORM\Column(nullable: true)]
    private ?int $calories = null;

    #[ORM\Column(nullable: true)]
    private ?int $tempsPreparation = null;


    public function getTypeBurger(): ?string
    {
        return $this->typeBurger;
    }
    public function setTypeBurger(?string $typeBurger): self
    {
        $this->typeBurger = $typeBurger;
        return $this;
    }
    public function getCalories(): ?int
    {
        return $this->calories;
    }
    public function setCalories(?int $calories): self

    {
        $this->calories = $calories;
        return $this;
    }
    public function getTempsPreparation(): ?int
    {
        return $this->tempsPreparation;
    }
    public function setTempsPreparation(?int $tempsPreparation): self
    {
        $this->tempsPreparation = $tempsPreparation;
        return $this;
    }

    public function hasImage(): bool
    {
        return $this->getCloudinaryUrl() !== null && $this->getCloudinaryUrl() !== '';
    }

    public function getFormattedPrix(): string
    {
        return number_format((float) $this->getPrix(), 0, ',', ' ') . ' FCFA';
    }

}