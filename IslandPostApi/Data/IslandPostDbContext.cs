using IslandPostApi.Models;
using Microsoft.EntityFrameworkCore;

namespace IslandPostApi.Data
{
    public class IslandPostDbContext(DbContextOptions<IslandPostDbContext> options) : DbContext(options)
    {
        public DbSet<Sale> Sales => Set<Sale>();
        public DbSet<Category> Category => Set<Category>();
        public DbSet<CorrelativeNumber> CorrelativeNumbers => Set<CorrelativeNumber>();
        public DbSet<DetailSale> DetailSale => Set<DetailSale>();
        public DbSet<Email> Email => Set<Email>();
        public DbSet<Menu> Menu => Set<Menu>();
        public DbSet<Negocio> Negocio => Set<Negocio>();
        public DbSet<Product> Products => Set<Product>();
        public DbSet<Rol> Rol => Set<Rol>();
        public DbSet<RolMenu> RolMenu => Set<RolMenu>();
        public DbSet<Sale> Sale => Set<Sale>();
        public DbSet<TypeDocumentSale> TypeDocumentSale => Set<TypeDocumentSale>();
        public DbSet<User> Users => Set<User>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Category>(entity =>
            {
                entity.HasKey(e => e.IdCategory)
                    .HasName("PK__Category__79D361B6930E16FF");

                entity.ToTable("Category");

                entity.Property(e => e.IdCategory).HasColumnName("idCategory");

                entity.Property(e => e.Description)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("description");

                entity.Property(e => e.IsActive).HasColumnName("isActive");

                entity.Property(e => e.RegistrationDate)
                    .HasColumnType("datetime")
                    .HasColumnName("registrationDate")
                    .HasDefaultValueSql("(getdate())");
            });

            modelBuilder.Entity<CorrelativeNumber>(entity =>
            {
                entity.HasKey(e => e.IdCorrelativeNumber)
                    .HasName("PK__Correlat__D71CDFB02EFC51E4");

                entity.ToTable("CorrelativeNumber");

                entity.Property(e => e.IdCorrelativeNumber).HasColumnName("idCorrelativeNumber");

                entity.Property(e => e.DateUpdate)
                    .HasColumnType("datetime")
                    .HasColumnName("dateUpdate");

                entity.Property(e => e.LastNumber).HasColumnName("lastNumber");

                entity.Property(e => e.Management)
                    .HasMaxLength(100)
                    .IsUnicode(false)
                    .HasColumnName("management");

                entity.Property(e => e.QuantityDigits).HasColumnName("quantityDigits");
            });

            modelBuilder.Entity<DetailSale>(entity =>
            {
                entity.HasKey(e => e.IdDetailSale)
                    .HasName("PK__DetailSa__D072342E21B249E9");

                entity.ToTable("DetailSale");

                entity.Property(e => e.IdDetailSale).HasColumnName("idDetailSale");

                entity.Property(e => e.BrandProduct)
                    .HasMaxLength(100)
                    .IsUnicode(false)
                    .HasColumnName("brandProduct");

                entity.Property(e => e.CategoryProducty)
                    .HasMaxLength(100)
                    .IsUnicode(false)
                    .HasColumnName("categoryProducty");

                entity.Property(e => e.DescriptionProduct)
                    .HasMaxLength(100)
                    .IsUnicode(false)
                    .HasColumnName("descriptionProduct");

                entity.Property(e => e.IdProduct).HasColumnName("idProduct");

                entity.Property(e => e.IdSale).HasColumnName("idSale");

                entity.Property(e => e.Price)
                    .HasColumnType("decimal(10, 2)")
                    .HasColumnName("price");

                entity.Property(e => e.Quantity).HasColumnName("quantity");

                entity.Property(e => e.Total)
                    .HasColumnType("decimal(10, 2)")
                    .HasColumnName("total");

                entity.HasOne(d => d.IdSaleNavigation)
                    .WithMany(p => p.DetailSales)
                    .HasForeignKey(d => d.IdSale)
                    .HasConstraintName("FK__DetailSal__idSal__300424B4");
            });

            modelBuilder.Entity<Menu>(entity =>
            {
                entity.HasKey(e => e.IdMenu)
                    .HasName("PK__Menu__C26AF48328C80B96");

                entity.ToTable("Menu");

                entity.Property(e => e.IdMenu).HasColumnName("idMenu");

                entity.Property(e => e.Controller)
                    .HasMaxLength(30)
                    .IsUnicode(false)
                    .HasColumnName("controller");

                entity.Property(e => e.Description)
                    .HasMaxLength(30)
                    .IsUnicode(false)
                    .HasColumnName("description");

                entity.Property(e => e.Icon)
                    .HasMaxLength(30)
                    .IsUnicode(false)
                    .HasColumnName("icon");

                entity.Property(e => e.IdMenuParent).HasColumnName("idMenuParent");

                entity.Property(e => e.IsActive).HasColumnName("isActive");

                entity.Property(e => e.PageAction)
                    .HasMaxLength(30)
                    .IsUnicode(false)
                    .HasColumnName("pageAction");

                entity.Property(e => e.RegistrationDate)
                    .HasColumnType("datetime")
                    .HasColumnName("registrationDate")
                    .HasDefaultValueSql("(getdate())");

                entity.HasOne(d => d.IdMenuParentNavigation)
                    .WithMany(p => p.InverseIdMenuParentNavigation)
                    .HasForeignKey(d => d.IdMenuParent)
                    .HasConstraintName("FK__Menu__idMenuPare__108B795B");
            });

            modelBuilder.Entity<Negocio>(entity =>
            {
                entity.HasKey(e => e.IdNegocio)
                    .HasName("PK__Negocio__70E1E107B97CE30F");

                entity.ToTable("Negocio");

                entity.Property(e => e.IdNegocio)
                    .ValueGeneratedNever()
                    .HasColumnName("idNegocio");

                entity.Property(e => e.Correo)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("correo");

                entity.Property(e => e.Direccion)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("direccion");

                entity.Property(e => e.Nombre)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("nombre");

                entity.Property(e => e.NombreLogo)
                    .HasMaxLength(100)
                    .IsUnicode(false)
                    .HasColumnName("nombreLogo");

                entity.Property(e => e.NumeroDocumento)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("numeroDocumento");

                entity.Property(e => e.PorcentajeImpuesto)
                    .HasColumnType("decimal(10, 2)")
                    .HasColumnName("porcentajeImpuesto");

                entity.Property(e => e.SimboloMoneda)
                    .HasMaxLength(5)
                    .IsUnicode(false)
                    .HasColumnName("simboloMoneda");

                entity.Property(e => e.Telefono)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("telefono");

                entity.Property(e => e.UrlLogo)
                    .HasMaxLength(500)
                    .IsUnicode(false)
                    .HasColumnName("urlLogo");
            });

            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasKey(e => e.IdProduct)
                    .HasName("PK__Product__5EEC79D18F8E118B");

                entity.ToTable("Product");

                entity.Property(e => e.IdProduct).HasColumnName("idProduct");

                entity.Property(e => e.BarCode)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("barCode");

                entity.Property(e => e.Brand)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("brand");

                entity.Property(e => e.Description)
                    .HasMaxLength(100)
                    .IsUnicode(false)
                    .HasColumnName("description");

                entity.Property(e => e.IdCategory).HasColumnName("idCategory");

                entity.Property(e => e.IsActive).HasColumnName("isActive");
                entity.Property(e => e.IsDiscount).HasColumnName("isDiscount");

                entity.Property(e => e.Photo).HasColumnName("photo");

                entity.Property(e => e.Price)
                    .HasColumnType("decimal(10, 2)")
                    .HasColumnName("price");

                entity.Property(e => e.Quantity).HasColumnName("quantity");

                entity.Property(e => e.RegistrationDate)
                    .HasColumnType("datetime")
                    .HasColumnName("registrationDate")
                    .HasDefaultValueSql("(getdate())");

                entity.HasOne(d => d.IdCategoryNavigation)
                    .WithMany(p => p.Products)
                    .HasForeignKey(d => d.IdCategory)
                    .HasConstraintName("FK__Product__idCateg__22AA2996");
            });

            modelBuilder.Entity<Rol>(entity =>
            {
                entity.HasKey(e => e.IdRol)
                    .HasName("PK__Rol__3C872F76804F2E15");

                entity.ToTable("Rol");

                entity.Property(e => e.IdRol).HasColumnName("idRol");

                entity.Property(e => e.Description)
                    .HasMaxLength(30)
                    .IsUnicode(false)
                    .HasColumnName("description");

                entity.Property(e => e.IsActive).HasColumnName("isActive");

                entity.Property(e => e.RegistrationDate)
                    .HasColumnType("datetime")
                    .HasColumnName("registrationDate")
                    .HasDefaultValueSql("(getdate())");
            });

            modelBuilder.Entity<RolMenu>(entity =>
            {
                entity.HasKey(e => e.IdRolMenu)
                    .HasName("PK__RolMenu__CD2045D86DACA6AF");

                entity.ToTable("RolMenu");

                entity.Property(e => e.IdRolMenu).HasColumnName("idRolMenu");

                entity.Property(e => e.IdMenu).HasColumnName("idMenu");

                entity.Property(e => e.IdRol).HasColumnName("idRol");

                entity.Property(e => e.IsActive).HasColumnName("isActive");

                entity.Property(e => e.RegistrationDate)
                    .HasColumnType("datetime")
                    .HasColumnName("registrationDate")
                    .HasDefaultValueSql("(getdate())");

                entity.HasOne(d => d.IdMenuNavigation)
                    .WithMany(p => p.RolMenus)
                    .HasForeignKey(d => d.IdMenu)
                    .HasConstraintName("FK__RolMenu__idMenu__182C9B23");

                entity.HasOne(d => d.IdRolNavigation)
                    .WithMany(p => p.RolMenus)
                    .HasForeignKey(d => d.IdRol)
                    .HasConstraintName("FK__RolMenu__idRol__173876EA");
            });

            modelBuilder.Entity<Sale>(entity =>
            {
                entity.HasKey(e => e.IdSale)
                    .HasName("PK__Sale__C4AEB198091B7829");

                entity.ToTable("Sale");

                entity.Property(e => e.IdSale).HasColumnName("idSale");

                entity.Property(e => e.ClientName)
                    .HasMaxLength(20)
                    .IsUnicode(false)
                    .HasColumnName("clientName");
                entity.Property(e => e.PaymentMethod)
                    .HasMaxLength(20)
                    .IsUnicode(false)
                    .HasColumnName("paymentMethod");

                entity.Property(e => e.CustomerDocument)
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .HasColumnName("customerDocument");

                entity.Property(e => e.IdTypeDocumentSale).HasColumnName("idTypeDocumentSale");

                entity.Property(e => e.IdUsers).HasColumnName("idUsers");

                entity.Property(e => e.RegistrationDate)
                    .HasColumnType("datetime")
                    .HasColumnName("registrationDate")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.SaleNumber)
                    .HasMaxLength(6)
                    .IsUnicode(false)
                    .HasColumnName("saleNumber");

                entity.Property(e => e.Subtotal).HasColumnType("decimal(10, 2)");

                entity.Property(e => e.Total)
                    .HasColumnType("decimal(10, 2)")
                    .HasColumnName("total");

                entity.Property(e => e.TotalTaxes)
                    .HasColumnType("decimal(10, 2)")
                    .HasColumnName("totalTaxes");

                entity.HasOne(d => d.IdTypeDocumentSaleNavigation)
                    .WithMany(p => p.Sales)
                    .HasForeignKey(d => d.IdTypeDocumentSale)
                    .HasConstraintName("FK__Sale__idTypeDocu__2B3F6F97");

                entity.HasOne(d => d.IdUsersNavigation)
                    .WithMany(p => p.Sales)
                    .HasForeignKey(d => d.IdUsers)
                    .HasConstraintName("FK__Sale__idUsers__2C3393D0");
            });

            modelBuilder.Entity<TypeDocumentSale>(entity =>
            {
                entity.HasKey(e => e.IdTypeDocumentSale)
                    .HasName("PK__TypeDocu__18211B893F81F3B8");

                entity.ToTable("TypeDocumentSale");

                entity.Property(e => e.IdTypeDocumentSale).HasColumnName("idTypeDocumentSale");

                entity.Property(e => e.Description)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("description");

                entity.Property(e => e.IsActive).HasColumnName("isActive");

                entity.Property(e => e.RegistrationDate)
                    .HasColumnType("datetime")
                    .HasColumnName("registrationDate")
                    .HasDefaultValueSql("(getdate())");
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.IdUsers)
                    .HasName("PK__Users__981CF2B10C1B1086");

                entity.Property(e => e.IdUsers).HasColumnName("idUsers");

                entity.Property(e => e.Email)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("email");

                entity.Property(e => e.IdRol).HasColumnName("idRol");

                entity.Property(e => e.IsActive).HasColumnName("isActive");

                entity.Property(e => e.Name)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("name");

                entity.Property(e => e.Password)
                    .HasMaxLength(100)
                    .IsUnicode(false)
                    .HasColumnName("password");

                entity.Property(e => e.Phone)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("phone");

                entity.Property(e => e.Photo).HasColumnName("photo");

                entity.Property(e => e.RegistrationDate)
                    .HasColumnType("datetime")
                    .HasColumnName("registrationDate")
                    .HasDefaultValueSql("(getdate())");

                entity.HasOne(d => d.IdRolNavigation)
                    .WithMany(p => p.Users)
                    .HasForeignKey(d => d.IdRol)
                    .HasConstraintName("FK__Users__idRol__1BFD2C07");
            });

            modelBuilder.Entity<Email>(entity =>
            {
                // Explicitly set IdEmail as the primary key
                entity.HasKey(e => e.IdEmail);

                entity.ToTable("Email");

                // Optional: configure property constraints
                entity.Property(e => e.IdEmail)
                      .ValueGeneratedOnAdd()
                      .HasColumnName("idEmail"); // auto-increment if desired

                entity.Property(e => e.EmailAddress)
                      .IsRequired()
                      .HasMaxLength(255)
                      .HasColumnName("emailAddress");

                entity.Property(e => e.Url)
                      .HasMaxLength(500)
                      .HasColumnName("url");

                entity.Property(e => e.Date)
                      .IsRequired()                // must always have a value
                      .HasColumnType("datetime")  // or "datetime2" depending on your preference
                      .HasColumnName("date")
                      .HasDefaultValueSql("GETDATE()");

                entity.Property(e => e.IsSent)
                      .IsRequired()                // must always have a value
                      .HasDefaultValue(false);     // default to false when inserting


            });
        }
    }
}
