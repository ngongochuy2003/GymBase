namespace GymBase.BLFile.BLStorage;

public interface IStorageResolver
{
    IFileStorage Resolve(string providerName);
    IFileStorage GetDefault();
}
