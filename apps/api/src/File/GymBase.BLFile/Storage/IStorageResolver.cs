namespace GymBase.BLFile.Storage;

public interface IStorageResolver
{
    IFileStorage Resolve(string providerName);
    IFileStorage GetDefault();
}
