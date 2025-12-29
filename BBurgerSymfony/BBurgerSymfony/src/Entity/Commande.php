<?php

namespace App\Entity;

use Doctrine\Common\Collections\ArrayCollection;
use Doctrine\Common\Collections\Collection;
use Doctrine\ORM\Mapping as ORM;

#[ORM\Entity]
#[ORM\Table(name: 'commande')]
class Commande
{
    #[ORM\Id]
    #[ORM\GeneratedValue]
    #[ORM\Column]
    private ?int $id = null;

    #[ORM\Column(length: 30, unique: true)]
    private ?string $numero = null;

    #[ORM\ManyToOne(targetEntity: User::class)]
    #[ORM\JoinColumn(name: 'user_id', nullable: false)]
    private ?User $user = null;

    #[ORM\Column(type: 'text')]
    private ?string $adresse = null;

    #[ORM\Column(type: 'decimal', precision: 10, scale: 2)]
    private ?string $montantTotal = '0';

    #[ORM\Column(length: 20)]
    private ?string $statut = 'EN_ATTENTE';

    #[ORM\Column(length: 20)]
    private ?string $typeRetrait = null;

    #[ORM\ManyToOne(targetEntity: ZoneLivraison::class)]
    #[ORM\JoinColumn(name: 'zone_id', nullable: true)]
    private ?ZoneLivraison $zone = null;

    #[ORM\ManyToOne(targetEntity: User::class)]
    #[ORM\JoinColumn(name: 'livreur_id', nullable: true)]
    private ?User $livreur = null;

    #[ORM\Column(type: 'text', nullable: true)]
    private ?string $notes = null;

    #[ORM\Column(type: 'datetime')]
    private ?\DateTimeInterface $createdAt = null;

    #[ORM\Column(type: 'datetime', nullable: true)]
    private ?\DateTimeInterface $updatedAt = null;

    #[ORM\OneToMany(targetEntity: LigneCommande::class, mappedBy: 'commande', cascade: ['persist', 'remove'])]
    private Collection $lignesCommande;

    public function __construct()
    {
        $this->createdAt = new \DateTime();
        $this->lignesCommande = new ArrayCollection();
        $this->numero = $this->generateNumero();
    }

    private function generateNumero(): string
    {
        return 'CMD' . date('YmdHis') . rand(100, 999);
    }

    public function getId(): ?int
    {
        return $this->id;
    }

    public function getNumero(): ?string
    {
        return $this->numero;
    }

    public function setNumero(string $numero): self
    {
        $this->numero = $numero;
        return $this;
    }

    public function getUser(): ?User
    {
        return $this->user;
    }

    public function setUser(?User $user): self
    {
        $this->user = $user;
        return $this;
    }

    public function getAdresse(): ?string
    {
        return $this->adresse;
    }

    public function setAdresse(string $adresse): self
    {
        $this->adresse = $adresse;
        return $this;
    }

    public function getMontantTotal(): ?string
    {
        return $this->montantTotal;
    }

    public function setMontantTotal(string $montantTotal): self
    {
        $this->montantTotal = $montantTotal;
        return $this;
    }

    public function getStatut(): ?string
    {
        return $this->statut;
    }

    public function setStatut(string $statut): self
    {
        $this->statut = $statut;
        return $this;
    }

    public function getTypeRetrait(): ?string
    {
        return $this->typeRetrait;
    }

    public function setTypeRetrait(string $typeRetrait): self
    {
        $this->typeRetrait = $typeRetrait;
        return $this;
    }

    public function getZone(): ?ZoneLivraison
    {
        return $this->zone;
    }

    public function setZone(?ZoneLivraison $zone): self
    {
        $this->zone = $zone;
        return $this;
    }

    public function getLivreur(): ?User
    {
        return $this->livreur;
    }

    public function setLivreur(?User $livreur): self
    {
        $this->livreur = $livreur;
        return $this;
    }

    public function getNotes(): ?string
    {
        return $this->notes;
    }

    public function setNotes(?string $notes): self
    {
        $this->notes = $notes;
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

    public function getLignesCommande(): Collection
    {
        return $this->lignesCommande;
    }

    public function addLigneCommande(LigneCommande $ligne): self
    {
        if (!$this->lignesCommande->contains($ligne)) {
            $this->lignesCommande->add($ligne);
            $ligne->setCommande($this);
        }
        return $this;
    }

    public function removeLigneCommande(LigneCommande $ligne): self
    {
        if ($this->lignesCommande->removeElement($ligne)) {
            if ($ligne->getCommande() === $this) {
                $ligne->setCommande(null);
            }
        }
        return $this;
    }

    public function getFormattedMontant(): string
    {
        return number_format((float)$this->montantTotal, 2, ',', ' ') . ' FCFA';
    }

    public function getStatutLabel(): string
    {
        return match($this->statut) {
            'EN_ATTENTE' => 'En attente',
            'VALIDEE' => 'Validée',
            'EN_PREPARATION' => 'En préparation',
            'PRETE' => 'Prête',
            'EN_LIVRAISON' => 'En livraison',
            'LIVREE' => 'Livrée',
            'TERMINEE' => 'Terminée',
            'ANNULEE' => 'Annulée',
            default => $this->statut
        };
    }
    
    
    public function peutEtreAnnulee(): bool
    {
        return in_array($this->statut, ['EN_ATTENTE', 'VALIDEE', 'EN_PREPARATION']);
    }
    
    
    public function peutEtreValidee(): bool
    {
        return $this->statut === 'EN_ATTENTE';
    }
    
    
    public function peutEtreMarqueePrete(): bool
    {
        return in_array($this->statut, ['VALIDEE', 'EN_PREPARATION']);
    }
    
    
    public function getNombreArticles(): int
    {
        $total = 0;
        foreach ($this->lignesCommande as $ligne) {
            $total += $ligne->getQuantite();
        }
        return $total;
    }
    

    public function getProduits(): array
    {
        $produits = [];
        foreach ($this->lignesCommande as $ligne) {
            $produits[] = [
                'produit' => $ligne->getProduit(),
                'quantite' => $ligne->getQuantite(),
                'prix_unitaire' => $ligne->getPrixUnitaire()
            ];
        }
        return $produits;
    }
}