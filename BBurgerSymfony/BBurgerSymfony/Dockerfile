FROM php:8.2-apache


RUN apt-get update && apt-get install -y --no-install-recommends \
    libpng-dev libjpeg-dev libfreetype6-dev \
    libzip-dev libpq-dev \
    zip unzip \
    git \
    && docker-php-ext-configure gd --with-freetype --with-jpeg \
    && docker-php-ext-install -j2 gd pdo pdo_mysql zip pdo_pgsql \
    && apt-get clean && rm -rf /var/lib/apt/lists/*


RUN a2enmod rewrite


COPY --from=composer:latest /usr/bin/composer /usr/bin/composer


COPY . /var/www/html/
WORKDIR /var/www/html


RUN composer install --no-dev --optimize-autoloader --no-interaction --no-scripts


RUN sed -i 's/Listen 80/Listen 8080/' /etc/apache2/ports.conf \
    && sed -i 's/:80/:8080/' /etc/apache2/sites-available/*.conf


RUN mkdir -p /var/www/html/var && chown -R www-data:www-data /var/www/html/var

EXPOSE 8080
CMD ["apache2-foreground"]