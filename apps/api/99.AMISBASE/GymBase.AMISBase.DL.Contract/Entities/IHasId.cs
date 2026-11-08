namespace GymBase.AMISBase.DL.Contract.Entities;

/// <summary>Đối tượng có khóa chính <c>Id</c>: entity và DTO trả ra API.</summary>
public interface IHasId
{
    Guid Id { get; }
}
