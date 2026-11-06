using Application.Commands;
using Application.Dtos;
using Application.Interfaces;
using Common;
using MediatR;

namespace Application.Handler.CommandHandler
{
    public class BackupCommandHandlers(IBackupService backupService) :
        IRequestHandler<CreateBackupCommand, ServiceResult<BackupFileDto>>,
        IRequestHandler<DeleteBackupCommand, ServiceResult<BackupFileDto>>
    {
        public async Task<ServiceResult<BackupFileDto>> Handle(
            CreateBackupCommand request,
            CancellationToken cancellationToken)
        {
            var file = await backupService.CreateAsync(cancellationToken);
            return ServiceResult<BackupFileDto>.Ok(file);
        }

        public async Task<ServiceResult<BackupFileDto>> Handle(
            DeleteBackupCommand request,
            CancellationToken cancellationToken)
        {
            var deleted = await backupService.DeleteAsync(request.FileName, cancellationToken);
            if (!deleted)
                return ServiceResult<BackupFileDto>.Failed("فایل پشتیبان موردنظر پیدا نشد.");

            return ServiceResult<BackupFileDto>.Ok(new BackupFileDto
            {
                FileName = request.FileName,
            });
        }
    }
}
