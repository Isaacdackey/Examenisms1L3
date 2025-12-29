<?php

namespace App\Service;

use Cloudinary\Cloudinary;
use Cloudinary\Api\Upload\UploadApi;
use Symfony\Component\HttpFoundation\File\UploadedFile;

class CloudinaryService
{
    private $cloudinary;
    private $isConfigured;

    public function __construct(
        string $cloudinaryUrl,
        string $cloudName = '',
        string $apiKey = '',
        string $apiSecret = ''
    ) {
        if (!empty($cloudName) && !empty($apiKey) && !empty($apiSecret)) {
            try {
                $this->cloudinary = new Cloudinary([
                    'cloud' => [
                        'cloud_name' => $cloudName,
                        'api_key' => $apiKey,
                        'api_secret' => $apiSecret,
                    ],
                    'url' => [
                        'secure' => true
                    ]
                ]);
                
                $config = $this->cloudinary->configuration;
                if ($config && 
                    $config->cloud->cloudName === $cloudName && 
                    $config->cloud->apiKey === $apiKey && 
                    $config->cloud->apiSecret === $apiSecret) {
                    $this->isConfigured = true;
                } else {
                    throw new \Exception('Configuration Cloudinary invalide');
                }
                
            } catch (\Exception $e) {
                $this->isConfigured = false;
                $this->cloudinary = null;
                error_log('Erreur Cloudinary: ' . $e->getMessage());
            }
        } else {
            $this->isConfigured = false;
            $this->cloudinary = null;
            error_log('Cloudinary: paramètres manquants');
        }

        if ($this->isConfigured && $this->cloudinary) {
            try {
                $this->cloudinary->configuration->url->analytics(false);
            } catch (\Exception $e) {
                
            }
        }
    }

    
    public function uploadImage(UploadedFile $file, array $options = []): array
    {
        if (!$this->isConfigured || !$this->cloudinary) {
            error_log('Cloudinary non configuré, utilisation du fallback local');
            return $this->uploadToLocal($file);
        }

        try {
            
            $uploadApi = new UploadApi($this->cloudinary->configuration);
            
            
            $defaultOptions = [
                'folder' => 'bburger/products',
                'resource_type' => 'image',
                'transformation' => [
                    'width' => 800,
                    'height' => 600,
                    'crop' => 'fill',
                    'quality' => 'auto:best',
                    'fetch_format' => 'auto'
                ]
            ];

            $mergedOptions = array_merge($defaultOptions, $options);

            
            $result = $uploadApi->upload($file->getRealPath(), $mergedOptions);

            return [
                'success' => true,
                'url' => $result['secure_url'],
                'public_id' => $result['public_id'],
                'format' => $result['format'],
                'width' => $result['width'],
                'height' => $result['height'],
                'bytes' => $result['bytes'],
                'created_at' => $result['created_at']
            ];

        } catch (\Exception $e) {
            error_log('Erreur upload Cloudinary: ' . $e->getMessage());
            return $this->uploadToLocal($file);
        }
    }


    private function uploadToLocal(UploadedFile $file): array
    {
        try {
            $filename = uniqid() . '.' . $file->guessExtension();
            $publicDir = __DIR__ . '/../../public/uploads/';
            
            if (!is_dir($publicDir)) {
                mkdir($publicDir, 0755, true);
            }
            
            $file->move($publicDir, $filename);
            
            return [
                'success' => true,
                'url' => '/uploads/' . $filename,
                'public_id' => 'local_' . $filename,
                'format' => $file->guessExtension(),
                'width' => 800,
                'height' => 600,
                'bytes' => $file->getSize(),
                'created_at' => date('Y-m-d H:i:s')
            ];
        } catch (\Exception $e) {
            return [
                'success' => false,
                'error' => $e->getMessage(),
                'url' => null,
                'public_id' => null
            ];
        }
    }

    
    public function deleteImage(string $publicId): array
    {
        if (!$this->isConfigured || !$this->cloudinary || strpos($publicId, 'local_') === 0) {
            return ['success' => true];
        }

        try {
            $uploadApi = new UploadApi($this->cloudinary->configuration);
            $result = $uploadApi->destroy($publicId);
            
            return [
                'success' => $result['result'] === 'ok',
                'result' => $result
            ];

        } catch (\Exception $e) {
            return [
                'success' => false,
                'error' => $e->getMessage()
            ];
        }
    }

    
    public function generateImageUrl(string $publicId, array $transformations = []): string
    {
        if (!$this->isConfigured || !$this->cloudinary || strpos($publicId, 'local_') === 0) {
            if (strpos($publicId, 'local_') === 0) {
                return str_replace('local_', '/uploads/', $publicId);
            }
            return '';
        }

        try {
            $defaultTransformations = [
                'width' => 400,
                'height' => 300,
                'crop' => 'fill',
                'quality' => 'auto'
            ];

            $mergedTransformations = array_merge($defaultTransformations, $transformations);

            return $this->cloudinary->image($publicId)
                ->toUrl($mergedTransformations);

        } catch (\Exception $e) {
            return '';
        }
    }

   
    public function isConfigured(): bool
    {
        return $this->isConfigured && $this->cloudinary !== null;
    }
}