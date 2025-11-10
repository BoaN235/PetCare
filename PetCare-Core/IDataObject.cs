namespace PetCare.Core;

public interface IDataObject
{
    Task Import();
    Task Export();
    Task Clear();
}