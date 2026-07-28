

class DocumentsService{
    getOfficePhotoUrl(officeId: string, photoId: string) {
        return `/documents/api/v1/photos/offices/${officeId}/avatar/${photoId}`;
    }
}

export const documentsService = new DocumentsService();