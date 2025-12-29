<?php

namespace App\Security;

use Symfony\Component\PasswordHasher\Exception\InvalidPasswordException;
use Symfony\Component\PasswordHasher\Hasher\PasswordHasherAwareInterface;
use Symfony\Component\PasswordHasher\PasswordHasherInterface;

class PlainPasswordHasher implements PasswordHasherInterface
{
    public function hash(string $plainPassword): string
    {
        return $plainPassword;
    }

    public function verify(string $hashedPassword, string $plainPassword): bool
    {
        return $plainPassword === $hashedPassword;
    }

    public function needsRehash(string $hashedPassword): bool
    {
        return false;
    }
}