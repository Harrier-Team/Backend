using Hackatohm.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Hackatohm.Infrastructure.Persistence;

/// <summary>
/// Contexto do Entity Framework Core para a base de dados do Hackatohm.
/// </summary>
public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Motor> Motores => Set<Motor>();
    public DbSet<Sensor> Sensores => Set<Sensor>();
    public DbSet<LeituraTemperatura> LeiturasTemperatura => Set<LeituraTemperatura>();
    public DbSet<RegraQualidade> RegrasQualidade => Set<RegraQualidade>();
    public DbSet<MotorRegraQualidade> MotoresRegrasQualidade => Set<MotorRegraQualidade>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Motor>(entity =>
        {
            entity.ToTable("motores");
            entity.HasKey(motor => motor.Id);
            entity.Property(motor => motor.Nome).HasMaxLength(100).IsRequired();
            entity.Property(motor => motor.Tipo).HasMaxLength(80).IsRequired();
        });

        modelBuilder.Entity<Sensor>(entity =>
        {
            entity.ToTable("sensores");
            entity.HasKey(sensor => sensor.Id);
            entity.Property(sensor => sensor.Id).HasMaxLength(64);
            entity.Property(sensor => sensor.Modelo).HasMaxLength(80).IsRequired();
            entity.Property(sensor => sensor.IpAtual).HasMaxLength(45);
            entity.HasOne(sensor => sensor.Motor)
                .WithMany(motor => motor.Sensores)
                .HasForeignKey(sensor => sensor.MotorId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<LeituraTemperatura>(entity =>
        {
            entity.ToTable("leituras_temperatura");
            entity.HasKey(leitura => leitura.Id);
            entity.Property(leitura => leitura.ValorCelsius).HasPrecision(5, 2);
            entity.Property(leitura => leitura.DataHora).HasColumnType("timestamp with time zone");
            entity.HasOne(leitura => leitura.Sensor)
                .WithMany()
                .HasForeignKey(leitura => leitura.SensorId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<RegraQualidade>(entity =>
        {
            entity.ToTable("regras_qualidade");
            entity.HasKey(regra => regra.Id);
            entity.Property(regra => regra.Nome).HasMaxLength(120).IsRequired();
            entity.Property(regra => regra.Descricao).HasMaxLength(255);
            entity.Property(regra => regra.TipoRegra).HasMaxLength(100).IsRequired();
            entity.Property(regra => regra.ParametrosJson).HasColumnType("jsonb").IsRequired();
            entity.Property(regra => regra.CriadoEm).HasColumnType("timestamp with time zone");
        });

        modelBuilder.Entity<MotorRegraQualidade>(entity =>
        {
            entity.ToTable("motores_regras_qualidade");
            entity.HasKey(mr => new { mr.MotorId, mr.RegraQualidadeId });

            entity.Property(mr => mr.VinculadoEm).HasColumnType("timestamp with time zone");

            entity.HasOne(mr => mr.Motor)
                .WithMany(m => m.RegrasAssociadas)
                .HasForeignKey(mr => mr.MotorId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(mr => mr.RegraQualidade)
                .WithMany(r => r.MotoresVinculados)
                .HasForeignKey(mr => mr.RegraQualidadeId)
                .OnDelete(DeleteBehavior.Cascade);

            // Índice para acelerar a busca no hot path de ingestão de telemetria
            entity.HasIndex(mr => new { mr.MotorId, mr.Ativo });
        });
    }
}
