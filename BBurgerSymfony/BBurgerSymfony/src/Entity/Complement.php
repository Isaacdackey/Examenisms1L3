<?php
namespace App\Entity;

use Doctrine\ORM\Mapping as ORM;

#[ORM\Entity]
#[ORM\Table(name: 'complement')]
class Complement extends Product
{
    #[ORM\Column(length: 20, nullable: true)]
    private ?string $typeComplement = null;

    #[ORM\Column(length: 20, nullable: true)]
    private ?string $volume = null;

    public function getTypeComplement(): ?string
    {
        return $this->typeComplement;
    }

    public function setTypeComplement(?string $typeComplement): self
    {
        $this->typeComplement = $typeComplement;
        return $this;
    }

    public function getVolume(): ?string
    {
        return $this->volume;
    }

    public function setVolume(?string $volume): self
    {
        $this->volume = $volume;
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

    public function getTypeLabel(): string
    {
        $types = [
            'BOISSON' => 'Boisson',
            'FRITE' => 'Frites',
        ];

        return $types[$this->typeComplement] ?? ucfirst($this->typeComplement ?? '');
    }

    public function getComplementType(): ?string
    {
        return $this->typeComplement;
    }

    public function setComplementType(?string $type): self
    {
        $this->typeComplement = $type;
        return $this;
    }
    
    
    public function getVolumeFormatted(): string
    {
        if (!$this->volume) {
            return '';
        }
        
        
        if (str_contains($this->volume, 'cl') || str_contains($this->volume, 'L')) {
            return $this->volume;
        }
        
        
        return ucfirst($this->volume);
    }
    
    
    public function isBoisson(): bool
    {
        return $this->typeComplement === 'BOISSON';
    }
    
    public function isFrite(): bool
    {
        return $this->typeComplement === 'FRITE';
    }
}