<?php

namespace App\Entity;

use Doctrine\ORM\Mapping as ORM;

#[ORM\Entity]
#[ORM\Table(name: 'zone_livraison')]
class ZoneLivraison
{
    #[ORM\Id]
    #[ORM\GeneratedValue]
    #[ORM\Column]
    private ?int $id = null;

    #[ORM\Column(length: 50)]
    private ?string $nom = null;

    #[ORM\Column(type: 'decimal', precision: 10, scale: 2)]
    private ?string $prixLivraison = '0';

    #[ORM\Column(type: 'text')]
    private ?string $quartiersCouverts = null;

    #[ORM\Column(type: 'datetime')]
    private ?\DateTimeInterface $createdAt = null;

    public function __construct()
    {
        $this->createdAt = new \DateTime();
    }

    public function getId(): ?int
    {
        return $this->id;
    }

    public function getNom(): ?string
    {
        return $this->nom;
    }

    public function setNom(string $nom): self
    {
        $this->nom = $nom;
        return $this;
    }

    public function getPrixLivraison(): ?string
    {
        return $this->prixLivraison;
    }

    public function setPrixLivraison(string $prixLivraison): self
    {
        $this->prixLivraison = $prixLivraison;
        return $this;
    }

    public function getQuartiersCouverts(): ?string
    {
        return $this->quartiersCouverts;
    }

    public function setQuartiersCouverts(string $quartiersCouverts): self
    {
        $this->quartiersCouverts = $quartiersCouverts;
        return $this;
    }

    public function getCreatedAt(): ?\DateTimeInterface
    {
        return $this->createdAt;
    }

    public function getFormattedPrixLivraison(): string
    {
        return number_format((float)$this->prixLivraison, 2, ',', ' ') . ' FCFA';
    }

    public function getQuartiersList(): array
    {
        if (!$this->quartiersCouverts) {
            return [];
        }
        return array_map('trim', explode(',', $this->quartiersCouverts));
    }
    
    
    public function couvreQuartier(string $quartier): bool
    {
        $quartiers = $this->getQuartiersList();
        return in_array(trim($quartier), $quartiers);
    }
    
    
    public function getQuartiersFormatted(): string
    {
        $quartiers = $this->getQuartiersList();
        return implode(', ', $quartiers);
    }
    
    
    public function __toString(): string
    {
        return $this->nom . ' (' . $this->getFormattedPrixLivraison() . ')';
    }
}