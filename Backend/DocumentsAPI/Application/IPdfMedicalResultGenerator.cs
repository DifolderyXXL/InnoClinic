using MicroserviceApiKernel.Results;

namespace DocumentsAPI.Application;

public interface IPdfMedicalResultGenerator
{
    public byte[] Generate(MedicalResultPdfData data);
}