using Microsoft.EntityFrameworkCore;
using Trackii.Api.Models;
using RouteModel = Trackii.Api.Models.Route;

namespace Trackii.Api.Data;

public sealed class TrackiiDbContext : DbContext
{
    public TrackiiDbContext(DbContextOptions<TrackiiDbContext> options) : base(options) { }

    public DbSet<Location> Locations => Set<Location>();
    public DbSet<Token> Tokens => Set<Token>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Device> Devices => Set<Device>();
    public DbSet<Area> Areas => Set<Area>();
    public DbSet<Family> Families => Set<Family>();
    public DbSet<Subfamily> Subfamilies => Set<Subfamily>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<RouteModel> Routes => Set<RouteModel>();
    public DbSet<RouteStep> RouteSteps => Set<RouteStep>();
    public DbSet<WorkOrder> WorkOrders => Set<WorkOrder>();
    public DbSet<WipItem> WipItems => Set<WipItem>();
    public DbSet<WipStepExecution> WipStepExecutions => Set<WipStepExecution>();
    public DbSet<ScanEvent> ScanEvents => Set<ScanEvent>();
    public DbSet<UnregisteredPart> UnregisteredParts => Set<UnregisteredPart>();
    public DbSet<ErrorCategory> ErrorCategories => Set<ErrorCategory>();
    public DbSet<ErrorCode> ErrorCodes => Set<ErrorCode>();
    public DbSet<ReworkItem> ReworkItems => Set<ReworkItem>();
    public DbSet<ReworkLog> ReworkLogs => Set<ReworkLog>();
    public DbSet<ScrapItem> ScrapItems => Set<ScrapItem>();
    public DbSet<ScrapLog> ScrapLogs => Set<ScrapLog>();
    public DbSet<WorkOrderItem> WorkOrderItems => Set<WorkOrderItem>();
    public DbSet<WorkOrderLog> WorkOrderLogs => Set<WorkOrderLog>();
    public DbSet<Warehouse> Warehouses => Set<Warehouse>();
    public DbSet<StockMovementItem> StockMovementItems => Set<StockMovementItem>();
    public DbSet<StockMovementLog> StockMovementLogs => Set<StockMovementLog>();
    public DbSet<InventoryBalance> InventoryBalances => Set<InventoryBalance>();
    public DbSet<InventorySnapshot> InventorySnapshots => Set<InventorySnapshot>();
    public DbSet<ProductionStats> ProductionStats => Set<ProductionStats>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Family>().Property(e => e.AreaId).HasColumnName("area_id");
        modelBuilder.Entity<Subfamily>().Property(e => e.FamilyId).HasColumnName("family_id");
        modelBuilder.Entity<Product>().Property(e => e.SubfamilyId).HasColumnName("subfamily_id");
        modelBuilder.Entity<WipItem>().Property(e => e.WorkOrderId).HasColumnName("work_order_id");

        modelBuilder.Entity<InventoryBalance>().HasKey(e => new { e.ProductId, e.WarehouseId });
        modelBuilder.Entity<ProductionStats>().HasKey(e => new { e.LocationId, e.RouteStepId, e.WorkOrderId, e.DateKey });

        modelBuilder.Entity<Token>()
            .HasOne(t => t.CreatedByUser)
            .WithMany(u => u.TokensCreated)
            .HasForeignKey(t => t.CreatedBy)
            .IsRequired(false);

        modelBuilder.Entity<ScrapLog>()
            .HasOne(s => s.ScrapItem)
            .WithMany(i => i.ScrapLogs)
            .HasForeignKey(s => s.ScrapItemId);

        modelBuilder.Entity<ScrapLog>()
            .HasOne(s => s.ErrorCode)
            .WithMany()
            .HasForeignKey(s => s.ErrorCodeId);

        modelBuilder.Entity<ReworkLog>()
            .HasOne(r => r.TargetRouteStep)
            .WithMany(rs => rs.ReworkLogs)
            .HasForeignKey(r => r.TargetRouteStepId);

        modelBuilder.Entity<ReworkLog>()
            .HasOne(r => r.ReworkItem)
            .WithMany(i => i.ReworkLogs)
            .HasForeignKey(r => r.ReworkItemId);

        modelBuilder.Entity<ReworkItem>()
            .HasOne(r => r.WorkOrder)
            .WithMany(w => w.ReworkItems)
            .HasForeignKey(r => r.WorkOrderId);

        modelBuilder.Entity<ReworkItem>()
            .HasOne(r => r.User)
            .WithMany(u => u.ReworkItems)
            .HasForeignKey(r => r.UserId);

        modelBuilder.Entity<WorkOrderLog>()
            .HasOne(w => w.WorkOrderItem)
            .WithMany(i => i.WorkOrderLogs)
            .HasForeignKey(w => w.WorkOrderItemId);

        modelBuilder.Entity<WorkOrderItem>()
            .HasOne(w => w.WorkOrder)
            .WithMany(wo => wo.WorkOrderItems)
            .HasForeignKey(w => w.WorkOrderId);

        modelBuilder.Entity<WorkOrderItem>()
            .HasOne(w => w.User)
            .WithMany(u => u.WorkOrderItems)
            .HasForeignKey(w => w.UserId);

        modelBuilder.Entity<ScrapItem>()
            .HasOne(s => s.WipStepExecution)
            .WithMany(w => w.ScrapItems)
            .HasForeignKey(s => s.WipStepExecutionId);
    }
}
