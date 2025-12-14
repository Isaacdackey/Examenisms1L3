CREATE EXTENSION IF NOT EXISTS "uuid-ossp";


CREATE TABLE "user" (
    id SERIAL PRIMARY KEY,
    nom VARCHAR(50) NOT NULL,
    prenom VARCHAR(50) NOT NULL,
    tel VARCHAR(20) UNIQUE NOT NULL,
    email VARCHAR(100) UNIQUE NOT NULL,
    password VARCHAR(255) NOT NULL,
    adresse TEXT,
    quartier VARCHAR(100),
    role VARCHAR(20) CHECK (role IN ('GESTIONNAIRE','CLIENT','LIVREUR')) DEFAULT 'CLIENT',
    vehicule VARCHAR(50),
    is_active BOOLEAN DEFAULT TRUE,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);


CREATE TABLE product (
    id SERIAL PRIMARY KEY,
    libelle VARCHAR(100) NOT NULL,
    description TEXT,
    prix DECIMAL(10,2) NOT NULL DEFAULT 0,

    -- CLOUDINARY
    cloudinary_url TEXT,
    cloudinary_public_id TEXT,

    is_archived BOOLEAN DEFAULT FALSE,
    type_product VARCHAR(20) CHECK (type_product IN ('BURGER','COMPLEMENT','MENU')),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);


CREATE TABLE burger (
    id INTEGER PRIMARY KEY REFERENCES product(id) ON DELETE CASCADE,
    type_burger VARCHAR(20) CHECK (
        type_burger IN (
            'CHEESEBURGER','FISHBURGER','CHICKENBURGER','BACONBURGER','VEGGIEBURGER'
        )
    ),
    calories INT,
    temps_preparation INT
);


CREATE TABLE complement (
    id INTEGER PRIMARY KEY REFERENCES product(id) ON DELETE CASCADE,
    type_complement VARCHAR(20) CHECK (type_complement IN ('BOISSON','FRITE')),
    volume VARCHAR(20)
);


CREATE TABLE menu (
    id INTEGER PRIMARY KEY REFERENCES product(id) ON DELETE CASCADE,
    burger_id INT NOT NULL REFERENCES burger(id),
    boisson_id INT NOT NULL REFERENCES complement(id),
    frites_id INT NOT NULL REFERENCES complement(id),
    reduction_pourcentage DECIMAL(5,2) DEFAULT 10.00,
    UNIQUE (burger_id, boisson_id, frites_id)
);


CREATE TABLE zone_livraison (
    id SERIAL PRIMARY KEY,
    nom VARCHAR(50) NOT NULL,
    prix_livraison DECIMAL(10,2) NOT NULL DEFAULT 0,
    quartiers_couverts TEXT NOT NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);


CREATE TABLE commande (
    id SERIAL PRIMARY KEY,
    numero VARCHAR(30) UNIQUE,
    user_id INT NOT NULL REFERENCES "user"(id),
    adresse TEXT NOT NULL,
    montant_total DECIMAL(10,2) NOT NULL DEFAULT 0,
    statut VARCHAR(20) CHECK (statut IN (
        'EN_ATTENTE','VALIDEE','EN_PREPARATION','PRETE',
        'EN_LIVRAISON','LIVREE','TERMINEE','ANNULEE'
    )) DEFAULT 'EN_ATTENTE',
    type_retrait VARCHAR(20) CHECK (type_retrait IN ('SUR_PLACE','A_EMPORTER','LIVRAISON')),
    zone_id INT REFERENCES zone_livraison(id),
    livreur_id INT REFERENCES "user"(id),
    notes TEXT,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);


CREATE TABLE ligne_commande (
    id SERIAL PRIMARY KEY,
    commande_id INT REFERENCES commande(id) ON DELETE CASCADE,
    produit_id INT REFERENCES product(id),
    quantite INT NOT NULL DEFAULT 1,
    prix_unitaire DECIMAL(10,2) NOT NULL,
    sous_total DECIMAL(10,2) GENERATED ALWAYS AS (quantite * prix_unitaire) STORED,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    UNIQUE (commande_id, produit_id)
);


CREATE TABLE payment (
    id SERIAL PRIMARY KEY,
    commande_id INT UNIQUE REFERENCES commande(id),
    montant DECIMAL(10,2) NOT NULL,
    reference_transaction VARCHAR(100) UNIQUE NOT NULL,
    moyen_paiement VARCHAR(20) CHECK (moyen_paiement IN ('WAVE','ORANGE_MONEY','CARTE','ESPECES')),
    statut_paiement VARCHAR(20) CHECK (statut_paiement IN ('EN_ATTENTE','PAYE','ECHEC','REMBOURSE')) DEFAULT 'EN_ATTENTE',
    date_paiement TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);


CREATE OR REPLACE FUNCTION generate_commande_numero()
RETURNS TRIGGER AS $$
DECLARE num INT;
BEGIN
    SELECT COALESCE(MAX(id),0)+1 INTO num FROM commande;
    NEW.numero := 'CMD-' || TO_CHAR(NOW(),'YYYYMMDD') || '-' || LPAD(num::text,4,'0');
    RETURN NEW;
END; $$ LANGUAGE plpgsql;

CREATE TRIGGER trg_commande_numero
BEFORE INSERT ON commande
FOR EACH ROW
EXECUTE FUNCTION generate_commande_numero();


CREATE OR REPLACE FUNCTION update_montant()
RETURNS TRIGGER AS $$
BEGIN
    UPDATE commande
    SET montant_total = (
        SELECT COALESCE(SUM(sous_total),0)
        FROM ligne_commande
        WHERE commande_id = NEW.commande_id
    )
    WHERE id = NEW.commande_id;
    RETURN NEW;
END; $$ LANGUAGE plpgsql;

CREATE TRIGGER trg_update_montant
AFTER INSERT ON ligne_commande
FOR EACH ROW EXECUTE FUNCTION update_montant();




CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- Supprime les tables existantes (CASCADE pour tout supprimer)
DROP TABLE IF EXISTS payment CASCADE;
DROP TABLE IF EXISTS ligne_commande CASCADE;
DROP TABLE IF EXISTS commande CASCADE;
DROP TABLE IF EXISTS menu CASCADE;
DROP TABLE IF EXISTS complement CASCADE;
DROP TABLE IF EXISTS burger CASCADE;
DROP TABLE IF EXISTS product CASCADE;
DROP TABLE IF EXISTS zone_livraison CASCADE;
DROP TABLE IF EXISTS "user" CASCADE;



CREATE TABLE "user" (
    id SERIAL PRIMARY KEY,
    nom VARCHAR(50) NOT NULL,
    prenom VARCHAR(50) NOT NULL,
    tel VARCHAR(20) NOT NULL UNIQUE,
    email VARCHAR(100) NOT NULL UNIQUE,
    password VARCHAR(255) NOT NULL,
    adresse TEXT,
    quartier VARCHAR(100),
    role VARCHAR(20) CHECK (role IN ('GESTIONNAIRE', 'CLIENT', 'LIVREUR')) DEFAULT 'CLIENT',
    vehicule VARCHAR(50),
    is_active BOOLEAN DEFAULT TRUE,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);


CREATE TABLE product (
    id SERIAL PRIMARY KEY,
    libelle VARCHAR(100) NOT NULL,
    description TEXT,
    prix DECIMAL(10,2) NOT NULL DEFAULT 0,
    cloudinary_url TEXT,
    cloudinary_public_id TEXT,
    is_archived BOOLEAN DEFAULT FALSE,
    type_product VARCHAR(20) CHECK (type_product IN ('BURGER','COMPLEMENT','MENU')),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);


CREATE TABLE burger (
    id INTEGER PRIMARY KEY REFERENCES product(id) ON DELETE CASCADE,
    type_burger VARCHAR(20) CHECK (
        type_burger IN ('CHEESEBURGER','FISHBURGER','CHICKENBURGER','BACONBURGER','VEGGIEBURGER')
    ),
    calories INTEGER,
    temps_preparation INTEGER
);


CREATE TABLE complement (
    id INTEGER PRIMARY KEY REFERENCES product(id) ON DELETE CASCADE,
    type_complement VARCHAR(20) CHECK (type_complement IN ('BOISSON','FRITE')),
    volume VARCHAR(20)
);


CREATE TABLE menu (
    id INTEGER PRIMARY KEY REFERENCES product(id) ON DELETE CASCADE,
    burger_id INTEGER NOT NULL,
    boisson_id INTEGER NOT NULL,
    frites_id INTEGER NOT NULL,
    reduction_pourcentage DECIMAL(5,2) DEFAULT 10.00,
    UNIQUE (burger_id, boisson_id, frites_id)
);


CREATE TABLE zone_livraison (
    id SERIAL PRIMARY KEY,
    nom VARCHAR(50) NOT NULL,
    prix_livraison DECIMAL(10,2) NOT NULL DEFAULT 0,
    quartiers_couverts TEXT NOT NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);


CREATE TABLE commande (
    id SERIAL PRIMARY KEY,
    numero VARCHAR(30) UNIQUE,
    user_id INTEGER NOT NULL REFERENCES "user"(id),
    adresse TEXT NOT NULL,
    montant_total DECIMAL(10,2) NOT NULL DEFAULT 0,
    statut VARCHAR(20) CHECK (statut IN (
        'EN_ATTENTE','VALIDEE','EN_PREPARATION','PRETE',
        'EN_LIVRAISON','LIVREE','TERMINEE','ANNULEE'
    )) DEFAULT 'EN_ATTENTE',
    type_retrait VARCHAR(20) CHECK (type_retrait IN ('SUR_PLACE','A_EMPORTER','LIVRAISON')),
    zone_id INTEGER REFERENCES zone_livraison(id),
    livreur_id INTEGER REFERENCES "user"(id),
    notes TEXT,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);


CREATE TABLE ligne_commande (
    id SERIAL PRIMARY KEY,
    commande_id INTEGER REFERENCES commande(id) ON DELETE CASCADE,
    produit_id INTEGER REFERENCES product(id),
    quantite INTEGER NOT NULL DEFAULT 1,
    prix_unitaire DECIMAL(10,2) NOT NULL,
    sous_total DECIMAL(10,2) GENERATED ALWAYS AS (quantite * prix_unitaire) STORED,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    UNIQUE (commande_id, produit_id)
);


CREATE TABLE payment (
    id SERIAL PRIMARY KEY,
    commande_id INTEGER UNIQUE REFERENCES commande(id),
    montant DECIMAL(10,2) NOT NULL,
    reference_transaction VARCHAR(100) UNIQUE NOT NULL,
    moyen_paiement VARCHAR(20) CHECK (moyen_paiement IN ('WAVE','ORANGE_MONEY','CARTE','ESPECES')),
    statut_paiement VARCHAR(20) CHECK (statut_paiement IN ('EN_ATTENTE','PAYE','ECHEC','REMBOURSE')) DEFAULT 'EN_ATTENTE',
    date_paiement TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

SELECT '✅ Partie 2 terminée : Tables créées' AS status;



CREATE TABLE "user" (
    id SERIAL PRIMARY KEY,
    nom VARCHAR(50) NOT NULL,
    prenom VARCHAR(50) NOT NULL,
    tel VARCHAR(20) NOT NULL UNIQUE,
    email VARCHAR(100) NOT NULL UNIQUE,
    password VARCHAR(255) NOT NULL,
    adresse TEXT,
    quartier VARCHAR(100),
    role VARCHAR(20) CHECK (role IN ('GESTIONNAIRE', 'CLIENT', 'LIVREUR')) DEFAULT 'CLIENT',
    vehicule VARCHAR(50),
    is_active BOOLEAN DEFAULT TRUE,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);


CREATE TABLE product (
    id SERIAL PRIMARY KEY,
    libelle VARCHAR(100) NOT NULL,
    description TEXT,
    prix DECIMAL(10,2) NOT NULL DEFAULT 0,
    cloudinary_url TEXT,
    cloudinary_public_id TEXT,
    is_archived BOOLEAN DEFAULT FALSE,
    type_product VARCHAR(20) CHECK (type_product IN ('BURGER','COMPLEMENT','MENU')),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);


CREATE TABLE burger (
    id INTEGER PRIMARY KEY REFERENCES product(id) ON DELETE CASCADE,
    type_burger VARCHAR(20) CHECK (
        type_burger IN ('CHEESEBURGER','FISHBURGER','CHICKENBURGER','BACONBURGER','VEGGIEBURGER')
    ),
    calories INTEGER,
    temps_preparation INTEGER
);


CREATE TABLE complement (
    id INTEGER PRIMARY KEY REFERENCES product(id) ON DELETE CASCADE,
    type_complement VARCHAR(20) CHECK (type_complement IN ('BOISSON','FRITE')),
    volume VARCHAR(20)
);


CREATE TABLE menu (
    id INTEGER PRIMARY KEY REFERENCES product(id) ON DELETE CASCADE,
    burger_id INTEGER NOT NULL,
    boisson_id INTEGER NOT NULL,
    frites_id INTEGER NOT NULL,
    reduction_pourcentage DECIMAL(5,2) DEFAULT 10.00,
    UNIQUE (burger_id, boisson_id, frites_id)
);


CREATE TABLE zone_livraison (
    id SERIAL PRIMARY KEY,
    nom VARCHAR(50) NOT NULL,
    prix_livraison DECIMAL(10,2) NOT NULL DEFAULT 0,
    quartiers_couverts TEXT NOT NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);


CREATE TABLE commande (
    id SERIAL PRIMARY KEY,
    numero VARCHAR(30) UNIQUE,
    user_id INTEGER NOT NULL REFERENCES "user"(id),
    adresse TEXT NOT NULL,
    montant_total DECIMAL(10,2) NOT NULL DEFAULT 0,
    statut VARCHAR(20) CHECK (statut IN (
        'EN_ATTENTE','VALIDEE','EN_PREPARATION','PRETE',
        'EN_LIVRAISON','LIVREE','TERMINEE','ANNULEE'
    )) DEFAULT 'EN_ATTENTE',
    type_retrait VARCHAR(20) CHECK (type_retrait IN ('SUR_PLACE','A_EMPORTER','LIVRAISON')),
    zone_id INTEGER REFERENCES zone_livraison(id),
    livreur_id INTEGER REFERENCES "user"(id),
    notes TEXT,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);


CREATE TABLE ligne_commande (
    id SERIAL PRIMARY KEY,
    commande_id INTEGER REFERENCES commande(id) ON DELETE CASCADE,
    produit_id INTEGER REFERENCES product(id),
    quantite INTEGER NOT NULL DEFAULT 1,
    prix_unitaire DECIMAL(10,2) NOT NULL,
    sous_total DECIMAL(10,2) GENERATED ALWAYS AS (quantite * prix_unitaire) STORED,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    UNIQUE (commande_id, produit_id)
);


CREATE TABLE payment (
    id SERIAL PRIMARY KEY,
    commande_id INTEGER UNIQUE REFERENCES commande(id),
    montant DECIMAL(10,2) NOT NULL,
    reference_transaction VARCHAR(100) UNIQUE NOT NULL,
    moyen_paiement VARCHAR(20) CHECK (moyen_paiement IN ('WAVE','ORANGE_MONEY','CARTE','ESPECES')),
    statut_paiement VARCHAR(20) CHECK (statut_paiement IN ('EN_ATTENTE','PAYE','ECHEC','REMBOURSE')) DEFAULT 'EN_ATTENTE',
    date_paiement TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

SELECT '✅ Partie 2 terminée : Tables créées' AS status;



CREATE OR REPLACE FUNCTION generate_commande_numero()
RETURNS TRIGGER AS $$
DECLARE 
    num INTEGER;
    today_date VARCHAR(8);
BEGIN
    SELECT COALESCE(MAX(id), 0) + 1 INTO num FROM commande;
    today_date := TO_CHAR(NOW(), 'YYYYMMDD');
    NEW.numero := 'CMD-' || today_date || '-' || LPAD(num::TEXT, 4, '0');
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;


CREATE TRIGGER trg_commande_numero
BEFORE INSERT ON commande
FOR EACH ROW
EXECUTE FUNCTION generate_commande_numero();


CREATE OR REPLACE FUNCTION update_montant_total()
RETURNS TRIGGER AS $$
BEGIN
    UPDATE commande
    SET montant_total = (
        SELECT COALESCE(SUM(sous_total), 0)
        FROM ligne_commande
        WHERE commande_id = NEW.commande_id
    ),
    updated_at = NOW()
    WHERE id = NEW.commande_id;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;


CREATE TRIGGER trg_update_montant
AFTER INSERT OR UPDATE OR DELETE ON ligne_commande
FOR EACH ROW
EXECUTE FUNCTION update_montant_total();


CREATE OR REPLACE FUNCTION update_updated_at_column()
RETURNS TRIGGER AS $$
BEGIN
    NEW.updated_at = CURRENT_TIMESTAMP;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;


CREATE TRIGGER update_user_updated_at BEFORE UPDATE ON "user" 
FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

CREATE TRIGGER update_product_updated_at BEFORE UPDATE ON product 
FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

CREATE TRIGGER update_commande_updated_at BEFORE UPDATE ON commande 
FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();




INSERT INTO "user" (nom, prenom, tel, email, password, role, adresse, quartier) VALUES
('Admin', 'System', '771234567', 'admin@brasilburger.com', 'admin123', 'GESTIONNAIRE', '123 Rue Principale', 'Plateau'),
('Diallo', 'Mamadou', '775678901', 'mamadou@livreur.com', 'livreur1', 'LIVREUR', '456 Avenue Liberté', 'Medina'),
('Sow', 'Fatou', '776789012', 'fatou@livreur.com', 'livreur2', 'LIVREUR', '789 Boulevard Fall', 'Fass'),
('Ndiaye', 'Aminata', '777890123', 'aminata@client.com', 'client1', 'CLIENT', '321 Rue Khalifa', 'Grand Yoff'),
('Gueye', 'Ibrahima', '778901234', 'ibrahima@client.com', 'client2', 'CLIENT', '654 Avenue Lamine', 'Mermoz');


INSERT INTO zone_livraison (nom, prix_livraison, quartiers_couverts) VALUES
('Zone Centre', 1000.00, 'Medina, Plateau, Fass, Colobane'),
('Zone Nord', 1500.00, 'Grand Yoff, Parcelles, Cambérène, Dieuppeul'),
('Zone Sud', 2000.00, 'Mermoz, Sacré Coeur, Ouakam, Ngor');


INSERT INTO product (libelle, description, prix, type_product) VALUES
('Cheese Burger', 'Burger au fromage fondant avec salade et tomate', 3500.00, 'BURGER'),
('Fish Burger', 'Burger au poisson pané avec sauce tartare', 3800.00, 'BURGER'),
('Chicken Burger', 'Burger au poulet croustillant avec sauce mayo', 3700.00, 'BURGER'),
('Bacon Burger', 'Burger avec bacon grillé et cheddar', 4200.00, 'BURGER'),
('Veggie Burger', 'Burger végétarien aux légumes grillés', 3200.00, 'BURGER');


INSERT INTO burger (id, type_burger, calories, temps_preparation) VALUES
(1, 'CHEESEBURGER', 650, 10),
(2, 'FISHBURGER', 550, 12),
(3, 'CHICKENBURGER', 600, 15),
(4, 'BACONBURGER', 750, 12),
(5, 'VEGGIEBURGER', 450, 8);

SELECT '✅ Partie 3 terminée : Fonctions et données partiellement insérées' AS status;





INSERT INTO product (libelle, description, prix, type_product) VALUES
('Coca-Cola 50cl', 'Boisson gazeuse', 1000.00, 'COMPLEMENT'),
('Fanta Orange 50cl', 'Boisson gazeuse orange', 1000.00, 'COMPLEMENT'),
('Eau Minérale 50cl', 'Eau plate', 500.00, 'COMPLEMENT'),
('Frites Classiques', 'Portion de frites', 1500.00, 'COMPLEMENT'),
('Frites Maison', 'Frites épaisses avec peau', 2000.00, 'COMPLEMENT');


INSERT INTO complement (id, type_complement, volume) VALUES
(6, 'BOISSON', '500ml'),
(7, 'BOISSON', '500ml'),
(8, 'BOISSON', '500ml'),
(9, 'FRITE', '200g'),
(10, 'FRITE', '250g');


INSERT INTO product (libelle, description, prix, type_product) VALUES
('Menu Cheese Burger', 'Cheese Burger + Coca-Cola + Frites Classiques', 5400.00, 'MENU'),
('Menu Chicken Burger', 'Chicken Burger + Fanta + Frites Maison', 6030.00, 'MENU'),
('Menu Veggie Burger', 'Veggie Burger + Eau + Frites Classiques', 4680.00, 'MENU');


INSERT INTO menu (id, burger_id, boisson_id, frites_id, reduction_pourcentage) VALUES
(11, 1, 6, 9, 10.00),
(12, 3, 7, 10, 10.00),
(13, 5, 8, 9, 10.00);




CREATE VIEW v_commandes_en_cours AS
SELECT 
    c.id,
    c.numero,
    CONCAT(u.prenom, ' ', u.nom) AS client,
    c.adresse,
    c.montant_total AS montant,
    c.statut,
    c.type_retrait,
    c.created_at
FROM commande c
JOIN "user" u ON c.user_id = u.id
WHERE DATE(c.created_at) = CURRENT_DATE
AND c.statut IN ('EN_ATTENTE','VALIDEE','EN_PREPARATION','PRETE','EN_LIVRAISON')
ORDER BY c.created_at DESC;


CREATE VIEW v_recettes_journalieres AS
SELECT 
    DATE(p.date_paiement) AS jour,
    SUM(p.montant) AS recette_totale,
    COUNT(DISTINCT p.commande_id) AS nb_commandes_payees,
    p.moyen_paiement,
    COUNT(*) AS nb_paiements
FROM payment p
WHERE p.statut_paiement = 'PAYE'
GROUP BY DATE(p.date_paiement), p.moyen_paiement
ORDER BY jour DESC;


CREATE INDEX idx_user_email ON "user"(email);
CREATE INDEX idx_user_role ON "user"(role, is_active);
CREATE INDEX idx_product_type ON product(type_product, is_archived);
CREATE INDEX idx_commande_statut ON commande(statut, created_at);
CREATE INDEX idx_commande_user ON commande(user_id);
CREATE INDEX idx_ligne_commande_commande ON ligne_commande(commande_id);
CREATE INDEX idx_payment_commande ON payment(commande_id);




SELECT 'STATISTIQUES :' AS titre;
SELECT 
    (SELECT COUNT(*) FROM "user") AS "Utilisateurs",
    (SELECT COUNT(*) FROM product) AS "Produits",
    (SELECT COUNT(*) FROM burger) AS "Burgers",
    (SELECT COUNT(*) FROM complement) AS "Compléments",
    (SELECT COUNT(*) FROM menu) AS "Menus",
    (SELECT COUNT(*) FROM zone_livraison) AS "Zones livraison";