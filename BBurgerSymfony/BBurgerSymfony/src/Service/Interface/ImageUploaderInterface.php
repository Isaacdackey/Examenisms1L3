<?php

namespace App\Service\Interface;

use Symfony\Component\HttpFoundation\File\UploadedFile;

interface ImageUploaderInterface
{
    public function upload(UploadedFile $file, array $options = []): array;
    public function delete(string $publicId): array;
    public function generateUrl(string $publicId, array $transformations = []): string;
    public function isConfigured(): bool;
}