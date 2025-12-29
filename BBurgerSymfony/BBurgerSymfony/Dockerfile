FROM php:8.2-apache


RUN apt-get update && apt-get install -y \
    libpng-dev \
    libjpeg-dev \
    libfreetype6-dev \
    libzip-dev \
    zip \
    unzip \
    git \
    curl \
    && docker-php-ext-configure gd --with-freetype --with-jpeg \
    && docker-php-ext-install -j$(nproc) gd pdo pdo_mysql zip pdo_pgsql


RUN a2enmod rewrite


COPY --from=composer:latest /usr/bin/composer /usr/bin/composer


COPY . /var/www/html/

WORKDIR /var/www/html


RUN composer install --no-dev --optimize-autoloader --no-interaction


RUN echo 'Listen 8080' > /etc/apache2/ports.conf
RUN sed -i 's/Listen 80/Listen 8080/g' /etc/apache2/ports.conf
RUN sed -i 's/:80/:8080/g' /etc/apache2/sites-available/*.conf


RUN chown -R www-data:www-data /var/www/html/var

EXPOSE 8080

CMD ["apache2-foreground"]