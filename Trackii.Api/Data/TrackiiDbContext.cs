using Microsoft.EntityFrameworkCore;
using Trackii.Api.Models;
using RouteModel = Trackii.Api.Models.Route;

namespace Trackii.Api.Data;

public sealed class TrackiiDbContext : DbContext
{
    public TrackiiDbContext(DbContextOptions<TrackiiDbContext> options) : base(options) { }

    public DbSet<Location> Locations => Set<Location>();
    public DbSet<Token> Tokens => Set<Token>();
    public DbSet<SystemToken> SystemTokens => Set<SystemToken>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Device> Devices => Set<Device>();
    public DbSet<Area> Areas => Set<Area>();
    public DbSet<Family> Families => Set<Family>();
    public DbSet<Subfamily> Subfamilies => Set<Subfamily>();
    public DbSet<SubfamilyActiveRoute> SubfamilyActiveRoutes => Set<SubfamilyActiveRoute>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<RouteModel> Routes => Set<RouteModel>();
    public DbSet<RouteStep> RouteSteps => Set<RouteStep>();
    public DbSet<RouteSubStep> RouteSubSteps => Set<RouteSubStep>();
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

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

        foreach (var entityEntry in entries)
        {
            var updatedAtProperty = entityEntry.Metadata.FindProperty("UpdatedAt");
            if (updatedAtProperty != null)
            {
                entityEntry.Property("UpdatedAt").CurrentValue = DateTime.Now;
            }

            if (entityEntry.State == EntityState.Added)
            {
                var createdAtProperty = entityEntry.Metadata.FindProperty("CreatedAt");
                if (createdAtProperty != null)
                {
                    entityEntry.Property("CreatedAt").CurrentValue = DateTime.Now;
                }
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Family>().Property(e => e.AreaId).HasColumnName("area_id");
        modelBuilder.Entity<Subfamily>().Property(e => e.FamilyId).HasColumnName("family_id");
        modelBuilder.Entity<Product>().Property(e => e.SubfamilyId).HasColumnName("subfamily_id");
        modelBuilder.Entity<WipItem>().Property(e => e.WorkOrderId).HasColumnName("work_order_id");

        modelBuilder.Entity<ScrapItem>(entity =>
        {
            entity.ToTable("scrap_item");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
        });

        modelBuilder.Entity<ScrapLog>(entity =>
        {
            entity.ToTable("scrap_log");
            entity.Property(e => e.ScrapItemId).HasColumnName("scrap_item_id");
            entity.Property(e => e.ErrorCodeId).HasColumnName("error_code_id");
            entity.Property(e => e.QtyScrapped).HasColumnName("qty_scrapped");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
        });

        modelBuilder.Entity<ReworkItem>(entity =>
        {
            entity.ToTable("rework_item");
            entity.Property(e => e.WorkOrderId).HasColumnName("work_order_id");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
        });

        modelBuilder.Entity<ReworkLog>(entity =>
        {
            entity.ToTable("rework_log");
            entity.Property(e => e.ReworkItemId).HasColumnName("rework_item_id");
            entity.Property(e => e.TargetRouteStepId).HasColumnName("target_route_step_id");
            entity.Property(e => e.QtyReworked).HasColumnName("qty_reworked");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
        });

        modelBuilder.Entity<WorkOrderItem>(entity =>
        {
            entity.ToTable("work_order_item");
            entity.Property(e => e.WorkOrderId).HasColumnName("work_order_id");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
        });

        modelBuilder.Entity<WorkOrderLog>(entity =>
        {
            entity.ToTable("work_order_log");
            entity.Property(e => e.WorkOrderItemId).HasColumnName("work_order_item_id");
            entity.Property(e => e.EventType).HasColumnName("event_type");
            entity.Property(e => e.FieldName).HasColumnName("field_name");
            entity.Property(e => e.OldValue).HasColumnName("old_value");
            entity.Property(e => e.NewValue).HasColumnName("new_value");
            entity.Property(e => e.ModifiedAt).HasColumnName("modified_at");
        });

        modelBuilder.Entity<Warehouse>(entity =>
        {
            entity.ToTable("warehouse");
            entity.Property(e => e.LocationId).HasColumnName("location_id");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
        });

        modelBuilder.Entity<StockMovementItem>(entity =>
        {
            entity.ToTable("stock_movement_item");
            entity.Property(e => e.MovementType).HasColumnName("movement_type");
            entity.Property(e => e.ReferenceType).HasColumnName("reference_type");
            entity.Property(e => e.ReferenceId).HasColumnName("reference_id");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
        });

        modelBuilder.Entity<StockMovementLog>(entity =>
        {
            entity.ToTable("stock_movement_log");
            entity.Property(e => e.StockMovementItemId).HasColumnName("stock_movement_item_id");
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.WarehouseId).HasColumnName("warehouse_id");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
        });

        modelBuilder.Entity<InventoryBalance>(entity =>
        {
            entity.ToTable("inventory_balance");
            entity.HasKey(e => new { e.ProductId, e.WarehouseId });
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.WarehouseId).HasColumnName("warehouse_id");
            entity.Property(e => e.QtyOnHand).HasColumnName("qty_on_hand");
            entity.Property(e => e.QtyReserved).HasColumnName("qty_reserved");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
        });

        modelBuilder.Entity<InventorySnapshot>(entity =>
        {
            entity.ToTable("inventory_snapshot");
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.WarehouseId).HasColumnName("warehouse_id");
            entity.Property(e => e.SnapshotAt).HasColumnName("snapshot_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
        });

        modelBuilder.Entity<ProductionStats>(entity =>
        {
            entity.ToTable("production_stats");
            entity.HasKey(e => new { e.LocationId, e.RouteStepId, e.WorkOrderId, e.DateKey });
            entity.Property(e => e.LocationId).HasColumnName("location_id");
            entity.Property(e => e.RouteStepId).HasColumnName("route_step_id");
            entity.Property(e => e.WorkOrderId).HasColumnName("work_order_id");
            entity.Property(e => e.DateKey).HasColumnName("date_key");
            entity.Property(e => e.TotalQtyIn).HasColumnName("total_qty_in");
            entity.Property(e => e.TotalQtyScrap).HasColumnName("total_qty_scrap");
            entity.Property(e => e.TotalQtyRework).HasColumnName("total_qty_rework");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
        });

        modelBuilder.Entity<SubfamilyActiveRoute>()
            .ToTable("subfamily_active_route");

        modelBuilder.Entity<SubfamilyActiveRoute>()
            .HasOne(sar => sar.Subfamily)
            .WithOne(sf => sf.ActiveRouteMapping)
            .HasForeignKey<SubfamilyActiveRoute>(sar => sar.SubfamilyId);

        modelBuilder.Entity<RouteSubStep>()
            .ToTable("route_sub_step");

        modelBuilder.Entity<SystemToken>()
            .ToTable("system_tokens");

        modelBuilder.Entity<ScanEvent>()
            .ToTable("scan_event");

        modelBuilder.Entity<ScanEvent>()
            .HasOne(se => se.Device)
            .WithMany()
            .HasForeignKey(se => se.DeviceId);

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

        modelBuilder.Entity<Warehouse>()
            .HasOne(w => w.Location)
            .WithMany(l => l.Warehouses)
            .HasForeignKey(w => w.LocationId)
            .IsRequired(false);

        modelBuilder.Entity<StockMovementItem>()
            .HasOne(s => s.User)
            .WithMany(u => u.StockMovementItems)
            .HasForeignKey(s => s.UserId);

        modelBuilder.Entity<StockMovementLog>()
            .HasOne(s => s.StockMovementItem)
            .WithMany(i => i.StockMovementLogs)
            .HasForeignKey(s => s.StockMovementItemId);

        modelBuilder.Entity<StockMovementLog>()
            .HasOne(s => s.Product)
            .WithMany(p => p.StockMovementLogs)
            .HasForeignKey(s => s.ProductId);

        modelBuilder.Entity<StockMovementLog>()
            .HasOne(s => s.Warehouse)
            .WithMany(w => w.StockMovementLogs)
            .HasForeignKey(s => s.WarehouseId);

        modelBuilder.Entity<InventoryBalance>()
            .HasOne(i => i.Product)
            .WithMany(p => p.InventoryBalances)
            .HasForeignKey(i => i.ProductId);

        modelBuilder.Entity<InventoryBalance>()
            .HasOne(i => i.Warehouse)
            .WithMany(w => w.InventoryBalances)
            .HasForeignKey(i => i.WarehouseId);

        modelBuilder.Entity<InventorySnapshot>()
            .HasOne(i => i.Product)
            .WithMany(p => p.InventorySnapshots)
            .HasForeignKey(i => i.ProductId);

        modelBuilder.Entity<InventorySnapshot>()
            .HasOne(i => i.Warehouse)
            .WithMany(w => w.InventorySnapshots)
            .HasForeignKey(i => i.WarehouseId);

        modelBuilder.Entity<InventorySnapshot>()
            .HasOne(i => i.CreatedByUser)
            .WithMany(u => u.InventorySnapshotsCreated)
            .HasForeignKey(i => i.CreatedBy);
    }
}
