<?php

namespace App\Entity;

use Doctrine\ORM\Mapping as ORM;

#[ORM\Entity]
#[ORM\Table(name: 'product')]
#[ORM\InheritanceType('JOINED')]
#[ORM\DiscriminatorColumn(name: 'type_product', type: 'string', length: 20)]
#[ORM\DiscriminatorMap([
    'BURGER' => 'Burger',
    'COMPLEMENT' => 'Complement',
    'MENU' => 'Menu'
])]
class Product
{
    #[ORM\Id]
    #[ORM\GeneratedValue]
    #[ORM\Column]
    private ?int $id = null;

    #[ORM\Column(length: 100)]
    private ?string $libelle = null;

    #[ORM\Column(type: 'text', nullable: true)]
    private ?string $description = null;

    #[ORM\Column(type: 'decimal', precision: 10, scale: 2)]
    private ?string $prix = '0';

    #[ORM\Column(type: 'text', nullable: true)]
    private ?string $cloudinaryUrl = null;

    #[ORM\Column(type: 'text', nullable: true)]
    private ?string $cloudinaryPublicId = null;

    #[ORM\Column(type: 'boolean')]
    private bool $isArchived = false;

    #[ORM\Column(type: 'datetime')]
    private ?\DateTimeInterface $createdAt = null;

    #[ORM\Column(type: 'datetime', nullable: true)]
    private ?\DateTimeInterface $updatedAt = null;

    public function __construct()
    {
        $this->createdAt = new \DateTime();
        $this->isArchived = false;
    }

    public function getId(): ?int
    {
        return $this->id;
    }

    public function getLibelle(): ?string
    {
        return $this->libelle;
    }

    public function setLibelle(string $libelle): self
    {
        $this->libelle = $libelle;
        return $this;
    }

    public function getDescription(): ?string
    {
        return $this->description;
    }

    public function setDescription(?string $description): self
    {
        $this->description = $description;
        return $this;
    }

    public function getPrix(): ?string
    {
        return $this->prix;
    }

    public function setPrix(string $prix): self
    {
        $this->prix = $prix;
        return $this;
    }

    public function getCloudinaryUrl(): ?string
    {
        return $this->cloudinaryUrl;
    }

    public function setCloudinaryUrl(?string $cloudinaryUrl): self
    {
        $this->cloudinaryUrl = $cloudinaryUrl;
        return $this;
    }

    public function getCloudinaryPublicId(): ?string
    {
        return $this->cloudinaryPublicId;
    }

    public function setCloudinaryPublicId(?string $cloudinaryPublicId): self
    {
        $this->cloudinaryPublicId = $cloudinaryPublicId;
        return $this;
    }

    public function isArchived(): bool
    {
        return $this->isArchived;
    }

    public function setIsArchived(bool $isArchived): self
    {
        $this->isArchived = $isArchived;
        return $this;
    }

    public function getCreatedAt(): ?\DateTimeInterface
    {
        return $this->createdAt;
    }

    public function getUpdatedAt(): ?\DateTimeInterface
    {
        return $this->updatedAt;
    }

    public function setUpdatedAt(?\DateTimeInterface $updatedAt): self
    {
        $this->updatedAt = $updatedAt;
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

    
    public function getType(): string
    {
        if ($this instanceof Burger) {
            return 'BURGER';
        }
        if ($this instanceof Menu) {
            return 'MENU';
        }
        if ($this instanceof Complement) {
            return 'COMPLEMENT';
        }
        return 'PRODUCT';
    }
    
    
    public function getTypeLabel(): string
    {
        return match($this->getType()) {
            'BURGER' => 'Burger',
            'MENU' => 'Menu',
            'COMPLEMENT' => 'Complément',
            default => 'Produit'
        };
    }
    
    
    public function __toString(): string
    {
        return $this->libelle . ' (' . $this->getTypeLabel() . ')';
    }
}