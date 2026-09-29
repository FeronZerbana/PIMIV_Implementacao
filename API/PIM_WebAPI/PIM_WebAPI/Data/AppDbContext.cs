using Microsoft.EntityFrameworkCore;
using PIM_WebAPI.Models;

namespace PIM_WebAPI.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Produto> Produtos { get; set; }
        public DbSet<Estoque> Estoques { get; set; }
        public DbSet<Venda> Vendas { get; set; }
        public DbSet<ItemVenda> ItemVendas { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuracao da Entidade Usuario
            modelBuilder.Entity<Usuario>(entity =>
            {
                entity.ToTable("Usuarios");
                entity.HasKey(u => u.ID);
                entity.HasIndex(u => u.Email).IsUnique();
                entity.Property(u => u.Nome).HasMaxLength(200).IsRequired();
                entity.Property(u => u.Email).HasMaxLength(250).IsRequired();
                entity.Property(u => u.Password_Hash).HasMaxLength(300).IsRequired();
                entity.Property(u => u.IsAdmin).HasDefaultValue(false);
            });

            // Configuracao da Entidade Produto
            modelBuilder.Entity<Produto>(entity =>
            {
                entity.ToTable("Produtos");
                entity.HasKey(p => p.ID);
                entity.Property(p => p.Preco).HasColumnType("decimal(8,2)").IsRequired();
                entity.Property(p => p.Descricao).HasMaxLength(2000);
                entity.Property(p => p.ImgURL).HasMaxLength(255);
            });

            // Configuracao da Entidade Estoque (1:1 com Produto)
            modelBuilder.Entity<Estoque>(entity =>
            {
                entity.ToTable("Estoques");
                entity.HasKey(e => e.ID);
                entity.HasIndex(e => e.ProdutoID).IsUnique();
                entity.HasOne(e => e.Produto)
                      .WithOne(p => p.Estoque)
                      .HasForeignKey<Estoque>(e => e.ProdutoID)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Configuracao da Entidade Venda
            modelBuilder.Entity<Venda>(entity =>
            {
                entity.ToTable("Vendas");
                entity.HasKey(v => v.ID);
                entity.HasOne(v => v.Usuario)
                      .WithMany()
                      .HasForeignKey(v => v.UsuarioID)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Configuracao da Entidade ItemVenda
            modelBuilder.Entity<ItemVenda>(entity =>
            {
                entity.ToTable("ItemVendas");
                entity.HasKey(iv => iv.ID);
                entity.Property(iv => iv.ValorUnitario).HasColumnType("decimal(8,2)").IsRequired();
                
                entity.HasOne(iv => iv.Venda)
                      .WithMany(v => v.Itens)
                      .HasForeignKey(iv => iv.VendaID)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(iv => iv.Produto)
                      .WithMany(p => p.ItemVendas)
                      .HasForeignKey(iv => iv.ProdutoID)
                      .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
