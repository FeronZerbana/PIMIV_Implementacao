using Microsoft.EntityFrameworkCore;
using PIM_WebApplication.Models;

namespace PIM_WebApplication.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Cliente> Clientes { get; set; } = null!;
        public DbSet<Produto> Produtos { get; set; } = null!;
        public DbSet<Estoque> Estoques { get; set; } = null!;
        public DbSet<Venda> Vendas { get; set; } = null!;
        public DbSet<ItemVenda> ItemVendas { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Cliente>(entity =>
            {
                entity.ToTable("CLIENTES");
                entity.HasKey(e => e.IdCliente);
                entity.Property(e => e.IdCliente).HasColumnName("id_cliente");
                entity.Property(e => e.Nome).HasColumnName("Nome").HasMaxLength(150).IsRequired();
                entity.Property(e => e.Email).HasColumnName("Email").HasMaxLength(150).IsRequired();
                entity.Property(e => e.Senha).HasColumnName("Senha").HasMaxLength(255).IsRequired();
                entity.Property(e => e.Telefone).HasColumnName("Telefone").HasMaxLength(30);
                entity.Property(e => e.IsAdmin).HasColumnName("isAdmin").HasDefaultValue(false);
            });

            modelBuilder.Entity<Produto>(entity =>
            {
                entity.ToTable("PRODUTOS");
                entity.HasKey(e => e.IdProduto);
                entity.Property(e => e.IdProduto).HasColumnName("id_Produto");
                entity.Property(e => e.TipoRoupa).HasColumnName("Tipo_Roupa").HasMaxLength(150).IsRequired();
                entity.Property(e => e.Preco).HasColumnName("Preco").HasColumnType("decimal(8,2)").IsRequired();
                entity.Property(e => e.Tamanho).HasColumnName("Tamanho").HasMaxLength(10);

                entity.HasOne(p => p.Estoque)
                      .WithOne(e => e.Produto)
                      .HasForeignKey<Estoque>(e => e.CodProduto)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Estoque>(entity =>
            {
                entity.ToTable("ESTOQUE");
                entity.HasKey(e => e.IdEstoque);
                entity.Property(e => e.IdEstoque).HasColumnName("id_Estoque");
                entity.Property(e => e.CodProduto).HasColumnName("Cod_do_Produto").IsRequired();
                entity.Property(e => e.QuantidadeDisponivel).HasColumnName("Quantidade_Disponivel").IsRequired();
            });

            modelBuilder.Entity<Venda>(entity =>
            {
                entity.ToTable("VENDAS");
                entity.HasKey(e => e.IdVenda);
                entity.Property(e => e.IdVenda).HasColumnName("id_Vendas");
                entity.Property(e => e.CodCliente).HasColumnName("Cod_do_Cliente").IsRequired();
                entity.Property(e => e.DataVenda).HasColumnName("Data_Venda").IsRequired();

                entity.HasMany(v => v.Itens)
                      .WithOne(i => i.Venda)
                      .HasForeignKey(i => i.CodVenda)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<ItemVenda>(entity =>
            {
                entity.ToTable("ITEM_VENDAS");
                entity.HasKey(e => e.IdItemVenda);
                entity.Property(e => e.IdItemVenda).HasColumnName("id_Item_Venda");
                entity.Property(e => e.CodVenda).HasColumnName("Cod_da_Venda").IsRequired();
                entity.Property(e => e.CodProduto).HasColumnName("Cod_do_Produto").IsRequired();
                entity.Property(e => e.Quantidade).HasColumnName("Quantidade").IsRequired();
                entity.Property(e => e.ValorUnitario).HasColumnName("Valor_Unitario").HasColumnType("decimal(8,2)").IsRequired();
                entity.Property(e => e.ValorTotal).HasColumnName("Valor_Total").HasColumnType("decimal(8,2)").IsRequired();

                entity.HasOne(i => i.Produto)
                      .WithMany()
                      .HasForeignKey(i => i.CodProduto)
                      .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
