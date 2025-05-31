using Construccion.Models;
using Microsoft.EntityFrameworkCore;

public partial class ConstruccionContext : DbContext
{
    public ConstruccionContext(DbContextOptions<ConstruccionContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Rol> Rols { get; set; }
    public virtual DbSet<Usuario> Usuarios { get; set; }
    public virtual DbSet<Partida> Partidas { get; set; }
    public virtual DbSet<Obra> Obras { get; set; }
    public virtual DbSet<Material> Materiales { get; set; }
    public virtual DbSet<SeguimientoObra> SeguimientoObras { get; set; }
    public virtual DbSet<Hito> Hitos { get; set; }
    public virtual DbSet<Bodega> Bodegas { get; set; }
    public virtual DbSet<Insumos> Insumos { get; set; }
    public virtual DbSet<SalidaMaterial> SalidaMateriales { get; set; }
    public virtual DbSet<UsuarioAcceso> UsuarioAccesos { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Rol>(entity =>
        {
            entity.HasKey(e => e.IdRol).HasName("PK_Rol");
            entity.ToTable("Rol");
            entity.Property(e => e.NombreRol).HasMaxLength(15).IsUnicode(false);
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.IdUsuario).HasName("PK_Usuario");
            entity.ToTable("Usuario");
            entity.Property(e => e.Clave).HasMaxLength(20).IsUnicode(false);
            entity.Property(e => e.Identificador).HasMaxLength(15).IsUnicode(false);
            entity.Property(e => e.NombreCompleto).HasMaxLength(40).IsUnicode(false);
            entity.HasOne(d => d.IdRolNavigation)
                .WithMany(p => p.Usuarios)
                .HasForeignKey(d => d.IdRol)
                .HasConstraintName("FK_Usuario_Rol");
        });

        modelBuilder.Entity<UsuarioAcceso>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_UsuarioAcceso");
            entity.ToTable("UsuarioAcceso");
            entity.Property(e => e.FechaHoraAcceso).HasColumnType("datetime");
            entity.Property(e => e.DireccionIP)
                .HasMaxLength(45)
                .IsUnicode(false)
                .IsRequired();
            entity.HasOne(d => d.Usuario)
                .WithMany()
                .HasForeignKey(d => d.IdUsuario)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_UsuarioAcceso_Usuario");
        });

        modelBuilder.Entity<Obra>(entity =>
        {
            entity.HasKey(e => e.IdObra).HasName("PK_Obra");
            entity.ToTable("Obras");
            entity.Property(e => e.NombreObra).HasMaxLength(100).IsUnicode(false);
            entity.Property(e => e.Cliente).HasMaxLength(100).IsUnicode(false);
            entity.Ignore(e => e.TotalPresupuesto);
            entity.Ignore(e => e.IVA);
            entity.Ignore(e => e.Total);
        });

        modelBuilder.Entity<Partida>(entity =>
        {
            entity.HasKey(e => e.IdPartida).HasName("PK_Partida");
            entity.ToTable("Partida");
            entity.Property(e => e.Nombre).HasMaxLength(100).IsUnicode(false);
            entity.Property(e => e.ManoDeObra).HasColumnType("decimal(18, 2)");
            entity.HasOne(d => d.Obra)
                .WithMany(p => p.Partidas)
                .HasForeignKey(d => d.IdObra)
                .HasConstraintName("FK_Partida_Obra");
            entity.Ignore(e => e.TotalMateriales);
            entity.Ignore(e => e.Subtotal);
        });

        modelBuilder.Entity<Material>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_Material");
            entity.ToTable("Material");
            entity.Property(e => e.Nombre).HasMaxLength(100).IsUnicode(false);
            entity.Property(e => e.TipoUnidad).HasMaxLength(10).IsUnicode(false);
            entity.Property(e => e.Precio).HasColumnType("decimal(18, 2)");
            entity.HasOne(d => d.Partida)
                .WithMany(p => p.Materiales)
                .HasForeignKey(d => d.IdPartida)
                .HasConstraintName("FK_Material_Partida");
        });


        modelBuilder.Entity<SeguimientoObra>(entity =>
        {
            entity.HasKey(e => e.IdSeguimiento).HasName("PK_SeguimientoObra");
            entity.ToTable("SeguimientoObra");
            entity.Property(e => e.FechaSeguimiento).HasColumnType("datetime");
            entity.HasOne(d => d.Obra)
                .WithMany()
                .HasForeignKey(d => d.IdObra)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_SeguimientoObra_Obra");
        });

        modelBuilder.Entity<Hito>(entity =>
        {
            entity.HasKey(e => e.IdHito).HasName("PK_Hito");
            entity.ToTable("Hitos");

            entity.Property(e => e.NombreHito)
                .IsRequired()
                .HasMaxLength(100)
                .IsUnicode(false);

            entity.Property(e => e.Finalizado)
                .IsRequired()
                .HasDefaultValue(false);

            entity.HasOne(d => d.Obra)
                .WithMany()
                .HasForeignKey(d => d.IdObra)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_Hito_Obra");

            entity.HasOne(d => d.Partida)
                .WithMany()  
                .HasForeignKey(d => d.IdPartida)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_Hito_Partida");
        });


        modelBuilder.Entity<Bodega>(entity =>
        {
            entity.HasKey(e => e.IdBodega).HasName("PK_Bodega");
            entity.ToTable("Bodega");
            entity.Property(e => e.NombreBodega).HasMaxLength(100).IsUnicode(false);
        });


        modelBuilder.Entity<Insumos>(entity =>
        {
            entity.HasKey(e => e.IdInsumos).HasName("PK_Insumos");
            entity.ToTable("Insumos");
            entity.Property(e => e.Nombre).HasMaxLength(100).IsUnicode(false);
            entity.Property(e => e.Tipo).HasMaxLength(50).IsUnicode(false);
            entity.Property(e => e.Cantidad).HasColumnType("decimal(18, 2)");


            entity.HasOne(d => d.Bodega)
                .WithMany(p => p.Insumos)
                .HasForeignKey(d => d.IdBodega)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_Insumos_Bodega");
        });

        modelBuilder.Entity<SalidaMaterial>(entity =>
        {
            entity.HasKey(e => e.IdSalida).HasName("PK_SalidaMaterial");
            entity.ToTable("SalidaMaterial");
            entity.Property(e => e.Cantidad).IsRequired();
            entity.Property(e => e.FechaSalida).HasColumnType("datetime");

            entity.HasOne(e => e.Bodega)
                  .WithMany()
                  .HasForeignKey(e => e.IdBodega)
                  .OnDelete(DeleteBehavior.Restrict)
                  .HasConstraintName("FK_SalidaMaterial_Bodega");

            entity.HasOne(e => e.Insumo)
                  .WithMany()
                  .HasForeignKey(e => e.IdInsumo)
                  .OnDelete(DeleteBehavior.Restrict)
                  .HasConstraintName("FK_SalidaMaterial_Insumos");

            
            entity.HasOne(e => e.Obra)
                  .WithMany()  
                  .HasForeignKey(e => e.IdObra)
                  .OnDelete(DeleteBehavior.SetNull)  
                  .HasConstraintName("FK_SalidaMaterial_Obra");
        });


        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}




