<?php
namespace App\Entity;

use Doctrine\ORM\Mapping as ORM;

#[ORM\Entity]
#[ORM\Table(name: 'menu')]
class Menu extends Product
{
    #[ORM\ManyToOne(targetEntity: Burger::class)]
    #[ORM\JoinColumn(name: 'burger_id', nullable: false)]
    private ?Burger $burger = null;

    #[ORM\ManyToOne(targetEntity: Complement::class)]
    #[ORM\JoinColumn(name: 'boisson_id', nullable: false)]
    private ?Complement $boisson = null;

    #[ORM\ManyToOne(targetEntity: Complement::class)]
    #[ORM\JoinColumn(name: 'frites_id', nullable: false)]
    private ?Complement $frites = null;

    #[ORM\Column(type: 'decimal', precision: 5, scale: 2, nullable: true)]
    private ?string $reductionPourcentage = '10.00';

    public function getBurger(): ?Burger
    {
        return $this->burger;
    }

    public function setBurger(?Burger $burger): self
    {
        $this->burger = $burger;
        $this->recalculerPrixSiComplet();
        return $this;
    }

    public function getBoisson(): ?Complement
    {
        return $this->boisson;
    }

    public function setBoisson(?Complement $boisson): self
    {
        $this->boisson = $boisson;
        $this->recalculerPrixSiComplet();
        return $this;
    }

    public function getFrites(): ?Complement
    {
        return $this->frites;
    }

    public function setFrites(?Complement $frites): self
    {
        $this->frites = $frites;
        $this->recalculerPrixSiComplet();
        return $this;
    }

    public function getReductionPourcentage(): ?string
    {
        return $this->reductionPourcentage;
    }

    public function setReductionPourcentage(?string $reductionPourcentage): self
    {
        $this->reductionPourcentage = $reductionPourcentage;
        $this->recalculerPrixSiComplet();
        return $this;
    }

    public function calculerPrixAuto(): float
    {
        if (!$this->burger || !$this->boisson || !$this->frites) {
            return (float) ($this->getPrix() ?: 0);
        }

        $prixBurger = (float) $this->burger->getPrix();
        $prixBoisson = (float) $this->boisson->getPrix();
        $prixFrites = (float) $this->frites->getPrix();

        if ($prixBurger <= 0 || $prixBoisson <= 0 || $prixFrites <= 0) {
            return (float) ($this->getPrix() ?: 0);
        }

        $prixTotal = $prixBurger + $prixBoisson + $prixFrites;

        if ($this->reductionPourcentage && is_numeric($this->reductionPourcentage)) {
            $reduction = $prixTotal * ((float) $this->reductionPourcentage / 100);
            $prixTotal -= $reduction;
        }

        return round($prixTotal, 2);
    }

    public function hasImage(): bool
    {
        return $this->getCloudinaryUrl() !== null && $this->getCloudinaryUrl() !== '';
    }

    public function getFormattedPrix(): string
    {
        return number_format((float) $this->getPrix(), 0, ',', ' ') . ' FCFA';
    }

    public function getPromotion(): int
    {
        return (int) $this->reductionPourcentage;
    }

    private function recalculerPrixSiComplet(): void
    {
        if ($this->burger && $this->boisson && $this->frites) {
            $nouveauPrix = $this->calculerPrixAuto();
            $this->setPrix((string) $nouveauPrix);
        }
    }

    public function getTypeLabel(): string
    {
        return "Menu " . $this->getLibelle();
    }
    

    public function getComposition(): string
    {
        if (!$this->burger || !$this->boisson || !$this->frites) {
            return 'Composition incomplète';
        }
        
        return sprintf(
            '%s + %s + %s',
            $this->burger->getLibelle(),
            $this->boisson->getLibelle(),
            $this->frites->getLibelle()
        );
    }
}