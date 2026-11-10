using GymBase.Base.Entities;

namespace GymBase.UnitTests.Base;

public class EntityTests
{
    private sealed class Package : BaseAuditableEntity, ISoftDelete
    {
        public bool is_deleted { get; set; }
    }

    private sealed class CheckIn : BaseEntity, ISoftDelete
    {
        public bool is_deleted { get; set; }
    }

    private static readonly DateTime T1 = new(2026, 11, 8, 1, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime T2 = new(2026, 11, 8, 2, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void New_entity_gets_uuid_v7_id()
    {
        var entity = new Package();

        Assert.NotEqual(Guid.Empty, entity.Id);
        Assert.Equal(7, entity.Id.Version);
    }

    [Fact]
    public void Ids_increase_in_creation_order()
    {
        var first = new Package();
        Thread.Sleep(2); // UUIDv7 sắp theo mili-giây
        var second = new Package();

        Assert.True(first.Id.CompareTo(second.Id) < 0);
    }

    [Fact]
    public void MarkCreated_sets_creation_audit_only()
    {
        var entity = new Package();

        entity.MarkCreated("admin", T1);

        Assert.Equal(T1, entity.ngay_tao);
        Assert.Equal("admin", entity.nguoi_tao);
        Assert.Null(entity.ngay_chinh_sua);
        Assert.Null(entity.nguoi_chinh_sua);
    }

    [Fact]
    public void MarkUpdated_keeps_creation_audit()
    {
        var entity = new Package();
        entity.MarkCreated("admin", T1);

        entity.MarkUpdated("trainer", T2);

        Assert.Equal(T1, entity.ngay_tao);
        Assert.Equal("admin", entity.nguoi_tao);
        Assert.Equal(T2, entity.ngay_chinh_sua);
        Assert.Equal("trainer", entity.nguoi_chinh_sua);
    }

    [Theory]
    [InlineData(DateTimeKind.Local)]
    [InlineData(DateTimeKind.Unspecified)]
    public void Non_utc_time_is_rejected(DateTimeKind kind)
    {
        var entity = new Package();
        var notUtc = DateTime.SpecifyKind(T1, kind);

        Assert.Throws<ArgumentException>(() => entity.MarkCreated("admin", notUtc));
        Assert.Throws<ArgumentException>(() => entity.MarkUpdated("admin", notUtc));
    }

    [Fact]
    public void Soft_delete_sets_flag_and_can_restore()
    {
        var entity = new CheckIn();
        Assert.False(entity.is_deleted);

        entity.MarkDeleted();

        Assert.True(entity.is_deleted);

        entity.Restore();

        Assert.False(entity.is_deleted);
    }
}
