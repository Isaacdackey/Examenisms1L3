using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using WebBBurger.Models;
using System.Reflection;
using Microsoft.Extensions.Configuration;

namespace WebBBurger.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Product> Products { get; set; } = null!;
        public DbSet<Commande> Commandes { get; set; } = null!;
        public DbSet<LigneCommande> LigneCommandes { get; set; } = null!;
        public DbSet<Payment> Payments { get; set; } = null!;
        public DbSet<ZoneLivraison> ZoneLivraisons { get; set; } = null!;
        public DbSet<Complement> Complements { get; set; } = null!;
        public DbSet<Burger> Burgers { get; set; } = null!;
        public DbSet<Menu> Menus { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

        
            modelBuilder.Entity<Commande>()
                .HasOne(c => c.User)
                .WithMany(u => u.Commandes)
                .HasForeignKey(c => c.UserId)
                .HasConstraintName("fk_commande_user");

            
            modelBuilder.Entity<Payment>()
                .HasOne(p => p.Commande)
                .WithOne(c => c.Payment)
                .HasForeignKey<Payment>(p => p.CommandeId)
                .HasConstraintName("fk_payment_commande");

            
            modelBuilder.Entity<LigneCommande>()
                .HasOne(lc => lc.Commande)
                .WithMany(c => c.LigneCommandes)
                .HasForeignKey(lc => lc.CommandeId)
                .HasConstraintName("fk_ligne_commande_commande");

    
            modelBuilder.Entity<LigneCommande>()
                .HasOne(lc => lc.Produit)
                .WithMany(p => p.LigneCommandes)
                .HasForeignKey(lc => lc.ProduitId)
                .HasConstraintName("fk_ligne_commande_produit");

            
            modelBuilder.Entity<Product>()
                .HasOne(p => p.Complement)
                .WithOne(c => c.Product)
                .HasForeignKey<Complement>(c => c.Id)
                .HasConstraintName("fk_complement_product");

            modelBuilder.Entity<Product>()
                .HasOne(p => p.Burger)
                .WithOne(b => b.Product)
                .HasForeignKey<Burger>(b => b.Id)
                .HasConstraintName("fk_burger_product");

            modelBuilder.Entity<Product>()
                .HasOne(p => p.Menu)
                .WithOne(m => m.Product)
                .HasForeignKey<Menu>(m => m.Id)
                .HasConstraintName("fk_menu_product");

    
            modelBuilder.Entity<Menu>()
                .HasOne(m => m.Burger)
                .WithMany()
                .HasForeignKey(m => m.BurgerId)
                .HasConstraintName("fk_menu_burger");

            modelBuilder.Entity<Menu>()
                .HasOne(m => m.Boisson)
                .WithMany()
                .HasForeignKey(m => m.BoissonId)
                .HasConstraintName("fk_menu_boisson");

            modelBuilder.Entity<Menu>()
                .HasOne(m => m.Frites)
                .WithMany()
                .HasForeignKey(m => m.FritesId)
                .HasConstraintName("fk_menu_frites");

        
            modelBuilder.Entity<Commande>()
                .HasOne(c => c.Zone)
                .WithMany(z => z.Commandes)
                .HasForeignKey(c => c.ZoneId)
                .HasConstraintName("fk_commande_zone");

            
            modelBuilder.Entity<Commande>()
                .HasOne(c => c.Livreur)
                .WithMany(u => u.Livraisons)
                .HasForeignKey(c => c.LivreurId)
                .HasConstraintName("fk_commande_livreur");

            

            modelBuilder.Entity<User>().ToTable("user");
            modelBuilder.Entity<Product>().ToTable("product");
            modelBuilder.Entity<Commande>().ToTable("commande");
            modelBuilder.Entity<LigneCommande>().ToTable("ligne_commande");
            modelBuilder.Entity<Payment>().ToTable("payment");
            modelBuilder.Entity<ZoneLivraison>().ToTable("zone_livraison");
            modelBuilder.Entity<Complement>().ToTable("complement");
            modelBuilder.Entity<Burger>().ToTable("burger");
            modelBuilder.Entity<Menu>().ToTable("menu");

            

            
            modelBuilder.Entity<User>(entity =>
            {
                entity.Property(u => u.Id).HasColumnName("id");
                entity.Property(u => u.Nom).HasColumnName("nom");
                entity.Property(u => u.Prenom).HasColumnName("prenom");
                entity.Property(u => u.Email).HasColumnName("email");
                entity.Property(u => u.Tel).HasColumnName("tel");
                entity.Property(u => u.Password).HasColumnName("password");
                entity.Property(u => u.Adresse).HasColumnName("adresse");
                entity.Property(u => u.Quartier).HasColumnName("quartier");
                entity.Property(u => u.Role).HasColumnName("role");
                entity.Property(u => u.IsActive).HasColumnName("is_active");
                entity.Property(u => u.CreatedAt).HasColumnName("created_at");
                entity.Property(u => u.UpdatedAt).HasColumnName("updated_at");
                entity.Property(u => u.Vehicule).HasColumnName("vehicule");
            });

            
            modelBuilder.Entity<Product>(entity =>
            {
                entity.Property(p => p.Id).HasColumnName("id");
                entity.Property(p => p.Libelle).HasColumnName("libelle");
                entity.Property(p => p.Description).HasColumnName("description");
                entity.Property(p => p.Prix).HasColumnName("prix").HasColumnType("decimal(10,2)");
                entity.Property(p => p.CloudinaryUrl).HasColumnName("cloudinary_url");
                entity.Property(p => p.CloudinaryPublicId).HasColumnName("cloudinary_public_id");
                entity.Property(p => p.IsArchived).HasColumnName("is_archived");
                entity.Property(p => p.TypeProduct).HasColumnName("type_product");
                entity.Property(p => p.CreatedAt).HasColumnName("created_at");
                entity.Property(p => p.UpdatedAt).HasColumnName("updated_at");
            });

            
            modelBuilder.Entity<Burger>(entity =>
            {
                entity.Property(b => b.Id).HasColumnName("id");
                entity.Property(b => b.TypeBurger).HasColumnName("type_burger");
                entity.Property(b => b.Calories).HasColumnName("calories");
                entity.Property(b => b.TempsPreparation).HasColumnName("temps_preparation");
            });

            
            modelBuilder.Entity<Complement>(entity =>
            {
                entity.Property(c => c.Id).HasColumnName("id");
                entity.Property(c => c.TypeComplement).HasColumnName("type_complement");
                entity.Property(c => c.Volume).HasColumnName("volume");
            });

        
            modelBuilder.Entity<Menu>(entity =>
            {
                entity.Property(m => m.Id).HasColumnName("id");
                entity.Property(m => m.BurgerId).HasColumnName("burger_id");
                entity.Property(m => m.BoissonId).HasColumnName("boisson_id");
                entity.Property(m => m.FritesId).HasColumnName("frites_id");
                entity.Property(m => m.ReductionPourcentage).HasColumnName("reduction_pourcentage").HasColumnType("decimal(5,2)");
            });

        
            modelBuilder.Entity<Commande>(entity =>
            {
                entity.Property(c => c.Id).HasColumnName("id");
                entity.Property(c => c.Numero).HasColumnName("numero");
                entity.Property(c => c.UserId).HasColumnName("user_id");
                entity.Property(c => c.Adresse).HasColumnName("adresse");
                entity.Property(c => c.MontantTotal).HasColumnName("montant_total").HasColumnType("decimal(10,2)");
                entity.Property(c => c.Statut).HasColumnName("statut");
                entity.Property(c => c.TypeRetrait).HasColumnName("type_retrait");
                entity.Property(c => c.ZoneId).HasColumnName("zone_id");
                entity.Property(c => c.LivreurId).HasColumnName("livreur_id");
                entity.Property(c => c.Notes).HasColumnName("notes");
                entity.Property(c => c.CreatedAt).HasColumnName("created_at");
                entity.Property(c => c.UpdatedAt).HasColumnName("updated_at");
            });

        
            modelBuilder.Entity<Payment>(entity =>
            {
                entity.Property(p => p.Id).HasColumnName("id");
                entity.Property(p => p.CommandeId).HasColumnName("commande_id");
                entity.Property(p => p.Montant).HasColumnName("montant").HasColumnType("decimal(10,2)");
                entity.Property(p => p.ReferenceTransaction).HasColumnName("reference_transaction");
                entity.Property(p => p.MoyenPaiement).HasColumnName("moyen_paiement");
                entity.Property(p => p.StatutPaiement).HasColumnName("statut_paiement");
                entity.Property(p => p.DatePaiement).HasColumnName("date_paiement");
                entity.Property(p => p.CreatedAt).HasColumnName("created_at");
            });

        
            modelBuilder.Entity<ZoneLivraison>(entity =>
            {
                entity.Property(z => z.Id).HasColumnName("id");
                entity.Property(z => z.Nom).HasColumnName("nom");
                entity.Property(z => z.PrixLivraison).HasColumnName("prix_livraison").HasColumnType("decimal(10,2)");
                entity.Property(z => z.QuartiersCouverts).HasColumnName("quartiers_couverts");
                entity.Property(z => z.CreatedAt).HasColumnName("created_at");
            });

            
            modelBuilder.Entity<LigneCommande>(entity =>
            {
                entity.Property(lc => lc.Id).HasColumnName("id");
                entity.Property(lc => lc.CommandeId).HasColumnName("commande_id");
                entity.Property(lc => lc.ProduitId).HasColumnName("produit_id");
                entity.Property(lc => lc.Quantite).HasColumnName("quantite");
                entity.Property(lc => lc.PrixUnitaire).HasColumnName("prix_unitaire");
                entity.Property(lc => lc.CreatedAt).HasColumnName("created_at");
            });

            
            modelBuilder.Entity<LigneCommande>()
                .Ignore(lc => lc.SousTotal);

        
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .HasDatabaseName("idx_user_email")
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Tel)
                .HasDatabaseName("idx_user_tel")
                .IsUnique();

            
            modelBuilder.Entity<Commande>()
                .HasIndex(c => c.Numero)
                .HasDatabaseName("idx_commande_numero")
                .IsUnique();

            modelBuilder.Entity<Commande>()
                .HasIndex(c => c.UserId)
                .HasDatabaseName("idx_commande_user_id");

            modelBuilder.Entity<Commande>()
                .HasIndex(c => c.Statut)
                .HasDatabaseName("idx_commande_statut");

        
            modelBuilder.Entity<Payment>()
                .HasIndex(p => p.ReferenceTransaction)
                .HasDatabaseName("idx_payment_reference")
                .IsUnique();

            
            modelBuilder.Entity<Product>()
                .HasIndex(p => p.TypeProduct)
                .HasDatabaseName("idx_product_type");

            
            modelBuilder.Entity<ZoneLivraison>()
                .HasIndex(z => z.Nom)
                .HasDatabaseName("idx_zone_nom")
                .IsUnique();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
        
                var configuration = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json")
                    .Build();

                var connectionString = configuration.GetConnectionString("DefaultConnection");
                optionsBuilder.UseNpgsql(connectionString, opt =>
                {
                    opt.EnableRetryOnFailure(3, TimeSpan.FromSeconds(5), null);
                });

                
                if (!Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")?.Equals("Development", StringComparison.OrdinalIgnoreCase) ?? true)
                {
                    optionsBuilder.EnableDetailedErrors(false);
                    optionsBuilder.EnableSensitiveDataLogging(false);
                }
            }
        }
    }

    
    public abstract class BaseEntity
    {
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}