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
        public DbSet<Cupom> Cupons { get; set; } = null!;
        public DbSet<CupomUso> CupomUsos { get; set; } = null!;

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
                entity.Property(e => e.FirstTime).HasColumnName("FirstTime").HasDefaultValue(true);
            });

            modelBuilder.Entity<Produto>(entity =>
            {
                entity.ToTable("PRODUTOS");
                entity.HasKey(e => e.IdProduto);
                entity.Property(e => e.IdProduto).HasColumnName("id_Produto");
                entity.Property(e => e.TipoInstrumento).HasColumnName("Tipo_Instrumento").HasMaxLength(150).IsRequired();
                entity.Property(e => e.Preco).HasColumnName("Preco").HasColumnType("decimal(8,2)").IsRequired();
                entity.Property(e => e.Modelo).HasColumnName("Modelo").HasMaxLength(100);
                entity.Property(e => e.ImagemUrl).HasColumnName("Imagem_Url").HasMaxLength(255);

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
                entity.Property(e => e.IdCupomUso).HasColumnName("id_CupomUso");

                entity.HasOne(i => i.Produto)
                      .WithMany()
                      .HasForeignKey(i => i.CodProduto)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(i => i.CupomUso)
                      .WithMany()
                      .HasForeignKey(i => i.IdCupomUso)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<Cupom>(entity =>
            {
                entity.ToTable("CUPONS");
                entity.HasKey(e => e.IdCupom);
                entity.Property(e => e.IdCupom).HasColumnName("id_Cupom");
                entity.Property(e => e.Valor).HasColumnName("VALOR").HasMaxLength(50).IsRequired();
                entity.HasIndex(e => e.Valor).IsUnique();
                entity.Property(e => e.Desconto).HasColumnName("DESCONTO").IsRequired();
                entity.Property(e => e.Ativo).HasColumnName("Ativo").HasDefaultValue(true);
                entity.Property(e => e.DataCriacao).HasColumnName("DataCriacao").HasDefaultValueSql("GETDATE()");
            });

            modelBuilder.Entity<CupomUso>(entity =>
            {
                entity.ToTable("CUPOM_USOS");
                entity.HasKey(e => e.IdCupomUso);
                entity.Property(e => e.IdCupomUso).HasColumnName("id_CupomUso");
                entity.Property(e => e.CodCliente).HasColumnName("Cod_do_Cliente").IsRequired();
                entity.Property(e => e.CodigoCupom).HasColumnName("Codigo_Cupom").HasMaxLength(50).IsRequired();
                entity.Property(e => e.IdVenda).HasColumnName("id_Venda");
                entity.Property(e => e.DataUso).HasColumnName("Data_Uso").HasDefaultValueSql("GETDATE()");
                entity.HasIndex(e => new { e.CodCliente, e.CodigoCupom }).IsUnique();

                entity.HasOne(e => e.Cliente)
                      .WithMany()
                      .HasForeignKey(e => e.CodCliente)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
